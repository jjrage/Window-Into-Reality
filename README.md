# Window Into Reality

This is MVP of 'Widnow Into Reality' project. Main files and code implementation placed in 'Assets' folder, which contains:
 - Samples - Google VR Cardboard samples.
 - Materials - folder that contains material for displaying stream from remote camera.
 - Plugins - folder that contains required files for building Android application.
 - Scenes - folder with two sample scenes.
 - Scripts - folder with scripts that implements logic of application.
 - XR - folder with unity's XR settings.
 
All application logic is in next scripts:
 - StreamHandler - responsible for parsing MJPEG stream from IP camera.
 - RemoteTexture - responsible for displaying parsed frames on unity scene.
 - FrameReadyEventArgs - responsible for handling parsed frames.
 - ErrorEventArgs - responsible for handling errors.

# Classes specifications
## StreamHandler.cs
 
```
public void ParseStream(Uri uri) {...} - Method for parsing stream from IP camera URL.
public void StopStream() {...} - Method for stop handling IP camera stream.
public int FindBytes(byte[] buff, byte[] search) {...} - Fucntion for searching bytes array 'search' in butes array 'buff'.
private void OnGetResponse(IAsyncResult asyncResult) {...} - Callback that invokes when user get response from request to IP camera.
```

## RemoteTexture.cs

```
private void Start() {...} - Monobehaviour method which is called by Unity at start of application. In this method user can apply some initial settings. 
private void Update() {...} - Monobehaviour method which is called by Unity every frame. In this method user need to display parsed frame from IP camera (when it will be ready for displaying). 
private void OnDestroy() {...} - Monobehaviour method which is called by Unity when object destroyed from scene. 
private void OnMjpegFrameReady(object sender, FrameReadyEventArgs e) {...} - Callback that invokes when StreamHandler parsed frame.
private void OnMjpegError(object sender, ErrorEventArgs e) {...} - Callback that invokes when StreamHandler got error.
```

# Used SDKs
## Third party SDKs
- Google Cardboard XR Plugin for Unity, version 1.4.1 - https://github.com/googlevr/cardboard-xr-plugin/releases/tag/v1.4.1
## Build-in SDKs
- JetBrains Rider Editor, version 3.0.5
- Test Framework, version 1.1.24
- TextMeshPro, version 3.0.4
- Timeline, version 1.5.4
- Unity Collaborate, version 1.3.9
- Unity UI, version 1.0.0
- Visual Studio Code Editor, version 1.2.3
- Visual Studio Editor, version 2.0.7
