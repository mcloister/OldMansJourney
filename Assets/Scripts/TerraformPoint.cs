using UnityEngine;
using System.Collections;

public class TerraformPoint : Terraform 
{
	GameObject toTerraform;

	// Use this for initialization
	protected override void Start () 
	{
		base.Start ();

		toTerraform = null;
	}
	
	// Update is called once per frame
	protected override void Update () {
		base.Update ();

		if (Input.GetMouseButtonUp (0)) 
		{
			toTerraform = null;
		}
	}
	
	override protected void findClosestObjects(float param)
	{
		SplineNode[] allNodes = spline.SplineNodes;
		
		Vector3 onSpline = spline.GetPositionOnSpline (param);

		float minDist = Mathf.Infinity;

		//we don't allow the first and the last control point to be moved
		for (int i = 1; i < allNodes.Length-1; i++) 
		{
			SplineNode n = allNodes[i];
			
			float dist = (onSpline - spline.GetPositionOnSpline(n.Parameters[spline].PosInSpline)).magnitude;
			if(dist < minDist)
			{
				minDist = dist;
				toTerraform = n.gameObject;
			}
		}
		
		if (toTerraform == null) 
		{
			Debug.LogWarning("couldn't find a node close enough to param " + param);
		}
	}
	
	
	protected override void terraform(Vector3 mousePosDiff)
	{
		toTerraform.transform.position += mousePosDiff * influence;
	}

}
