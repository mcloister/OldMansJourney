using UnityEngine;
using System.Collections;

public class CameraMan : MonoBehaviour {
	public Transform target;
	public float horizontalDamping = 3;
	public float verticalDamping = 3;
	public float distance = 60;

	Bounds limits;
	// Use this for initialization
	void Start () 
	{
		BoxCollider collider = GetComponent<BoxCollider> ();
		if (collider)
			limits = collider.bounds;
	}
	
	// Update is called once per frame
	void LateUpdate () 
	{
		if (!target)
			return;

		Vector3 currentPosition = transform.position;
		Vector3 wantedPosition = target.position;

		if(horizontalDamping > 0)
			wantedPosition.x = Mathf.SmoothStep (currentPosition.x, wantedPosition.x, horizontalDamping * Time.deltaTime);
		if(verticalDamping > 0)
			wantedPosition.y = Mathf.SmoothStep (currentPosition.y, wantedPosition.y, verticalDamping * Time.deltaTime);

//		float horizontalLimit = limits.min.x + Camera.main.wi / 2;
//
//		if(wantedPosition.x < limits.min.x)
//			wantedPosition.x = limits.min.x


		wantedPosition.z -= distance+target.position.z;
		transform.position = wantedPosition;

		// Always look at the target
//		transform.LookAt (target);
	}
}
