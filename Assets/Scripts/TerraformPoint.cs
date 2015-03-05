using UnityEngine;
using System.Collections;

public class TerraformPoint : Terraform 
{
	public float minDistance;

	ArrayList toTerraform;
	SplineNode pickedPoint;
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
		//we don't allow the first and the last control point to be moved, so no need to find them
		for (int i = 1; i < allNodes.Length-1; i++) 
		{
			SplineNode n = allNodes[i];
	
			float dist = (onSpline - spline.GetPositionOnSpline(n.Parameters[spline].PosInSpline)).magnitude;
			if(dist < minDist)
			{
				minDist = dist;
				pickedPoint = n;
			}
		}

		frontier = pickedPoint;

		toTerraform.Add (pickedPoint);
		
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

			toTerraform.Clear();
			toTerraform.Add(pickedPoint);
			frontier = pickedPoint;

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

			toTerraform.Clear();
			toTerraform.Add(pickedPoint);
			frontier = pickedPoint;
			
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
			Vector3 newPos = n.transform.position + mousePosDiff * influence;

			if(limits.extents.y == 0 || (newPos.y < limits.max.y && newPos.y > limits.min.y) ) //limits.Contains(newPos)
				n.transform.position = newPos;
		}

		//if we are getting to close to our neighbour, just move it along and update our data
		if ( neighbour != null) 
		{
			float distance = Mathf.Abs (frontier.transform.position.x - neighbour.transform.position.x);

			//nothing to do if we are still far enough away
			if (distance >  minDistance)
				return;

			//push neighbour away again to always keep the minDistance
			Vector3 nPos = neighbour.transform.position;
			nPos.x += (minDistance-distance) * direction;
			neighbour.transform.position = nPos;

			toTerraform.Add (neighbour);

			frontier = neighbour;

			
			//now find the neighbour of our new frontier
			float minDist = Mathf.Infinity;
			//dragging right
			if(direction >= 0)
			{
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
			//dragging left
			else
			{
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

		}

	}

}
