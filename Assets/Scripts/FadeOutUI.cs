using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeOutUI : MonoBehaviour 
{
	public float fadeDuration = 1;

	protected Vector3 mousePosOnDown = Vector3.zero;
	protected Vector3 mouseDelta = Vector3.zero;

	protected Graphic graphic;
	protected bool faded;

	// Use this for initialization
	void Start () {
		graphic = GetComponent<Graphic> ();
		
		if (graphic == null)
			enabled = false;
	}
	
	// Update is called once per frame
	protected void Update () 
	{
		if (faded)
			return;

		if (Input.GetMouseButtonDown (0))
		{
			mousePosOnDown = normalizeMousePos(Input.mousePosition);
			mouseDelta = Vector3.zero;
		}
		
		if (Input.GetMouseButton (0)) 
		{
			mouseDelta = normalizeMousePos (Input.mousePosition) - mousePosOnDown;
		} 
	}
	
	protected IEnumerator fadeOut()
	{
		faded = true;

		Color color = graphic.color;
		
		float startTime = 0.0f;
		
		while (startTime < fadeDuration) 
		{
			startTime += Time.deltaTime;
			
			float newAlpha = Mathf.Lerp(1, 0, startTime/fadeDuration);
			color.a = newAlpha;
			
			graphic.color = color;
			
			yield return null;
		}

		enabled = false;
	}
	
	private Vector3 normalizeMousePos(Vector3 mousePos)
	{
		Vector3 adjustedMousePos = mousePos;
		
		//normalize it to current screen resolution
		adjustedMousePos.x /= Screen.width;
		adjustedMousePos.y /= Screen.height;
		
		return adjustedMousePos;
	}
}
