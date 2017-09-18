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

	private bool needsUpdate;


	private Table() {
		this.objects = new Objects ();
		this.needsUpdate = true;

		this.SetupObjects ();
	}

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

	public bool NeedsUpdate() {
		return this.needsUpdate;
	}

	public void CreateFromJSON(string jsonString)
	{ // static function that returns Table which holds Class objects 
		_instance = JsonUtility.FromJson<Table>(jsonString);
	}

	/// <summary>
	/// Creates the grid with data passed from the Scanners class.
	/// </summary>
	/// <returns><c>true</c>, if grid was created, <c>false</c> otherwise.</returns>
	/// <param name="table">Table.</param>
	/// <param name="scannersParentName">Scanners parent name.</param>
	public void CreateGrid(ref int[,] currIds) {
		if (currIds == null)
			return;

		if (this.grid != null) {
			needsUpdate = false;
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
	}

	public void PrintGrid() {
		if (this.grid != null) {
			string ids = "";
			for (int i = 0; i < this.grid.Count; i++) {
				ids += this.grid [i].type;
			}
			Debug.Log (ids);
		}
	}

	public int GetType(int index) {
		return this.grid [index].type;
	}
		

	public void UpdateDock(int newDockId) {
		if (newDockId != this.objects.dockID) {
			this.objects.SetDockId (newDockId);
			needsUpdate = true;
		}
	}

	public void UpdateSlider(int newSliderVal) {
		if (newSliderVal != this.objects.slider1) {
			this.objects.SetSlider (newSliderVal);
			needsUpdate = true;
		}
	}

	private void SetupObjects() {
		// Initialize with random densities
		this.objects.density = new List<int>();
		int buildingTypesCount = GameObject.Find ("CityScope").GetComponent<CityScopeVis> ().GetBuildingTypeCount ();

		for (int i = 0; i < buildingTypesCount; i++)
			this.objects.density.Add((int)(UnityEngine.Random.Range(0f, 20f)));

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
