using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class Terraform : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

	protected float influence = 70;

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
		if (characterMovement.spline.GetInstanceID() == spline.GetInstanceID())
			return;
		if (selector.draggedSplines.ContainsKey (eventData.pointerId))
			return;

		selector.draggedSplines.Add (eventData.pointerId, this);

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

		if(colliderMesh != null)
			colliderMesh.UpdateMesh();

		if(sound != null)
			sound.Stop();
	}

	
	// Update is called once per frame
	virtual protected void Update () 
	{
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
