using UnityEngine;
using System.Collections;

public class BreakOnCollision : MonoBehaviour {

	ParticleSystem breakEffect;
	// Use this for initialization
	void Start () {
		Transform e = transform.Find ("Break Effect");
		if (e)
			breakEffect = e.GetComponent<ParticleSystem> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter(Collision collision)
	{
		GameObject collider = collision.collider.transform.gameObject;
		if (collider.layer == LayerMask.NameToLayer ("Dynamic")) 
		{
			if(breakEffect)
				breakEffect.Play();

			collider.SetActive(false);

			GetComponent<MeshRenderer>().enabled = false;
//			gameObject.SetActive(false);

		}
	}
}
