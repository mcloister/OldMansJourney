using UnityEngine;
using UnityEngine.EventSystems;
using UnityDebugger;
using System.Collections;

public class PassDragEventsUp : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
	
	// Use this for initialization
	void Start () {
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		ExecuteEvents.ExecuteHierarchy<IBeginDragHandler> (transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
	}
	
	public void OnDrag(PointerEventData eventData)
	{
		ExecuteEvents.ExecuteHierarchy<IDragHandler> (transform.parent.gameObject, eventData, ExecuteEvents.dragHandler);
	}
	
	public void OnEndDrag(PointerEventData eventData)
	{
		ExecuteEvents.ExecuteHierarchy<IEndDragHandler> (transform.parent.gameObject, eventData, ExecuteEvents.endDragHandler);
	}
}
