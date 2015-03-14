using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FPSDisplay : MonoBehaviour {

	int frameCount = 0;
	float dt = 0.0f;
	float fps = 0.0f;
	float updateRate = 4.0f;  // 4 updates per sec.

	Text text;
	string initialString;
	void Start()
	{
		text = GetComponent<Text>();
		initialString = text.text;
	}

	void Update()
	{
		frameCount++;
		dt += Time.deltaTime;
		if (dt > 1.0f/updateRate)
		{
			fps = frameCount / dt ;
			frameCount = 0;
			dt -= 1.0f/updateRate;

			if(text != null)
				text.text = string.Format (initialString, fps);
		}
	}
}
