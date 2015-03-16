using UnityEngine;
using System.Collections;

public class PingPongScaler : MonoBehaviour {
	public float start = 0;
	public float target = 1;
	public float speed = 1.0f;

	// Use this for initialization
	void Start () {
		transform.localScale = new Vector3(start, start, start);
	}
	
	// Update is called once per frame
	void Update () {
		float newScale = start + Mathf.PingPong(Time.time * speed, target-start);
		transform.localScale = new Vector3(newScale, newScale, newScale);
	}

	void OnDisable() 
	{
		transform.localScale = new Vector3(start, start, start);
	}
}
