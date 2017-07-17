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
    /// the grid basic unit mesh (prbably cube)
    /// </summary>
    private GameObject _gridObject;
    ///<summary>
    /// default base material for grid GOs
    /// </summary>
    public Material _material;
    public Table _table;
    /// <summary> 
    /// parent of grid GOs
    /// </summary>
    public GameObject _gridHolder;
    /// <summary> 
    /// text mesh for type display 
    /// </summary>
    public GameObject textMeshPrefab;
    /// <summary> 
    /// list of types colors
    /// </summary>
    public Color[] colors;

    private int[] notBuildingTypes = new int[] { (int)Brick.INVALID, (int)Brick.MASK, (int)Brick.ROAD, (int)Brick.PARK, (int)Brick.PARKING, (int)Brick.STREET };
    private int[] buildingTypes = new int[] { (int)Brick.RL, (int)Brick.RM, (int)Brick.RS, (int)Brick.RL, (int)Brick.OL, (int)Brick.OM, (int)Brick.OS };

    IEnumerator Start()
    {
        _table = new Table();
        _table.objects = new Objects();
        _table.objects.density = new List<int>(new int[] { 5, 8, 20, 1, 10, 3 });

        while (true)
        {
            if (_dataSource == DataSource.REMOTE)
            {
                _url = _urlStart + _tableName.ToString();
            }
            else if (_dataSource == DataSource.LOCAL)
            {
                _url = _urlLocalHost;
            }
            yield return new WaitForSeconds(_delayWWW);

            // For JSON parsing
            if (_dataSource != DataSource.INTERNAL) // if table data is online 
            {
                WWW _www = new WWW(_url);
                yield return _www;
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
                        drawTable();
                        // prints last update time to console 
                        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
                        var lastUpdateTime = epochStart.AddSeconds(System.Math.Round(_table.timestamp / 1000d)).ToLocalTime();
                        print("CityIO new data has arrived." + '\n' + "JSON was created at: " + lastUpdateTime + '\n' + _www.text);
                    }
                }
            }
            else
			{ // for app data _gridHolder.transform.name
				bool update = Table.CreateFromDecoder(ref _table, "ScannersParent");
                _newCityioDataFlag = true;
                if (_table.grid != null && update)
                    drawTable();
            }
        }
    }

    void drawTable()
    {
        /*  strat update table with clean grid */
        foreach (Transform child in _gridHolder.transform)
        {
            GameObject.Destroy(child.gameObject.GetComponent<Renderer>().material);
            GameObject.Destroy(child.gameObject);
        }


        for (int i = 0; i < _table.grid.Count; i++) // loop through list of all cells grid objects 
        {
            /* make the grid cells in generic form */
            _gridObject = GameObject.CreatePrimitive(PrimitiveType.Cube); //make cell cube 
            _gridObject.transform.parent = _gridHolder.transform; //put into parent object for later control

            // Objects properties 
            _gridObject.GetComponent<Renderer>().material = _material;
            _gridObject.transform.localPosition =
                  new Vector3((_table.grid[i].x * _cellSizeInMeters), 0, (_table.grid[i].y * _cellSizeInMeters)); //compensate for scale shift due to height


            // Render grid cells based on type, height, etc

            if (buildingTypes.Contains(_table.grid[i].type)) //if this cell is one of the buildings types
            {
                _gridObject.transform.position = new Vector3(_gridObject.transform.position.x,
                _gridHolder.transform.position.y + (_table.objects.density[_table.grid[i].type] * _floorHeight) * 0.5f,
                _gridObject.transform.position.z); //compensate for scale shift and x,y array

                _gridObject.transform.localScale = new Vector3(cellShrink * _cellSizeInMeters,
               (_table.objects.density[_table.grid[i].type] * _floorHeight),
               cellShrink * _cellSizeInMeters); // go through all 'densities' to match Type to Height

                var _tmpColor = colors[_table.grid[i].type];
                _tmpColor.a = 0.8f;
                _gridObject.GetComponent<Renderer>().material.color = _tmpColor;

            }
            else if (notBuildingTypes.Contains(_table.grid[i].type))
            {
                if (_table.grid[i].type == (int)Brick.ROAD) //road
                {
                    _gridObject.transform.localPosition =
                    new Vector3((_table.grid[i].x * _cellSizeInMeters), 0, (_table.grid[i].y * _cellSizeInMeters)); //compensate for scale shift and x,y array
                    _gridObject.transform.localScale = new Vector3(cellShrink * _cellSizeInMeters, 0.25f, cellShrink * _cellSizeInMeters);
                    var _tmpColor = Color.white;
                    _tmpColor.a = 1f;
                    _gridObject.GetComponent<Renderer>().material.color = _tmpColor;
                }

                else if (_table.grid[i].type == (int)Brick.PARKING) // if parking
                {
                    _gridObject.transform.localScale = new Vector3(cellShrink * _cellSizeInMeters, 0.25f, cellShrink * _cellSizeInMeters);
                    _gridObject.transform.localPosition = new Vector3
                    (_table.grid[i].x * _cellSizeInMeters, 0, _table.grid[i].y * _cellSizeInMeters); //compensate for scale shift and x,y array
                    var _tmpColor = Color.white;
                    _tmpColor.a = 1f;
                    _gridObject.GetComponent<Renderer>().material.color = _tmpColor;
                }

                else if (_table.grid[i].type == (int)Brick.STREET) // if street
                {
                    _gridObject.transform.localScale = new Vector3(_cellSizeInMeters, 1, _cellSizeInMeters);
                    _gridObject.transform.localPosition = new Vector3
                    (_table.grid[i].x * _cellSizeInMeters, 0, _table.grid[i].y * _cellSizeInMeters); //compensate for scale shift and x,y array
                    var _tmpColor = Color.white;
                    _tmpColor.a = 1f;
                    _gridObject.GetComponent<Renderer>().material.color = _tmpColor;
                }
                else //if other non building type
                {
                    _gridObject.transform.localPosition =
                    new Vector3((_table.grid[i].x * _cellSizeInMeters), 0, (_table.grid[i].y * _cellSizeInMeters)); //hide base plates 
                    _gridObject.transform.localScale = new Vector3
                    (cellShrink * _cellSizeInMeters * 0.85f, 0.85f, cellShrink * _cellSizeInMeters * 0.85f);
                    _gridObject.GetComponent<Renderer>().material.color = Color.white;
                }
            }

            //* naming the new object *//

            if (_table.grid[i].type > (int)Brick.INVALID && _table.grid[i].type < (int)Brick.ROAD) //if object has is building with Z height
            {
                _gridObject.name =
                          ("Type: " + _table.grid[i].type + " X: " +
                          _table.grid[i].x.ToString() + " Y: " +
                          _table.grid[i].y.ToString() +
                          " Height: " + (_table.objects.density[_table.grid[i].type]).ToString());
            }
            else // if object is flat by nature
            {
                _gridObject.name =
                          ("Type w/o height: " + _table.grid[i].type + " X: " +
                          _table.grid[i].x.ToString() + " Y: " +
                          _table.grid[i].y.ToString());
            }
            // ShowBuildingTypeText(i, cityIOGeo.transform.localScale.y); /// call for showing type text 
        }
    }

    private void ShowBuildingTypeText(int i, float height) //mesh type text metod 
    {
        GameObject textMesh = GameObject.Instantiate(textMeshPrefab, new Vector3((_table.grid[i].x * _cellSizeInMeters),
                                  height + 20, (_table.grid[i].y * _cellSizeInMeters)),
                                  _gridObject.transform.rotation, transform) as GameObject; //spwan prefab text

        textMesh.GetComponent<TextMesh>().text = _table.grid[i].type.ToString();
        textMesh.GetComponent<TextMesh>().fontSize = 1000; // 
        textMesh.GetComponent<TextMesh>().color = Color.black;
        textMesh.GetComponent<TextMesh>().characterSize = 0.25f;
    }
}