using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplineSelector : MonoBehaviour 
{
	public Dictionary<int, Terraform> draggedSplines;

	private MoveOnSpline characterMovement;


	// Use this for initialization
	void Start () {
		draggedSplines = new Dictionary<int, Terraform> (10);
	}
	
	// Update is called once per frame
	void Update () {
//		if (Input.GetMouseButtonDown (0)) 
//		{
//			Vector3 mouse = Input.mousePosition;
//			Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3 (mouse.x, mouse.y, -Camera.main.gameObject.transform.position.z));
//			
////			Debug.Log ("finding closest spline to mouse pos: " + mouse);
//			
//			findClosestSpline(mouseWorld);
//
////			Debug.Log ("found: " + spline);
//
//			//disable MeshCollider because it's too expensive to be updated while spline is terraformed
//			if(spline != null)
//				Destroy (spline.gameObject.GetComponent<MeshCollider>());
//
//		}
//		
//		if (Input.GetMouseButton (0)) 
//		{
//		}
//		
//		if (Input.GetMouseButtonUp (0)) 
//		{
//			//now we can update the meshcollider
//			if(spline != null)
//				spline.gameObject.AddComponent<MeshCollider>();
//
//			spline = null;
//		}
	}
	
	private void findClosestSpline(Vector3 mouseWorld)
	{
//		RaycastHit hit;
//		if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out hit, 200))
//			return;
//
//		if (!hit.collider.gameObject.transform.CompareTag("Spline"))
//			return;
//		
//		Spline s = hit.collider.gameObject.transform.GetComponent<Spline> ();
//
//		//don't allow terraforming of spline where character is currently moving
//		if (characterMovement != null && s.GetInstanceID () == characterMovement.spline.GetInstanceID ()) 
//		{
//			return;
//		}
//
//		spline = s;


//		float minDist = Mathf.Infinity;
//		foreach(GameObject o in allSplines)
//		{
//			Spline s = o.GetComponent<Spline>();
//			
//			Vector3 mouseSpline = mouseWorld;
//			mouseSpline.z = s.transform.position.z;
//			
//			Vector3 onSpline = s.GetPositionOnSpline(s.GetClosestPointParamToRay(Camera.main.ScreenPointToRay(Input.mousePosition), 3));
//			float dist = (onSpline - mouseSpline).magnitude;
//			
//			Debug.Log ("mW: " + mouseWorld + " " + s.name + " mS: " + mouseSpline + " pos on spline: " + onSpline + " dist: " + dist);
//			
//			if(dist < minDist)
//			{
//				minDist = dist;
//				spline = s;
//			}
//		}
	}

	public bool isBeingDragged(Spline spline)
	{
		foreach (KeyValuePair<int, Terraform> p in draggedSplines) 
		{
			if(p.Value.gameObject.GetInstanceID() == spline.gameObject.GetInstanceID())
			{
				return true;
			}
		}

		return false;
	}

}
