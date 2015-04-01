using UnityEngine;
using UnityEngine.EventSystems;
using UnityDebugger;
using System.Collections;

public class UpdateMeshOnDrag : MonoBehaviour, IBeginDragHandler, IEndDragHandler {

	public float updateTime = 0.16667f;

	private SplineMesh mesh;
	private float updateCounter = 0;

	bool isBeingDragged = false;

	// Use this for initialization
	void Start () {
		mesh = GetComponent<SplineMesh> ();
	}
	public void OnBeginDrag(PointerEventData eventData)
	{
		updateCounter = 0.0f;

		isBeingDragged = true;
	}
	
//	public void OnDrag(PointerEventData eventData)
//	{
//	}
	
	public void OnEndDrag(PointerEventData eventData)
	{
		mesh.UpdateMesh();

		isBeingDragged = false;
	}

	void Update()
	{
		if (!isBeingDragged)
			return;


		updateCounter += Time.deltaTime;

		if(updateCounter > updateTime)
		{
			mesh.UpdateMesh();
//			Debugger.Log ("updated collider mesh of " + transform.parent.name);
			updateCounter = 0.0f;
		}
	}
}
