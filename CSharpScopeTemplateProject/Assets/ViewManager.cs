using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewManager : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnViewChanged(int currView) {
		if (currView == 0) {
			GameObject.Find ("Grid_UI_Camera").GetComponent<Camera> ().enabled = true;
		} else {
			GameObject.Find ("Grid_UI_Camera").GetComponent<Camera> ().enabled = false;
		}
	}


}
