using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegoUI {
	public GameObject uiParent;

	public void CreateScannerParent(string name, GameObject parent) {
		uiParent = new GameObject ();
		uiParent.transform.parent = parent.transform;
		uiParent.transform.localPosition = new Vector3 (0, 0, 0);
		uiParent.name = name;
	}
}
