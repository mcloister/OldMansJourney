using UnityEngine;
using System.Collections;

struct Formable
{
	public float weight;
	public Transform transform;

	public Formable(Transform t, float w)
	{
		transform = t;
		weight = w;
	}
}

public class Terraform : MonoBehaviour {

	public GameObject currentNode;
	public float influence = 1;
	public float radius = 3;

	private GameObject[] allSplines;
	public Spline spline;
	public ArrayList toTerraform;
	private Vector3 lastMousePos;

	// Use this for initialization
	void Start () {
		allSplines = GameObject.FindGameObjectsWithTag("Spline");

		toTerraform = new ArrayList();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)) 
		{
			Vector3 mouse = Input.mousePosition;
			Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3 (mouse.x, mouse.y, -Camera.main.gameObject.transform.position.z));

			Debug.Log ("finding closes spline to mouse pos: " + mouse);

			findClosestSpline(mouseWorld);

			outputControlPoints();

			findClosestObjects(spline.GetClosestPointParam(mouseWorld, 3));

			lastMousePos = normalizeMousePos(mouse);
		}

		if (Input.GetMouseButton (0)) 
		{
			Vector3 mousePos = normalizeMousePos(Input.mousePosition);

			Vector3 mousePosDiff = mousePos - lastMousePos;
			
			Debug.Log("mouse pos: " + Input.mousePosition + " adjusted mouse pos: " + mousePos + " mouse diff: " + mousePosDiff);

//			currentNode.transform.position += mousePosDiff * influence;
			foreach(Formable f in toTerraform)
				f.transform.position += mousePosDiff * influence * f.weight;


			lastMousePos = mousePos;

			spline.UpdateSpline();
		}

		if (Input.GetMouseButtonUp (0)) 
		{
//			Debug.Log(currentNode.name + " position: " + currentNode.transform.position);

			toTerraform.Clear();
		}
	}

	private void findClosestSpline(Vector3 mouseWorld)
	{
		float minDist = Mathf.Infinity;
		foreach(GameObject o in allSplines)
		{
			Spline s = o.GetComponent<Spline>();

			Vector3 onSpline = s.GetPositionOnSpline(s.GetClosestPointParam(mouseWorld, 3));
			float dist = (onSpline - mouseWorld).magnitude;

			Debug.Log ("pos on spline: " + onSpline + " dist: " + dist);

			if(dist < minDist)
			{
				minDist = dist;
				spline = s;
			}
		}
	}

	private void findClosestObjects(float param)
	{
		SplineNode[] allNodes = spline.SplineNodes;

		Vector3 onSpline = spline.GetPositionOnSpline (param);

		//we don't allow the first and the last control point to be moved
		for (int i = 1; i < allNodes.Length-1; i++) 
		{
			SplineNode n = allNodes[i];

			float dist = (onSpline - spline.GetPositionOnSpline(n.Parameters[spline].PosInSpline)).magnitude;
			if(dist < radius)
			{
				toTerraform.Add(new Formable(n.transform, 1-dist/radius));
			}
		}

		if (toTerraform.Count == 0) 
		{
			Debug.LogWarning("couldn't find a node close enough to param " + param);
		}
	}

	private Vector3 normalizeMousePos(Vector3 mousePos)
	{
		Vector3 adjustedMousePos = mousePos;

		//normalize it to current screen resolution
		adjustedMousePos.x /= Screen.width;
		adjustedMousePos.y /= Screen.height;

		return adjustedMousePos;
	}

	private void outputControlPoints()
	{
		foreach (SplineNode n in spline.SplineNodes) 
		{
			Debug.Log(n.gameObject.name + " position: " + n.transform.position);
		}
	}

}
