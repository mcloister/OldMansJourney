using UnityEngine;
using System.Collections;

public class BreakOnCollision : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter(Collision collision)
	{
		GameObject collider = collision.collider.transform.gameObject;
		if (collider.layer == LayerMask.NameToLayer ("Dynamic")) 
		{
			collider.SetActive(false);
			gameObject.SetActive(false);

		}
	}
}
