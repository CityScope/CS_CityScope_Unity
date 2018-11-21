using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System;

public class Visualizations : MonoBehaviour
{
    public cityIO _city_IO_script;

	public SiteData siteData;

    /// <summary>
    /// counter for the double loop
    /// </summary>
    private int _loopsCounter = 0;
    /// <summary>
    /// The GO to show the grid
    /// </summary>
    private GameObject[] _floorsGeometries;
	private GameObject floorsParent;

    private GameObject[] _typesGeometries;
	private GameObject typesParent;

    [Range(0.1f, 1)]
    public float _cellShrink;
    public float _cellSize;
    /// <summary>
    /// the type that is out of the table are for matters of calc
    /// </summary>
    public int _outOfBoundsType = -1; // make sure this is indeed the type 
    [Range(1f, 100)]
    public int _zAxisMultiplier;
    public int _addToYHeight = 450;

    /// <summary>
    /// get the # range of types
    /// </summary>
   // private float _rangeOfTypes;
    private float _rangeOfFloors;

    /// <summary>
    /// vars for neighbor searching 
    /// </summary>
    public int _windowSearchDim = 40;

	public bool _staticHeatmaps = false;
	private bool firstUpdate = true;

	// Heatmaps
	private const int NUM_HEATMAPS = 3;
	public enum HeatmapType { RES = 0, OFFICE = 1, PARK = 2 };
	private HeatMap[] heatmaps;
	private GameObject heatmapsParent;


	private int _gridX;
	private int _gridY;

	private bool setup;

	private cityIO cityIO;

	private List<int> _typesList = new List<int>();
	private List<int> _floorsList = new List<int>();

	RaycastHit hit;
	GameObject meshRaycaster;
	private float[] meshYPositions;

	////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////
	/// 
	/// SETUP
	/// 
	////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////

    void Awake()
    {
		cityIO = GameObject.Find ("cityIO").GetComponent<cityIO> ();
        if (GameObject.Find("SiteData"))
		    siteData = GameObject.Find ("SiteData").GetComponent<SiteData> ();

		setup = false;

		EventManager.StartListening ("updateData", OnUpdateData);
		EventManager.StartListening ("siteInitialized", OnSiteInitialized);
    }

	private void OnSiteInitialized() {
		_gridX = (int)siteData.GetGridSize ().x;
		_gridY = (int)siteData.GetGridSize ().y;

		_floorsList = siteData.GetFloors ();
		_typesList = siteData.GetTypes ();
	
		meshYPositions = new float[(_gridX-1) * _gridY];
		// compute mesh height
		meshRaycaster = new GameObject();

		SetupFloors ();
		SetupHeatmaps ();
		SetupTypesViz ();

		Debug.Log ("Site setup in Visualizations complete.");
		setup = true;
	}

	private void CreateParent(ref GameObject parent) {
		parent = new GameObject ();
		parent.transform.parent = this.transform;
		parent.SetActive (false);
	}

	/// <summary>
	/// Initializes the floor geometries
	/// </summary>
	private bool SetupFloors() {
		int index = 0;
		int _floorHeight = 0;

		if (floorsParent == null) {
			CreateParent (ref floorsParent);
			floorsParent.name = "Floors";
		}


			
		_floorsGeometries = new GameObject[(_gridX-1) * _gridY];
		_rangeOfFloors = (Mathf.Abs(_floorsList.Max()) + Mathf.Abs(_floorsList.Min()));

		for (int x = 0; x < _gridX - 1; x++) {
			for (int y = 0; y < _gridY; y++) {
				
				var _shiftFloorsHeightAboveZero = _floorsList[index] + Mathf.Abs(_floorsList.Min()); // move list item from subzero


				meshRaycaster.transform.parent = this.transform;
				meshRaycaster.transform.localPosition = new Vector3 (x * _cellSize, 1000, y * _cellSize);

				if (Physics.Raycast (meshRaycaster.transform.position, Vector3.down, out hit)) {
					meshYPositions [index] = hit.point.y;
				}

				if (_typesList[index] != _outOfBoundsType && _floorsList[index] > 0)
				{ 
					// if not on the area which is out of the physical model space
					_floorsGeometries[index] = GameObject.CreatePrimitive(PrimitiveType.Cube); //make cell cube
					_floorsGeometries[index].name = (_floorsList[index].ToString() + "Floors ");
					_floorsGeometries[index].transform.parent = floorsParent.transform;
					_floorsGeometries[index].transform.localPosition = new Vector3(x * _cellSize, meshYPositions [index] + _shiftFloorsHeightAboveZero * (_zAxisMultiplier / 2) + _addToYHeight, y * _cellSize); //compensate for scale shift due to height                                                                                                                                                    
					//color the thing

					_floorsGeometries[index].transform.GetComponent<Renderer>().material.color = Color.HSVToRGB(1, 1, (_floorsList[index]) / _rangeOfFloors);// this creates color based on value of cell!
					_floorHeight = _shiftFloorsHeightAboveZero * _zAxisMultiplier;
					_floorsGeometries[index].transform.localScale = new Vector3(_cellShrink * _cellSize, _floorHeight, _cellShrink * _cellSize);


				}
				index++;
			}
		}

		GameObject.Destroy (meshRaycaster);
		return true;
	}

	private void UpdateFloor(int index) {
		if (_floorsGeometries [index] == null)
			return;

		// update range with new item
		_rangeOfFloors = (Mathf.Abs(_floorsList.Max()) + Mathf.Abs(_floorsList.Min()));
		
		var _shiftFloorsHeightAboveZero = _floorsList [index] + Mathf.Abs(_floorsList.Min());

		//_floorsGeometries[index].transform.localPosition = new Vector3(_floorsGeometries[index].transform.position.x, _shiftFloorsHeightAboveZero * (_zAxisMultiplier / 2) + _addToYHeight, _floorsGeometries[index].transform.position.y); //compensate for scale shift due to height                                                                                                                                                    
		//color the thing
		_floorsGeometries[index].transform.GetComponent<Renderer>().material.color = Color.HSVToRGB(1, 1, (_floorsList[index]) / _rangeOfFloors);// this creates color based on value of cell!
		float _floorHeight = _shiftFloorsHeightAboveZero * _zAxisMultiplier;
		_floorsGeometries[index].transform.localScale = new Vector3(_cellShrink * _cellSize, _floorHeight, _cellShrink * _cellSize);
	}

	/// <summary>
	/// Initializes the types visualization.
	/// </summary>
	/// <returns><c>true</c>, if types viz was setuped, <c>false</c> otherwise.</returns>
	private bool SetupTypesViz() {
		_loopsCounter = 0;
		//_rangeOfTypes = (Mathf.Abs(_typesList.Max()) + Mathf.Abs(_typesList.Min()));

		if (typesParent == null) {
			CreateParent (ref typesParent);
			typesParent.name = "Types";
		}

		if (_typesGeometries == null)
			_typesGeometries = new GameObject[(_gridX - 1) * _gridY];

		for (int x = 0; x < _gridX - 1; x++)
		{
			for (int y = 0; y < _gridY; y++)
			{
				var _shiftTypeListAboveZero = _typesList [_loopsCounter];
				//+ Mathf.Abs(_typesList.Min()); // move list item from subzero
				// var _shiftFloorListAboveZero = _floorsList[_loopsCounter] + Mathf.Abs(_floorsList.Min()); // move list item from subzero

				if (_typesList[_loopsCounter] != _outOfBoundsType)
				{ // if not on the area which is out of the physical model space
					_typesGeometries[_loopsCounter] = GameObject.CreatePrimitive(PrimitiveType.Quad); //make cell cube
					_typesGeometries[_loopsCounter].name = ("Types " + _typesList[_loopsCounter].ToString() + " " + _loopsCounter.ToString());
					/*_typesGeometries.transform.localPosition = new Vector3(x * _cellSize,
                       _shiftFloorListAboveZero * _zAxisMultiplier + _addToYHeight,
                      y * _cellSize);   //move and rotate */
					_typesGeometries[_loopsCounter].transform.localPosition = new Vector3(x * _cellSize, meshYPositions [_loopsCounter] + _addToYHeight, y * _cellSize);   //move and rotate
					Quaternion _tmpRot = transform.localRotation;
					_tmpRot.eulerAngles = new Vector3(90, 0, 0.0f);
					_typesGeometries[_loopsCounter].transform.localRotation = _tmpRot;
					_typesGeometries[_loopsCounter].transform.localScale = new Vector3(_cellShrink * _cellSize,
						_cellShrink * _cellSize,
						_cellShrink * _cellSize);
					_typesGeometries[_loopsCounter].transform.parent = typesParent.transform; //put into parent object for later control

					if (_typesList[_loopsCounter] == (int) Brick.INVALID || _typesList[_loopsCounter] >= (int) Brick.ROAD)
					{
						_typesGeometries [_loopsCounter].SetActive (false);
						_typesGeometries[_loopsCounter].transform.localScale = new Vector3(0.25f * _cellSize, 0.25f * _cellSize, 0.25f * _cellSize);
						_typesGeometries[_loopsCounter].transform.GetComponent<Renderer>().material.color = Color.black;
					}
					else
					{
						Color currColor = cityIO.GetColor (_shiftTypeListAboveZero);
						_typesGeometries [_loopsCounter].transform.GetComponent<Renderer> ().material.color = currColor;
					}

					_typesGeometries[_loopsCounter].transform.GetComponent<Renderer>().receiveShadows = false;
					_typesGeometries[_loopsCounter].transform.GetComponent<Renderer>().shadowCastingMode =
						UnityEngine.Rendering.ShadowCastingMode.Off;
				}

				// Update type for heatmaps too
				foreach (HeatMap hm in heatmaps) {
					hm.UpdateType (x, y, _typesList[_loopsCounter], _loopsCounter);
				}

				_loopsCounter++;
			}
		}

		return true;
	}

	private void UpdateType(int index) {
		if (_typesGeometries [index] == null)
			return;
		
		var _shiftTypeListAboveZero = _typesList[index]; 

		_typesGeometries[index].name = ("Types " + _typesList[index].ToString());

		if (_typesList[index] == (int)Brick.INVALID)
		{
			//_typesGeometries[index].transform.localScale = new Vector3(0.25f * _cellSize, 0.25f * _cellSize, 0.25f * _cellSize);
			_typesGeometries[index].transform.GetComponent<Renderer>().material.color = Color.black;
			_typesGeometries [index].SetActive (false);
		}
		else
		{
			_typesGeometries[index].transform.localScale = new Vector3(_cellShrink * _cellSize, _cellShrink * _cellSize, _cellShrink * _cellSize);
			Color currColor = cityIO.GetColor (_shiftTypeListAboveZero);
			_typesGeometries [index].transform.GetComponent<Renderer> ().material.color = currColor;
		}
	}

	private void SetupHeatmaps() {
		_loopsCounter = 0;

		// Initialize parent object
		if (heatmapsParent == null) {
			CreateParent (ref heatmapsParent);
			heatmapsParent.name = "Heatmaps";
			heatmaps = new HeatMap[NUM_HEATMAPS];
		}

		// Init custom search lists
		List<Brick> officeTypes = new List<Brick> { Brick.OL, Brick.OM, Brick.OS };
		List<Brick> resTypes = new List<Brick> { Brick.RL, Brick.RM, Brick.RS };
		List<Brick> parkTypes = new List<Brick> { Brick.PARK };
		List<Brick> allTypes = new List<Brick> ();
		foreach (Brick brick in System.Enum.GetValues(typeof(Brick))) {
			allTypes.Add (brick);
		}
			
		for (int i = 0; i < NUM_HEATMAPS; i++) {
			heatmaps[i] = new HeatMap(_gridX-1, _gridY, _windowSearchDim, _cellSize, _cellShrink, _addToYHeight, ((HeatmapType)i).ToString());
			heatmaps[i].SetParent (heatmapsParent);

			// Set up search types & origin types for each heatmap
			if (i == (int)HeatmapType.OFFICE) {
				heatmaps [i].SetOriginTypes (resTypes);
				heatmaps [i].SetSearchTypes (officeTypes);
				heatmaps [i].CreateTitle ("Proximity to work spaces");
			} else if (i == (int)HeatmapType.RES) {
				heatmaps [i].SetOriginTypes (officeTypes);
				heatmaps [i].SetSearchTypes (resTypes);
				heatmaps [i].CreateTitle ("Proximity to residential spaces");
			} else if (i == (int)HeatmapType.PARK) {
				heatmaps [i].SetOriginTypes (allTypes);
				heatmaps [i].SetSearchTypes (parkTypes);
				heatmaps [i].CreateTitle ("Proximity to parks");
			}
		}

		// Initialize geos
		for (int x = 0; x < _gridX - 1; x++) {
			for (int y = 0; y < _gridY; y++) {
				if (siteData.GetType(_loopsCounter) != _outOfBoundsType) { // if not on the area which is out of the physical model space
					// Init heatmap geometries for each heatmap object
					foreach (HeatMap hm in heatmaps) {
						hm.CreateHeatmapGeo (x, y, _loopsCounter, siteData.GetType(_loopsCounter), meshYPositions [_loopsCounter]);
					}
				}
				_loopsCounter++;
			}
		}

		UpdateHeatmaps ();
	}

	private void UpdateHeatmaps() {
		foreach (HeatMap hm in heatmaps) {
			hm.UpdateHeatmap ();
		}
		Debug.Log ("Updated heatmaps.");
	}

	////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////
	/// 
	/// UPDATES
	/// 
	////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////

	/// 
    /// <summary>
    /// Viz of floor heights 
    /// </summary>
    public void FloorsViz() // make the height map //
    {
		if (floorsParent == null)
			return;

		floorsParent.SetActive (true);
    }

	public void HideTitles() {
		// Toggle heatmap parents
		if (heatmaps == null)
			return;
		
		for (int i = 0; i < heatmaps.Length; i++) {
			heatmaps[i].SetParentActive (false);
		}
	}

    /// <summary>
    /// Viz of different landuse types 
    /// </summary>
    public void TypesViz() // create types map //
    {
		if (typesParent == null)
			return;
		
		typesParent.SetActive (true);
    }

    /// <summary>
    /// ------------
    /// PSEUDO CODE:
    /// ------------
    /// create array from data 
    /// run 2xloops of x, y
    /// find location of item x,y
    /// store its location in new array
    /// create search 'window' around it:
    /// [x-n, x+n, y-n, y+n]
    /// if found Target item, measure Manhatten distance to it
    /// add distances to _varDist and create new array of [x,y,_varDist]
    /// loop through array, look for min, max of _varDist
    /// assign color/Y axis/other viz based on value
    ///
    /// </summary>

    public void HeatmapViz(HeatmapType heatmapType)
    {
		if (heatmapsParent == null)
			return;

		heatmapsParent.SetActive (true);

		// Toggle heatmap parents
		heatmaps[(int)heatmapType].SetParentActive (true);
    }
		
	/// <summary>
	/// ~~~~~ COULD THREAD ~~~~~
	/// Raises the update data event.
	/// </summary>
	public void OnUpdateData() {
		if (!setup)
			return;
		
		UpdateFloorsAndTypes ();

		if (_staticHeatmaps && !firstUpdate)
			return;
		UpdateHeatmaps ();

		if (_staticHeatmaps)
			firstUpdate = false;
	}
		

	/// <summary>
	/// Updates the types // only update interactive part
	/// </summary>
	private void UpdateFloorsAndTypes() {
		Debug.Log ("Update floors & types in Visualizations.");

		int index = siteData.GetInteractiveIndex();
		int gridIndex = 0;

		///
		/// The interactive grid is indexed the other way! 
		/// so have to iterate r-l up-down
		/// 
		int interactiveEndX = (int) (siteData.GetInteractiveGridLocation().x + siteData.GetInteractiveGridDim().x);
		int interactiveEndY = (int) (siteData.GetInteractiveGridLocation().y + siteData.GetInteractiveGridDim().y);

		for (int j = interactiveEndX; j > (int)siteData.GetInteractiveGridLocation().x ; j--) {
			for (int i = (int)siteData.GetInteractiveGridLocation().y; i < interactiveEndY; i++) {
				index = i * (int)(_gridY) + j;
				// Update interactive part only
				// outer check only in data source is INTERNAL or has bool for update
				//if (cityIO.ShouldUpdateGrid (gridIndex)) {
					if (siteData.IsIndexInsideInteractiveArea (index)) {
						_typesList [index] = cityIO.GetGridType (gridIndex);
						_floorsList [index] = cityIO.GetFloorHeight (gridIndex);
						UpdateType (index);
						UpdateFloor (index);

						// Update type for heatmaps too
						foreach (HeatMap hm in heatmaps) {
							hm.UpdateType (i, j, _typesList [index], index);
						}
					}
				//}
				gridIndex++;
			}
		}
	}

}