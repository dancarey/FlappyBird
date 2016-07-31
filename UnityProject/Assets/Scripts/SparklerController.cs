using UnityEngine;
using System.Collections;

public class SparklerController : MonoBehaviour {

    public ParticleSystem sparkler;

    private Vector3 location;

	// Use this for initialization
	void Start () {

        //Application.RequestUserAuthorization(

        location = new Vector3 (0, 0, 0);
	}
	
	// Update is called once per frame
    void FixedUpdate () {

        //Screen.currentResolution.width;

        //sparkler.transform.position = location;
	}
}
