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

	Spline[] allSplines;
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

	// Use this for initialization
	void Start () 
	{

		initialSpeed = speed;

		GameObject[] splineObjects =  GameObject.FindGameObjectsWithTag("Spline");

		allSplines = new Spline[splineObjects.Length];
		
		paramOnSplines = new Dictionary<Spline, float> ();

		for (int i = 0; i < splineObjects.Length; i++) 
		{
			allSplines [i] = splineObjects [i].GetComponent<Spline> ();
			float p = allSplines[i].GetClosestPointParam(transform.position, 3);
			paramOnSplines.Add(allSplines[i], p);
		}

		updateTransform ();

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
			if(currentDirection != direction)
			{
//				paramOnSplines[spline] = spline.GetClosestPointParam(target, 3);
				param = spline.GetClosestPointParam(target, 3);

				direction = 0;	//stop moving
			}
			//if not move along on current spline
			else
			{
				paramOnSplines[spline] += (speed * direction * Time.deltaTime) / spline.Length;
			}

			//position and rotate us
			updateTransform();

			//now it's time to find out if we need to switch to one of the other splines

			Vector3 pos = spline.GetPositionOnSpline(paramOnSplines[spline]);
			Vector2 posOnScreen = (Vector2)Camera.main.WorldToScreenPoint(pos);

			if(Debug.isDebugBuild)
				spline.transform.Find("CharacterPos").position = pos;

			Waterfall waterfall = spline.GetComponent<Waterfall>();
			float switchThresholdFactor = (waterfall) ? waterfall.switchThresholdFactor : 1;

			// check each other spline if we need to switch spline
			foreach (Spline s in allSplines) 
			{
				//don't compare to active spline
				if(s.GetInstanceID() == spline.GetInstanceID())
					continue;

				float otherP = s.GetClosestPointParamToRay(Camera.main.ScreenPointToRay(posOnScreen), 5);
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
					//waterfalls can only be switched to from a specific direction
					//unless we are coming from a waterfall
					Waterfall otherWaterfall = s.GetComponent<Waterfall>();
					if(otherWaterfall != null && waterfall == null)
					{
						if(otherWaterfall.direction != direction)
							continue;
					}

					Vector3 tangent = spline.GetTangentToSpline(paramOnSplines[spline]) * direction;
					Vector3 otherTangent = s.GetTangentToSpline(paramOnSplines[s]) * direction;

					Debug.Log (printPath(spline.transform) + " & " + printPath (s.transform) + " are crossing! t.y: " + tangent.y + " oT.y: " + otherTangent.y);

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

							Vector3 screenBottom = Camera.main.ScreenToWorldPoint(new Vector3(0,0, -Camera.main.transform.position.z + transform.position.z));

							setTarget(new Vector3(transform.position.x, screenBottom.y, transform.position.z));
						}
						//switching to a nomral spline
						else
						{
							speed = initialSpeed;

							if(waterfall != null)
								direction = 0;			//after a waterfall stop where we currently are
							else
								setTarget (targetMousePos + new Vector3(0,0, spline.transform.position.z - oldSpline.transform.position.z));		//recalculate target on new spline
						}
					}
				}


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
		float p = paramOnSplines [spline];
		transform.position = spline.GetPositionOnSpline (p) + new Vector3 (0, transform.localScale.z / 2, 0);
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
}
