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
		if (currView == (int)Camera.GRID_UI) {
			cameras[(int)Camera.GRID_UI].GetComponent<UnityEngine.Camera>().enabled = true;
		} else {
			cameras[(int)Camera.GRID_UI].GetComponent<UnityEngine.Camera>().enabled = false;
		}
	}


}
