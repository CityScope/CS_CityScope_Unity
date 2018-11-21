using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]// have to have this in every JSON class!
public class Table
{
	public List<Grid> grid;
	public Objects objects;
	public string id;
	public long timestamp;

	public Table() {
		this.objects = new Objects ();
	}

	public static Table CreateFromJSON(string jsonString)
	{ // static function that returns Table which holds Class objects 
		return JsonUtility.FromJson<Table>(jsonString);
	}

	/// <summary>
	/// Creates Table object from GridDecoder.
	/// Returns true if there are changes to the grid.
	/// </summary>
	/// <param name="table">Table.</param>
	public bool CreateGridFromDecoder(string scannersParentName)
	{
		bool needsUpdate = false;
		CreateGrid(scannersParentName, ref needsUpdate);
		return needsUpdate;
	}

	public void UpdateObjectsFromDecoder(string scannersParentName)
	{
		UpdateObjects (scannersParentName);
	}

	/// <summary>
	/// Creates the grid with data passed from the Scanners class.
	/// </summary>
	/// <returns><c>true</c>, if grid was created, <c>false</c> otherwise.</returns>
	/// <param name="table">Table.</param>
	/// <param name="scannersParentName">Scanners parent name.</param>
	private bool CreateGrid(string scannersParentName, ref bool needsUpdate) {
		int[,] currIds = GameObject.Find (scannersParentName).GetComponent<Scanners> ().GetCurrentIds();
		if (currIds == null)
			return false;

		if (this.grid != null) {
			for (int i = 0; i < currIds.GetLength (0); i++) {
				for (int j = 0; j < currIds.GetLength (1); j++) {
					int currType = this.grid [i * currIds.GetLength (1) + j].type;

					if (currType != currIds [i, j]) {
						this.grid [i * currIds.GetLength (1) + j].type = currIds [i, j];
						needsUpdate = true;
						this.grid [i * currIds.GetLength (1) + j].SetUpdate(true);
					}
					else
						this.grid [i * currIds.GetLength (1) + j].SetUpdate(false);
				}
			}
		}
		else {
			needsUpdate = true;
			Debug.Log ("Creating new table grid list");
			CreateObjects (scannersParentName, ref needsUpdate);

			this.grid = new List<Grid> ();
			for (int i = 0; i < currIds.GetLength (0); i++) {
				for (int j = 0; j < currIds.GetLength (1); j++) {
					Grid currGrid = new Grid ();
					currGrid.type = currIds [i, j];
					currGrid.x = j;
					currGrid.y = i;
					currGrid.rot = 180;
					currGrid.SetUpdate(true);
					this.grid.Add (currGrid);
					currGrid = null;
				}
			}
		}
		return needsUpdate;
	}

	/// <summary>
	/// Populates the Table class' Objects with dock ID, slider values, etc from the Scanners class.
	/// </summary>
	/// <returns><c>true</c>, if objects was created, <c>false</c> otherwise.</returns>
	/// <param name="table">Table.</param>
	/// <param name="scannerParentName">Scanner parent name.</param>
	/// <param name="needsUpdate">Needs update.</param>
	private bool CreateObjects(string scannersParentName, ref bool needsUpdate) {
		if (this.objects.density != null) {
			UpdateDock (scannersParentName);
		}
		else {
			SetupObjects (scannersParentName);
			needsUpdate = true;
		}
		return true;
	}

	public void UpdateObjects(string scannersParentName) {
		UpdateDock (scannersParentName);
		UpdateSlider (scannersParentName);
	}

	private void UpdateDock(string scannersParentName) {
		if (!GameObject.Find (scannersParentName))
			return;
		
		int newDockId = GameObject.Find (scannersParentName).GetComponent<Scanners> ().GetDockId ();
		if (newDockId != this.objects.dockID) {
			this.objects.SetDockId (newDockId);
		}
	}

	private void UpdateSlider(string scannersParentName) {
		if (!GameObject.Find (scannersParentName))
			return;
		
		int newSliderVal = GameObject.Find (scannersParentName).GetComponent<Scanners> ().GetSliderValue();
		if (newSliderVal != this.objects.slider1) {
			this.objects.SetSlider (newSliderVal);
		}
	}

	private void SetupObjects(string scannersParentName) {
		if (!GameObject.Find (scannersParentName))
			return;
		
		// Initialize with random densities
		this.objects.density = new List<int>();
		int buildingTypesCount = GameObject.Find ("cityIO").GetComponent<cityIO> ().GetBuildingTypeCount ();

		for (int i = 0; i < buildingTypesCount; i++)
			this.objects.density.Add((int)(UnityEngine.Random.Range(0f, 20f)));

		this.objects.SetDockId (GameObject.Find (scannersParentName).GetComponent<Scanners> ().GetDockId());
		this.objects.SetSlider (GameObject.Find (scannersParentName).GetComponent<Scanners> ().GetSliderValue());
	}

	public string WriteToJSON()
	{
		return JsonUtility.ToJson (this);
	}
}
