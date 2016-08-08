using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;

public class DeviceCameraController : MonoBehaviour
{
    public Camera mainCamera;

    // Items linked to scene objects
    public RawImage webCamRawImage;
    public AspectRatioFitter imageFitter;

    public Canvas UiCalibrationCanvas;

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
    float pixHue, pixSat, pixVal;

    // Sliders in the UI
    public Slider hueMinSlider;
    public Slider hueMaxSlider;

    public Slider satMinSlider;
    public Slider satMaxSlider;

    public Slider valMinSlider;
    public Slider valMaxSlider;

    // Temp vars for finding average location of matched pixels
    int pixMatchCount, xCoordTotal, yCoordTotal ;

    float xCamCoordAvg, yCamCoordAvg;

    public GameObject sparkler;

    float screenPixCoordX, screenPixCoordY;

    float screenRatioX, screenRatioY;

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
            processedTexture = new Texture2D (activeCameraTexture.width, activeCameraTexture.height);

            // Assign our processed texture to the rawImage object on the screen.
            webCamRawImage.texture = processedTexture;
            webCamRawImage.material.mainTexture = processedTexture;

            // Perform rotation or vertical flipping if necessary
            FixCameraGeometry ();
        }
            

        //---------------------------------------------------------------------
        // The below code will run on every camera frame
        //---------------------------------------------------------------------

        // Reset vars for finding average location of matched pixels
        xCoordTotal = 1;
        xCamCoordAvg = 1;

        yCoordTotal = 1;
        yCamCoordAvg = 1;

        pixMatchCount = 1;

        // Pull the current camera frame's pixels into the our editable pixel array
        framePixels = activeCameraTexture.GetPixels32 ();

        // Loop through every pixel in the camera frame.
        // y is for each row, x is for each pixel on that row
        for (int y = 0; y < activeCameraTexture.height; y++)
        {
            for (int x = 0; x < activeCameraTexture.width; x++)
            {
                // Find the pixel's index in the array
                pixNdx = xyToIndex (x, y, activeCameraTexture.width);

                // Convert the RGB values to HSV
                Color.RGBToHSV (framePixels [pixNdx], out pixHue, out pixSat, out pixVal);

                // ****** BEGIN MODIFYING HSV VALUES  ****** //

                // If the pixel's hue, saturation or value is within our sliders,
                // then we have a match.
                if ((hueMinSlider.value <= pixHue && pixHue <= hueMaxSlider.value) &&
                    (satMinSlider.value <= pixSat && pixSat <= satMaxSlider.value) &&
                    (valMinSlider.value <= pixVal && pixVal <= valMaxSlider.value)) {

                    // Accumulate the x and y totals so we can average them later.
                    pixMatchCount++;
                    xCoordTotal += x;
                    yCoordTotal += y;

                } else {
                    // Modify the pixel's value (lightness) so that it looks black
                    pixVal = 0.0f;
                }

                // ****** END MODIFYING HSV VALUES ****** //

                // Take our HSV pixel and convert it back to RGB so the texture can use it
                framePixels [pixNdx] = Color.HSVToRGB (pixHue, pixSat, pixVal);
            }
        }

        // Find the average location of valid pixels in our camera texture frame
        xCamCoordAvg = xCoordTotal / pixMatchCount;
        yCamCoordAvg = yCoordTotal / pixMatchCount;

        // Calculate current scene camera bounds in scene units at z=0
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float cameraHeight = mainCamera.orthographicSize * 2;
        Bounds bounds = new Bounds( mainCamera.transform.position, new Vector3(cameraHeight * screenAspect, cameraHeight, 0));

        float xOrthoUnitCoord = xCamCoordAvg * ( bounds.size.x  / (float)activeCameraTexture.width );
        float xOrthoUnitFinal = bounds.min.x + xOrthoUnitCoord;

        float yOrthoUnitCoord = yCamCoordAvg * ( bounds.size.y  / (float)activeCameraTexture.height );
        float yOrthoUnitFinal = bounds.min.y + yOrthoUnitCoord; //MINUS???

        // Move Sparkler by calculating screen coordinate conversion, to go from camera coords to screen coords
        sparkler.gameObject.transform.position = new Vector3( -xOrthoUnitFinal, yOrthoUnitFinal, 0);

        // If the calibration canvas is enabled, then we need to push everything back to a viewable texture
        if ( UiCalibrationCanvas.enabled )
        {
            // Push the modified pixels out to the processed texture
            processedTexture.SetPixels32 ( framePixels );

            // Apply the changes to the texture, or else nothing will happen.
            processedTexture.Apply ();
        }

    } //END Update()

    //---------------------------------------------------------------------
    // Returns the index to an array ordered in rows.
    //---------------------------------------------------------------------
    private int xyToIndex( int x, int y, int width)
    {
        return (y * width) + x;
    }


    //---------------------------------------------------------------------
    // Converts an index for an array ordered in rows, to x/y coords.
    //---------------------------------------------------------------------
    private void indexToXY( int index, int width, out int x, out int y)
    {
        x = Mathf.FloorToInt (index / width);
        y = index % width;
    }
        
} //END CLASS