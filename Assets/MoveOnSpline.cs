using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveOnSpline : MonoBehaviour {
	public Spline spline;
	public float param;
	public Vector3 target;
	public float speed = 2;
	public float dragThreshold = 0.001f;
	public float stopThreshold = 0.001f;

	Spline[] allSplines;
//	Dictionary<Spline, float> paramOnSplines;
	Vector3 lastMousePos = Vector3.zero;
	Vector3 mouseDelta = Vector3.zero;
	Vector3 posOnDown;
	int direction;

	// Use this for initialization
	void Start () 
	{
		GameObject[] splineObjects =  GameObject.FindGameObjectsWithTag("Spline");

		allSplines = new Spline[splineObjects.Length];

		for (int i = 0; i < splineObjects.Length; i++) 
			allSplines[i] = splineObjects[i].GetComponent<Spline>();

		spline = allSplines [1];

//		paramOnSplines = new Dictionary<Spline, float> ();
//
//		paramOnSplines.Add (spline, 0.5f);

		param = 0.5f;

		direction = 0;
	}
	
	// Update is called once per frame
	void Update () 
	{

		if (Input.GetMouseButtonDown (0))
		{
			lastMousePos = normalizeMousePos(Input.mousePosition);
			mouseDelta = Vector3.zero;
		}

		if (Input.GetMouseButtonUp (0)) 
		{
			if(mouseDelta.magnitude < dragThreshold)
			{
				Vector3 mousePos = Input.mousePosition;
				setTarget(Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -(Camera.main.transform.position.z + spline.gameObject.transform.position.z))));
			}
		}
		
		if (Input.GetMouseButton (0)) 
		{
			//move character along with dragged spline
			param = spline.GetClosestPointParam(transform.position, 3);
			transform.position = spline.GetPositionOnSpline(param) + new Vector3(0,transform.localScale.z/2,0);
			transform.rotation = spline.GetOrientationOnSpline( param );

			mouseDelta = normalizeMousePos (Input.mousePosition) - lastMousePos;
		} 
		//only move character if no other hill is being dragged
		else if(direction != 0)
		{
			int currentDirection = (target.x - transform.position.x) > 0 ? 1 : -1;

			if(currentDirection != direction)
			{
//				paramOnSplines[spline] = spline.GetClosestPointParam(target, 3);
				param = spline.GetClosestPointParam(target, 3);

				direction = 0;	//stop moving
			}
			else
			{
//				foreach (var pair in paramOnSplines) 
//					paramOnSplines[pair.Key] = pair.Value + speed * direction;
//				paramOnSplines[spline] += speed * direction;

				param += speed * direction;
			}

//			transform.position = spline.GetPositionOnSpline(paramOnSplines[spline]);
			transform.position = spline.GetPositionOnSpline(param) + new Vector3(0,transform.localScale.z/2,0);
			transform.rotation = spline.GetOrientationOnSpline( param );


		}
	
	}

	public void setTarget(Vector3 worldPosition)
	{
		target = spline.GetPositionOnSpline(spline.GetClosestPointParam(worldPosition, 3));
		direction = (target.x - transform.position.x) > 0 ? 1 : -1;
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
