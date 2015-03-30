using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class Terraform : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

	public float influence = 1;

	protected SplineSelector selector;
	protected MoveOnSpline characterMovement;
	protected Spline spline;
	protected List<FormableNode> formableNodes;
	protected Vector3 lastMousePos;
	
	protected SplineMesh colliderMesh;
	private float updateMeshCounter = 0.0f;
	private float updateMeshTime = 0.166f;
	
	protected AudioSource sound;

	// Use this for initialization
	virtual protected void Start () 
	{
		selector = GameObject.FindGameObjectWithTag ("GameController").GetComponent<SplineSelector> ();

		characterMovement = GameObject.FindGameObjectWithTag("Character").GetComponent<MoveOnSpline>();

		spline = GetComponent<Spline> ();

		formableNodes = new List<FormableNode> (3);

		//find all nodes that are formable
		foreach(SplineNode sN in spline.SplineNodes)
		{
			Transform t = sN.transform;
			if(!t.CompareTag("Formable Node"))
				continue;
			
			formableNodes.Add(t.GetComponent<FormableNode>());
		}

		//do we have a seperate collider child
		Transform cT = transform.Find("Collider");
		if(cT != null)
			colliderMesh = cT.GetComponent<SplineMesh>();
	

		GameObject[] sounds = GameObject.FindGameObjectsWithTag ("Sound");
		
		foreach (GameObject o in sounds) 
		{
			if(o.name == "Terraform Sound")
				sound = o.GetComponent<AudioSource>();
		}

	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		Debug.Log ("eD: " + eventData);
		if (characterMovement.spline.GetInstanceID() == spline.GetInstanceID())
			return;
		if (selector.draggedSplines.ContainsKey (eventData.pointerId))
			return;

		selector.draggedSplines.Add (eventData.pointerId, this);

		//disable MeshCollider because it's too expensive to be updated while spline is terraformed
		//TODO: move the MeshCollider to a seperate GameObject
		if(spline.gameObject.GetComponent<MeshCollider>() != null)
			Destroy (spline.gameObject.GetComponent<MeshCollider>());

		findClosestObjects(spline.GetClosestPointParam(eventData.worldPosition, 3), eventData.pointerId);
		
		
		if(colliderMesh != null)
			updateMeshCounter = 0.0f;
		
		if(sound != null)
			sound.Play();
	}
	
	public void OnDrag(PointerEventData eventData)
	{
		Terraform touchedTerraform;
		if (!selector.draggedSplines.TryGetValue (eventData.pointerId, out touchedTerraform)) 
			return;


		terraform(normalizeScreenPos(eventData.delta), eventData.pointerId);

		spline.UpdateSpline();
		
		if(colliderMesh != null)
		{
			updateMeshCounter += Time.deltaTime;
			if(updateMeshCounter > updateMeshTime)
			{
				colliderMesh.UpdateMesh();
				updateMeshCounter = 0.0f;
			}
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (!selector.draggedSplines.ContainsKey (eventData.pointerId)) 
			return;

		endTerraforming(eventData.pointerId);

		selector.draggedSplines.Remove (eventData.pointerId);
		
		//now we can update the meshcollider if no other touch is terraforming it
		if(!selector.draggedSplines.ContainsValue(this))
			spline.gameObject.AddComponent<MeshCollider>();

		if(colliderMesh != null)
			colliderMesh.UpdateMesh();

		if(sound != null)
			sound.Stop();
	}

	
	// Update is called once per frame
	virtual protected void Update () 
	{
//		if (Input.GetMouseButtonDown (0)) 
//		{
//			lastMousePos = normalizeMousePos(Input.mousePosition);
//		}
//
//		if (Input.GetMouseButton (0)) 
//		{
//			//don't do anything if SplineSelector hasn't found a spline yet.
//			if(selector.spline == null)
//				return;
//
//			//this is the first mouse down after SplineSelector has found closest spline
//			// -> get the points we will transform during this stroke
//			if(spline == null)
//			{
//				spline = selector.spline;
//
//				Vector3 mouse = Input.mousePosition;
//				Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3 (mouse.x, mouse.y, -Camera.main.gameObject.transform.position.z));
//
//				Transform limitsObject = spline.transform.Find ("Limits");
//				if (limitsObject != null) 
//					limits = limitsObject.GetComponent<BoxCollider>().bounds;
//
//				//find all nodes that are formable
//				formableNodes.Clear();
//				foreach(SplineNode sN in spline.SplineNodes)
//				{
//					Transform t = sN.transform;
//					if(!t.CompareTag("Formable Node"))
//						continue;
//					
//					formableNodes.Add(t.GetComponent<FormableNode>());
//				}
//
//				findClosestObjects(spline.GetClosestPointParam(mouseWorld, 3));
//
//				if(sound != null)
//					sound.Play();
//			}
//
//			Vector3 mousePos = normalizeMousePos(Input.mousePosition);
//			Vector3 mousePosDiff = mousePos - lastMousePos;
//			
////			Debug.Log("mouse pos: " + Input.mousePosition + " adjusted mouse pos: " + mousePos + " mouse diff: " + mousePosDiff);
//
//			terraform(mousePosDiff);
//
//			lastMousePos = mousePos;
//
//			spline.UpdateSpline();
//		}
//
//		if (Input.GetMouseButtonUp (0)) 
//		{
//			spline = null;
//
//			if(sound != null)
//				sound.Stop();
//
////			Destroy(limits);
//			limits = new Bounds(Vector3.zero, Vector3.zero);
//		}
	}

	virtual protected void findClosestObjects(float param, int pointerId)
	{
	}
	
	virtual protected void terraform(Vector3 delta, int pointerId)
	{
		
	}
	
	virtual protected void endTerraforming(int pointerId)
	{
		
	}

	private Vector2 normalizeScreenPos(Vector2 screenPos)
	{
		Vector2 adjustedScreenPos = screenPos;

		//normalize it to current screen resolution
		adjustedScreenPos.x /= Screen.width;
		adjustedScreenPos.y /= Screen.height;

		return adjustedScreenPos;
	}

}
