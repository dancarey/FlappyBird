using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UiMainCanvasController : MonoBehaviour {

    public Canvas UiSettingsCanvas;

    void Start ()
    {
        UiSettingsCanvas.enabled = false;
    }

    public void ToggleSettings ()
    {
        UiSettingsCanvas.enabled = ! UiSettingsCanvas.enabled ;
    }
}
