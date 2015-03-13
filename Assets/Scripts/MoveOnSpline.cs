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

	List<Spline> closeSplines;
	List<Spline> remoteSplines;
	private Spline oldSpline;
	Dictionary<Spline, float> paramOnSplines;

	Vector3 targetMousePos;
	Vector3 mousePosOnDown = Vector3.zero;
	Vector3 mouseDelta = Vector3.zero;
	Vector3 posOnDown;

	float initialSpeed = 0.0f;

	int direction;

	GameObject targetPosObj;
	GameObject targetMousePosObj;

	float sinceLastRemoteCheck;

	// Use this for initialization
	void Start () 
	{

		initialSpeed = speed;

		GameObject[] splineObjects =  GameObject.FindGameObjectsWithTag("Spline");

		Debug.Log ("Number of Splines: " + splineObjects.Length);

		closeSplines = new List<Spline>(splineObjects.Length);
		closeSplines.Add (spline);
		remoteSplines = new List<Spline>(splineObjects.Length);
		
		paramOnSplines = new Dictionary<Spline, float> ();

		// position on initila spline
		paramOnSplines.Add (spline, spline.GetClosestPointParam (transform.position, 3));
		updateTransform ();

		//determin which splines are close and which are far away
		Vector3 pos = spline.GetPositionOnSpline(paramOnSplines[spline]);
		Vector2 posOnScreen = (Vector2)Camera.main.WorldToScreenPoint(pos);
		for (int i = 0; i < splineObjects.Length; i++) 
		{
			Spline s = splineObjects [i].GetComponent<Spline> ();

			if(s.GetInstanceID() == spline.GetInstanceID())
				continue;

			float p = s.GetClosestPointParam(transform.position, 3);
			paramOnSplines.Add(s, p);

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

//				StartCoroutine(checkRemoteSplines());
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
			int currentDirection = (target.x - transform.position.x) > 0 ? 1 : -1;
			if(Vector3.Distance(target, transform.position - new Vector3 (0, transform.localScale.y / 2, 0)) < stopThreshold)
			{
//				paramOnSplines[spline] = spline.GetClosestPointParam(target, 3);
//				param = spline.GetClosestPointParam(target, 3);

//				if(Debug.isDebugBuild)
//					Debug.Log ("at target! pos: " + transform.position + " t.pos: " + target);

				stopMoving();
			}
			//if not, move along on current spline
			else
			{
				paramOnSplines[spline] += (speed * direction * Time.deltaTime) / spline.Length;
			}

			//position and rotate us
			updateTransform();

			//now it's time to find out if we need to switch to one of the other splines

			Vector3 pos = spline.GetPositionOnSpline(paramOnSplines[spline]);
			Vector2 posOnScreen = (Vector2)Camera.main.WorldToScreenPoint(pos);
			Ray rayFromPos = Camera.main.ScreenPointToRay(posOnScreen);
			Debug.DrawRay(rayFromPos.origin, rayFromPos.direction * 150, Color.yellow);

			if(Debug.isDebugBuild)
				spline.transform.Find("CharacterPos").position = pos;

			Waterfall waterfall = spline.GetComponent<Waterfall>();
			float switchThresholdFactor = (waterfall) ? waterfall.switchThresholdFactor : 1;
			
			List<Spline> toRemove =new List<Spline>();

			//first check the remotesplines every other frame
			sinceLastRemoteCheck += Time.deltaTime;
			if(sinceLastRemoteCheck > 0.1666f)
			{
				for (int rI = 0; rI < remoteSplines.Count; rI++)
				{
					Spline s = remoteSplines[rI];
					
					float p = s.GetClosestPointParamToRay(rayFromPos, 5);
					
					Vector2 otherPosOnScreen = (Vector2)Camera.main.WorldToScreenPoint(s.GetPositionOnSpline(p));
					float dis = Vector2.Distance(posOnScreen, otherPosOnScreen );

//					if(Debug.isDebugBuild)
//						Debug.Log("distance to " + printPath(s.transform) + " is " + dis);

					if(dis <= remoteThreshold)
					{
						closeSplines.Add (s);
						toRemove.Add(s);
					}
					
					paramOnSplines[s] = p;
					
					//			i++;
				}
				foreach (Spline rmS in toRemove) 
					remoteSplines.Remove(rmS);

				
				if(toRemove.Count > 0)
				{
					Debug.Log ("LISTS CHANGED! remoteSplines (" + remoteSplines.Count + ")" + " - > closeSplines (" + closeSplines.Count + ")");
					printSplineLists();
				}

				toRemove.Clear();

				sinceLastRemoteCheck = 0.0f;
			}

			// check each spline that is close enough if we need to switch spline
//			int sI = 0;
//			foreach (Spline s in closeSplines) 
			for (int cI = 0; cI < closeSplines.Count; cI++)
			{
				Spline s = closeSplines[cI];

				//don't compare to active spline
				if(s.GetInstanceID() == spline.GetInstanceID())
					continue;
				
				//waterfalls can only be switched to from a specific direction
				//unless we are coming from a waterfall
				Waterfall otherWaterfall = s.GetComponent<Waterfall>();
				if(otherWaterfall != null)
				{
					if(otherWaterfall.direction != direction)
						continue;
				}
				
				float otherP = s.GetClosestPointParamToRay(rayFromPos, 5);
				float pDiff = paramOnSplines[s] - otherP;
				paramOnSplines[s] = otherP;

				//TODO: if pDiff is too high, maybe we should check for positions in between as well
//				if(pDiff > 0.01)
//					Debug.Log ("PDIFF: " + pDiff);

				Vector3 otherPos = s.GetPositionOnSpline(paramOnSplines[s]);

				if(Debug.isDebugBuild)
					s.transform.Find("CharacterPos").position = otherPos;

				Vector2 otherPosOnScreen = (Vector2)Camera.main.WorldToScreenPoint(otherPos);
				float dis = Vector3.Distance(posOnScreen, otherPosOnScreen);

//				float worldDis = Vector3.Distance(pos, otherPos) - Mathf.Abs (spline.transform.position.z - s.transform.position.z);

//				if(dis < 1)
//				{
//					
//					Debug.Log ("distance between " + printPath (spline.transform) + " and " + printPath(s.transform));
//
//					Debug.Log("World Coordinates: " + pos + " vs. " + otherPos + " = " + worldDis);
//					
//					Debug.Log("screen Coordinates: " + posOnScreen + " vs. " + otherPosOnScreen + " = " + dis);
//
//				}

				//are we close enough to switch?
				if(dis < switchThreshold * switchThresholdFactor)
				{

					Vector3 tangent = spline.GetTangentToSpline(paramOnSplines[spline]) * direction;
					Vector3 otherTangent = s.GetTangentToSpline(paramOnSplines[s]) * direction;

//					Debug.Log (printPath(spline.transform) + " & " + printPath (s.transform) + " are crossing! t.y: " + tangent.y + " oT.y: " + otherTangent.y);

					//is the other spline moving above ours?
					if(otherTangent.y >	tangent.y)// || spline.transform.position.z == s.transform.position.z && (oldSpline == null || oldSpline.GetInstanceID() != s.GetInstanceID()) )
					{
						Spline oldSpline = spline;
//						StartCoroutine(forgetOldSpline());

						spline = s;
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
					}
				}
				else if(dis > remoteThreshold)
				{
					toRemove.Add(s);
					remoteSplines.Add (s);
				}
//				sI++;
			}
			
			
			foreach (Spline rmS in toRemove) 
				closeSplines.Remove(rmS);
			
			if(toRemove.Count > 0)
			{
				Debug.Log ("LISTS CHANGED!  closeSplines (" + closeSplines.Count + ") remoteSplines (" + remoteSplines.Count + ")");
				printSplineLists();
			}
			toRemove.Clear ();
		}
	
	}


//	IEnumerator checkRemoteSplines()
//	{
//		yield return new WaitForSeconds (0.16667f);
//
//		Vector3 pos = spline.GetPositionOnSpline(paramOnSplines[spline]);
//		Vector2 posOnScreen = (Vector2)Camera.main.WorldToScreenPoint(pos);
//
////		int i = 0;
////		foreach (Spline s in remoteSplines) 
//		for (int i = 0; i < remoteSplines.Count; i++)
//		{
//			Spline s = remoteSplines[i];
//
//			float p = s.GetClosestPointParam(transform.position, 3);
//
//			Vector2 otherPosOnScreen = (Vector2)Camera.main.WorldToScreenPoint(s.GetPositionOnSpline(p));
//			float dis = Vector2.Distance(posOnScreen, otherPosOnScreen );
//			if(dis <= remoteThreshold)
//			{
//				remoteSplines.RemoveAt(i);
//				closeSplines.Add (s);
//			}
//			
//			paramOnSplines.Add(s, p);
//
////			i++;
//		}
//	}

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
		float p = paramOnSplines [spline];
		transform.position = spline.GetPositionOnSpline (p) + new Vector3 (0, transform.localScale.y / 2, 0);
		transform.rotation = spline.GetOrientationOnSpline (p);
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
//		StopCoroutine (checkRemoteSplines());
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
}
