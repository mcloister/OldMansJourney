using UnityEngine;
using System.Collections;

public class SplineSelector : MonoBehaviour {

	private GameObject[] allSplines;
	public Spline spline;

	// Use this for initialization
	void Start () {
		allSplines = GameObject.FindGameObjectsWithTag("Spline");

		spline = null;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)) 
		{
			Vector3 mouse = Input.mousePosition;
			Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3 (mouse.x, mouse.y, -Camera.main.gameObject.transform.position.z));
			
			Debug.Log ("finding closest spline to mouse pos: " + mouse);
			
			findClosestSpline(mouseWorld);

			//disable MeshCollider because it's too expensive to be updated while spline is terraformed
			if(spline != null)
				Destroy (spline.gameObject.GetComponent<MeshCollider>());

//			spline.GetComponent<MeshCollider>().sharedMesh = new Mesh();
//			spline.GetComponent<MeshCollider>().enabled = false;

			//			outputControlPoints();
		}
		
		if (Input.GetMouseButton (0)) 
		{
		}
		
		if (Input.GetMouseButtonUp (0)) 
		{
			//now we can update the meshcollider
			if(spline != null)
				spline.gameObject.AddComponent<MeshCollider>();
//			spline.GetComponent<MeshCollider>().sharedMesh = spline.GetComponent<MeshFilter>().mesh;
//			spline.GetComponent<MeshCollider>().enabled = true;

			spline = null;
		}
	}
	
	private void findClosestSpline(Vector3 mouseWorld)
	{
		RaycastHit hit;
		if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out hit, 200))
			return;

		if (!hit.collider.gameObject.transform.CompareTag("Spline"))
			return;
		
		spline = hit.collider.gameObject.transform.GetComponent<Spline> ();


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
	
	private void outputControlPoints()
	{
		foreach (SplineNode n in spline.SplineNodes) 
		{
			Debug.Log(n.gameObject.name + " position: " + n.transform.position);
		}
	}
	
}
