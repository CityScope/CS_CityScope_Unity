using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class proximity : MonoBehaviour
{
	public Transform player;
	Renderer render;

	// Use this for initialization
	void Start ()
	{

		render = gameObject.GetComponent<Renderer> ();

	}
	// Update is called once per frame
	void Update ()
	{

		render.sharedMaterial.SetVector ("_PlayerPosition", player.position);

	}
}
