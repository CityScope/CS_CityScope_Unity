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

	public static Table CreateFromJSON(string jsonString)
	{ // static function that returns Table which holds Class objects 
		return JsonUtility.FromJson<Table>(jsonString);
	}

	/// <summary>
	/// Creates Table object from GridDecoder.
	/// Returns true if there are changes to the grid.
	/// </summary>
	/// <param name="table">Table.</param>
	public static bool CreateFromDecoder(ref Table table, string scannersParentName)
	{
		bool needsUpdate = false;
		int[,] currIds = GameObject.Find (scannersParentName).GetComponent<Scanners> ().GetCurrentIds();
		if (currIds == null)
			return false;

		if (table.grid != null) {
			for (int i = 0; i < currIds.GetLength (0); i++) {
				for (int j = 0; j < currIds.GetLength (1); j++) {
					int currType = table.grid [i * currIds.GetLength (0) + j].type;
					if (currType != currIds [i, j]) {
						table.grid [i * currIds.GetLength (0) + j].type = currIds [i, j];
						needsUpdate = true;
					}
				}
			}
		}
		else {
			needsUpdate = true;
			Debug.Log ("Creating new table grid list");
			table.grid = new List<Grid> ();
			for (int i = 0; i < currIds.GetLength (0); i++) {
				for (int j = 0; j < currIds.GetLength (1); j++) {
					Grid currGrid = new Grid ();
					currGrid.type = currIds [i, j];
					currGrid.x = i;
					currGrid.y = j;
					currGrid.rot = 180;
					table.grid.Add (currGrid);
					currGrid = null;
				}
			}
		}

		return needsUpdate;
	}
}
