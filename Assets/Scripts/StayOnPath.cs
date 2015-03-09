using UnityEngine;
using System.Collections;

public class StayOnPath : MonoBehaviour {

	public Spline spline;
	public Vector3 offset;

	float param;
	// Use this for initialization
	void Start () 
	{
		if (spline == null)
			return;

		param = spline.GetClosestPointParam (transform.position, 3);
	}
	
	// Update is called once per frame
	void Update () 
	{
		transform.position = spline.GetPositionOnSpline (param) + offset;
	}
}
