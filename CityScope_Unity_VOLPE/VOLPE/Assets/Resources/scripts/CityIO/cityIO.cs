using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


/// <summary> class start </summary>

[System.Serializable]  // have to have this in every JSON class!
public class Grid
{
    public int type;
    public int x;
    public int y;
    public int rot;
}

[System.Serializable] // have to have this in every JSON class!
public class Objects
{
    public float slider1;
    public int toggle1;
    public int toggle2;
    public int toggle3;
    public int toggle4;
    public int dockID;
    public int dockRotation;
    public int IDMax;
    public List<int> density;
    public int pop_young;
    public int pop_mid;
    public int pop_old;
}

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
}

/// <summary> class end </summary>


public class cityIO : MonoBehaviour
{
    //	private string localJson = "file:///Users/noyman/GIT/KendallAgents/Assets/Resources/scripts/citymatrix_volpe.json"; //local file
    private string urlStart = "https://cityio.media.mit.edu/table/p/citymatrix_"; // amended to JSONP 
    public string urlTable = "volpe";
    private string url;
    public int delayWWW;
    private WWW www;
    private string cityIOtext;
    private string cityIOtext_Old;
    //this one look for changes
    public bool _flag = false;
    public int tableX;
    public int tableY;
    public int cellWorldSize;
    public float cellShrink;
    public float floorHeight;
    private GameObject _cube;
    public Material _material;

    public Table _Cells;
    public GameObject gridParent;
    public GameObject textMeshPrefab;
    public static List<GameObject> gridObjects = new List<GameObject>();
    //new list!

    public Color[] colors;


    IEnumerator Start()
    {

        while (true)
        {

            url = urlStart + urlTable;
            //WWW www = new WWW (url);
            WWW www = new WWW(url);

            yield return www;
            yield return new WaitForSeconds(delayWWW);
            cityIOtext = www.text; //just a cleaner Var
            if (cityIOtext != cityIOtext_Old)
            {
                cityIOtext_Old = cityIOtext; //new data has arrived from server 
                jsonHandler();
            }
        }
    }

    void jsonHandler()
    {
        _Cells = Table.CreateFromJSON(cityIOtext); // get parsed JSON into Cells variable --- MUST BE BEFORE CALLING ANYTHING FROM CELLS!!
        drawTable();


        // prints last update time to console 
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        var lastUpdateTime = epochStart.AddSeconds(System.Math.Round(_Cells.timestamp / 1000d)).ToLocalTime();
        print("Table was updated." + '\n' + "Following JSON from CityIO server was created at: " + lastUpdateTime + '\n' + cityIOtext);

        _flag = true;
    }


    void drawTable()
    {

        foreach (Transform child in gridParent.transform)
        {
            GameObject.Destroy(child.gameObject); // strat clean 
            gridObjects.Clear(); // clean list!!!
        }

        for (int i = 0; i < _Cells.grid.Count; i++)
        {

            _cube = GameObject.CreatePrimitive(PrimitiveType.Cube); //make cell cube  
            _cube.GetComponent<Renderer>().material = _material;
            _cube.transform.parent = gridParent.transform; //put into parent object for later control 
            _cube.transform.position = new Vector3((_Cells.grid[i].x * cellWorldSize), 0, (_Cells.grid[i].y * cellWorldSize)); //compensate for scale shift due to height

            //meshTextType (i); /// call if you need type text float 

            for (int n = -5; n < _Cells.objects.density.Count + 1; n++)
            { //go through all 'densities' to match Type to Height. Add +1 so #6 (Road could be in. Fix in JSON Needed) 
                if (_Cells.grid[i].type == n)
                {

                    if (n == 6)
                    { //Street -- Should be fixed in JSON formatting 
                        _cube.transform.localScale = new Vector3(cellShrink * cellWorldSize, 0, cellShrink * cellWorldSize);
                        var _tmpColor = Color.gray;
                        _tmpColor.a = 0.75f;
                        _cube.GetComponent<Renderer>().material.color = _tmpColor;

                    }
                    else if (_Cells.grid[i].type > -1 && _Cells.grid[i].type < 6)
                    { //if this cell is one of the buildings types
                        _cube.transform.localScale = new Vector3(cellShrink * cellWorldSize, (_Cells.objects.density[n] * floorHeight), cellShrink * cellWorldSize);
                        _cube.transform.position = new Vector3(_cube.transform.position.x, (_Cells.objects.density[n] * floorHeight) / 2, _cube.transform.position.z); //compensate for scale shift and x,y array
                        _cube.AddComponent<NavMeshObstacle>();
                        _cube.GetComponent<NavMeshObstacle>().carving = true;


                        var _tmpColor = colors[_Cells.grid[i].type];
                        _tmpColor.a = 0.5f;
                        _cube.GetComponent<Renderer>().material.color = _tmpColor;


                    }
                    else
                    { //if green or other non building 
                        _cube.transform.position = new Vector3(_cube.transform.position.x, -5, _cube.transform.position.z); //hide base plates 
                        _cube.transform.localScale = new Vector3(cellShrink * cellWorldSize, 0, cellShrink * cellWorldSize);
                        _cube.AddComponent<NavMeshObstacle>();
                        _cube.GetComponent<NavMeshObstacle>().carving = true;
                        _cube.GetComponent<Renderer>().material.color = Color.green;
                    }

                }
            }

            gridObjects.Add(_cube); //add this Gobj to list
        }
    }

    private void meshTextType(int i) //mesh type text metod 
    {

        GameObject textMesh = GameObject.Instantiate(textMeshPrefab, new Vector3((_Cells.grid[i].x * cellWorldSize),
                                  _cube.transform.localScale.y + floorHeight, (_Cells.grid[i].y * cellWorldSize)),
                                  _cube.transform.rotation, transform) as GameObject; //spwan prefab text

        textMesh.GetComponent<TextMesh>().text = _Cells.grid[i].type.ToString();
        textMesh.GetComponent<TextMesh>().fontSize = 150;
        textMesh.GetComponent<TextMesh>().color = Color.black;
        textMesh.GetComponent<TextMesh>().characterSize = .5f;
    }

}


