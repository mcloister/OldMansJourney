using UnityEngine;
using UnityDebugger;
using UnityEngine.EventSystems;
using System.Collections;

public class UpdateMeshOnPointerDown : MonoBehaviour {

	private SplineMesh mesh;

	// Use this for initialization
	void Start () {
		mesh = GetComponent<SplineMesh> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (!mesh)
			return;

		if (Input.GetMouseButtonDown (0))
			updateMesh ();

		Touch[] myTouches = Input.touches;
		foreach (Touch t in myTouches) 
		{
			if(t.phase == TouchPhase.Began) 
			{
				updateMesh();
				break;
			}
		}
	}

	void updateMesh()
	{
		if (mesh)
		{
			Debugger.Log ("updating mesh of " + transform.parent.name);
			mesh.UpdateMesh();
			mesh.UpdateMesh();	//somehow the MeshCollider needs two calls to UpdateMesh to update itself...strange but whatever
		}

	}
}
