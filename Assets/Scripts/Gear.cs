using UnityEngine;
using System.Collections;

public class Gear : MonoBehaviour {

	public float ratio = 1;
	public float transmission;
	public Vector2 limits = new Vector2(0,1);
	public float activationThreshold = 0.01f;

//	public bool lockAtLimits;
	public bool moving;

	float lastRotationZ;
	// Use this for initialization
	void Start () {
		transmission = limits.x;
	}
	
	// Update is called once per frame
	void Update () 
	{
		float rotationZ = transform.localRotation.eulerAngles.z;

		if (Mathf.Abs (Mathf.DeltaAngle (lastRotationZ, rotationZ)) > activationThreshold) {
			moving = true;

			transmission += Mathf.DeltaAngle (lastRotationZ, rotationZ) * ratio * Time.deltaTime;

			float clamped = Mathf.Clamp (transmission, limits.x, limits.y);

//			if(lockAtLimits)
//			{
//				if(clamped - transmission < Mathf.Epsilon)
//					rigidbody.constraints = RigidbodyConstraints.FreezeRotationZ;
//				else
//					rigidbody.constraints = RigidbodyConstraints.None;
//
//			}

			transmission = clamped;

		} else
			moving = false;
		
		lastRotationZ = rotationZ;
	}
}
