using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewManager : MonoBehaviour {

	public enum Camera
	{
		GRID_UI = 0,
		COLOR_3D = 1,
		PROJECTION = 2,
		CITYIO = 3,
	};

	public GameObject[] cameras;

	public void OnViewChanged(int currView) {
		cameras[currView].GetComponent<UnityEngine.Camera>().enabled = true;

		for (int i = 0; i < cameras.Length; i++) {
			if (i != currView && cameras[i] != null)
				cameras[i].GetComponent<UnityEngine.Camera>().enabled = false;
		}
	}


}
