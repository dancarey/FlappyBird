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

    // Holds the current frame's pixels as we manipulate them
    Color32[] framePixels;
    // Texture that recieves the final processed form of the frame
    Texture2D processedTexture;
    // Temp variable to hold current pixel's index in image processing loop
    int pixNdx;
    // Temp variables to hold the hue, saturation, vibrance for a HSVcolor.
    float hOrig, sOrig, vOrig, hNew, sNew, vNew;

    // Hue window
    float hCeil = 0.6f;
    float hFloor = 0.4f;

    // Saturation window
    float sCeil = 0.6f;
    float sFloor = 0.4f;

    // Valence window
    float vCeil = 0.6f;
    float vFloor = 0.4f;

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

    //---------------------------------------------------------------------
    // Update -  function will run on every camera frame
    //---------------------------------------------------------------------
    void Update()
    {
        // If we never got a camera to work with, don't do anything.
        if (activeCameraTexture == null) {
            return;
        }

        // If the camera's resolution is set to < 100, then the camera isn't fully initialized yet, and we
        // need to do nothing until Unity reports the proper resolution. It might take a few seconds
        // before Unity reports the correct resolution.
        if (activeCameraTexture.width < 100) {
            Debug.Log ("Waiting for camera to initialize...");
            return;
        }

        // When a camera is initialized, this only runs once.
        if (!this.cameraInitialized) { 
            
            this.cameraInitialized = true;

            Debug.Log ("Camera is initialized at " + activeCameraTexture.width + "x" + activeCameraTexture.height);

            // assign our processed texture variable a new texture, with the camera pixel dimensions
            processedTexture = new Texture2D( activeCameraTexture.width, activeCameraTexture.height );

            // Assign our processed texture to the rawImage object on the screen.
            webCamRawImage.texture = processedTexture;
            webCamRawImage.material.mainTexture = processedTexture;

            // Perform rotation or vertical flipping if necessary
            FixCameraGeometry ();
        }

        //---------------------------------------------------------------------
        // The below code will run on every camera frame
        //---------------------------------------------------------------------

        // Pull the current camera frame's pixels into the our editable pixel array
        framePixels = activeCameraTexture.GetPixels32();

        // Loop through every pixel in the camera frame.
        // y is for each row, x is for each pixel on that row
        for (int y = 0; y < activeCameraTexture.height; y++)
        {
            for (int x = 0; x < activeCameraTexture.width; x++)
            {
                // Find the pixel's index in the array
                pixNdx = xyToIndex (x, y, activeCameraTexture.width);

                // Convert the RGB values to HSV
                Color.RGBToHSV( framePixels [pixNdx], out hOrig, out sOrig, out vOrig );

                // ****** BEGIN MODIFYING HSV VALUES  ****** //

                // Adjust Hue (color wheel)
                if (hOrig < hFloor || hOrig > hCeil)
                {
                    hNew = hOrig;
                } else {
                    hNew = hOrig;
                }

                // Adjust Saturation (color intensity)
                if ( sOrig < sFloor || sOrig > sCeil )
                {
                    sNew = 0.0f;
                } else {
                    sNew = sOrig;
                }

                // Adjust Valence (whiteness)
                if ( vOrig < vFloor || vOrig > vCeil )
                {
                    vNew = 0.0f;
                } else {
                    vNew = vOrig;
                }

                // ****** END MODIFYING HSV VALUES ****** //

                // Take our changed HSV color and convert it back to RGB so the texture can use it
                framePixels [pixNdx] = Color.HSVToRGB (hNew, sNew, vNew);
            }
        }

        // Push the modified pixels out to the processed texture
        processedTexture.SetPixels32 ( framePixels );

        // Apply the changes to the texture, or else nothing will happen.
        processedTexture.Apply ();

    } //END Update()

    //---------------------------------------------------------------------
    // Returns the index to an array ordered in rows.
    //---------------------------------------------------------------------
    private int xyToIndex( int x, int y, int width)
    {
        return (y * width) + x;
    } //END xyToIndex()


    //---------------------------------------------------------------------
    // Converts an index for an array ordered in rows, to x/y coords.
    //---------------------------------------------------------------------
    private void indexToXY( int index, int width, out int x, out int y)
    {
        x = Mathf.FloorToInt (index / width);
        y = index % width;

    } //END indexToXY()
        
} //END CLASS