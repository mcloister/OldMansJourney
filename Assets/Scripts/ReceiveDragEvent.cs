using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ReceiveDragEvent : MonoBehaviour, IDragHandler {
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void OnDrag(PointerEventData data)
	{
		Debug.Log ("hello from draghandler: " + data);
	}
	
}
