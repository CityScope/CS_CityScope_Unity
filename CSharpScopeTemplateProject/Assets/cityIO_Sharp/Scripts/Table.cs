using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]// have to have this in every JSON class!
public class Table
{
	private static Table _instance;

	public List<Grid> grid;
	public Objects objects;
	public string id;
	public long timestamp;

	public static Table Instance {
		get {
			if (applicationIsQuitting) {
				Debug.LogWarning("[Singleton] Instance '"+ typeof(Table) +
					"' already destroyed on application quit." +
					" Won't create again - returning null.");
				return null;
			}

			if (_instance == null) {
				_instance = new Table ();
			}

			return _instance;
		}
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
		Scanners scanners = GameObject.Find (scannersParentName).GetComponent<Scanners> ();
		CreateGrid(ref scanners, ref needsUpdate);
		return needsUpdate;
	}

	public void UpdateObjectsFromDecoder(string scannersParentName)
	{
		Scanners scanners = GameObject.Find (scannersParentName).GetComponent<Scanners> ();
		UpdateObjects (ref scanners);
	}

	/// <summary>
	/// Creates the grid with data passed from the Scanners class.
	/// </summary>
	/// <returns><c>true</c>, if grid was created, <c>false</c> otherwise.</returns>
	/// <param name="table">Table.</param>
	/// <param name="scannersParentName">Scanners parent name.</param>
	private bool CreateGrid(ref Scanners scanners, ref bool needsUpdate) {
		int[,] currIds = scanners.GetCurrentIds();
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
			CreateObjects (ref scanners, ref needsUpdate);

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
	private bool CreateObjects(ref Scanners scanners, ref bool needsUpdate) {
		if (this.objects.density != null) {
			UpdateDock (ref scanners);
		}
		else {
			SetupObjects (ref scanners);
			needsUpdate = true;
		}
		return true;
	}

	public void UpdateObjects(ref Scanners scanners) {
		UpdateDock (ref scanners);
		UpdateSlider (ref scanners);
	}

	private void UpdateDock(ref Scanners scanners) {
		int newDockId = scanners.GetDockId ();
		if (newDockId != this.objects.dockID) {
			this.objects.SetDockId (newDockId);
		}
	}

	private void UpdateSlider(ref Scanners scanners) {
		int newSliderVal = scanners.GetSliderValue();
		if (newSliderVal != this.objects.slider1) {
			this.objects.SetSlider (newSliderVal);
		}
	}

	private void SetupObjects(ref Scanners scanners) {
		// Initialize with random densities
		this.objects.density = new List<int>();
		int buildingTypesCount = GameObject.Find ("CityScope").GetComponent<CityScopeVis> ().GetBuildingTypeCount ();

		for (int i = 0; i < buildingTypesCount; i++)
			this.objects.density.Add((int)(UnityEngine.Random.Range(0f, 20f)));

		this.objects.SetDockId (scanners.GetDockId());
		this.objects.SetSlider (scanners.GetSliderValue());
	}

	public string WriteToJSON()
	{
		return JsonUtility.ToJson (this);
	}

	private static bool applicationIsQuitting = false;
	/// <summary>
	/// When Unity quits, it destroys objects in a random order.
	/// In principle, a Singleton is only destroyed when application quits.
	/// If any script calls Instance after it have been destroyed, 
	///   it will create a buggy ghost object that will stay on the Editor scene
	///   even after stopping playing the Application. Really bad!
	/// So, this was made to be sure we're not creating that buggy ghost object.
	/// </summary>
	/// 
	public void OnDestroy () {
		applicationIsQuitting = true;
	}
}
