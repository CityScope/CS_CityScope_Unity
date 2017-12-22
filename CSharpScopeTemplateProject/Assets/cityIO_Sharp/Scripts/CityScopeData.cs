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


// Class for posting JSON data
public class TableDataPost
{
    public System.Text.UTF8Encoding encoding;
    public Dictionary<string, string> header;
    public string jsonString;

    public TableDataPost()
    {
        encoding = new System.Text.UTF8Encoding();
        header = new Dictionary<string, string>();
        header.Add("Content-Type", "text/plain");
        jsonString = "";
    }
}
public class CityScopeData : MonoBehaviour
{
    ///<summary>
    /// data refresh rate in seconds 
    /// Tables data is here: https://cityio.media.mit.edu/table/cityio_meta
    /// </summary>
    private string _urlStart = "https://cityio.media.mit.edu/api/table/citymatrix";
    private string _urlLocalHost = "http://localhost:8080//table/citymatrix";

    public enum DataSource { LOCALHOST = 0, REMOTE = 1, INTERNAL = 2 }; // select data stream source in editor
    [Header("Source of grid data")]
    public DataSource _dataSource = DataSource.INTERNAL;
    ///<summary>
    /// data refresh rate in seconds 
    /// </summary>
    public float _delay;
    public float _postDelay;

    [Header("If sending scanned data to server")]
    public bool _sendData;
    public string postTableURL = "https://cityio.media.mit.edu/api/table/update/";
    private TableDataPost tableDataPost;
    ///<summary>
    /// table name list
    /// </summary>
    public enum TableName { _andorra = 0, _volpe = 1, _test = 2 }; // select data stream source in editor

    [Header("If data source is remote or localhost:")]
    public TableName _tableName = TableName._volpe;
    private string _url;

    private WWW _www;
    private WWW post;

    private string _oldData;
    ///<summary>
    /// flag to rise when new data arrives 
    /// </summary>
    public bool _newCityioDataFlag = false;
    private bool uiChanged = false;
    public bool debug = false;

    void Awake()
    {
        tableDataPost = new TableDataPost();

        // Listeners to update slider & dock values
        EventManager.StartListening("sliderChange", OnSliderChanged);
        EventManager.StartListening("dockChange", OnDockChanged);
    }

    IEnumerator Start()
    {
        while (true)
        {
            if (_dataSource == DataSource.REMOTE)
                _url = _urlStart + _tableName.ToString();
            else if (_dataSource == DataSource.LOCALHOST)
                _url = _urlLocalHost;

            yield return new WaitForSeconds(_delay);

            // For JSON parsing
            if (_dataSource != DataSource.INTERNAL) // if table data is online 
            {
                _www = new WWW(_url);
                yield return _www;
                UpdateTableFromUrl();
            }
            else
            { // for app data 
                Table.Instance.SetInternal(true);
                UpdateTableInternal();
            }
        }
    }

    /// <summary>
    /// Updates the table with data read from URL.
    /// </summary>
    private void UpdateTableFromUrl()
    {
        if (!string.IsNullOrEmpty(_www.error))
        {
            Debug.Log(_www.error); // use this for transfering to local server 
        }
        else
        {
            if (_www.text != _oldData)
            {
                _oldData = _www.text; //new data has arrived from server 
                Table.Instance.CreateFromJSON(_www.text); // get parsed JSON into Cells variable --- MUST BE BEFORE CALLING ANYTHING FROM CELLS!!
                _newCityioDataFlag = true;
                EventManager.TriggerEvent("updateData");
                // prints last update time to console 
                System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
                var lastUpdateTime = epochStart.AddSeconds(System.Math.Round(Table.Instance.timestamp / 1000d)).ToLocalTime();
                print("CityIO new data has arrived." + '\n' + "JSON was created at: " + lastUpdateTime + '\n' + _www.text);
            }
        }
    }

    /// <summary>
    /// Updates the table from the grid scanned internally in Unity.
    /// </summary>
    private void UpdateTableInternal()
    {
        // Update Grid values
        bool update = Table.Instance.NeedsUpdate();

        _newCityioDataFlag = true;
        if (Table.Instance.grid != null && (update))
        {
            EventManager.TriggerEvent("updateData");
            if (_sendData)
                SendData();
        }

        if (uiChanged)
            uiChanged = false;
    }

    /// <summary>
    /// Sending Table data to server
    /// </summary>
    private void SendData()
    {
        tableDataPost.jsonString = Table.Instance.WriteToJSON();

        post = new WWW(postTableURL, tableDataPost.encoding.GetBytes(tableDataPost.jsonString), tableDataPost.header);
        StartCoroutine(WaitForWWW(post));
    }

    /// <summary>
    /// Coroutine for data posting to server
    /// </summary>
    /// <returns>The for WW.</returns>
    /// <param name="www">Www.</param>
    IEnumerator WaitForWWW(WWW www)
    {
        yield return new WaitForSeconds(_postDelay);
        yield return www;

        string txt = "";
        if (string.IsNullOrEmpty(www.error))
        {
            txt = www.text;  //text of success
            if (debug)
                Debug.Log("jsonString: " + txt);
            else
                Debug.Log("JSON posted.");
        }
        else
        {
            txt = www.error;  //error
            Debug.Log("JSON post failed: " + txt);

        }
    }


    public bool ShouldUpdateGrid(int index)
    {
        return (Table.Instance.grid[index].ShouldUpdate() || Table.Instance.NeedsUpdate());
    }

    /////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////
    /// 
    /// GETTERS
    /// 
    /////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////

    public int GetGridType(int index)
    {
        return Table.Instance.grid[index].type;
    }

    public void SetDataSending(bool isSending)
    {
        this._sendData = isSending;
    }

    /////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////
    /// 
    /// EVENTS
    /// 
    /////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////

    public void OnSliderChanged()
    {
        uiChanged = true;
    }

    public void OnDockChanged()
    {
        uiChanged = true;
    }

}
