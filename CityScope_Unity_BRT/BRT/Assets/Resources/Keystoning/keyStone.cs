using System;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class keyStone : MonoBehaviour
{

	// quad control vars
	public Vector3[] _vertices;
	private GameObject[] corners;
	public bool _useKeystone = true;
	public int selectedCorner;

	void Start ()
	{

		Destroy (this.GetComponent <MeshCollider> ()); //destroy so we can make one in dynamically 
		transform.gameObject.AddComponent <MeshCollider> (); //add new collider 
	
		Mesh mesh = GetComponent<MeshFilter> ().mesh; // get this GO mesh
		mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };	

		cornerMaker (); //make the corners for visual controls 
	}

	void Update ()
	{
		Mesh mesh = GetComponent<MeshFilter> ().mesh;
		mesh.vertices = _vertices;
		 
		// Zero out the left and bottom edges, 
		// leaving a right trapezoid with two sides on the axes and a vertex at the origin.
		var shiftedPositions = new Vector2[] {
			Vector2.zero,
			new Vector2 (0, _vertices [1].y - _vertices [0].y),
			new Vector2 (_vertices [2].x - _vertices [1].x, _vertices [2].y - _vertices [3].y),
			new Vector2 (_vertices [3].x - _vertices [0].x, 0)
		};
		mesh.uv = shiftedPositions;

		var widths_heights = new Vector2[4];
		widths_heights [0].x = widths_heights [3].x = shiftedPositions [3].x;
		widths_heights [1].x = widths_heights [2].x = shiftedPositions [2].x;
		widths_heights [0].y = widths_heights [1].y = shiftedPositions [1].y;
		widths_heights [2].y = widths_heights [3].y = shiftedPositions [2].y;
		mesh.uv2 = widths_heights;

		onOffObjects (_useKeystone); // toggles onoff at each click
		if (_useKeystone) {
			OnSceneControl ();
		}

		transform.GetComponent<MeshCollider> ().sharedMesh = mesh;// make new collider based on updated mesh 
	}

	/// Methods section 

	private void cornerMaker ()
	{

		corners = new GameObject[_vertices.Length]; // make corners spheres 
		for (int i = 0; i < _vertices.Length; i++) {
			var obj = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			obj.transform.SetParent (transform);
			obj.GetComponent<Renderer> ().material.color = i == selectedCorner ? Color.green : Color.red;
			corners [i] = obj;
		}
	}


	private void onOffObjects (bool visible)
	{
		for (int i = 0; i < _vertices.Length; i++) {
			GameObject sphere = corners [i];
			sphere.transform.position = transform.TransformPoint (_vertices [i]);
			sphere.SetActive (visible);
		}
	}

	private void OnSceneControl ()
	{
		if (!_useKeystone)
			return;

		var corner = Input.GetKeyDown ("1") ? 0 : (Input.GetKeyDown ("2") ? 1 : (Input.GetKeyDown ("3") ? 2 : (Input.GetKeyDown ("4") ? 3 : selectedCorner)));
		if (corner != selectedCorner) {
			corners [selectedCorner].GetComponent<Renderer> ().material.color = Color.red;
			corners [corner].GetComponent<Renderer> ().material.color = Color.green;
			selectedCorner = corner;
			Debug.Log("Selection changed to " + selectedCorner.ToString());
		}

		float speed = 0.5f;

		if (Input.GetKey (KeyCode.LeftShift))
			speed *= 10;
		else if (Input.GetKey (KeyCode.LeftControl) && Input.GetKey (KeyCode.LeftAlt))
			speed *= 0.1f;
		else if (Input.GetKey (KeyCode.LeftAlt))
			speed *=0.01f; 

		var v = _vertices [selectedCorner];

		if (Input.GetKeyDown (KeyCode.UpArrow))
			v = v + speed * Vector3.up;
		else if (Input.GetKeyDown (KeyCode.DownArrow))
			v = v + speed * Vector3.down;
		else if (Input.GetKeyDown (KeyCode.LeftArrow))
			v = v + speed * Vector3.left;
		else if (Input.GetKeyDown (KeyCode.RightArrow))
			v = v + speed * Vector3.right;

		_vertices [selectedCorner] = v;
	}
}