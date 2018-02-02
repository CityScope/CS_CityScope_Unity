using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class CityScopeVis : MonoBehaviour {

	private Color _tmpColor;
	private float height;
	private float yPos;
	private Vector3 gridObjectPosition;
	private Vector3 gridObjectScale;
	///<summary>
	/// the grid basic unit cubes
	/// </summary>
	private GameObject[] _gridObjects;

	/// <summary> 
	/// parent of grid GOs
	/// </summary>
	public GameObject _gridHolder;
	/// <summary> 
	/// text mesh for type display 
	/// </summary>
	public GameObject textMeshPrefab;
	private GameObject[] textMeshes;
	public GameObject _textMeshParent;

	[Header("CityIO settings")]
	///<summary>
	/// real world size of cells in Meters 
	/// </summary>
	public float _cellSizeInMeters;
	///<summary>
	/// how much to reduce cell size when displying 
	/// </summary>
	public float cellShrink;
	///<summary>
	/// height of single floor
	/// </summary>
	public float _floorHeight;

	///<summary>
	/// default base material for grid GOs
	/// </summary>
	public Material _material;

	private float textOffset = 20f;

	/// <summary>
	/// list of types gridColors
	/// </summary>
	public Color[] gridColors = new Color[6];


	private int[] notBuildingTypes = new int[] { (int)Brick.INVALID, (int)Brick.MASK, (int)Brick.ROAD, (int)Brick.PARK, (int)Brick.AMENITIES, (int)Brick.STREET };
	private int[] buildingTypes = new int[] { (int)Brick.RL, (int)Brick.RM, (int)Brick.RS, (int)Brick.RL, (int)Brick.OL, (int)Brick.OM, (int)Brick.OS };

	void Awake() {
		_tmpColor = Color.black;
		height = 0f;
		yPos = 0f;

		EventManager.StartListening ("updateData", OnUpdateData);
	}

	/// <summary>
	/// Creates array of gridobjects 
	/// </summary>
	private void SetupTable() {
		if (_gridHolder == null)
			_gridHolder = new GameObject ();
		
		_gridObjects = new GameObject[Table.Instance.grid.Count];

		for (int i = 0; i < Table.Instance.grid.Count; i++) // loop through list of all cells grid objects 
			CreateGridObject(i);
	}


	/// <summary>
	/// Creates the grid object.
	/// </summary>
	/// <param name="i">The index.</param>
	private void CreateGridObject(int i) {
		/* make the grid cells in generic form */
		_gridObjects[i] = GameObject.CreatePrimitive(PrimitiveType.Cube); //make cell cube 
		
		_gridObjects[i].transform.parent = _gridHolder.transform; //put into parent object for later control

		gridObjectPosition = new Vector3((Table.Instance.grid[i].x * _cellSizeInMeters), 0, (Table.Instance.grid[i].y * _cellSizeInMeters));
		gridObjectScale = new Vector3(cellShrink * _cellSizeInMeters, 0, cellShrink * _cellSizeInMeters);

		// Objects properties 
		_gridObjects[i].GetComponent<Renderer>().material = _material;
		_gridObjects[i].transform.localPosition = gridObjectPosition; //compensate for scale shift due to height
		_gridObjects[i].transform.localScale = gridObjectScale;
		_gridObjects [i].SetActive (false);

		//AddBuildingTypeText (i, textOffset);
	}

	private void SetGridObject(int i) {
		// compensate for scale shift and x,y array
		gridObjectPosition = new Vector3((Table.Instance.grid[i].x * _cellSizeInMeters), yPos, (Table.Instance.grid[i].y * _cellSizeInMeters));
		gridObjectScale = new Vector3(cellShrink * _cellSizeInMeters, height, cellShrink * _cellSizeInMeters);

		_gridObjects[i].transform.localPosition = gridObjectPosition;
		_gridObjects[i].transform.localScale = gridObjectScale; // go through all 'densities' to match Type to Height
		_gridObjects[i].GetComponent<Renderer>().material.color = _tmpColor;
	}

	public bool isBuilding(int type) {
		return buildingTypes.Contains (type);
	}

	/// <summary>
	/// Updates the grid object's height and scale given the current type.
	/// </summary>
	/// <param name="i">The index.</param>
	private void UpdateGridObject(int i) {
		if (buildingTypes.Contains(Table.Instance.grid[i].type)) //if this cell is one of the buildings types
		{
			height = _gridObjects[i].transform.position.y + (Table.Instance.objects.density[Table.Instance.grid[i].type] * _floorHeight); 
			yPos = Table.Instance.objects.density[Table.Instance.grid[i].type] * _floorHeight * 0.5f;

			int type = Table.Instance.GetType (i);
			_tmpColor = gridColors[type];
			_tmpColor.a = 0.8f;

			SetGridObject (i);
		}
		else if (notBuildingTypes.Contains(Table.Instance.grid[i].type))
		{
			_tmpColor = Color.white;
			_tmpColor.a = 1f;

			if (Table.Instance.grid[i].type == (int)Brick.ROAD)
			{
				yPos = 0;
				height = 0.25f;
			}
			else if (Table.Instance.grid[i].type == (int)Brick.AMENITIES) 
			{
				yPos = 0f;
				height = 0.25f;
			}
			else if (Table.Instance.grid[i].type == (int)Brick.STREET)
			{
				yPos = 0f;
				height = 1f;
			}
			else //if other non building type
			{
				yPos = 0f;
				height = 0.85f;

				_gridObjects[i].transform.localScale = new Vector3
					(cellShrink * _cellSizeInMeters * 0.85f, 0.85f, cellShrink * _cellSizeInMeters * 0.85f);
			}
			SetGridObject (i);
		}
		NameGridObject (i);
		UpdateBuildingTypeText (i, true);

		if (!_gridObjects[i].activeSelf)
			_gridObjects [i].SetActive (true);
	}

	/// <summary>
	/// Updates the table if the given grid object changed or if the slider/ dock changed
	/// </summary>
	private void UpdateTable() {
		for (int i = 0; i < Table.Instance.grid.Count; i++) { // loop through list of all cells grid objects 
			if ((Table.Instance.grid[i].ShouldUpdate()) || Table.Instance.NeedsUpdate())
				UpdateGridObject(i);
		}
	}


	public void DrawTable()
	{
		if (_gridObjects == null)
			SetupTable ();

		UpdateTable ();
	}

	private void NameGridObject(int i) {
		if (Table.Instance.grid[i].type > (int)Brick.INVALID && Table.Instance.grid[i].type < (int)Brick.ROAD) //if object has is building with Z height
		{
			_gridObjects[i].name =
				("Type: " + Table.Instance.grid[i].type + " X: " +
					Table.Instance.grid[i].x.ToString() + " Y: " +
					Table.Instance.grid[i].y.ToString() +
					" Height: " + (Table.Instance.objects.density[Table.Instance.grid[i].type]).ToString());
		}
		else // if object is flat by nature
		{
			_gridObjects[i].name =
				("Type w/o height: " + Table.Instance.grid[i].type + " X: " +
					Table.Instance.grid[i].x.ToString() + " Y: " +
					Table.Instance.grid[i].y.ToString());
		}
	}

	private void AddBuildingTypeText(int i, float height) //mesh type text method 
	{
		if (textMeshes == null) 
			textMeshes = new GameObject[Table.Instance.grid.Count];

		textMeshes[i] = GameObject.Instantiate(textMeshPrefab, new Vector3((_gridObjects[i].transform.position.x),
			height + _gridObjects[i].transform.localScale.y, _gridObjects[i].transform.position.z),
			_gridObjects[i].transform.rotation, transform) as GameObject; //spwan prefab text
		textMeshes[i].transform.parent = _textMeshParent.transform;

		Quaternion _tmpRot = textMeshes [i].transform.localRotation;
		_tmpRot.eulerAngles = new Vector3(90, 0, 0.0f);
		textMeshes [i].transform.localRotation = _tmpRot;

		textMeshes[i].GetComponent<TextMesh>().text = Table.Instance.grid[i].type.ToString();
		textMeshes[i].GetComponent<TextMesh>().fontSize = 10; // 
		textMeshes[i].GetComponent<TextMesh>().color = Color.gray;
		textMeshes[i].GetComponent<TextMesh>().characterSize = 0.25f;

		textMeshes [i].SetActive (false);
	}

	private void UpdateBuildingTypeText(int i, bool enabled) {
		if (textMeshes == null)
			return;
		
		if (enabled && Table.Instance.grid [i].type != (int)Brick.INVALID)
			textMeshes [i].SetActive (true);
		else
			textMeshes [i].SetActive (false); 
		textMeshes [i].GetComponent<TextMesh> ().text = Enum.GetName (typeof(Brick), (Table.Instance.grid [i].type));
		float xPos = textMeshes [i].transform.localPosition.x;
		float zPos = textMeshes [i].transform.localPosition.z;
		textMeshes[i].transform.localPosition = new Vector3(xPos, _gridObjects[i].transform.localPosition.y + _gridObjects[i].transform.localScale.y * 0.5f, zPos);
	}

	public int GetBuildingTypeCount() {
		if (buildingTypes == null)
			return -1;
		
		return (int) buildingTypes.Length;
	}

	public Color GetColor(int i) {
		if (gridColors.Length > i)
			return gridColors [i];
		return Color.black;
	}

	public int GetFloorHeight(int index) {
		if (Table.Instance.grid.Count <= index)
			return -1;
		if (buildingTypes.Contains(Table.Instance.grid [index].type) && Table.Instance.objects.density != null)
			return (int)((Table.Instance.objects.density [Table.Instance.grid [index].type] * _floorHeight) * 0.5f);
		return -1;
	}

	public void OnUpdateData() {
		DrawTable ();
	}

}
