using UnityEngine;
using System.Collections;

public class BreakOnCollision : MonoBehaviour {

	ParticleSystem breakEffect;
	MeshRenderer renderer;
	BoxCollider collider;
	// Use this for initialization
	void Start () {
		Transform e = transform.Find ("Break Effect");
		if (e)
			breakEffect = e.GetComponent<ParticleSystem> ();

		renderer = GetComponent<MeshRenderer> ();
		collider = GetComponent<BoxCollider> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter(Collision collision)
	{
		GameObject other = collision.collider.transform.gameObject;
		if (other.layer == LayerMask.NameToLayer ("Dynamic")) 
		{
			if(breakEffect)
				breakEffect.Play();

			other.SetActive(false);

			renderer.enabled = false;
			collider.enabled = false;
//			gameObject.SetActive(false);

		}
	}
}
