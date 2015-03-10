using UnityEngine;
using System.Collections;

//struct FormableNode
//{
//	public SplineNode splineNode;
//	public Rect limits;
//	public Transform transform;
//
//	public FormableNode(SplineNode n)
//	{
//		splineNode = n;
//		if(n != null)
//			transform = n.transform;
//		else
//			transform = null;
//		
//		limits = new Rect (0, 0, 0, 0);
//	}
//
//	public FormableNode(SplineNode n, Bounds b)
//	{
//		splineNode = n;
//		if(n != null)
//			transform = n.transform;
//		else
//			transform = null;
//
//		limits = new Rect (0, 0, 0, 0);
//
//		setLimits (b);
//	}
//	
//	public FormableNode(SplineNode n, BoxCollider bc)
//	{
//		splineNode = n;
//		if(n != null)
//			transform = n.transform;
//		else
//			transform = null;
//		
//		limits = new Rect (0, 0, 0, 0);
//
//		if(bc != null)
//			setLimits (bc.bounds);
//	}
//
//	public void setLimits(Bounds b)
//	{
//		limits = new Rect (b.min.x, b.min.y, b.size.x, b.size.y);
//	}
//
//	public bool allowForming(Vector3 newPos)
//	{
//		if (limits.width == 0)
//			return true;
//
//		return limits.Contains (new Vector2 (newPos.x, newPos.y));
//	}
//}

public class Terraform : MonoBehaviour {

	public float influence = 1;

	protected SplineSelector selector;
	protected Spline spline;
	protected ArrayList formableNodes;
	protected Vector3 lastMousePos;
	
	protected Bounds limits;

	// Use this for initialization
	virtual protected void Start () 
	{
		selector = GameObject.FindGameObjectWithTag ("GameController").GetComponent<SplineSelector> ();

		limits = new Bounds(Vector3.zero, Vector3.zero);

		formableNodes = new ArrayList ();
	}
	
	// Update is called once per frame
	virtual protected void Update () 
	{
		if (Input.GetMouseButtonDown (0)) 
		{
			lastMousePos = normalizeMousePos(Input.mousePosition);
		}

		if (Input.GetMouseButton (0)) 
		{
			//don't do anything if SplineSelector hasn't found a spline yet.
			if(selector.spline == null)
				return;

			//this is the first mouse down after SplineSelector has found closest spline
			// -> get the points we will transform during this stroke
			if(spline == null)
			{
				spline = selector.spline;

				Vector3 mouse = Input.mousePosition;
				Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3 (mouse.x, mouse.y, -Camera.main.gameObject.transform.position.z));

				Transform limitsObject = spline.transform.Find ("Limits");
				if (limitsObject != null) 
					limits = limitsObject.GetComponent<BoxCollider>().bounds;

				//find all nodes that are formable and store their limits
				formableNodes.Clear();
				for (int i = 0; i < spline.transform.childCount; i++)
				{
					Transform t = spline.transform.GetChild(i);
					if(!t.CompareTag("Formable Node"))
						continue;

					formableNodes.Add(t.GetComponent<FormableNode>());
				}

				findClosestObjects(spline.GetClosestPointParam(mouseWorld, 3));
			}

			Vector3 mousePos = normalizeMousePos(Input.mousePosition);
			Vector3 mousePosDiff = mousePos - lastMousePos;
			
//			Debug.Log("mouse pos: " + Input.mousePosition + " adjusted mouse pos: " + mousePos + " mouse diff: " + mousePosDiff);

			terraform(mousePosDiff);

			lastMousePos = mousePos;

			spline.UpdateSpline();
		}

		if (Input.GetMouseButtonUp (0)) 
		{
			spline = null;

//			Destroy(limits);
			limits = new Bounds(Vector3.zero, Vector3.zero);
		}
	}

	virtual protected void terraform(Vector3 mousePosDiff)
	{

	}

	virtual protected void findClosestObjects(float param)
	{
	}

	private Vector3 normalizeMousePos(Vector3 mousePos)
	{
		Vector3 adjustedMousePos = mousePos;

		//normalize it to current screen resolution
		adjustedMousePos.x /= Screen.width;
		adjustedMousePos.y /= Screen.height;

		return adjustedMousePos;
	}

}
