using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveOnSpline : MonoBehaviour {
	public Spline spline;
	public float param;
	public Vector3 target;
	public float speed = 2;
	public float dragThreshold = 0.001f;
	public float stopThreshold = 0.001f;
	public float switchThreshold = 0.5f;
	public float remoteThreshold = 10.0f;

	public float maxDistancePerFrame = 0.1f;

	List<Spline> closeSplines;
	List<Spline> remoteSplines;
	private Spline oldSpline;
	float curParameter;
	float curDistance;

	Vector3 targetMousePos;
	Vector3 mousePosOnDown = Vector3.zero;
	Vector3 mouseDelta = Vector3.zero;
	Vector3 posOnDown;

	float initialSpeed = 0.0f;

	int direction;

	GameObject targetPosObj;
	GameObject targetMousePosObj;

	AudioSource tap;
	AudioSource walking;

	float sinceLastRemoteCheck;

	struct InterpolationData
	{
		public float parameter;
		public Vector2 screenPos;

		public InterpolationData(float p, Vector2 sPos)
		{
			parameter = p;
			screenPos = sPos;
		}
	}

	// Use this for initialization
	void Start () 
	{
		Application.targetFrameRate = 60;

		initialSpeed = speed;

		GameObject[] splineObjects =  GameObject.FindGameObjectsWithTag("Spline");

		Debug.Log ("Number of Splines: " + splineObjects.Length);

		closeSplines = new List<Spline>(splineObjects.Length);
		closeSplines.Add (spline);
		remoteSplines = new List<Spline>(splineObjects.Length);
		

		// position on initila spline
		curParameter = spline.GetClosestPointParam (transform.position, 3);
		curDistance = spline.ConvertNormalizedParameterToDistance (curParameter);
		updateTransform ();

		//determin which splines are close and which are far away
		Vector3 pos = spline.GetPositionOnSpline(curParameter);
		Vector2 posOnScreen = (Vector2)Camera.main.WorldToScreenPoint(pos);
		for (int i = 0; i < splineObjects.Length; i++) 
		{
			Spline s = splineObjects [i].GetComponent<Spline> ();

			if(s.GetInstanceID() == spline.GetInstanceID())
				continue;

			float p = s.GetClosestPointParam(transform.position, 3);

			float dis = Vector2.Distance(posOnScreen, (Vector2)Camera.main.WorldToScreenPoint(s.GetPositionOnSpline(p)));

			if(dis > remoteThreshold)
				remoteSplines.Add(s);
			else
				closeSplines.Add (s);
		}
		sinceLastRemoteCheck = 0.0f;


		param = 0.5f;

		direction = 0;

		targetPosObj = GameObject.Find ("TargetPos");
		targetMousePosObj = GameObject.Find ("TargetMousePos");

		///adapt threshold to different resolutions
		//a higher resolution means smaller pixel, or more pixels per world unit, does the threshold has to be adjusted
		switchThreshold *= Camera.main.pixelWidth / 1024.0f;

		GameObject[] sounds = GameObject.FindGameObjectsWithTag ("Sound");

		foreach (GameObject o in sounds) 
		{
			if(o.name == "Tap")
			   tap = o.GetComponent<AudioSource>();
			else if(o.name == "walking")
				walking = o.GetComponent<AudioSource>();
		}
	}
	
	// Update is called once per frame
	void Update () 
	{

		if (Input.GetMouseButtonDown (0))
		{
			mousePosOnDown = normalizeMousePos(Input.mousePosition);
			mouseDelta = Vector3.zero;

			posOnDown = transform.position;
		}

		if (Input.GetMouseButtonUp (0)) 
		{
			if(spline.GetComponent<Waterfall>() == null && mouseDelta.magnitude < dragThreshold)
			{
				Vector3 mousePos = Input.mousePosition;
				setTarget(Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -Camera.main.transform.position.z + spline.gameObject.transform.position.z)));

				if(tap != null)
							tap.Play();

				if(walking != null)
					walking.Play();
			}
		}
		
		if (Input.GetMouseButton (0)) 
		{
			//move character along with dragged spline
			updateTransform();

			mouseDelta = normalizeMousePos (Input.mousePosition) - mousePosOnDown;
		} 


		//are we moving?
		if(direction != 0)
		{
			//first determine if we are already at target and should stop
			if(Vector3.Distance(target, transform.position - new Vector3 (0, transform.localScale.y / 2, 0)) < stopThreshold)
			{

//				if(Debug.isDebugBuild)
//					Debug.Log ("at target! pos: " + transform.position + " t.pos: " + target);

				stopMoving();
				return;
			}

			//if not, move along on current spline
			float oldP = curParameter;
			curParameter = oldP + (speed * direction * Time.deltaTime) / spline.Length;
			float oldD = curDistance;
			curDistance = spline.ConvertNormalizedParameterToDistance(curParameter);

			//position and rotate us
			updateTransform();

			//now it's time to find out if we need to switch to one of the other splines

			//world and screen pos of our character on current spline
			Vector3 pos = spline.GetPositionOnSpline(curParameter);
			Vector2 posOnScreen = (Vector2)Camera.main.WorldToScreenPoint(pos);

			//we need to interpolate the spline parameter if we have moved too far since last frame
			Vector3 avgPos;
			Vector2 avgPosOnScreen;
			List<InterpolationData> interpolations = new List<InterpolationData>(3);

			//we always want to check the position of our current parameter
			interpolations.Add(new InterpolationData(curParameter, posOnScreen));

			float distanceSinceLastFrame = Mathf.Abs (curDistance - oldD);

			//close enough -> we don't need to interpolate position on spline between frames
			if(distanceSinceLastFrame < maxDistancePerFrame)
			{
				avgPos = pos;
				avgPosOnScreen = posOnScreen;
			}
			//we moved too far since last frame -> need to also check the positions that we passed since last frame
			else
			{
				//calcualte an avg pos between last and current frame
				avgPos = spline.GetPositionOnSpline(oldP + (curParameter - oldP)/2.0f);
				avgPosOnScreen = (Vector2)Camera.main.WorldToScreenPoint(avgPos);

				//now calculate and save interpolated screen position.
				//we'll compare these to the closest point to avgPos on all other splines later on
				float lastInterpolatedP = (oldP < curParameter) ? oldP : curParameter;
				float targetP = (oldP < curParameter) ? curParameter : oldP;

				float deltaP = spline.ConvertDistanceToNormalizedParameter(maxDistancePerFrame)/3.0f;
				
				Debug.Log ("Interpolating on " + printPath(spline.transform) + " between " + oldD + " and " + curDistance + " distance: " + distanceSinceLastFrame);
				Debug.Log ("in parameters: " + oldP + " and " + curParameter + " distance: " + Mathf.Abs (curParameter - oldP) + " deltaP is: " + deltaP);

				while (true) 
				{
					float interpolatedP = lastInterpolatedP + deltaP;
					if(interpolatedP > targetP)
					{
//						if(Debug.isDebugBuild)
//						{
//							Debug.Log("finished interpolating!");
//							foreach(InterpolationData d in interpolations)
//								Debug.Log (d.parameter + " pos: " + d.screenPos);
//						}

						break;
					}
					
					interpolations.Add(new InterpolationData(interpolatedP, (Vector2)Camera.main.WorldToScreenPoint(spline.GetPositionOnSpline(interpolatedP))));
					
					lastInterpolatedP = interpolatedP;
				}
			}

			Ray rayFromAvgPos = Camera.main.ScreenPointToRay(avgPosOnScreen);
//			Debug.DrawRay(rayFromPos.origin, rayFromPos.direction * 150, Color.yellow);
			
			if(Debug.isDebugBuild)
				spline.transform.Find("CharacterPos").position = pos;
			
			Waterfall waterfall = spline.GetComponent<Waterfall>();
			float switchThresholdFactor = (waterfall) ? waterfall.switchThresholdFactor : 1;

			//first check the remotesplines every other frame
			sinceLastRemoteCheck += Time.deltaTime;
			if(sinceLastRemoteCheck > 0.1667f)
			{
				checkRemoteSplines(rayFromAvgPos, avgPosOnScreen);
		
				sinceLastRemoteCheck = 0.0f;
			}

			
			List<Spline> toRemove =new List<Spline>();
			// then check each spline that is close enough if we need to switch spline
			for (int cI = 0; cI < closeSplines.Count; cI++)
			{
				Spline otherSpline = closeSplines[cI];
				
				//don't compare to active spline
				if(otherSpline.GetInstanceID() == spline.GetInstanceID())
					continue;
				
				//waterfalls can only be switched to from a specific direction
				//unless we are coming from a waterfall
				Waterfall otherWaterfall = otherSpline.GetComponent<Waterfall>();
				if(otherWaterfall != null)
				{
					if(otherWaterfall.direction != direction)
						continue;
				}

				
				if(Debug.isDebugBuild && interpolations.Count > 1)
					Debug.Log ("Comparing " + printPath(spline.transform) + " to " + printPath (otherSpline.transform));

				float otherP = otherSpline.GetClosestPointParamToRay(rayFromAvgPos, 5);

				Vector3 otherPos = otherSpline.GetPositionOnSpline(otherP);
				
				if(Debug.isDebugBuild)
					otherSpline.transform.Find("CharacterPos").position = otherPos;

				bool switched = false;

				//in screenspace to take persepctive into account
				Vector2 otherPosOnScreen = (Vector2)Camera.main.WorldToScreenPoint(otherPos);

				//compare position on otherSpline with all interpolated positions on current spline
				foreach(InterpolationData interpolated in interpolations)
				{
					float dis = Vector3.Distance(interpolated.screenPos, otherPosOnScreen);
					
					if(Debug.isDebugBuild && interpolations.Count > 1)
					{
						Debug.Log ("interpolated screenPos: " + interpolated.screenPos + " otherScreenPos: " + otherPosOnScreen + " dis: " + dis);
					}
					//if we are far away stop checking this spline every frame
					if(dis > remoteThreshold)
					{
						toRemove.Add(otherSpline);
						remoteSplines.Add (otherSpline);

						break;		//no need to check the other interpolated positions
					}
					//are we close enough to switch?
					else if(dis < switchThreshold * switchThresholdFactor)
					{
						Vector3 tangent = spline.GetTangentToSpline(interpolated.parameter) * direction;
						Vector3 otherTangent = otherSpline.GetTangentToSpline(otherP) * direction;
						
						Debug.Log (printPath(spline.transform) + " & " + printPath (otherSpline.transform) + " are crossing! dir: " + direction + " t.y: " + tangent.y + " oT.y: " + otherTangent.y + " p: " + interpolated.parameter + " oP: " + otherP);
						
						
						//is the other spline going up, that is moving higher?
						if(otherTangent.y >	tangent.y)// || spline.transform.position.z == s.transform.position.z && (oldSpline == null || oldSpline.GetInstanceID() != s.GetInstanceID()) )
						{
							Spline oldSpline = spline;
							//						StartCoroutine(forgetOldSpline());
							
							if(Debug.isDebugBuild)
								Debug.Log("SWITCHED to spline: " + printPath(otherSpline.transform));
							
							spline = otherSpline;
							curParameter = otherP;
							curDistance = spline.ConvertNormalizedParameterToDistance(otherP);

							updateTransform ();
							
							//switching to a waterfall
							if(otherWaterfall!=null)
							{
								speed = otherWaterfall.fallSpeed;
								
								//							Vector3 screenBottom = Camera.main.ScreenToWorldPoint(new Vector3(0,0, -Camera.main.transform.position.z + transform.position.z));
								
								float targetParam = (otherWaterfall.direction >0) ? 1.0f : 0.0f;
								setTarget(spline.GetPositionOnSpline(targetParam));
							}
							//switching to a normal spline
							else
							{
								speed = initialSpeed;
								
								if(waterfall != null)
									stopMoving();			//after a waterfall stop where we currently are
								else
									setTarget (targetMousePos + new Vector3(0,0, spline.transform.position.z - oldSpline.transform.position.z));		//recalculate target on new spline
							}

							switched = true;
							break; //no need to check other interpolated parameters anymore
						}
					}
				}
				
				//remove any spline that is now far enough away to only be checked occasionally
				foreach (Spline rmS in toRemove) 
					closeSplines.Remove(rmS);
				
//				if(Debug.isDebugBuild && toRemove.Count > 0)
//				{
//					Debug.Log ("LISTS CHANGED!  closeSplines (" + closeSplines.Count + ") remoteSplines (" + remoteSplines.Count + ")");
//					Debug.Log ("removed from closeSpline:");
//					foreach (Spline rmS in toRemove) 
//						Debug.Log (printPath(rmS.transform));
//				}
				toRemove.Clear ();

				if(switched)
					break;
			}
		}
	
	}

	public void setTarget(Vector3 worldPosition)
	{
		targetMousePos = worldPosition;

		target = spline.GetPositionOnSpline(spline.GetClosestPointParam(worldPosition, 3));
		direction = (target.x - transform.position.x) > 0 ? 1 : -1;

		if (Debug.isDebugBuild) 
		{
			if (targetMousePosObj != null)
				targetMousePosObj.transform.position = targetMousePos;
			if (targetPosObj != null)
				targetPosObj.transform.position = target;

		}
	}


	private void updateTransform()
	{
		transform.position = spline.GetPositionOnSpline (curParameter) + new Vector3 (0, transform.localScale.y / 2, 0);
		transform.rotation = spline.GetOrientationOnSpline (curParameter);
	}

	
	private Vector3 normalizeMousePos(Vector3 mousePos)
	{
		Vector3 adjustedMousePos = mousePos;
		
		//normalize it to current screen resolution
		adjustedMousePos.x /= Screen.width;
		adjustedMousePos.y /= Screen.height;
		
		return adjustedMousePos;
	}

	private IEnumerator forgetOldSpline()
	{
		yield return new WaitForSeconds(0.5f);

		oldSpline = null;
	}

	public string printPath(Transform t)
	{
		string path = t.name;

		while (t.parent != null) 
		{
			t = t.parent;
			path = t.name + "." + path;
		}

		return path;
	}

	public void stopMoving()
	{
		direction = 0;

		
		if(walking != null)
			walking.Stop();
	}

	void printSplineLists()
	{
		Debug.Log ("REMOTESPLINES count: " + remoteSplines.Count);
		foreach (Spline s in remoteSplines)
			Debug.Log (printPath (s.transform));
		
		Debug.Log ("CLOSESPLINES count: " + closeSplines.Count);
		foreach (Spline s in closeSplines)
			Debug.Log (printPath (s.transform));
	}

	void checkRemoteSplines(Ray ray, Vector2 compareTo)
	{
		List<Spline> toRemove =new List<Spline>();
		for (int rI = 0; rI < remoteSplines.Count; rI++)
		{
			Spline s = remoteSplines[rI];
			
			float p = s.GetClosestPointParamToRay(ray, 5);
			
			Vector2 posOnScreen = (Vector2)Camera.main.WorldToScreenPoint(s.GetPositionOnSpline(p));
			float dis = Vector2.Distance(compareTo, posOnScreen );
			
			//					if(Debug.isDebugBuild)
			//						Debug.Log("distance to " + printPath(s.transform) + " is " + dis);
			
			if(dis <= remoteThreshold)
			{
				closeSplines.Add (s);
				toRemove.Add(s);
			}
			
			
			//			i++;
		}
		foreach (Spline rmS in toRemove) 
			remoteSplines.Remove(rmS);
		
		
//		if(Debug.isDebugBuild && toRemove.Count > 0)
//		{
//			Debug.Log ("LISTS CHANGED! remoteSplines (" + remoteSplines.Count + ")" + " - > closeSplines (" + closeSplines.Count + ")");
//			Debug.Log ("ADDED to closeSpline:");
//			foreach (Spline rmS in toRemove) 
//				Debug.Log (printPath(rmS.transform));
//		}
		
		toRemove.Clear();
	}
}
