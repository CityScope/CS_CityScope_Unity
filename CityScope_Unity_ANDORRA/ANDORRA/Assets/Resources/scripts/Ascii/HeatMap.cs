using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeatMapItem {
	public int x;
	public int y;
	public float z;
	public int type;
	public GameObject geo;
	public float score;
}

public class HeatMap {
	GameObject heatmapParent; 
	HeatMapItem[] heatmapItems;
	private GameObject currentHeatmapParent;
	public GameObject title;
	Text titleText;

	List<Brick> searchTypes;
	List<Brick> originTypes;

	int gridX;
	int gridY;
	float cellSize;
	float cellShrink;
	float yOffset;
	int searchDim;

	public List<Vector2> vertices;

	List<int> updateIndices;

	// Store types in 2d matrix for quick lookup
	int[,] _typesArray;

	float maxScore;

	float gradientScale = 0.4f;

	public HeatMap(int sizeX, int sizeY, int searchDimension, float _cellSize, float _cellShrink, float _addToYHeight, string name) {
		this.heatmapItems = new HeatMapItem[sizeX * sizeY];
		this.gridX = sizeX;
		this.gridY = sizeY;
		this.searchDim = searchDimension;

		this.updateIndices = new List<int> ();
		this._typesArray = new int[sizeX,sizeY];

		this.cellSize = _cellSize;
		this.cellShrink = _cellShrink;
		this.yOffset = _addToYHeight;

		maxScore = -2f;

		this.currentHeatmapParent = new GameObject ();
		this.currentHeatmapParent.SetActive (false);
		this.currentHeatmapParent.name = name;

		vertices = new List<Vector2> ();

	}

	public void CreateTitle(string titleString) {
		GameObject canvas = GameObject.Find ("HeatmapTitles");
		this.title = new GameObject ();
		title.transform.parent = canvas.transform;
		titleText = title.AddComponent<Text> ();
		titleText.text = titleString;
		title.name = currentHeatmapParent.name + "_name";
		this.title.transform.localScale = new Vector3(1, 1, 1);
		this.title.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 50);

		Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
		titleText.font = ArialFont;
		titleText.fontSize = 30;
		titleText.fontStyle = FontStyle.Bold;
		//titleText.resizeTextForBestFit = true;
		titleText.resizeTextMinSize = 1;
		titleText.horizontalOverflow = HorizontalWrapMode.Overflow;
		titleText.SetNativeSize ();

		this.title.SetActive (false);

	}

	public void SetSearchTypes(List<Brick> searched) {
		this.searchTypes = searched;
	}

	public void SetOriginTypes(List<Brick> origins) {
		this.originTypes = origins;
	}

	public void SetParent(GameObject parentObject) {
		this.heatmapParent = parentObject;
		this.currentHeatmapParent.transform.parent = heatmapParent.transform;
	}

	public GameObject GetParentObject() {
		return currentHeatmapParent;
	}

	public void SetParentActive(bool isActive) {
		this.currentHeatmapParent.SetActive (isActive);
		this.title.SetActive (isActive);
	}

	public void CreateHeatmapGeo(int x, int y, int index, int type, float meshYPosition) {
		updateIndices.Add (index);
		heatmapItems[index] = new HeatMapItem ();

		heatmapItems [index].x = x;
		heatmapItems [index].y = y;
		heatmapItems [index].z = meshYPosition;
		heatmapItems [index].type = type;
		heatmapItems [index].score = -2f;

		heatmapItems[index].geo = GameObject.CreatePrimitive(PrimitiveType.Quad); //make cell cube
		heatmapItems[index].geo.name = (currentHeatmapParent.name + " Type: " + type);

		heatmapItems[index].geo.transform.localPosition = new Vector3(x * cellSize, meshYPosition + yOffset, y * cellSize);
		Quaternion _tmpRot = heatmapParent.transform.localRotation;
		_tmpRot.eulerAngles = new Vector3(90, 0, 0.0f);
		heatmapItems[index].geo.transform.localRotation = _tmpRot;

		vertices.Add (new Vector2(heatmapItems [index].geo.transform.localPosition.x, heatmapItems [index].geo.transform.localPosition.z));

		heatmapItems[index].geo.transform.localScale = new Vector3(cellSize * cellShrink, cellSize * cellShrink, cellSize * cellShrink);
		heatmapItems[index].geo.transform.GetComponent<Renderer>().receiveShadows = false;
		heatmapItems[index].geo.transform.GetComponent<Renderer>().shadowCastingMode =
			UnityEngine.Rendering.ShadowCastingMode.Off;
		heatmapItems[index].geo.transform.parent = currentHeatmapParent.transform; //put into parent object for later control
	}

	// Update types separately from score compute loop!
	public void UpdateType(int x, int y, int type, int index) {
		if (heatmapItems != null && heatmapItems[index] != null) {
			if (_typesArray != null)
				_typesArray[x, y] = type;
			heatmapItems [index].type = type;
			updateIndices.Add (index);
		}
	}

	/// <summary>
	/// Searches the neighbors // brute force
	/// </summary>
	private void UpdateHeatmapItem(int x, int y, int type, int index) 
	{
		int newScore = -1;
		if (heatmapItems [index] == null)
			return;

		// for the brick at (x, y), 
		// check if it's one of the origin types that needs a score
		// if so, check if its neighbors are in the search type array & increment score if so
		if (this.originTypes.Contains((Brick)heatmapItems [index].type)) {
			ComputeScore (x, y, ref newScore);
			if (newScore > maxScore)
				maxScore = newScore;
		}

		heatmapItems[index].score = newScore;
	}

	// Applies remapped score to current object & changes its color according to it
	private void ApplyScore(int index) {
		if (heatmapItems [index] == null)
			return;
		
		if (heatmapItems[index].score >= 0) {
			heatmapItems [index].geo.transform.localPosition =
				new Vector3(heatmapItems[index].x * cellSize, heatmapItems[index].z + yOffset + (heatmapItems[index].score * 2), heatmapItems[index].y * cellSize);
			heatmapItems [index].geo.name = ("Results count: " + heatmapItems[index].score.ToString());
			var _tmpColor = heatmapItems[index].score * gradientScale; // color color spectrum based on cell score/max potential score 
			heatmapItems [index].geo.transform.GetComponent<Renderer> ().material.color =
				Color.HSVToRGB (_tmpColor, 1, 1);
			this.heatmapItems [index].geo.SetActive (true);
		}
		else
		{
			this.heatmapItems [index].geo.SetActive (false);
			//heatmapItems[index].geo.transform.GetComponent<Renderer>().material.color = Color.HSVToRGB(0, 0, 0);
			//heatmapItems[index].geo.transform.localScale = new Vector3(cellSize * 0.9f, cellSize * 0.9f, cellSize * 0.9f);
		}
	}

	public void UpdateHeatmap() {
		int index = 0;
		if (updateIndices.Count >= 1 && false) {
			for (int i = 0; i < updateIndices.Count; i++) {
				for (int x = heatmapItems[updateIndices[i]].x; x < gridX; x++) {
					for (int y = 0; y < gridY; y++) {
						UpdateHeatmapItem (x, y, heatmapItems[updateIndices[i]].type, updateIndices[i]);
					}
				}
			}
		}
		else {
			for (int x = 0; x < gridX; x++) {
				for (int y = 0; y < gridY; y++) {
					if (this.originTypes.Contains((Brick)_typesArray[x, y]))
						UpdateHeatmapItem (x, y, _typesArray [x, y], index);
					index++;
				}
			}
		}


		for (int i = 0; i < heatmapItems.Length; i++) {
			if (heatmapItems[i] != null && heatmapItems[i].score >= 0 )
				heatmapItems[i].score = Remap (heatmapItems[i].score, 0, maxScore, 0, 1f);
			ApplyScore (i);
		}
	}

	//
	// From https://forum.unity3d.com/threads/re-map-a-number-from-one-range-to-another.119437/
	// Remap
	private float Remap (float value, float from1, float to1, float from2, float to2) {
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}

	/// <summary>
	/// Computes the score within the current search window 
	/// by checking each other object if they belong in the search list
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="_cellScoreCount">Cell score count.</param>
	private void ComputeScore(int x, int y, ref int _cellScoreCount) {
		for (int _windowX = x - searchDim; _windowX < x + searchDim; _windowX++)
		{
			for (int _windowY = y - searchDim; _windowY < y + searchDim; _windowY++)
			{
				if (_windowX > 0 && _windowY > 0 && _windowX < gridX && _windowY < gridY)
				{ // make sure window area is not outside grid bounds 
					if (this.searchTypes.Contains((Brick)_typesArray[_windowX, _windowY]))
					{
						_cellScoreCount++;
					}
				}
			}
		}
	}
}
