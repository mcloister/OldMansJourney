using UnityEngine;
using System.Collections;

public class Goal : MonoBehaviour {

	public GameObject character;
	public float winDistance = 0.5f;

	Canvas winCanvas;
	public GameObject winEffect;

	AudioSource winSound;

	bool reached;

	// Use this for initialization
	void Start () 
	{
		GameObject winObject = GameObject.Find ("WinCanvas");
		if (winObject)
			winCanvas = winObject.GetComponent<Canvas> ();

//		winEffect = transform.Find ("winEffect").gameObject;
		winEffect.SetActive (false);


		if (character == null) 
		{
			character = GameObject.FindGameObjectWithTag("Character");
		}

		GameObject[] sounds = GameObject.FindGameObjectsWithTag ("Sound");
		foreach (GameObject o in sounds) 
		{
			if(o.name == "Winning Sound")
				winSound = o.GetComponent<AudioSource>();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (reached)
			return;

		if (character != null) {
			if (Vector3.Distance (character.transform.position, transform.position) < winDistance) 
			{
				reached = true;

				if(winSound != null)
					winSound.Play();

				winCanvas.enabled = true;
				winEffect.SetActive(true);
			}
		}
	}
}
