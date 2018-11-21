using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinLogo : MonoBehaviour
{

    public int _rotSpeed = 5;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(gameObject.transform.position, Vector3.up, _rotSpeed * Time.deltaTime); //-Input.GetAxis("Horizontal")

    }
}
