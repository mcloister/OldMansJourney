using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplineSelector : MonoBehaviour 
{
	public Dictionary<int, Terraform> draggedSplines;

	private MoveOnSpline characterMovement;


	// Use this for initialization
	void Start () {
		draggedSplines = new Dictionary<int, Terraform> (10);
	}
	
	// Update is called once per frame
	void Update () {

	}
	
	private void findClosestSpline(Vector3 mouseWorld)
	{

	}

	public bool isBeingDragged(Spline spline)
	{
		foreach (KeyValuePair<int, Terraform> p in draggedSplines) 
		{
			if(p.Value.gameObject.GetInstanceID() == spline.gameObject.GetInstanceID())
			{
				return true;
			}
		}

		return false;
	}

}
