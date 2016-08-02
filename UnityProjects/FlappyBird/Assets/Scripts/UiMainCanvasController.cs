using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UiMainCanvasController : MonoBehaviour {

    public Canvas UiSettingsCanvas;
    public Canvas UiDebugCanvas;

    void Start ()
    {
        UiSettingsCanvas.enabled = false;
        UiDebugCanvas.enabled = false;
    }

    public void ToggleSettings ()
    {
        UiSettingsCanvas.enabled = ! UiSettingsCanvas.enabled ;
    }
}
