using UnityEngine;
using System.Collections;

public class Terraform : MonoBehaviour {

	public float influence = 1;

	protected SplineSelector selector;
	protected Spline spline;
	protected Vector3 lastMousePos;

	// Use this for initialization
	virtual protected void Start () 
	{
		selector = GameObject.FindGameObjectWithTag ("GameController").GetComponent<SplineSelector> ();
	}
	
	// Update is called once per frame
	virtual protected void Update () 
	{
		if (Input.GetMouseButtonDown (0)) 
		{
			lastMousePos = normalizeMousePos(Input.mousePosition);
		}

		if (Input.GetMouseButton (0)) 
		{
			//don't do anything if SplineSelector hasn't found a spline yet.
			if(selector.spline == null)
				return;

			//this is the first mouse down after SplineSelector has found closest spline
			// -> get the points we will transform during this stroke
			if(spline == null)
			{
				spline = selector.spline;

				Vector3 mouse = Input.mousePosition;
				Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3 (mouse.x, mouse.y, -Camera.main.gameObject.transform.position.z));

				findClosestObjects(spline.GetClosestPointParam(mouseWorld, 3));
			}

			Vector3 mousePos = normalizeMousePos(Input.mousePosition);
			Vector3 mousePosDiff = mousePos - lastMousePos;
			
//			Debug.Log("mouse pos: " + Input.mousePosition + " adjusted mouse pos: " + mousePos + " mouse diff: " + mousePosDiff);

			terraform(mousePosDiff);

			lastMousePos = mousePos;

			spline.UpdateSpline();
		}

		if (Input.GetMouseButtonUp (0)) 
		{
			spline = null;
		}
	}

	virtual protected void terraform(Vector3 mousePosDiff)
	{

	}

	virtual protected void findClosestObjects(float param)
	{
	}

	private Vector3 normalizeMousePos(Vector3 mousePos)
	{
		Vector3 adjustedMousePos = mousePos;

		//normalize it to current screen resolution
		adjustedMousePos.x /= Screen.width;
		adjustedMousePos.y /= Screen.height;

		return adjustedMousePos;
	}

}
