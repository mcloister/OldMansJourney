using UnityEngine;
using System.Collections;

public class TerraformPoint : Terraform 
{
	ArrayList toTerraform;
	SplineNode frontier;
	SplineNode neighbour;
	float direction;

	// Use this for initialization
	protected override void Start () 
	{
		base.Start ();

		toTerraform = new ArrayList();
		neighbour = null;
		frontier = null;
	}
	
	// Update is called once per frame
	protected override void Update () {
		base.Update ();

		if (Input.GetMouseButtonUp (0)) 
		{
			toTerraform.Clear();

			direction = 0;
			neighbour = null;
			frontier = null;
		}
	}
	
	override protected void findClosestObjects(float param)
	{
		SplineNode[] allNodes = spline.SplineNodes;
		
		Vector3 onSpline = spline.GetPositionOnSpline (param);

		float minDist = Mathf.Infinity;
		SplineNode closest = null;
		//we don't allow the first and the last control point to be moved, so no need to find them
		for (int i = 1; i < allNodes.Length-1; i++) 
		{
			SplineNode n = allNodes[i];
			
			float dist = (onSpline - spline.GetPositionOnSpline(n.Parameters[spline].PosInSpline)).magnitude;
			if(dist < minDist)
			{
				minDist = dist;
				closest = n;
			}
		}

		toTerraform.Add (closest);
		
		if (toTerraform == null) 
		{
			Debug.LogWarning("couldn't find a node close enough to param " + param);
		}
	}
	
	
	protected override void terraform(Vector3 mousePosDiff)
	{
		//start dragging left
		if (mousePosDiff.x < 0 && direction >= 0) 
		{
			direction = -1;

			//find leftmost control point we are moving, aka our frontier
			float leftmostX = Mathf.Infinity;
			foreach (SplineNode n in toTerraform) 
			{
				if(n.transform.position.x < leftmostX)
				{
					leftmostX = n.transform.position.x;
					frontier = n;
				}
			}

			//now find the left neighbour of our frontier
			float minDist = Mathf.Infinity;
			foreach(SplineNode n in spline.SplineNodes)
			{
				if(n.transform.position.x < frontier.transform.position.x)
				{
					float dist = Mathf.Abs (frontier.transform.position.x - n.transform.position.x);
					if(dist < minDist)
					{
						minDist = dist;
						neighbour = n;
					}
				}
			}
		}

		//start dragging right
		if (mousePosDiff.x > 0 && direction <= 0) 
		{
			direction = 1;
			
			//find leftmost control point we are moving, aka our frontier
			float rightmostX = -Mathf.Infinity;
			foreach (SplineNode n in toTerraform) 
			{
				if(n.transform.position.x > rightmostX)
				{
					rightmostX = n.transform.position.x;
					frontier = n;
				}
			}
			
			//now find the right neighbour of our frontier
			float minDist = Mathf.Infinity;
			foreach(SplineNode n in spline.SplineNodes)
			{
				if(n.transform.position.x > frontier.transform.position.x)
				{
					float dist = Mathf.Abs (frontier.transform.position.x - n.transform.position.x);
					if(dist < minDist)
					{
						minDist = dist;
						neighbour = n;
					}
				}
			}
		}

		//terraform all nodes we need to move
		foreach (SplineNode n in toTerraform) 
		{
			n.transform.position += mousePosDiff * influence;
		}

		//if we are getting to close to our neighbour, just move it along and update our data
		if ( (mousePosDiff.x < 0 && frontier.transform.position.x <= neighbour.transform.position.x) ||
		    (mousePosDiff.x > 0 && frontier.transform.position.x >= neighbour.transform.position.x) ) 
		{
			toTerraform.Add (neighbour);

			direction = 0;		//this will force us to find a new frontier and neighbour
		}

	}

}
