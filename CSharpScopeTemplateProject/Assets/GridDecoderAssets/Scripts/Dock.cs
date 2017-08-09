using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dock : LegoUI {
	private GameObject[,] dockScanners;

	private int dockId;

	/// <summary>
	/// Creates the dock scanner.
	/// </summary>
	public Dock(GameObject parentObject, int gridSize, float _scannerScale) {
		CreateScannerParent ("Dock parent", parentObject);

		dockScanners = new GameObject[gridSize, gridSize];

		for (int x = 0; x < gridSize; x++) {
			for (int y = 0; y < gridSize; y++) {
				dockScanners[x, y] = GameObject.CreatePrimitive (PrimitiveType.Quad);
				dockScanners[x, y].name = "dock_" + y + x;
				dockScanners[x, y].transform.parent = this.uiParent.transform;
				dockScanners[x, y].transform.localScale = new Vector3 (_scannerScale, _scannerScale, _scannerScale);  
				dockScanners[x, y].transform.localPosition = new Vector3 (x * _scannerScale * 2, 0.2f, y * _scannerScale * 2);
				dockScanners[x, y].transform.Rotate (90, 0, 0); 
			}
		}
	}

	/// <summary>
	/// Updates the dock ID scanner.
	/// </summary>
	public void UpdateDock() {
		string key = "";
		int currDockId = GameObject.Find ("ScannersParent").GetComponent<Scanners> ().FindCurrentId (key, 0, 0, ref dockScanners, false);

		// Notify CityIO
		if (dockId != currDockId) {
			dockId = currDockId;
			EventManager.TriggerEvent ("dockChange");
			Debug.Log ("Dock ID changed to " + this.dockId);
		}
	}

	public Vector3 GetDockPosition() {
		return this.uiParent.transform.localPosition;
	}

	public int GetDockId() {
		return dockId;
	}

	public void SetDockPosition(Vector3 position) {
		this.uiParent.transform.localPosition = position;
	}
}
