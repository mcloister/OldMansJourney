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

public class TerraformBrush : Terraform {

	public float radius = 3;

	public ArrayList toTerraform;

	// Use this for initialization
	protected override void Start () 
	{
		base.Start ();

		toTerraform = new ArrayList();
	}
	
	// Update is called once per frame
	protected override void Update () {
		base.Update ();

		if (Input.GetMouseButtonUp (0)) 
		{
			toTerraform.Clear();
		}
	}

	override protected void findClosestObjects(float param, int pointerId)
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
	
	
	protected override void terraform(Vector3 mousePosDiff, int pointerId)
	{
		foreach(Formable f in toTerraform)
			f.transform.position += mousePosDiff * influence * f.weight;
	}

}
