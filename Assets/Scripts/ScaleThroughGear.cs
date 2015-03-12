using UnityEngine;
using System.Collections;

public class ScaleThroughGear : MonoBehaviour {

	public Gear gear;
	public Vector3 affectedAxes = Vector3.one;
	public int scaleDirection = -1; //-1 = left, 0 = symmetrical, 1 = right

	Vector3 initialScale;
	Vector3 initialPosition;

	// Use this for initialization
	void Start () {
		initialScale = transform.localScale;
		initialPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (!gear.moving)
			return;

		Vector3 currentScale = affectedAxes * gear.transmission;

		transform.localScale = currentScale + initialScale;

		transform.position = (scaleDirection * currentScale)/2 + initialPosition;
	}
}
