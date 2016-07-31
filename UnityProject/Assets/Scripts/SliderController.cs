using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SliderController : MonoBehaviour {

    public Slider sliderPercent;

    void Start ()
    {
        sliderPercent = sliderPercent.GetComponent<Slider>();
    }

    void UpdatePercent (float newPercent)
    {
        sliderPercent.value = newPercent;

        Debug.Log("Slider value updated to " + sliderPercent.value);
    }

}