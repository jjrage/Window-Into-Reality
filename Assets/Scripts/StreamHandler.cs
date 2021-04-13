using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;

public class StreamHandler
{
    public StreamHandler(int chunkSize = 4 * 1024)
    {
        _context = SynchronizationContext.Current;
        _chunkSize = chunkSize;
    }

    #region Public properties

    #region Events
    public event EventHandler<FrameReadyEventArgs> FrameReady; //Event occurs when freame parsed and ready for display.
    public event EventHandler<ErrorEventArgs> Error; //Event occurs when got some error.
    #endregion

    public byte[] CurrentFrame { get; private set; } //Current frame data in bytes.

    #endregion

    #region Private properties
    private readonly byte[] JpegHeader = new byte[] { 0xff, 0xd8 }; //Signature of JPEG file. https://en.wikipedia.org/wiki/JPEG
    private SynchronizationContext _context; //Class for synchronization code between threads, e.g UI and main working thread.
    private int _errorCode = -1; //Code that occur in case of error.
    private int _chunkSize = 1024 * 4; //JPEG converts an image into chunks of 8x8 blocks of pixels. Here we define size of these blocks.
    private string _defaultLogin = "admin"; //Default login in case if camera has authentication.
    private string _defaultPassword = "admin"; //Default password in case if camera has authentication.
    private bool _streamActive; //State of stream. 
    #endregion

    #region Public methods

    public void ParseStream(Uri uri)
    {
        ParseStream(uri, _defaultLogin, _defaultPassword);
    }

    public void StopStream()
    {
        _streamActive = false;
    }

    #endregion

    #region Private methods
    private void ParseStream(Uri uri, string username, string password)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri); //Creating HTTP request.

        if (!string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(password))
        {
            request.Credentials = new NetworkCredential(username, password); //Authentication credentials in case if needed.
        }

        request.BeginGetResponse(OnGetResponse, request); //Asynchronous handling of Web response.
    }

    private int FindBytes(byte[] buff, byte[] search)
    {
        for (int start = 0; start < buff.Length - search.Length; start++)
        {
            if (buff[start] == search[0]) // First character from 'search' data has found.
            {
                int next;

                for (next = 1; next < search.Length; next++) //Traverse the rest of the bytes.
                {
                    if (buff[start + next] != search[next])
                    {
                        break; //Exit from serach in case if bytes don't match.
                    }
                }

                if (next == search.Length)
                {
                    return start;
                }
            }
        }

        return _errorCode; //Return error code in case if 'search' data not found in 'buff' data.
    }

    private void OnGetResponse(IAsyncResult asyncResult)
    {
        byte[] imageBuffer = new byte[1024 * 1024]; //Default image buffer.

        // HTTP request data
        HttpWebRequest req = (HttpWebRequest)asyncResult.AsyncState;

        try
        {
            HttpWebResponse resp = (HttpWebResponse)req.EndGetResponse(asyncResult); //Received response.
            Debug.Log("Response received");
            // find our magic boundary value
            string contentType = resp.Headers["Content-Type"];

            if (!string.IsNullOrEmpty(contentType) && !contentType.Contains("=")) //Check if we have MJPEG content on HTTP page, throw exception if not.
            {
                Debug.Log("MJPEG Exception thrown");
                throw new Exception("Invalid content-type header.  The camera is likely not returning a proper MJPEG stream.");
            }

            //Parse boundary data from response headers.
            string boundary = resp.Headers["Content-Type"].Split('=')[1].Replace("\"", "");
            byte[] boundaryBytes = Encoding.UTF8.GetBytes(boundary.StartsWith("--") ? boundary : "--" + boundary);

            Stream s = resp.GetResponseStream(); //Get data stream from response.
            BinaryReader br = new BinaryReader(s); //Read data stream from response.

            _streamActive = true;
            byte[] buff = br.ReadBytes(_chunkSize); //Get data buffer from stream.

            while (_streamActive)
            {
                // find the JPEG header
                int imageStart = FindBytes(buff, JpegHeader); //Find JPEG header from gotted data.

                if (imageStart != _errorCode) //Proceed only if we found JPEG header.
                {
                    // Copy the start of the JPEG image to the imageBuffer
                    int size = buff.Length - imageStart;
                    Array.Copy(buff, imageStart, imageBuffer, 0, size);

                    while (true)
                    {
                        buff = br.ReadBytes(_chunkSize); //Keep reading data from response.
                        int imageEnd = FindBytes(buff, boundaryBytes); // Find the end of the jpeg

                        if (imageEnd != _errorCode) //Proceed only if we found end of JPEG data.
                        {
                            //Copy the remainder of the JPEG to the imageBuffer
                            Array.Copy(buff, 0, imageBuffer, size, imageEnd);
                            size += imageEnd;

                            // Copy the latest frame into `CurrentFrame`
                            byte[] frame = new byte[size];
                            Array.Copy(imageBuffer, 0, frame, 0, size);
                            CurrentFrame = frame;

                            //Check if frame ready to be drawn
                            if (FrameReady != null)
                            {
                                FrameReady(this, new FrameReadyEventArgs());
                            }
                            //Copy the leftover data to the start
                            Array.Copy(buff, imageEnd, buff, 0, buff.Length - imageEnd);

                            //Fill the remainder of the buffer with new data and start over
                            byte[] temp = br.ReadBytes(imageEnd);

                            Array.Copy(temp, 0, buff, buff.Length - imageEnd, temp.Length);
                            break;
                        }

                        //Copy all of the data to the imageBuffer
                        Array.Copy(buff, 0, imageBuffer, size, buff.Length);
                        size += buff.Length;

                        if (!_streamActive)
                        {
                            resp.Close();
                            break;
                        }
                    }
                }
            }
            resp.Close();
        }
        catch (Exception ex)
        {
            if (Error != null)
            {
                _context.Post(delegate { Error(this, new ErrorEventArgs() { Message = ex.Message }); }, null);
            }

            return;
        }
    }
    #endregion
}

public class FrameReadyEventArgs : EventArgs
{

}

public sealed class ErrorEventArgs : EventArgs
{
    public string Message { get; set; }
    public int ErrorCode { get; set; }
}