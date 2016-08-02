using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UiSettingsCanvasController : MonoBehaviour {
    
    public Text versionText ;
    public Canvas UiDebugCanvas ;
    public Canvas UiWebcamCanvas ;

    // Use this for initialization
    void Start ()
    {
        UiWebcamCanvas.enabled = true;
        versionText.text = "FlappyBird v" + Constants.VERSION_STRING ;
    }

    public void ToggleCalibration ()
    {
        UiWebcamCanvas.enabled = ! UiWebcamCanvas.enabled ;
    }

    public void ToggleDebug ()
    {
        UiDebugCanvas.enabled = ! UiDebugCanvas.enabled  ;
    }
}