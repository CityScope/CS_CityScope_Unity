using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraControl : MonoBehaviour
{
    public Transform _target;
    private float _startSize;
    public float _endSize = 200;
    //temp for test
    public float _rotSpeed = 5f;
    public float _transitionDuration = 1;
    private float _time = 0.0f;
    cityIO _script;
    public GameObject _cityIOgameObj;
    public Camera _thisCam;
    public bool _boolRotateCamera = false;


    void Start()
    {
        _script = _cityIOgameObj.transform.GetComponent<cityIO>();
        _startSize = _thisCam.orthographicSize;
    }

    void Update()
    {

        _time -= Time.deltaTime * (Time.timeScale / _transitionDuration);
        if (_script._newCityioDataFlag == true)
        { // data IS indeed flowing from cityIO 
            if (_script._table.objects.dockID == -1 || _boolRotateCamera == true)
            { // is brick in edit slot?
                if (_thisCam.orthographicSize != _startSize)
                {
                    _thisCam.orthographicSize = Mathf.Lerp(_startSize, _endSize, _time);
                }
                transform.RotateAround(_target.position, Vector3.up, _rotSpeed * Time.deltaTime); //-Input.GetAxis("Horizontal")
            }
            else
            {
                _thisCam.orthographicSize = Mathf.Lerp(_startSize, _endSize, _time);
                transform.RotateAround(_target.position, Vector3.up, 0); //-Input.GetAxis("Horizontal")
            }
        }
    }
}