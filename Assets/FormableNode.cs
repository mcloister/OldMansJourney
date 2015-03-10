using UnityEngine;
using System.Collections;

public class FormableNode : MonoBehaviour {
	public SplineNode splineNode = null;

	public Rect limits = new Rect(0,0,0,0);


	// Use this for initialization
	void Start () {
	
		splineNode = GetComponent<SplineNode> ();
		BoxCollider bc = GetComponent<BoxCollider> ();
		if (bc != null) 
			setLimits(bc.bounds);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	
	public void setLimits(Bounds b)
	{
		limits = new Rect (b.min.x, b.min.y, b.size.x, b.size.y);
	}
	
	public bool allowForming(Vector3 newPos)
	{
		if (limits.width == 0)
			return true;
		
		return limits.Contains (new Vector2 (newPos.x, newPos.y));
	}
}
