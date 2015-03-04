using UnityEngine;
using System.Collections;

public class Terraform : MonoBehaviour {

	public GameObject currentNode;
	public float influence = 1;

	private Spline spline;
	private Vector3 lastMousePos;

	// Use this for initialization
	void Start () {
		spline = GetComponent<Spline> ();

	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)) 
		{
			outputControlPoints();

			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			float param = spline.GetClosestPointParamToRay(ray, 5);

			currentNode = getClosestObject(param);

			lastMousePos = adjustMousePos(Input.mousePosition);
		}

		if (Input.GetMouseButton (0)) 
		{
			Vector3 mousePos = adjustMousePos(Input.mousePosition);

			Vector3 mousePosDiff = mousePos - lastMousePos;
			
			Debug.Log("mouse pos: " + Input.mousePosition + " adjusted mouse pos: " + mousePos + " mouse diff: " + mousePosDiff);

			currentNode.transform.position += mousePosDiff * influence;

			lastMousePos = mousePos;

			spline.UpdateSpline();
		}

		if (Input.GetMouseButtonUp (0)) 
		{
			Debug.Log(currentNode.name + " position: " + currentNode.transform.position);
		}
	}


	private GameObject getClosestObject(float param)
	{
		SplineNode[] allNodes = spline.SplineNodes;

		float minParamDifference = Mathf.Infinity;
		GameObject closestObject = null;
		foreach (SplineNode n in allNodes) 
		{
			float paramDifference = Mathf.Abs(param - n.Parameters[spline].PosInSpline);
			if(paramDifference < minParamDifference)
			{
				minParamDifference = paramDifference;
				closestObject = n.gameObject;
			}
		}

		if (closestObject == null) 
		{
			Debug.LogWarning("couldn't find a node close to param " + param);
		}

		return closestObject;
	}

	private Vector3 adjustMousePos(Vector3 mousePos)
	{

		Vector3 adjustedMousePos = mousePos;

		//switch z and y because our scene is set up this way
		adjustedMousePos.z = adjustedMousePos.y;
		adjustedMousePos.y = 0;

		//normalize it to current screen resolution
		adjustedMousePos.x /= Screen.width;
		adjustedMousePos.z /= Screen.height;

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
