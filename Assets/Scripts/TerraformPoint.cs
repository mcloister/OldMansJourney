using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class TerraformPoint : Terraform 
{
	public float minDistance;

	Dictionary<int, List<FormableNode>> toTerraform;
//	ArrayList toTerraform;
	Dictionary<int, FormableNode> pickedNodes;
//	FormableNode pickedNode;
	Dictionary<int, FormableNode> frontiers;
//	FormableNode frontier;
	Dictionary<int, FormableNode> neighbours;
//	FormableNode neighbour;
	Dictionary<int, float> directions;
//	float direction;

	// Use this for initialization
	protected override void Start () 
	{
		base.Start ();

		toTerraform = new Dictionary<int, List<FormableNode>>();
		pickedNodes = new Dictionary<int, FormableNode> ();
		neighbours = new Dictionary<int, FormableNode>();
		frontiers = new Dictionary<int, FormableNode>();
		directions = new Dictionary<int, float> ();
	}
	
	override protected void findClosestObjects(float param, int pointerId)
	{
//		SplineNode[] allNodes = spline.SplineNodes;
		
		Vector3 onSpline = spline.GetPositionOnSpline (param);

		float minDist = Mathf.Infinity;

		FormableNode pickedNode = null;
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
		
		if (pickedNode == null) 
		{
			Debug.LogError("couldn't find a node close enough to param " + param);
			return;
		}
		
		pickedNodes.Add (pointerId, pickedNode);

		frontiers.Add(pointerId, pickedNodes[pointerId]);

		List<FormableNode> temp = new List<FormableNode> ();
		temp.Add (pickedNodes[pointerId]);
		toTerraform.Add (pointerId, temp);

		//		Destroy (temp);

		neighbours.Add (pointerId, null);
		directions.Add (pointerId, 0);
	}
	
	
	protected override void terraform(Vector3 mousePosDiff, int pointerId)
	{
		if (!directions.ContainsKey (pointerId))
			return;
		if (!pickedNodes.ContainsKey (pointerId))
			return;
		if (!frontiers.ContainsKey (pointerId))
			return;
		if (!neighbours.ContainsKey (pointerId))
			return;


		//start dragging left
		if (mousePosDiff.x < 0 && directions[pointerId] >= 0) 
		{
			directions[pointerId] = -1;

			toTerraform[pointerId].Clear();
			toTerraform[pointerId].Add(pickedNodes[pointerId]);
			frontiers[pointerId] = pickedNodes[pointerId];

			//now find the left neighbour of our frontier
			findLeftNeighbour(pointerId);
		}

		//start dragging right
		if (mousePosDiff.x > 0 && directions[pointerId] <= 0) 
		{
			directions[pointerId] = 1;

			toTerraform[pointerId].Clear();
			toTerraform[pointerId].Add(pickedNodes[pointerId]);
			frontiers[pointerId] = pickedNodes[pointerId];
			
			//now find the right neighbour of our frontier
			findRightNeighbour(pointerId);

		}

		//terraform all nodes we need to move
		foreach (FormableNode fN in toTerraform[pointerId]) 
		{
			Vector3 newPos = fN.transform.position + mousePosDiff * influence;

			if( fN.allowForming(newPos) ) //limits.Contains(newPos)
				fN.transform.position = newPos;
			else
			{
				//make sure we're never pushed outside of our limits
				float offset = 0.01f;

				Vector3 curPos = fN.transform.position;

				if(curPos.x <= fN.limits.xMin)
					curPos.x = fN.limits.xMin + offset;
				if(curPos.y <= fN.limits.yMin)
					curPos.y = fN.limits.xMin + offset;

				
				if(curPos.x >= fN.limits.xMax)
					curPos.x = fN.limits.xMax - offset;
				if(curPos.y >= fN.limits.yMax)
					curPos.y = fN.limits.yMax - offset;

				fN.transform.position = curPos;
			}

		}

		//if we are getting too close to our neighbour, just move it along and update our data
		if ( neighbours[pointerId] != null) 
		{
			float distance = Mathf.Abs (frontiers[pointerId].transform.position.x - neighbours[pointerId].transform.position.x);

			//nothing to do if we are still far enough away
			if (distance >  minDistance)
				return;

			//if we have come too close already, push neighbour away again to always keep the minDistance
//			Vector3 nPos = neighbour.transform.position;
//			nPos.x += (minDistance-distance) * direction;
//			neighbour.transform.position = nPos;

			toTerraform[pointerId].Add (neighbours[pointerId]);

			frontiers[pointerId] = neighbours[pointerId];
			neighbours[pointerId] = null;
			
			//now find the neighbour of our new frontier
			//dragging right
			if(directions[pointerId] >= 0)
			{
				findRightNeighbour(pointerId);
			}
			//dragging left
			else
			{
				findLeftNeighbour(pointerId);
			}

		}

	}
	
	override protected void endTerraforming(int pointerId)
	{
		base.endTerraforming (pointerId);
		
		toTerraform.Remove (pointerId);
		
		pickedNodes.Remove (pointerId);
		frontiers.Remove(pointerId);
		neighbours.Remove(pointerId);
		directions.Remove(pointerId);
	}

	void findRightNeighbour(int pointerId)
	{
		if (!frontiers.ContainsKey (pointerId))
			return;
		if (!neighbours.ContainsKey (pointerId))
			return;

		float minDist = Mathf.Infinity;
		foreach (FormableNode fN in formableNodes) {
			
			if (fN.transform.position.x > frontiers[pointerId].transform.position.x) {
				float dist = Mathf.Abs (frontiers[pointerId].transform.position.x - fN.transform.position.x);
				if (dist < minDist) {
					minDist = dist;
					neighbours[pointerId] = fN;
				}
			}
		}
		
	}
	
	void findLeftNeighbour(int pointerId)
	{
		if (!frontiers.ContainsKey (pointerId))
			return;
		if (!neighbours.ContainsKey (pointerId))
			return;

		float minDist = Mathf.Infinity;
		foreach (FormableNode fN in formableNodes) {
			
			if (fN.transform.position.x < frontiers[pointerId].transform.position.x) {
				float dist = Mathf.Abs (frontiers[pointerId].transform.position.x - fN.transform.position.x);
				if (dist < minDist) {
					minDist = dist;
					neighbours[pointerId] = fN;
				}
			}
		}
		
	}
}	