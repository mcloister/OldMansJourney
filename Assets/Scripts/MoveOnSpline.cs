using UnityEngine;
using UnityDebugger;
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
	AudioSource switchSound;

	float sinceLastRemoteCheck;

	bool inWaterfall;

	Ray debugRay;

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

		//only lower part of current spline will be touchable
		addTouchableOffset (spline, 3);

		initialSpeed = speed;

		GameObject[] splineObjects =  GameObject.FindGameObjectsWithTag("Spline");

		Debugger.Log ("Number of Splines: " + splineObjects.Length);

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

		targetPosObj = GameObject.FindGameObjectWithTag ("TargetPos");
		if(targetPosObj != null)
			targetPosObj.SetActive (false);
		targetMousePosObj = GameObject.Find ("TargetMousePos");

		///adapt threshold to different resolutions
		//a higher resolution means smaller pixel, or more pixels per world unit, thus the threshold has to be adjusted
		switchThreshold *= Camera.main.pixelWidth / 1024.0f;

		GameObject[] sounds = GameObject.FindGameObjectsWithTag ("Sound");

		foreach (GameObject o in sounds) 
		{
			if(o.name == "Tap Sound")
			   tap = o.GetComponent<AudioSource>();
			else if(o.name == "Walking Sound")
				walking = o.GetComponent<AudioSource>();
			else if(o.name == "Switch Sound")
				switchSound = o.GetComponent<AudioSource>();
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

				if(targetPosObj != null)
					targetPosObj.SetActive (true);

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
			if(Vector3.Distance(target, transform.position - new Vector3 (0, transform.localScale.y / 2, -1)) < stopThreshold)
			{

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

				while (true) 
				{
					float interpolatedP = lastInterpolatedP + deltaP;
					if(interpolatedP > targetP)
					{

						break;
					}
					
					interpolations.Add(new InterpolationData(interpolatedP, (Vector2)Camera.main.WorldToScreenPoint(spline.GetPositionOnSpline(interpolatedP))));
					
					lastInterpolatedP = interpolatedP;
				}
			}

			Ray rayFromAvgPos = Camera.main.ScreenPointToRay(avgPosOnScreen);

			if(Debug.isDebugBuild)
				spline.transform.Find("CharacterPos").position = pos;
			
			Waterfall waterfall = spline.GetComponent<Waterfall>();
			float switchThresholdFactor = (waterfall) ? waterfall.switchThresholdFactor : 1;

			//first update the remotesplines every other frame
			sinceLastRemoteCheck += Time.deltaTime;
			if(sinceLastRemoteCheck > 0.1667f)
			{
				checkRemoteSplines(rayFromAvgPos, avgPosOnScreen);
		
				sinceLastRemoteCheck = 0.0f;
			}


			// then check each spline that is close enough if we need to switch spline
			List<Spline> toRemove =new List<Spline>();
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

//						Debugger.Log (printPath(spline.transform) + " & " + printPath (otherSpline.transform) + " are crossing! dir: " + direction + " t.y: " + tangent.y + " oT.y: " + otherTangent.y + " p: " + interpolated.parameter + " oP: " + otherP);
						
						
						//is the other spline going up, that is moving higher?
						if(otherTangent.y >	tangent.y)// || spline.transform.position.z == s.transform.position.z && (oldSpline == null || oldSpline.GetInstanceID() != s.GetInstanceID()) )
						{
							switchTo(otherSpline, otherP);

							switched = true;
							break; //no need to check other interpolated parameters anymore
						}
					}
				}
				
				//remove any spline that is now far enough away to only be checked occasionally
				foreach (Spline rmS in toRemove) 
					closeSplines.Remove(rmS);

				toRemove.Clear ();

				if(switched)
					break;
			}
		}

		if (Debug.isDebugBuild) 
		{
//			Debug.DrawRay(debugRay.origin, debugRay.direction);
		}
	
	}

	public void setTarget(Vector3 worldPosition)
	{
		targetMousePos = worldPosition;
		float param = spline.GetClosestPointParam (worldPosition, 3);

		target = spline.GetPositionOnSpline(param);
		direction = (target.x - transform.position.x) > 0 ? 1 : -1;

		//don't move if an immovable object is in the way
		//we cast a ray from characters center along tangent at current position on spline
		RaycastHit hit;
		string[] layers = {"Dynamic"};
		if (Physics.Raycast (transform.position, spline.GetTangentToSpline (param) * direction, out hit, 10, LayerMask.GetMask (layers))) 
		{
			if(hit.collider.CompareTag("Immovable"))
			{
				direction = 0;	//don't move;
				return;
			}
		}

		if (Debug.isDebugBuild) 
		{
			if (targetMousePosObj != null)
				targetMousePosObj.transform.position = targetMousePos;
		}
		if (targetPosObj != null)
			targetPosObj.transform.position = target + new Vector3(0,0,-1);

	}


	private void updateTransform()
	{
		transform.position = spline.GetPositionOnSpline (curParameter) + new Vector3 (0, transform.localScale.y / 2, -1);
//		if (inWaterfall)
//			transform.rotation = Quaternion.identity;
//		else
//			transform.rotation = spline.GetOrientationOnSpline (curParameter);
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

		if(targetPosObj != null)
			targetPosObj.SetActive (false);

		if(walking != null)
			walking.Stop();
	}

	void printSplineLists()
	{
		Debugger.Log ("REMOTESPLINES count: " + remoteSplines.Count);
		foreach (Spline s in remoteSplines)
			Debugger.Log (printPath (s.transform));
		
		Debugger.Log ("CLOSESPLINES count: " + closeSplines.Count);
		foreach (Spline s in closeSplines)
			Debugger.Log (printPath (s.transform));
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
			
			if(dis <= remoteThreshold)
			{
				closeSplines.Add (s);
				toRemove.Add(s);
			}
			
			
			//			i++;
		}
		foreach (Spline rmS in toRemove) 
			remoteSplines.Remove(rmS);

		
		toRemove.Clear();
	}

	public void switchTo(Spline newSpline, float newP)
	{
		if (switchSound != null)
			switchSound.Play ();

		Spline oldSpline = spline;

		addTouchableOffset (oldSpline, -5);		//old spline will be touchable above the line again
		addTouchableOffset (newSpline, 3);		//new spline is only touchable a bit below spline line

		Waterfall waterfall = oldSpline.GetComponent<Waterfall> ();
		//						StartCoroutine(forgetOldSpline());
		
		Debugger.Log("SWITCHED to spline: " + printPath(newSpline.transform));
		
		spline = newSpline;
		curParameter = newP;
		curDistance = spline.ConvertNormalizedParameterToDistance(newP);
		
		updateTransform ();

		Waterfall newWaterfall = newSpline.GetComponent<Waterfall> ();
		
		inWaterfall = newWaterfall != null;

		//switching to a waterfall
		if(newWaterfall!=null)
		{
			speed = newWaterfall.fallSpeed;

			float targetParam = (newWaterfall.direction >0) ? 1.0f : 0.0f;
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
	}

	void OnCollisionEnter(Collision collision) 
	{
		if(direction != 0 && collision.collider.CompareTag("Immovable"))// gameObject.layer == LayerMask.NameToLayer("Dynamic"))
			stopMoving();
	}
		
		void addTouchableOffset(Spline spline, float addend)
	{
		Transform collider = spline.transform.Find ("Collider");
		if (!collider) 
			collider = spline.transform.Find ("Collision/Touchable Area");
		if(!collider)
			collider = spline.transform;

		SplineMesh mesh = collider.GetComponent<SplineMesh> ();
		if (!mesh)
			return;
		MoveVerticesBelowCurve verticesModifier = collider.GetComponent<MoveVerticesBelowCurve> ();
		if (!verticesModifier)
			return;

		verticesModifier.moveOffset = mesh.xyScale.y/2 + addend;

		mesh.UpdateMesh ();
		mesh.UpdateMesh ();	//need to call it twice, otherwise it wont update correctly...strange!

	}
}
