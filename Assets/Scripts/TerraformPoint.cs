using UnityEngine;
using System.Collections;

public class TerraformPoint : Terraform 
{
	public float minDistance;

	ArrayList toTerraform;
	FormableNode pickedNode;
	FormableNode frontier;
	FormableNode neighbour;
	float direction;

	// Use this for initialization
	protected override void Start () 
	{
		base.Start ();

		toTerraform = new ArrayList();
		neighbour = new FormableNode(null);
		frontier = new FormableNode(null);
	}
	
	// Update is called once per frame
	protected override void Update () {
		base.Update ();

		if (Input.GetMouseButtonUp (0)) 
		{
			toTerraform.Clear();

			direction = 0;
			neighbour = new FormableNode(null);
			frontier = new FormableNode(null);
		}
	}
	
	override protected void findClosestObjects(float param)
	{
//		SplineNode[] allNodes = spline.SplineNodes;
		
		Vector3 onSpline = spline.GetPositionOnSpline (param);

		float minDist = Mathf.Infinity;

		//we don't allow the first and the last control point to be moved, so no need to find them
		foreach(FormableNode fN in formableNodes)
		{
			SplineNode n = fN.splineNode;
	
			float dist = (onSpline - spline.GetPositionOnSpline(n.Parameters[spline].PosInSpline)).magnitude;
			if(dist < minDist)
			{
				minDist = dist;
				pickedNode = fN;
			}
		}

		frontier = pickedNode;

		toTerraform.Add (pickedNode);
		
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
			toTerraform.Add(pickedNode);
			frontier = pickedNode;

			//now find the left neighbour of our frontier
			findLeftNeighbour();
		}

		//start dragging right
		if (mousePosDiff.x > 0 && direction <= 0) 
		{
			direction = 1;

			toTerraform.Clear();
			toTerraform.Add(pickedNode);
			frontier = pickedNode;
			
			//now find the right neighbour of our frontier
			findRightNeighbour();

		}

		//terraform all nodes we need to move
		foreach (FormableNode fN in toTerraform) 
		{
			Vector3 newPos = fN.transform.position + mousePosDiff * influence;

			if( fN.allowForming(newPos) ) //limits.Contains(newPos)
				fN.transform.position = newPos;
			else
			{
				//make sure we're never pushed outside of our limits
//				float offset = 0.01f;
//
//				Vector3 curPos = fN.transform.position;
//
//				if(curPos.x <= fN.limits.xMin)
//					curPos.x = fN.limits.xMin + offset;
//				if(curPos.y <= fN.limits.yMin)
//					curPos.y = fN.limits.xMin + offset;
//
//				
//				if(curPos.x >= fN.limits.xMax)
//					curPos.x = fN.limits.xMax - offset;
//				if(curPos.y >= fN.limits.yMax)
//					curPos.y = fN.limits.yMax - offset;
//
//				fN.transform.position = curPos;
			}

		}

		//if we are getting to close to our neighbour, just move it along and update our data
		if ( neighbour.splineNode != null) 
		{
			float distance = Mathf.Abs (frontier.transform.position.x - neighbour.transform.position.x);

			//nothing to do if we are still far enough away
			if (distance >  minDistance)
				return;

			//if we have come too close already, push neighbour away again to always keep the minDistance
//			Vector3 nPos = neighbour.transform.position;
//			nPos.x += (minDistance-distance) * direction;
//			neighbour.transform.position = nPos;

			toTerraform.Add (neighbour);

			frontier = neighbour;

			
			//now find the neighbour of our new frontier
			//dragging right
			if(direction >= 0)
			{
				findRightNeighbour();
			}
			//dragging left
			else
			{
				findLeftNeighbour();
			}

		}

	}

	void findRightNeighbour()
	{
		float minDist = Mathf.Infinity;
		foreach (FormableNode fN in formableNodes) {
			
			if (fN.splineNode.transform.position.x > frontier.transform.position.x) {
				float dist = Mathf.Abs (frontier.transform.position.x - fN.transform.position.x);
				if (dist < minDist) {
					minDist = dist;
					neighbour = fN;
				}
			}
		}
		
	}
	
	void findLeftNeighbour()
	{
		float minDist = Mathf.Infinity;
		foreach (FormableNode fN in formableNodes) {
			
			if (fN.splineNode.transform.position.x < frontier.transform.position.x) {
				float dist = Mathf.Abs (frontier.transform.position.x - fN.transform.position.x);
				if (dist < minDist) {
					minDist = dist;
					neighbour = fN;
				}
			}
		}
		
	}
}	