using UnityEngine;
using System;
using UnityEngine.UI;

/// <summary>
/// A Unity3D Script to dipsplay Mjpeg streams. Apply this script to the mesh that you want to use to view the Mjpeg stream. 
/// </summary>
public class RemoteTexture : MonoBehaviour
{
    #region Editor properties
    /// <param name="streamAddress">
    /// Set this to be the network address of the mjpg stream. 
    /// Example: "http://'camera.ip.adress'/mjpg/video.mjpg"
    /// </param>
    [SerializeField]
    public string streamAddress;

    /// <summary>
    /// Chunk size for stream processor in kilobytes.
    /// </summary>
    [SerializeField]
    private int chunkSize = 4; //Chunk size for stream processor in kilobytes

    /// <summary>
    /// Material for displaying remote stream.
    /// </summary>
    [SerializeField]
    public Material _materialForDisplay;
    #endregion

    #region Private properties

    #region Constants
    private const int InitWidth = 2;
    private const int InitHeight = 2;
    #endregion

    private StreamHandler mjpeg;
    private Texture2D tex;
    private bool updateFrame = false;
    #endregion

    #region Private methods

    #region Monobehaviour
    private void Start()
    {
        mjpeg = new StreamHandler(chunkSize * 1024);
        mjpeg.FrameReady += OnMjpegFrameReady;
        mjpeg.Error += OnMjpegError;
        Uri mjpegAddress = new Uri(streamAddress);
        mjpeg.ParseStream(mjpegAddress);
        tex = new Texture2D(InitWidth, InitHeight, TextureFormat.ARGB32, false);
    }

    private void Update()
    {
        if (updateFrame)
        {
            tex.LoadImage(mjpeg.CurrentFrame);
            // Assign texture to renderer's material.
            _materialForDisplay.SetTexture("_MainTex", tex);
            updateFrame = false;
        }
    }

    private void OnDestroy()
    {
        _materialForDisplay.SetTexture("_MainTex", null);
        mjpeg.StopStream();
    }
    #endregion

    private void OnMjpegFrameReady(object sender, FrameReadyEventArgs e)
    {
        updateFrame = true;
    }

    private void OnMjpegError(object sender, ErrorEventArgs e)
    {
        Debug.Log($"Error received while reading the MJPEG: {e.Message}");
    }
    #endregion
}