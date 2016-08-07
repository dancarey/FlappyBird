using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CalibrationCanvasController : MonoBehaviour {

    public Slider hueMinSlider;
    public Slider hueMaxSlider;

    public Slider satMinSlider;
    public Slider satMaxSlider;

    public Slider valMinSlider;
    public Slider valMaxSlider;

	// Use this for initialization
	void Start () {
	
	}
	
    // -----------------------------------------------------------------
    // Check overruns for HUE slider
    void OnHueMinSliderChange(float newVal)
    {
        // Make sure we're not over the max slider's value.
        if( newVal > hueMaxSlider.value )
        {
            hueMinSlider.value = hueMaxSlider.value;
        }
    }

    void OnHueMaxSliderChange(float newVal)
    {
        // Make sure we're not over the max slider's value.
        if( newVal < hueMinSlider.value )
        {
            hueMaxSlider.value = hueMinSlider.value;
        }
    }

    // -----------------------------------------------------------------
    // Check overruns for SATURATION slider
    void OnSatMinSliderChange(float newVal)
    {
        // Make sure we're not over the max slider's value.
        if( newVal > satMaxSlider.value )
        {
            satMinSlider.value = satMaxSlider.value;
        }
    }

    void OnSatMaxSliderChange(float newVal)
    {
        // Make sure we're not over the max slider's value.
        if( newVal < satMinSlider.value )
        {
            satMaxSlider.value = satMinSlider.value;
        }
    }

    // -----------------------------------------------------------------
    // Check overruns for HUE slider
    void OnValMinSliderChange(float newVal)
    {
        // Make sure we're not over the max slider's value.
        if( newVal > valMaxSlider.value )
        {
            valMinSlider.value = valMaxSlider.value;
        }
    }

    void OnValMaxSliderChange(float newVal)
    {
        // Make sure we're not over the max slider's value.
        if( newVal < valMinSlider.value )
        {
            valMaxSlider.value = valMinSlider.value;
        }
    }
}
