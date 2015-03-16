﻿using UnityEngine;
using System.Collections;

public class StayOnPath : MonoBehaviour {

	public Spline spline;
	public Vector3 offset;

	float param;
	
	
	SplineSelector selector;

	// Use this for initialization
	void Start () 
	{
		GameObject controller = GameObject.FindGameObjectWithTag ("GameController");
		if(controller != null)
			selector = controller.GetComponent<SplineSelector> ();

		if (spline == null)
			return;

		param = spline.GetClosestPointParam (transform.position, 3);
	}
	
	// Update is called once per frame
	void Update () 
	{		
		if(Input.GetMouseButtonDown(0))
			param = spline.GetClosestPointParam (transform.position, 3);

		if(selector != null && selector.spline.GetInstanceID() == spline.GetInstanceID())
			transform.position = spline.GetPositionOnSpline (param) + offset;
	}
}
