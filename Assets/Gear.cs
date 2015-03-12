using UnityEngine;
using System.Collections;

public class Gear : MonoBehaviour {

	public float ratio = 1;
	public float transmission;
	public Vector2 limits = new Vector2(0,1);

	float lastRotationZ;
	// Use this for initialization
	void Start () {
		transmission = 0;
	}
	
	// Update is called once per frame
	void Update () 
	{
		float rotationZ = transform.localRotation.eulerAngles.z;

//		if (Mathf.DeltaAngle (lastRotationZ, rotationZ) > Mathf.Epsilon) 
//		{
			transmission += Mathf.DeltaAngle (lastRotationZ, rotationZ) * ratio * Time.deltaTime;

			transmission = Mathf.Clamp (transmission, limits.x, limits.y);
//		}

		lastRotationZ = rotationZ;
	}
}
