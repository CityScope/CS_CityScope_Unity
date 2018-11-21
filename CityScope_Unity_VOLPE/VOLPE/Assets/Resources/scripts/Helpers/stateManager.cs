using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class stateManager : MonoBehaviour
{

	cityIO _script;
	public GameObject _cityIOgameObj;

	public GameObject _heatmap;
	public GameObject _heatmapText;

	public GameObject _night;
	public GameObject _nightText;

	public GameObject _day;



	void Start ()
	{
		_script = _cityIOgameObj.transform.GetComponent<cityIO> ();
	}

	void Update ()
	{
		if (_script._flag == true) { // data IS flowing from cityIO 
			
			if (_script._Cells.objects.toggle1 == 0) { 
				cleanOthers (transform);
				showState (_day);

			} else if (_script._Cells.objects.toggle1 == 6) {
				cleanOthers (transform);
				showState (_night);


			} else if (_script._Cells.objects.toggle1 >= 7) {
				cleanOthers (transform);
				showState (_heatmap);

		
			} else {

				cleanOthers (transform);
				showState (_day);
			}

		}
	}

	void cleanOthers (Transform t)
	{
		foreach (Transform child in transform) {
			child.gameObject.SetActive (false);
		}
	}

	void showState (GameObject t)
	{
		t.transform.gameObject.SetActive (true); 
	}

}

