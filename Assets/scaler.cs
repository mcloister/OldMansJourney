using UnityEngine;
using System.Collections;

public class scaler : MonoBehaviour {
	public Vector3 start = new Vector3(0,0,0);
	public Vector3 target = new Vector3(1,1,1);
	public float duration = 1.0f;
	float timeTaken = 0.0f;

	// Use this for initialization
	void Start () {
		transform.localScale = start;
	}
	
	// Update is called once per frame
	void Update () {
		timeTaken += Time.deltaTime;
		float t = timeTaken / duration;
		transform.localScale = Vector3.Lerp (start, target, t);
	}
}
