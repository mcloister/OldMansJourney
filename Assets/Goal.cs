using UnityEngine;
using System.Collections;

public class Goal : MonoBehaviour {

	public GameObject character;
	public float winDistance = 0.5f;

	GameObject canvas;

	// Use this for initialization
	void Start () 
	{
		canvas = GameObject.Find ("WinCanvas");
		canvas.SetActive (false);


		if (character == null) 
		{
			character = GameObject.FindGameObjectWithTag("Character");
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (character != null) {
//			Debug.Log("dis to character: " + Vector3.Distance (character.transform.position, transform.position));
			if (Vector3.Distance (character.transform.position, transform.position) < winDistance)
				canvas.SetActive (true);
		}
	}
}
