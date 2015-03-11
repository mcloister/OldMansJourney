using UnityEngine;
using System.Collections;

public class Waterfall : MonoBehaviour {
	public int direction = 1;

	public float fallSpeed = 0;
	public float switchThresholdFactor = 1;

	// Use this for initialization
	void Start () {
	
		if (fallSpeed == 0.0f)
			fallSpeed = GameObject.FindGameObjectWithTag ("Character").GetComponent<MoveOnSpline> ().speed;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
