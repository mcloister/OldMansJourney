﻿using UnityEngine;
using System.Collections;

public class StayOnPath : MonoBehaviour {

	public Spline spline;
	public Vector3 offset;
//	public bool updateParam;

	float param;
	
	
	SplineSelector selector;
	bool wasKinematic;
	Vector3 velocity;
	Vector3 angularVelocity;

	// Use this for initialization
	void Start () 
	{
		GameObject controller = GameObject.FindGameObjectWithTag ("GameController");
		if(controller != null)
			selector = controller.GetComponent<SplineSelector> ();

		if (spline == null)
			return;

		param = spline.GetClosestPointParam (transform.position, 3);

		if (rigidbody)
			wasKinematic = rigidbody.isKinematic;
	}
	
	// Update is called once per frame
	void Update () 
	{		
		if (Input.GetMouseButtonDown (0)) 
		{
			if(rigidbody)
			{
				param = spline.GetClosestPointParam (transform.position, 3);

				wasKinematic = rigidbody.isKinematic;
				velocity = rigidbody.velocity;
				angularVelocity = rigidbody.angularVelocity;
				rigidbody.isKinematic = true;
			}
		}

		if (spline != null && selector != null && selector.isBeingDragged (spline))
		{
			transform.position = spline.GetPositionOnSpline (param) + offset;
		}

		if (Input.GetMouseButtonUp (0)) 
		{
			if(rigidbody)
			{
				rigidbody.isKinematic = wasKinematic;
				if(!wasKinematic)
				{
					rigidbody.velocity = velocity;
					rigidbody.angularVelocity = angularVelocity;
				}
			}
		}
	}
}
