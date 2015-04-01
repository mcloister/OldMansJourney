using UnityEngine;
using UnityEngine.EventSystems;
using UnityDebugger;
using System.Collections;

public class UpdateMeshOnDrag : MonoBehaviour, IBeginDragHandler, IEndDragHandler {

	public float updateEvery = 0.16667f;

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

		//if updateEvery is -1 it means it will never be updated while being dragged but only OnEndDrag
		if(updateEvery >= 0)
			isBeingDragged = true;
	}
	
//	public void OnDrag(PointerEventData eventData)
//	{
//	}
	
	public void OnEndDrag(PointerEventData eventData)
	{
		mesh.UpdateMesh();
		mesh.UpdateMesh();	//not sure why, but if we call updateMesh only once, the mesh is only accurate on every 2nd drag...(f)

		isBeingDragged = false;
	}

	void Update()
	{
		if (!isBeingDragged)
			return;


		updateCounter += Time.deltaTime;

		if(updateCounter > updateEvery)
		{
			mesh.UpdateMesh();
//			Debugger.Log ("updated collider mesh of " + transform.parent.name);
			updateCounter = 0.0f;
		}
	}
}
