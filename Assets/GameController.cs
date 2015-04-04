using UnityEngine;
using UnityDebugger;
using System.Collections;

public class GameController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Debugger.LogLevel = UnityDebugger.LogLevel.Info;

		
		Application.targetFrameRate = 60;
	}
	
	// Update is called once per frame
	void Update () 
	{

	}
}
