///<summary>
/// Class to download, parse and visualize CityScope table data in grid format.
/// This class can handle multiple sources of remote/local data and display realtime changes to table grid.
/// To parse this data, class requiers a JSON Class that mirror the table data format (JSONclass.cs)
/// </summary>


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System;

public class cityIO : MonoBehaviour
{
    ///<summary>
    /// data refresh rate in seconds 
    /// Tables data is here: https://cityio.media.mit.edu/table/cityio_meta
    /// </summary>
    private string _urlStart = "https://cityio.media.mit.edu/api/table/citymatrix";
    private string _urlLocalHost = "http://localhost:8080//table/citymatrix";

    public enum DataSource { LOCAL = 0, REMOTE = 1, INTERNAL = 2 }; // select data stream source in editor
    public DataSource _dataSource = DataSource.INTERNAL;
    ///<summary>
    /// table name list
    /// </summary>
    public enum TableName { _andorra = 0, _volpe = 1, _test = 2 }; // select data stream source in editor
    public TableName _tableName = TableName._volpe;
    private string _url;
    ///<summary>
    /// data refresh rate in seconds 
    /// </summary>
    public float _delayWWW;
    private WWW _www;
    private string _oldData;
    ///<summary>
    /// flag to rise when new data arrives 
    /// </summary>
    public bool _newCityioDataFlag = false;
    public int _tableX;
    public int _tableY;
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
    /// the grid basic unit cubes
    /// </summary>
    private GameObject[] _gridObjects;
    ///<summary>
    /// default base material for grid GOs
    /// </summary>
    public Material _material;
    public Table _table;

	private bool uiChanged = false;
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

	private float textOffset = 20f;
    /// <summary> 
    /// list of types colors
    /// </summary>
    public Color[] colors;

	private Color _tmpColor;
	private float height;
	private float yPos;
	private Vector3 gridObjectPosition;
	private Vector3 gridObjectScale;


    private int[] notBuildingTypes = new int[] { (int)Brick.INVALID, (int)Brick.MASK, (int)Brick.ROAD, (int)Brick.PARK, (int)Brick.AMENITIES, (int)Brick.STREET };
    private int[] buildingTypes = new int[] { (int)Brick.RL, (int)Brick.RM, (int)Brick.RS, (int)Brick.RL, (int)Brick.OL, (int)Brick.OM, (int)Brick.OS };


	void Awake() {
		_tmpColor = Color.black;
		height = 0f;
		yPos = 0f;
		_table = new Table();

		// Listeners to update slider & dock values
		EventManager.StartListening ("sliderChange", OnSliderChanged);
		EventManager.StartListening ("dockChange", OnDockChanged);
	}

    IEnumerator Start()
    {
        while (true)
        {
            if (_dataSource == DataSource.REMOTE)
                _url = _urlStart + _tableName.ToString();
            else if (_dataSource == DataSource.LOCAL)
                _url = _urlLocalHost;
			
            yield return new WaitForSeconds(_delayWWW);

            // For JSON parsing
            if (_dataSource != DataSource.INTERNAL) // if table data is online 
            {
				_www = new WWW(_url);
				yield return _www;
				UpdateTableFromUrl ();
            }
            else
			{ // for app data _gridHolder.transform.name
				UpdateTableInternal();
            }
        }
    }

	/// <summary>
	/// Updates the table with data read from URL.
	/// </summary>
	private void UpdateTableFromUrl() {
		if (!string.IsNullOrEmpty(_www.error))
		{
			Debug.Log(_www.error); // use this for transfering to local server 
		}
		else
		{
			if (_www.text != _oldData)
			{
				_oldData = _www.text; //new data has arrived from server 
				_table = Table.CreateFromJSON(_www.text); // get parsed JSON into Cells variable --- MUST BE BEFORE CALLING ANYTHING FROM CELLS!!
				_newCityioDataFlag = true;
				DrawTable();
				// prints last update time to console 
				System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
				var lastUpdateTime = epochStart.AddSeconds(System.Math.Round(_table.timestamp / 1000d)).ToLocalTime();
				print("CityIO new data has arrived." + '\n' + "JSON was created at: " + lastUpdateTime + '\n' + _www.text);
			}
		}
	}

	/// <summary>
	/// Updates the table from the grid scanned internally in Unity.
	/// </summary>
	private void UpdateTableInternal() {
		// Update UI values (slider, dock)
 		if (uiChanged)
			_table.UpdateObjectsFromDecoder ("ScannersParent");

		// Update Grid values
		bool update = _table.CreateGridFromDecoder("ScannersParent");
		_newCityioDataFlag = true;
		if (_table.grid != null && (update || uiChanged)) {
			EventManager.TriggerEvent ("updateData");
			DrawTable();
		}

		if (uiChanged)
			uiChanged = false;
	}

	public bool ShouldUpdateGrid(int index) {
		return (_table.grid [index].update || uiChanged);
	}

	/// <summary>
	/// Creates the grid object.
	/// </summary>
	/// <param name="i">The index.</param>
	private void CreateGridObject(int i) {
		/* make the grid cells in generic form */
		_gridObjects[i] = GameObject.CreatePrimitive(PrimitiveType.Cube); //make cell cube 
		_gridObjects[i].transform.parent = _gridHolder.transform; //put into parent object for later control

		gridObjectPosition = new Vector3((_table.grid[i].x * _cellSizeInMeters), 0, (_table.grid[i].y * _cellSizeInMeters));
		gridObjectScale = new Vector3(cellShrink * _cellSizeInMeters, 0, cellShrink * _cellSizeInMeters);

		// Objects properties 
		_gridObjects[i].GetComponent<Renderer>().material = _material;
		_gridObjects[i].transform.localPosition = gridObjectPosition; //compensate for scale shift due to height
		_gridObjects[i].transform.localScale = gridObjectScale;
		_gridObjects [i].SetActive (false);

		AddBuildingTypeText (i, textOffset);
	}

	private void SetGridObject(int i) {
		// compensate for scale shift and x,y array
		gridObjectPosition = new Vector3((_table.grid[i].x * _cellSizeInMeters), yPos, (_table.grid[i].y * _cellSizeInMeters));
		gridObjectScale = new Vector3(cellShrink * _cellSizeInMeters, height, cellShrink * _cellSizeInMeters);

		_gridObjects[i].transform.localPosition = gridObjectPosition;
		_gridObjects[i].transform.localScale = gridObjectScale; // go through all 'densities' to match Type to Height
		_gridObjects[i].GetComponent<Renderer>().material.color = _tmpColor;
	}

	/// <summary>
	/// Updates the grid object's height and scale given the current type.
	/// </summary>
	/// <param name="i">The index.</param>
	private void UpdateGridObject(int i) {
		if (buildingTypes.Contains(_table.grid[i].type)) //if this cell is one of the buildings types
		{
			height = _gridObjects[i].transform.position.y + (_table.objects.density[_table.grid[i].type] * _floorHeight); 
			yPos = _table.objects.density[_table.grid[i].type] * _floorHeight * 0.5f;
			_tmpColor = colors[_table.grid[i].type];
			_tmpColor.a = 0.8f;

			SetGridObject (i);
		}
		else if (notBuildingTypes.Contains(_table.grid[i].type))
		{
			_tmpColor = Color.white;
			_tmpColor.a = 1f;

			if (_table.grid[i].type == (int)Brick.ROAD)
			{
				yPos = 0;
				height = 0.25f;
			}
			else if (_table.grid[i].type == (int)Brick.AMENITIES) 
			{
				yPos = 0f;
				height = 0.25f;
			}
			else if (_table.grid[i].type == (int)Brick.STREET)
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

	private void NameGridObject(int i) {
		if (_table.grid[i].type > (int)Brick.INVALID && _table.grid[i].type < (int)Brick.ROAD) //if object has is building with Z height
		{
			_gridObjects[i].name =
				("Type: " + _table.grid[i].type + " X: " +
					_table.grid[i].x.ToString() + " Y: " +
					_table.grid[i].y.ToString() +
					" Height: " + (_table.objects.density[_table.grid[i].type]).ToString());
		}
		else // if object is flat by nature
		{
			_gridObjects[i].name =
				("Type w/o height: " + _table.grid[i].type + " X: " +
					_table.grid[i].x.ToString() + " Y: " +
					_table.grid[i].y.ToString());
		}
	}

	/// <summary>
	/// Creates array of gridobjects 
	/// </summary>
	private void SetupTable() {
		_gridObjects = new GameObject[_table.grid.Count];

		for (int i = 0; i < _table.grid.Count; i++) // loop through list of all cells grid objects 
			CreateGridObject(i);
	}

	/// <summary>
	/// Updates the table if the given grid object changed or if the slider/ dock changed
	/// </summary>
	private void UpdateTable() {
		for (int i = 0; i < _table.grid.Count; i++) { // loop through list of all cells grid objects 
			if ((_table.grid[i].update || uiChanged))
				UpdateGridObject(i);
		}
	}
		

    private void DrawTable()
    {
		if (_gridObjects == null)
			SetupTable ();
		
		UpdateTable ();
    }

    private void AddBuildingTypeText(int i, float height) //mesh type text metod 
    {
		if (textMeshes == null) 
			textMeshes = new GameObject[_table.grid.Count];
		
		textMeshes[i] = GameObject.Instantiate(textMeshPrefab, new Vector3((_gridObjects[i].transform.position.x),
			height + _gridObjects[i].transform.localScale.y, _gridObjects[i].transform.position.z),
			_gridObjects[i].transform.rotation, transform) as GameObject; //spwan prefab text
		textMeshes[i].transform.parent = _textMeshParent.transform;

		Quaternion _tmpRot = textMeshes [i].transform.localRotation;
		_tmpRot.eulerAngles = new Vector3(90, 0, 0.0f);
		textMeshes [i].transform.localRotation = _tmpRot;

		textMeshes[i].GetComponent<TextMesh>().text = _table.grid[i].type.ToString();
		textMeshes[i].GetComponent<TextMesh>().fontSize = 10; // 
		textMeshes[i].GetComponent<TextMesh>().color = Color.gray;
		textMeshes[i].GetComponent<TextMesh>().characterSize = 0.25f;

		textMeshes [i].SetActive (false);
    }

	private void UpdateBuildingTypeText(int i, bool enabled) {
		if (enabled && _table.grid [i].type != (int)Brick.INVALID)
			textMeshes [i].SetActive (true);
		else
			textMeshes [i].SetActive (false); 
		textMeshes [i].GetComponent<TextMesh> ().text = Enum.GetName (typeof(Brick), (_table.grid [i].type));
		float xPos = textMeshes [i].transform.localPosition.x;
		float zPos = textMeshes [i].transform.localPosition.z;
		textMeshes[i].transform.localPosition = new Vector3(xPos, _gridObjects[i].transform.localPosition.y + _gridObjects[i].transform.localScale.y * 0.5f, zPos);
	}

	/////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////
	/// 
	/// GETTERS
	/// 
	/////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////

	public Color GetColor(int i) {
		if (colors.Length > i)
			return colors [i];
		return Color.black;
	}

	public int GetGridType(int index) {
		return _table.grid [index].type;
	}

	public int GetFloorHeight(int index) {
		if (_table.grid.Count <= index)
			return -1;
		if (buildingTypes.Contains(_table.grid [index].type) && _table.objects.density != null)
			return (int)((_table.objects.density [_table.grid [index].type] * _floorHeight) * 0.5f);
		return -1;
	}

	public int GetBuildingTypeCount() {
		return (int) buildingTypes.Length;
	}

	/////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////
	/// 
	/// EVENTS
	/// 
	/////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////

	public void OnSliderChanged() {
		uiChanged = true;
		Debug.Log ("Slider value changed.");
	}

	public void OnDockChanged() {
		uiChanged = true;
		Debug.Log ("Dock value changed.");
	}

}
