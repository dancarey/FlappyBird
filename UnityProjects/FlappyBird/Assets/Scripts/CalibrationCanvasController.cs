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


	void Start ()
    {
	    // Set defaults for all slider values
        hueMinSlider.value = 0.0f;
        hueMaxSlider.value = 1.0f;

        satMinSlider.value = 0.0f;
        satMaxSlider.value = 1.0f;

        valMinSlider.value = 0.0f;
        valMaxSlider.value = 1.0f;
	}
	
    // -----------------------------------------------------------------

    // Check overruns for HUE slider
    public void OnHueMinSliderChange()
    {
        // Make sure we're not over the max slider's value.
        if( hueMinSlider.value > hueMaxSlider.value )
        {
            hueMinSlider.value = hueMaxSlider.value;
        }
        //Debug.Log ("Hue minimum changed to " + hueMinSlider.value);
    }

    public void OnHueMaxSliderChange()
    {
        // Make sure we're not over the max slider's value.
        if( hueMaxSlider.value < hueMinSlider.value )
        {
            hueMaxSlider.value = hueMinSlider.value;
        }
        //Debug.Log ("Hue maximum changed to " + hueMaxSlider.value);
    }

    // -----------------------------------------------------------------

    // Check overruns for SATURATION slider
    public void OnSatMinSliderChange()
    {
        // Make sure we're not over the max slider's value.
        if( satMinSlider.value  > satMaxSlider.value )
        {
            satMinSlider.value = satMaxSlider.value;
        }
        //Debug.Log ("Saturation minimum changed to " + satMinSlider.value);
    }

    public void OnSatMaxSliderChange()
    {
        // Make sure we're not over the max slider's value.
        if( satMaxSlider.value  < satMinSlider.value )
        {
            satMaxSlider.value = satMinSlider.value;
        }
        //Debug.Log ("Saturation maximum changed to " + satMaxSlider.value);
    }

    // -----------------------------------------------------------------

    // Check overruns for HUE slider
    public void OnValMinSliderChange()
    {
        // Make sure we're not over the max slider's value.
        if( valMinSlider.value  > valMaxSlider.value )
        {
            valMinSlider.value = valMaxSlider.value;
        }
        //Debug.Log ("Value minimum changed to " + valMinSlider.value);
    }

    public void OnValMaxSliderChange()
    {
        // Make sure we're not over the max slider's value.
        if( valMaxSlider.value  < valMinSlider.value )
        {
            valMaxSlider.value = valMinSlider.value;
        }
        //Debug.Log ("Value maximum changed to " + valMaxSlider.value);
    }
        
}
