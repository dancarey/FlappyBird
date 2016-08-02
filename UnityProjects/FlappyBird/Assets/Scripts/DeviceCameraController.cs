using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;

public class DeviceCameraController : MonoBehaviour
{
    // Items linked to scene objects
    public RawImage webCamRawImage;
    public AspectRatioFitter imageFitter;

    // Currently active camera device and texture
    WebCamDevice activeCameraDevice;
    WebCamTexture activeCameraTexture;

    // Image rotation
    Vector3 rotationVector = new Vector3(0f, 0f, 0f);

    // Image flipping uvRect
    Rect vertDefaultRect = new Rect(1f, 0f, -1f, 1f);
    Rect vertFixedRect = new Rect(0f, 1f, 1f, -1f);

    // "Camera is initialized" flag
    bool cameraInitialized = false;

    Color32[] framePixels;
    Texture2D processedTexture;

    void Start()
    {
        // Make sure we have at least one camera
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.Log ("No camera devices detected.");
            return;
        }

        // Loop through each camera device
        for( int i=0 ; i < WebCamTexture.devices.Length; i++ )
        {
            // Print info about the camera
            Debug.Log(
                "Device " + i + " : \n" +
                "  - Name: " + WebCamTexture.devices[i].name + "\n" +
                "  - IsFrontFacing: " + WebCamTexture.devices[i].isFrontFacing.ToString() + "\n" +
                "  - IsNull: " + WebCamTexture.devices[i].Equals(null)
            );

            // If the camera is front facing, set it as the active camera
            if ( WebCamTexture.devices[ i ].isFrontFacing )
            {
                SetActiveCamera (i);
            }
        }

        // If no cameras were detected as front facing, just use the first camera.
        if( activeCameraTexture == null ) { SetActiveCamera ( 0 ); }
    }

    public void SetActiveCamera( int deviceIndex )
    {
        if ( activeCameraTexture != null ) { activeCameraTexture.Stop (); }

        activeCameraDevice = WebCamTexture.devices.ElementAt( deviceIndex );
        activeCameraTexture = new WebCamTexture (
            WebCamTexture.devices.ElementAt( deviceIndex ).name, // name of device to use as input
            640, // requested width
            480,  // requested height
            30    // requested frames per second (fps)
        );

        activeCameraTexture.Play();

        this.cameraInitialized = false;
    }

    void FixCameraGeometry()
    {
        // Rotate image to show correct orientation 
        rotationVector.z = -activeCameraTexture.videoRotationAngle;
        webCamRawImage.rectTransform.localEulerAngles = rotationVector;

        // Set AspectRatioFitter's ratio
        float videoRatio = (float)activeCameraTexture.width / (float)activeCameraTexture.height;
        imageFitter.aspectRatio = videoRatio;

        // Unflip if vertically flipped
        webCamRawImage.uvRect = activeCameraTexture.videoVerticallyMirrored ? vertFixedRect : vertDefaultRect;
    }

    // This function runs on every frame
    void Update()
    {
        // If the camera's resolution is set to < 100, then the camera isn't fully initialized yet, and we
        // need to do nothing until Unity reports the proper resolution. This is a Unity bug workaround.
        if (activeCameraTexture.width < 100){
            Debug.Log("Waiting for camera to initialize...");
            return;
        }

        // Once a camera is initialized, this only runs once.
        if ( ! this.cameraInitialized ) { 
            
            this.cameraInitialized = true;

            Debug.Log ("Camera is initialized at " + activeCameraTexture.width + "x" + activeCameraTexture.height);

            framePixels = new Color32[ activeCameraTexture.width * activeCameraTexture.height ];

            processedTexture = new Texture2D (activeCameraTexture.width, activeCameraTexture.height);

            // Assign our processed data to the rawImage object on the screen.
            webCamRawImage.texture = processedTexture;
            webCamRawImage.material.mainTexture = processedTexture;

            // Perform rotation or vertical flipping if necessary
            FixCameraGeometry ();
        }

        // Grab the current frame's pixels
        framePixels = activeCameraTexture.GetPixels32 ();

        // Loop through every pixel, row (y) and column (x) in the camera frame
        for (int y = 0; y < 640; y++)
        {
            for (int x = 0; x < 480; x++)
            {
                framePixels [x * y + x].a = 255;
                framePixels [x * y + x].r = 60;
                framePixels [x * y + x].g = 0;
                framePixels [x * y + x].b = 0;
            }
        }


        processedTexture.SetPixels32 ( framePixels );

        processedTexture.Apply();

        /*

        // Temp variables to hold the hue, saturation, vibrance for a HSVcolor.
        float hOrig, sOrig, vOrig, hChanged, sChanged, vChanged;
        // Temp variables to hold a color in rgb notation
        Color rgbPixelOrig, rgbPixelChanged;

        // Loop through every pixel, row (y) and column (x) in the camera frame
        for (int y = 0; y < activeCameraTexture.width; y++)
        {
            for (int x = 0; x < activeCameraTexture.height; x++)
            {
                
                // Get the originalframePixels is a 1D array instead of 2D, so we have to calculate it's 1D index.
                rgbPixelOrig = framePixels.ElementAt( (y*x) + x );

                // Convert the rgb camera pixel value to HSV for easy comparison to our rules
                Color.RGBToHSV( rgbPixelOrig, out hOrig, out sOrig, out vOrig );


                // Take our changed HSV color and convert it back to RGB so the texture can use it
                rgbPixelChanged = Color.HSVToRGB (hChanged, sChanged, vChanged);

                // Save the pixel into our processed texture
                processedTexture.SetPixel (y, x, rgbPixelChanged);
            }
        }

        // Apply the changes to the texture, or else nothing will happen.
        processedTexture.Apply ();

        */

    } //END UPDATE()
        
} //END CLASS