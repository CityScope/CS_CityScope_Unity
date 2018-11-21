using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class stateManager : MonoBehaviour
{

    public enum ModeEnum { DEMO = 0, INTERACTIVE = 1 }; // select data stream source in editor
    [Header("Mode Selector")]

    public ModeEnum _modeSelector = ModeEnum.DEMO;

    public int _changeModeEverySeconds = 15;
    [Header("Context Objects")]
    public GameObject _andorraCityScope;
    public GameObject _contextHolder;
    public GameObject _cellTowers;
    [Header("Heatmaps Objects")]
    public GameObject _heatmapHolder;
    public cityIO _cityIOscript;
    public Visualizations _heatmapsScript;

    public GameObject _andorraHeatmap;
    public GameObject _floorsUI;
	public GameObject _heatmapsUI;

    private int _sliderState = 3;
    private int _oldState;

	bool setup;

    private const int NUM_STATES = 6;
	private enum HeatmapState { CITYIO = 0, LANDUSE = 1, RES_PROXIMITY = 3, OFFICE_PROXIMITY = 2, PARK_PROXIMITY = 4,  FLOORS = 5, CELL = 6 };

    void Awake()
    {
		setup = false;

        if (_modeSelector == ModeEnum.INTERACTIVE)
        {
            _sliderState = (int)_cityIOscript._table.objects.slider1; //gets the slider 
            _oldState = _sliderState;
            StateControl(_sliderState);
        }
        else
        {
            StartCoroutine(DemoMode());
        }

		EventManager.StartListening ("siteInitialized", OnSiteInitialized);
    }

	private void OnSiteInitialized() {
		setup = true;
	}

    void Update()
    {
        _sliderState = (int)_cityIOscript._table.objects.slider1; //gets the slider 
        if (_sliderState != _oldState)
        {
            Debug.Log("Slider state changed to " + _sliderState);
            StateControl(_sliderState);
            _oldState = _sliderState;
        }
    }

    IEnumerator DemoMode()
    {
        while (true)
        {
            for (int i = 0; i < NUM_STATES; i++)
            {
                yield return new WaitForEndOfFrame();
                StateControl(i);
                yield return new WaitForSeconds(_changeModeEverySeconds);
            }
        }
    }
    void StateControl(int _sliderState)
    {
		if (setup != true)
			return;
		
        CleanOldViz(_contextHolder, _heatmapHolder);
        ShowContext(_andorraCityScope);

        switch (_sliderState)
        {
            default:
                print("Default: Basic Sat view and cityIO grid" + '\n');
                _floorsUI.SetActive(false);
				_heatmapsUI.SetActive(false);
                break;
            case (int)HeatmapState.CITYIO:
                print("State 0: Basic Sat view and cityIO grid" + '\n');
                _floorsUI.SetActive(false);
				_heatmapsUI.SetActive(false);
                break;
            case (int)HeatmapState.LANDUSE: // LANDUSE 
                _heatmapsScript.TypesViz();
                print("State 2: Land use map" + '\n');
                _floorsUI.SetActive(false);
				_heatmapsUI.SetActive(false);
                break;
            case (int)HeatmapState.RES_PROXIMITY: // HEATMAP
                _heatmapsScript.HeatmapViz(Visualizations.HeatmapType.RES);
                print("State 3: Proximity to Res HeatMap" + '\n');
                _floorsUI.SetActive(false);
				_heatmapsUI.SetActive(true);
                break;
            case (int)HeatmapState.OFFICE_PROXIMITY: // HEATMAP
                _heatmapsScript.HeatmapViz(Visualizations.HeatmapType.OFFICE);
                print("State 4: Proximity to Offices HeatMap" + '\n');
                _floorsUI.SetActive(false);
				_heatmapsUI.SetActive(true);
                break;
            case (int)HeatmapState.PARK_PROXIMITY: // HEATMAP
                _heatmapsScript.HeatmapViz(Visualizations.HeatmapType.PARK);
                print("State 5: Proximity to Parks HeatMap" + '\n');
                _floorsUI.SetActive(false);
				_heatmapsUI.SetActive(true);
                break;
			case (int)HeatmapState.FLOORS: // FLOORS
				_heatmapsScript.FloorsViz();
				_floorsUI.SetActive(true);
				_heatmapsUI.SetActive(false);
				print("State 1: Floors map" + '\n');
				break;
            case (int)HeatmapState.CELL: // Cell towers
                ShowContext(_cellTowers);
                print("State 6: Celltowers heatmap" + '\n');
                _floorsUI.SetActive(false);
				_heatmapsUI.SetActive(false);
                break;
        }
    }

    void CleanOldViz(GameObject _contextHolder, GameObject _heatmapHolder)
    {
        foreach (Transform child in _contextHolder.transform)
        {
            child.gameObject.SetActive(false);
        }

        foreach (Transform child in _heatmapHolder.transform)
        {
            child.gameObject.SetActive(false);
        }

        _heatmapsScript.HideTitles();
    }
    void ShowContext(GameObject t)
    {
        t.transform.gameObject.SetActive(true);
    }
}


