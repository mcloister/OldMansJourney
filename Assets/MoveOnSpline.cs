using UnityEngine;
using System.Collections;

public class MoveOnSpline : MonoBehaviour {
	public Spline spline;
	public Vector3 target;
	public float speed = 2;

	Spline[] allSplines;

	// Use this for initialization
	void Start () 
	{
		GameObject[] splineObjects =  GameObject.FindGameObjectsWithTag("Spline");

		allSplines = new Spline[splineObjects.Length];

		for (int i = 0; i < splineObjects.Length; i++) 
			allSplines[i] = splineObjects[i].GetComponent<Spline>();

		spline = allSplines [0];
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void updateTarget(Object data)
	{
		Debug.Log ("hello EventSystem: " + data);
	}
}
