using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class AddForceFromMouse : MonoBehaviour, IBeginDragHandler, IDragHandler {

	public Vector3 startPosition;
	public Vector3 currentForce;
	public float forceFactor = 1;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		startPosition = eventData.worldPosition;
	}

	public void OnDrag(PointerEventData eventData)
	{
		currentForce = eventData.worldPosition - startPosition * forceFactor;

		rigidbody.AddForceAtPosition (currentForce, startPosition);

	}
}
