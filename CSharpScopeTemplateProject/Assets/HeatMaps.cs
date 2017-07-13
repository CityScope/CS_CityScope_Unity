using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class HeatMaps : MonoBehaviour
{
    public cityIO _city_IO_script;
    public float _delay = 0.5f;
    private int _counter = 0;

    [Range(0.1f, 1)]
    public float _cellShrink = 0.95f;
    public float _cellSize = 1;

    /// <summary>
    /// vars for neighbor searching 
    /// </summary>
    public int _searchDist = 10;
    private GameObject _heatmapGO;
    private int _cellScoreCount = 0;
    public int _addToYAxis = 5;

    private GameObject[,] _heatMapObjects;


    IEnumerator Start()
    {
        _heatMapObjects = new GameObject[_city_IO_script._tableX, _city_IO_script._tableY];

        while (true)
        {
            if (_city_IO_script.GetComponent<cityIO>()._newCityioDataFlag)
            {
                MakeHeatmapGrid();
                SearchNeighbors();
                yield return new WaitForSeconds(_delay);
            }
            else
            {
                yield return null;
            }
        }
    }

    void MakeHeatmapGrid() // create the base quads 
    {
        // start new grid by cleaning old one 
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject.GetComponent<Renderer>().material);
            GameObject.Destroy(child.gameObject);
        }

        
        _counter = 0;
        for (int x = 0; x < _city_IO_script._tableX; x++)
        {
            for (int y = 0; y < _city_IO_script._tableY; y++)
            {
                _heatMapObjects[x, y] = (GameObject)GameObject.CreatePrimitive(PrimitiveType.Quad); //make cell cube
                _heatMapObjects[x, y].transform.parent = transform; //put into parent object for later control
                _heatMapObjects[x, y].name = ("X: " + x + " Y: " + y + " Type: " + _city_IO_script._table.grid[_counter].type +
                " JSON #: " + _counter);

                _heatMapObjects[x, y].transform.localPosition =
                     new Vector3(_city_IO_script._table.grid[_counter].x * _cellSize, _addToYAxis,
                     _city_IO_script._table.grid[_counter].y * _cellSize);

                Quaternion _tmpRot = transform.localRotation;
                _tmpRot.eulerAngles = new Vector3(90, 0, 0.0f);
                _heatMapObjects[x, y].transform.localRotation = _tmpRot;

                _heatMapObjects[x, y].transform.localScale = new Vector3
                 (_cellShrink * _cellSize, _cellShrink * _cellSize, _cellShrink * _cellSize);

                _heatMapObjects[x, y].transform.GetComponent<Renderer>().receiveShadows = false;
                _heatMapObjects[x, y].transform.GetComponent<Renderer>().shadowCastingMode =
                     UnityEngine.Rendering.ShadowCastingMode.Off;

                _counter = _counter + 1;

            }
        }
    }
    /// <summary>
    /// create array from data 
    /// run 2xloops of x, y
    /// find location of item x,y
    /// store its location in new array
    /// create search 'window' around it:
    /// [x-n, x+n, y-n, y+n]
    /// if found Target item, measure Manhatten distance to it
    /// add distances to _v.arDist and create new array of [x,y,_varDist]
    /// loop through array, look for min, max of _varDist
    /// assign color/Y axis/other viz based on value
    ///
    /// </summary>
    void SearchNeighbors()
    {
        // // start new grid by cleaning old one 
        // foreach (Transform child in transform)
        // {
        //     GameObject.Destroy(child.gameObject.GetComponent<Renderer>().material);
        //     GameObject.Destroy(child.gameObject);
        // }

        _counter = 0;
        for (int x = 0; x < _city_IO_script._tableX; x++)
        {
            for (int y = 0; y < _city_IO_script._tableY; y++)
            {

                if (_city_IO_script._table.grid[_counter].type == 4) // what is the cells type we're searching for? 
                {
                    _heatMapObjects[x, y].transform.GetComponent<Renderer>().material.color = Color.red; // Color and move the selected object
                    _heatMapObjects[x, y].transform.localScale = new Vector3(_cellSize, _cellSize, _cellSize);
                    _cellScoreCount = 0; //decalre a tmp counter  

                    for (int _windowX = x - _searchDist;
                     _windowX < x + _searchDist; _windowX++)
                    {
                        for (int _windowY = y - _searchDist;
                        _windowY < y + _searchDist; _windowY++)
                        {
                            // make sure window area is not outside grid bounds 
                            if (_windowX > 0 && _windowY > 0
                                && _windowX < _city_IO_script._tableX
                                && _windowY < _city_IO_script._tableY)
                            {
                                if (_city_IO_script._table.grid[(_windowX * _city_IO_script._tableX) + _windowY].type < 4)
                                {
                                    print("for this go " + _heatMapObjects[x, y].name.ToString() + " found obj " +
                                    ((_windowX * _city_IO_script._tableX) + _windowY) + '\n');
                                    _cellScoreCount = _cellScoreCount + 1;
                                    var _tmpColor = _cellScoreCount / Mathf.Pow(2 * _searchDist, 2); // color spectrum based on cell score/max potential score 
                                    _heatMapObjects[_windowX, _windowY].transform.GetComponent<Renderer>().material.color =
                                    Color.HSVToRGB(1, 1, _tmpColor);

                                    _heatMapObjects[_windowX, _windowY].transform.localPosition =
                                    new Vector3(_heatMapObjects[_windowX, _windowY].transform.localPosition.x,
                                    _heatMapObjects[_windowX, _windowY].transform.localPosition.y + 1,
                                    _heatMapObjects[_windowX, _windowY].transform.localPosition.z);

                                }
                            }
                        }
                    }
                }
                else
                {
                    _heatMapObjects[x, y].transform.GetComponent<Renderer>().material.color =
                                       Color.HSVToRGB(0, 0, 0);
                    _heatMapObjects[x, y].transform.localScale =
                      new Vector3(_cellShrink * _cellSize, _cellShrink * _cellSize, _cellShrink * _cellSize);
                }
                _counter = _counter + 1;

            }
        }
    }
}
