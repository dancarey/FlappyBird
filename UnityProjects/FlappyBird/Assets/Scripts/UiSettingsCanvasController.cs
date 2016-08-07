using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UiSettingsCanvasController : MonoBehaviour {
    
    public Text versionText ;
    public Canvas UiDebugCanvas ;
    public Canvas UiWebcamCanvas ;
    public Canvas UiCalibrationCanvas ;

    // Use this for initialization
    void Start ()
    {
        //UiWebcamCanvas.enabled = true;
        versionText.text = "FlappyBird v" + Constants.VERSION_STRING ;
    }

    public void ToggleCalibration ()
    {
        UiWebcamCanvas.enabled = ! UiWebcamCanvas.enabled ;
        UiCalibrationCanvas.enabled = ! UiCalibrationCanvas.enabled;
    }

    public void ToggleDebug ()
    {
        UiDebugCanvas.enabled = ! UiDebugCanvas.enabled  ;
    }
}