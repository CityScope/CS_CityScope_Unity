using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
	[Header("Camera Rot/Zoom")]

	public Transform _thisCamTarget;
	[Range(0.0f, 2f)]
	public float _rotSpeed = 0.0f;
	private float _time = 0.0f;
	public Camera _thisCam;

	void Update()
	{
		// zoom in and out 
		float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
		if (_thisCam.orthographicSize > 3f)
		{
			_thisCam.orthographicSize += (mouseScroll / 10);

		}
		else if (_thisCam.orthographicSize < 3f && _thisCam.orthographicSize > 0f)
		{
			_thisCam.orthographicSize += (mouseScroll / 20);
		}
		// Rotate the camera 
		_time -= Time.deltaTime;
		transform.RotateAround(_thisCamTarget.position, Vector3.up, (_rotSpeed * _thisCam.orthographicSize) * Time.deltaTime);

		// key controls for camera 
//		if (Input.GetKey("up"))
//		{
//			CameraLocation(1, 0);
//		}
//		else if (Input.GetKey("down"))
//		{
//			CameraLocation(-1, 0);
//		}
//		else if (Input.GetKey("left"))
//		{
//			CameraLocation(0, 1);
//		}
//		else if (Input.GetKey("right"))
//		{
//			CameraLocation(0, -1);
//		}
	}
	void CameraLocation(int _keyVer, int _keyHor)
	{
		_thisCam.transform.localPosition = new Vector3
			(_thisCam.transform.localPosition.x + _keyVer * _thisCam.orthographicSize / 20,
				_thisCam.transform.localPosition.y,
				_thisCam.transform.localPosition.z + _keyHor * _thisCam.orthographicSize / 20);

	}

}

