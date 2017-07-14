using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class HeatMaps : MonoBehaviour
{
    /// <summary>
    ///  cityIO script call
    /// </summary>
    public cityIO _city_IO_script;
    public float _delay = 0.5f;
    /// <summary>
    /// loops count
    /// </summary>
    private int _counter = 0;
    [Range(0.1f, 1)]
    public float _cellInset = 0.95f;
    /// <summary>
    /// cell size in meters 
    /// </summary>
    public float _cellSize = 1;
    /// <summary>
    /// radius of 2d search window around object in array  
    /// </summary>
    public int _searchDist = 10;
    /// <summary>
    /// the single grid object 
    /// </summary>
    private GameObject _heatMapGO;
    /// <summary>
    /// Y/[z] height of heatmap 
    /// </summary>
    public int _addToYAxis = 5;
    /// <summary>
    /// 2d array of heatmap GO's 
    /// </summary>
    private GameObject[,] _heatMapObjects;
    IEnumerator Start()
    {
        _heatMapObjects = new GameObject[_city_IO_script._tableX, _city_IO_script._tableY];
        bool _boolMakeHeatmap = true;
        while (true)
        {
            if (_city_IO_script.GetComponent<cityIO>()._newCityioDataFlag)
            {
                if (_boolMakeHeatmap)
                {
                    MakeHeatmapGrid();
                    _boolMakeHeatmap = false;
                }
                else
                {
                    SearchNeighbors();
                }
                yield return new WaitForSeconds(_delay);
            }
            else
            {
                yield return null;
            }
        }
    }
    void MakeHeatmapGrid() // create the base quads once
    {
        _counter = 0;
        for (int x = 0; x < _city_IO_script._tableX; x++)
        {
            for (int y = 0; y < _city_IO_script._tableY; y++)
            {
                _heatMapObjects[x, y] = (GameObject)GameObject.CreatePrimitive(PrimitiveType.Quad); //make cell cube
                _heatMapObjects[x, y].transform.parent = transform; //put into parent object for later control
                _heatMapObjects[x, y].transform.localPosition = new Vector3(_city_IO_script._table.grid[_counter].x * _cellSize, _addToYAxis, _city_IO_script._table.grid[_counter].y * _cellSize);
                Quaternion _tmpRot = transform.localRotation;
                _tmpRot.eulerAngles = new Vector3(90, 0, 0.0f);
                _heatMapObjects[x, y].transform.localRotation = _tmpRot;
                _heatMapObjects[x, y].transform.localScale = new Vector3(_cellInset * _cellSize, _cellInset * _cellSize, _cellInset * _cellSize);
                _heatMapObjects[x, y].transform.GetComponent<Renderer>().receiveShadows = false;
                _heatMapObjects[x, y].transform.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                _heatMapObjects[x, y].transform.GetComponent<Renderer>().material.color = Color.red;
                _counter = _counter + 1;
            }
        }
    }
    /// <summary>
    /// create array from data >> run 2xloops of x, y >> find location of item x,y >> store its location in new array
    /// create search 'window' around it: [x-n, x+n, y-n, y+n]>> if found Target item, measure Manhatten distance to it
    /// add distances to _v.arDist and create new array of [x,y,_varDist] >>loop through array, look for min, max of _varDist >> assign color/Y axis/other viz based on value
    /// </summary>
    void SearchNeighbors() // modify quads based on analysis 
    {
        _counter = 0;
        for (int x = 0; x < _city_IO_script._tableX; x++)
        {
            for (int y = 0; y < _city_IO_script._tableY; y++)
            {
                _heatMapObjects[x, y].name = ("X: " + x + " Y: " + y + " Type: " + _city_IO_script._table.grid[_counter].type + " JSON #: " + _counter);
                if (_city_IO_script._table.grid[_counter].type == 5) // what is the cells type we're searching for? 
                {
                    _heatMapObjects[x, y].transform.GetComponent<Renderer>().material.color = Color.green; // Color and move the selected object
                    _heatMapObjects[x, y].transform.localScale = new Vector3(_cellSize, _cellSize, _cellSize);
                    for (int _windowX = x - _searchDist;
                     _windowX <= x + _searchDist; _windowX++)
                    {
                        for (int _windowY = y - _searchDist;
                        _windowY <= y + _searchDist; _windowY++)
                        {
                            // make sure window area is not outside grid bounds 
                            if (_windowX > -1 && _windowY > -1 &&
                            _windowX < _city_IO_script._tableX &&
                            _windowY < _city_IO_script._tableY)
                            {
                                if (_city_IO_script._table.grid[(_windowX * _city_IO_script._tableX) + _windowY].type == -1)
                                {
                                    _heatMapObjects[_windowX, _windowY].name =
                                    (" Type: " + _city_IO_script._table.grid[(_windowX * _city_IO_script._tableX) + _windowY].type);
                                    var _dist = Vector3.Distance(_heatMapObjects[x, y].transform.position, _heatMapObjects[_windowX, _windowY].transform.position);
                                    _heatMapObjects[_windowX, _windowY].transform.GetComponent<Renderer>().material.color = Color.Lerp(Color.green, Color.red, (_dist / _searchDist));
                                    Vector3 _thisPos = _heatMapObjects[_windowX, _windowY].transform.position;
                                }
                            }
                        }
                    }
                }
                _counter = _counter + 1;
            }
        }
    }
}
