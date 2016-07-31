using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;

public class DeviceCameraController : MonoBehaviour
{
    // Items linked to scene objects
    public RawImage webcamRawImage;
    public RectTransform imageParent;
    public AspectRatioFitter imageFitter;

    // Device cameras
    WebCamDevice frontCameraDevice;
    WebCamDevice backCameraDevice;
    WebCamDevice activeCameraDevice;

    WebCamTexture frontCameraTexture;
    WebCamTexture backCameraTexture;
    WebCamTexture activeCameraTexture;

    // "Camera is initialized" flag
    bool cameraInitialized = false;

    // Image rotation
    Vector3 rotationVector = new Vector3(0f, 0f, 0f);

    // Image uvRect
    Rect defaultRect = new Rect(0f, 0f, 1f, 1f);
    Rect fixedRect = new Rect(0f, 1f, 1f, -1f);

    // Image Parent's scale
    Vector3 defaultScale = new Vector3(1f, 1f, 1f);
    Vector3 fixedScale = new Vector3(-1f, 1f, 1f);

    Color[] framePixels;
    Texture2D processedTexture;

    void Start()
    {
        // Check for device cameras
        Debug.Log("Number of cameras found:" + WebCamTexture.devices.Length);

        if (WebCamTexture.devices.Length == 0) {
            return;
        }

        if (WebCamTexture.devices.Length > 2) {
            Debug.Log("Only two cameras are supported, using first two cameras as back and front.");
        }

        // Get the device's cameras and create WebCamTextures with them
        frontCameraDevice = WebCamTexture.devices.Last();
        backCameraDevice = WebCamTexture.devices.First();

        frontCameraTexture = new WebCamTexture(frontCameraDevice.name, 1280, 720, 30);
        backCameraTexture = new WebCamTexture(backCameraDevice.name, 1280, 720, 30);

        // Set camera filter modes for a smoother looking image
        frontCameraTexture.filterMode = FilterMode.Trilinear;
        backCameraTexture.filterMode = FilterMode.Trilinear;

        // Set which camera to use by default
        SetActiveCamera(frontCameraTexture);
    }
        
    public void SetActiveCamera(WebCamTexture cameraToUse)
    {
        if (activeCameraTexture != null) {
            activeCameraTexture.Stop();
        }

        activeCameraTexture = cameraToUse;
        activeCameraDevice = WebCamTexture.devices.FirstOrDefault(device => device.name == cameraToUse.deviceName);

        webcamRawImage.texture = activeCameraTexture;
        webcamRawImage.material.mainTexture = activeCameraTexture;

        activeCameraTexture.Play();

        // since we've switched cameras, we need to reinitialize.
        cameraInitialized = false;
    }

    // Switches between the device's front and back camera.  If there's only one camera, it's ok.. they will both
    // be set to the same camera since in the start() function we set them to .first and .last
    public void SwitchCamera()
    {
        SetActiveCamera(activeCameraTexture.Equals(frontCameraTexture) ? backCameraTexture : frontCameraTexture);
    }

    // This function runs on every frame
    void Update()
    {
        // If the camera's resolution is set to < 100, then the camera isn't fully initialized yet, and we
        // need to do nothing until Unity reports the proper resolution.
        if (activeCameraTexture.width < 100) {
            Debug.Log("Waiting for camera to initialize...");
            return;
        }

        // Once a camera is initialized, this only runs once.
        if ( ! cameraInitialized ) { 
            
            cameraInitialized = true;

            // Rotate image to show correct orientation 
            rotationVector.z = -activeCameraTexture.videoRotationAngle;
            webcamRawImage.rectTransform.localEulerAngles = rotationVector;

            // Set AspectRatioFitter's ratio
            float videoRatio = (float)activeCameraTexture.width / (float)activeCameraTexture.height;
            imageFitter.aspectRatio = videoRatio;

            // Unflip if vertically flipped
            webcamRawImage.uvRect = activeCameraTexture.videoVerticallyMirrored ? fixedRect : defaultRect;

            // Mirror front-facing camera's image horizontally to look more natural
            if (activeCameraDevice.isFrontFacing ) {
                imageParent.localScale = fixedScale;
            } else {
                imageParent.localScale = defaultScale;
            }
                
            Debug.Log ("Camera is initialized at " + activeCameraTexture.width + "x" + activeCameraTexture.height);

            framePixels = new Color[ activeCameraTexture.width * activeCameraTexture.height ];

            processedTexture = new Texture2D (activeCameraTexture.width, activeCameraTexture.height);
        }
            
        // Grab the current frame's pixels
        framePixels = activeCameraTexture.GetPixels ();

        // Temp variables to hold the hue, saturation, vibrance for a HSVcolor.
        float hOrig, sOrig, vOrig, hChanged, sChanged, vChanged;
        // Temp variables to hold a color in rgb notation
        Color rgbOrig, rgbChanged;

        // Loop through every pixel, row (y) and column (x) in the camera frame
        for (int y = 0; y < activeCameraTexture.width; y++)
        {
            for (int x = 0; x < activeCameraTexture.height; x++)
            {
                // Get the originalframePixels is a 1D array instead of 2D, so we have to calculate it's 1D index.
                rgbOrig = framePixels[(y*x) + x];

                // Convert the rgb camera pixel value to HSV for easy comparison to our rules
                Color.RGBToHSV( rgbOrig, out hOrig, out sOrig, out vOrig );

                // Now we change the color values based on our rules
                if (hOrig > .5) {
                    hChanged = 1;
                } else {
                    hChanged = hOrig;
                }

                if (sOrig > .5) {
                    sChanged = 1;
                } else {
                    sChanged = sOrig;
                }

                if (vOrig > .5) {
                    vChanged = 1;
                } else {
                    vChanged = vOrig;
                }

                // Take our changed HSV color and convert it back to RGB so the texture can use it
                rgbChanged = Color.HSVToRGB (hChanged, sChanged, vChanged);

                // Save the pixel into our processed texture
                processedTexture.SetPixel (x, y, rgbChanged);
            }
        }

        // Apply the changes to the texture, or else nothing will happen.
        processedTexture.Apply ();

        // Assign our processed data to the rawImage object on the screen.
        webcamRawImage.texture = processedTexture;

    }
        
}