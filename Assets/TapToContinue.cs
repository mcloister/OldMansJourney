using UnityEngine;
using System.Collections;

public class TapToContinue : MonoBehaviour {

	Canvas winCanvas;
	
	// Use this for initialization
	void Start () 
	{
		GameObject winObject = GameObject.Find ("WinCanvas");
		if (winObject)
			winCanvas = winObject.GetComponent<Canvas> ();
	
	}
	
	// Update is called once per frame
	void Update () {
		if (winCanvas != null && winCanvas.enabled) 
		{
			if(Input.GetMouseButtonUp(0))
				Application.LoadLevel(Application.loadedLevel + 1);
		}
	}
}
