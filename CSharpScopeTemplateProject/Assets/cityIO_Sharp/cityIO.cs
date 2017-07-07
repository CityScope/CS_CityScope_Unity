using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class cityIO : MonoBehaviour
{
    // Tables data are here: https://cityio.media.mit.edu/table/cityio_meta
    private string _urlStart = "https://cityio.media.mit.edu/api/table/citymatrix";
    private string _urlLocalHost = "http://localhost:8080//table/citymatrix";

	public enum DataSource { LOCAL = 0, REMOTE = 1, INTERNAL = 2 };

	public DataSource _dataSource = DataSource.INTERNAL;

    public string _tableAddUnderscore = "";
    private string _url;
    public int _delayWWW;
    private WWW _www;
    private string _oldText;
    public bool _newCityioDataFlag = false;
    public int _tableX;
    public int _tableY;
    public float _cellWorldSize;
    public float cellShrink;
    public float floorHeight;
    private GameObject cityIOGeo;
    public Material _material;
    public Table _table;
    public GameObject _gridHolder;
    public GameObject textMeshPrefab;
    public Color[] colors;

	private int[] notBuildingTypes = new int[] { (int)Brick.INVALID, (int)Brick.MASK, (int)Brick.ROAD, (int)Brick.PARK, (int)Brick.PARKING, (int)Brick.STREET };
	private int[] buildingTypes = new int[] { (int)Brick.RL, (int)Brick.RM, (int)Brick.RS, (int)Brick.RL, (int)Brick.OL, (int)Brick.OM, (int)Brick.OS };

    IEnumerator Start()
    {

		_table = new Table ();
		_table.objects = new Objects ();
		_table.objects.density = new List<int> (new int[] {5, 8, 20, 0, 10, 3});

        while (true)
        {
			if (_dataSource == DataSource.REMOTE)
            {
                _url = _urlStart + _tableAddUnderscore;
            }
			else if (_dataSource == DataSource.LOCAL)
            {
                _url = _urlLocalHost + _tableAddUnderscore;
            }

			yield return new WaitForSeconds (_delayWWW);

			// For JSON parsing
			if (_dataSource != DataSource.INTERNAL) {
				WWW _www = new WWW (_url);
				yield return _www;
				if (!string.IsNullOrEmpty (_www.error)) {
					Debug.Log (_www.error); // use this for transfering to local server 
				} else {
					//if (_www.text != _oldText) {
						_oldText = _www.text; //new data has arrived from server 
						_table = Table.CreateFromJSON (_www.text); // get parsed JSON into Cells variable --- MUST BE BEFORE CALLING ANYTHING FROM CELLS!!
						_newCityioDataFlag = true;
						drawTable ();
						// prints last update time to console 
						System.DateTime epochStart = new System.DateTime (1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
						var lastUpdateTime = epochStart.AddSeconds (System.Math.Round (_table.timestamp / 1000d)).ToLocalTime ();
						print ("CityIO new data has arrived." + '\n' + "JSON was created at: " + lastUpdateTime + '\n' + _www.text);
					//}
				}
			} else { // for app data
				bool update = Table.CreateFromDecoder(ref _table);
				_newCityioDataFlag = true;
				if (_table.grid != null && update) {
					drawTable ();
				}
			}
        }
    }

    void drawTable()
    {
        /*  strat update table with clean grid */
        foreach (Transform child in _gridHolder.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        for (int i = 0; i < _table.grid.Count; i++) // loop through list of all cells grid objects 
        {
            /* make the grid cells in generic form */
            cityIOGeo = GameObject.CreatePrimitive(PrimitiveType.Cube); //make cell cube 
            cityIOGeo.transform.parent = _gridHolder.transform; //put into parent object for later control

            // Objects properties 
            cityIOGeo.GetComponent<Renderer>().material = _material;
            cityIOGeo.transform.localPosition =
                  new Vector3((_table.grid[i].x * _cellWorldSize), 0, (_table.grid[i].y * _cellWorldSize)); //compensate for scale shift due to height


            // Render grid cells based on type, height, etc

			if (buildingTypes.Contains(_table.grid[i].type)) //if this cell is one of the buildings types
            {
                cityIOGeo.transform.position = new Vector3(cityIOGeo.transform.position.x,
                _gridHolder.transform.position.y + (_table.objects.density[_table.grid[i].type] * floorHeight) * 0.5f,
                cityIOGeo.transform.position.z); //compensate for scale shift and x,y array

                cityIOGeo.transform.localScale = new Vector3(cellShrink * _cellWorldSize,
               (_table.objects.density[_table.grid[i].type] * floorHeight),
               cellShrink * _cellWorldSize); // go through all 'densities' to match Type to Height

                var _tmpColor = colors[_table.grid[i].type];
                _tmpColor.a = 0.8f;
                cityIOGeo.GetComponent<Renderer>().material.color = _tmpColor;

            }
            else if (notBuildingTypes.Contains(_table.grid[i].type))
            {
				if (_table.grid[i].type == (int)Brick.ROAD)
                { //Street
                    cityIOGeo.transform.localPosition =
                    new Vector3((_table.grid[i].x * _cellWorldSize), 0, (_table.grid[i].y * _cellWorldSize)); //compensate for scale shift and x,y array
                    cityIOGeo.transform.localScale = new Vector3(cellShrink * _cellWorldSize, 0.25f, cellShrink * _cellWorldSize);
                    var _tmpColor = Color.gray;
                    _tmpColor.a = 1f;
                    cityIOGeo.GetComponent<Renderer>().material.color = _tmpColor;
                }

				else if (_table.grid[i].type == (int)Brick.PARKING) // if parking
                {
                    cityIOGeo.transform.localScale = new Vector3(cellShrink * _cellWorldSize, 0.25f, cellShrink * _cellWorldSize);
                    cityIOGeo.transform.localPosition = new Vector3
                    (_table.grid[i].x * _cellWorldSize, 0, _table.grid[i].y * _cellWorldSize); //compensate for scale shift and x,y array
                    var _tmpColor = Color.green;
                    _tmpColor.a = 1f;
                    cityIOGeo.GetComponent<Renderer>().material.color = _tmpColor;
                }

				else if (_table.grid[i].type == (int)Brick.STREET) // if street
                {
                    cityIOGeo.transform.localScale = new Vector3(_cellWorldSize, 1, _cellWorldSize);
                    cityIOGeo.transform.localPosition = new Vector3
                    (_table.grid[i].x * _cellWorldSize, 0, _table.grid[i].y * _cellWorldSize); //compensate for scale shift and x,y array
                    var _tmpColor = Color.gray;
                    _tmpColor.a = 1f;
                    cityIOGeo.GetComponent<Renderer>().material.color = _tmpColor;
                }
                else //if other non building type
                {
                    cityIOGeo.transform.localPosition =
                    new Vector3((_table.grid[i].x * _cellWorldSize), 0, (_table.grid[i].y * _cellWorldSize)); //hide base plates 
                    cityIOGeo.transform.localScale = new Vector3
                    (cellShrink * _cellWorldSize * 0.85f, 0.85f, cellShrink * _cellWorldSize * 0.85f);
                    cityIOGeo.GetComponent<Renderer>().material.color = Color.black;
                }
            }

            //* naming the new object *//

			if (_table.grid[i].type > (int)Brick.INVALID && _table.grid[i].type < (int)Brick.ROAD) //if object has is building with Z height
            {
                cityIOGeo.name =
                          ("Type: " + _table.grid[i].type + " X: " +
                          _table.grid[i].x.ToString() + " Y: " +
                          _table.grid[i].y.ToString() +
                          " Height: " + (_table.objects.density[_table.grid[i].type]).ToString());
            }
            else // if object is flat by nature
            {
                cityIOGeo.name =
                          ("Type w/o height: " + _table.grid[i].type + " X: " +
                          _table.grid[i].x.ToString() + " Y: " +
                          _table.grid[i].y.ToString());
            }
           // ShowBuildingTypeText(i, cityIOGeo.transform.localScale.y); /// call if you need type text float 
        }
    }

    private void ShowBuildingTypeText(int i, float height) //mesh type text metod 
    {
        GameObject textMesh = GameObject.Instantiate(textMeshPrefab, new Vector3((_table.grid[i].x * _cellWorldSize),
                                  height + 20, (_table.grid[i].y * _cellWorldSize)),
                                  cityIOGeo.transform.rotation, transform) as GameObject; //spwan prefab text

        textMesh.GetComponent<TextMesh>().text = _table.grid[i].type.ToString();
        textMesh.GetComponent<TextMesh>().fontSize = 1000; // 
        textMesh.GetComponent<TextMesh>().color = Color.black;
        textMesh.GetComponent<TextMesh>().characterSize = 0.25f;
    }
}