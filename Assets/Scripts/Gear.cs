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

	AudioSource sound;

	// Use this for initialization
	void Start () {
		transmission = limits.x;
		GameObject[] sounds = GameObject.FindGameObjectsWithTag ("Sound");
		
		foreach (GameObject o in sounds) 
		{
			if(o.name == "crank")
				sound = o.GetComponent<AudioSource>();
		}
	}

	// Update is called once per frame
	void Update () 
	{
		float rotationZ = transform.localRotation.eulerAngles.z;

		if (Mathf.Abs (Mathf.DeltaAngle (lastRotationZ, rotationZ)) > activationThreshold) {
			if (!moving) {
				moving = true;
				
				StopCoroutine (stopSound());
				if(!sound.isPlaying)
					sound.Play ();
			}

			transmission += Mathf.DeltaAngle (lastRotationZ, rotationZ) * ratio;

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

		} else if (moving) {
			moving = false;
			StartCoroutine (stopSound());
		}
		
		lastRotationZ = rotationZ;
	}

	IEnumerator stopSound()
	{
		yield return new WaitForSeconds (1f);

		sound.Stop ();
	}
}
