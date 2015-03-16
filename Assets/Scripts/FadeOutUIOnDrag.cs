using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeOutUIOnDrag : FadeOutUI 
{

	// Update is called once per frame
	void Update () 
	{
		base.Update ();

		if (faded)
			return;
		
		if (Input.GetMouseButton (0)) 
		{
			if(mouseDelta.magnitude > 0.1)
			{
				StartCoroutine(fadeOut());
			}
		}
	}
}
