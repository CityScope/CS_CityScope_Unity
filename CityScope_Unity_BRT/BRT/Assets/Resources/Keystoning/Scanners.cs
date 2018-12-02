using System;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Linq;



public class Scanners : MonoBehaviour
{

	// webcam and scanner vars
	public static GameObject[,] scannersList;
	public static int[,] currentIds;

	public GameObject _gridParent;
	public int _numOfScannersX;
	public int _numOfScannersY;
	private GameObject _scanner;
	RaycastHit hit;
	RenderTexture rTex;
	Texture2D _texture;
	GameObject keystonedQuad;

	public int _refreshRate = 10;
	public float _scannerScale = 0.5f;
	public bool _useWebcam;
	public bool _showRays = false;
	public float xOffset = 0;
	public float zOffset = 0;
	public bool refresh = false;
	public bool debug = true;

	// Scanner positioning and scaling
	public int _gridSize = 2;
	public float _scannerY = 0;
	public float _zDistance = 0;

	// red, black, white, gray
	private Vector3[] colors = new Vector3[] { 
		new Vector3 (1f, 0f, 0f), 
		new Vector3 (0.1f, 0.5f, 0.1f), 
		new Vector3 (1f, 1f, 0f), 
		new Vector3 (0.5f, 0.5f, 0.5f)
	};
	
	private Texture2D hitTex;

	private Dictionary<string, int> idList = new Dictionary<string, int> {
		{ "0000", 0 },
		{ "1111", 1 }, 
		{ "2222", 2 },
		{ "3333", 3 }
	};

	IEnumerator Start ()
	{
		scannersList = new GameObject[_numOfScannersX, _numOfScannersY];
		currentIds = new int[_numOfScannersX / _gridSize, _numOfScannersY / _gridSize];
		scannersMaker ();

		// Find copy mesh with RenderTexture
		keystonedQuad = GameObject.Find ("KeystonedTextureQuad");

		_texture = new Texture2D (GetComponent<Renderer> ().material.mainTexture.width, 
			GetComponent<Renderer> ().material.mainTexture.height);
	
		while (true) {

			yield return new WaitForEndOfFrame ();
			setTexture ();
			yield return new WaitForSeconds (_refreshRate);

			// Assign render texture from keystoned quad texture copy & copy it to a Texture2D
			assignRenderTexture ();

			// Assign scanner colors
			for (int i = 0; i < _numOfScannersX; i += _gridSize) {
				for (int j = 0; j < _numOfScannersY; j += _gridSize) {
					string key = "";

					for (int k = 0; k < _gridSize; k++) {
						for (int m = 0; m < _gridSize; m++) {
							key += findColor (i + k, j + m); 
						}
					}

					// Check all rotations

					if (idList.ContainsKey (key)) {
						currentIds [i / _gridSize, j / _gridSize] = idList [key];
					} else {
						currentIds [i / _gridSize, j / _gridSize] = -1;
					}
				}
			}

			// Debugging matrix vis
			if (debug) {
				printMatrix ();
			}
		}
	}

	/// <summary>
	/// Prints the ID matrix.
	/// </summary>
	private void printMatrix ()
	{
		string matrix = "";

		if ((int)(currentIds.Length) <= 1) {
			Debug.Log ("Empty dictionary.");
			return;
		}
		for (int i = 0; i < currentIds.GetLength (0); i++) {
			for (int j = 0; j < currentIds.GetLength (1); j++) {
				matrix += currentIds [i, j] + "";
			}
			matrix += "\n";
		}
		//Debug.Log (matrix);
	}

	/// <summary>
	/// Finds the color below scanner item[i, j].
	/// </summary>
	/// <param name="i">The row index.</param>
	/// <param name="j">The column index.</param>
	private int findColor (int i, int j)
	{
		if (Physics.Raycast (scannersList [i, j].transform.position, Vector3.down, out hit, 6)) {
			// Get local tex coords w.r.t. triangle

			if (!hitTex) {
				Debug.Log ("No hit texture");
				scannersList [i, j].GetComponent<Renderer> ().material.color = Color.magenta;
				return -1;
			} else {
				int _locX = Mathf.RoundToInt (hit.textureCoord.x * hitTex.width);
				int _locY = Mathf.RoundToInt (hit.textureCoord.y * hitTex.height); 
				Color pixel = hitTex.GetPixel (_locX, _locY);
				int pixelID = closestColor (pixel);
				Color currPixel = new Color (colors [pixelID].x, colors [pixelID].y, colors [pixelID].z);

				//paint scanner with the found color 
				scannersList [i, j].GetComponent<Renderer> ().material.color = currPixel;

				if (_showRays) {
					Debug.DrawLine (scannersList [i, j].transform.position, hit.point, pixel, 200, false);
					Debug.Log (hit.point);
				}
				return pixelID;
			}
		} else { 
			scannersList [i, j].GetComponent<Renderer> ().material.color = Color.magenta; //paint scanner with Out of bounds  color 
			return -1;
		}
	}

	/// <summary>
	/// Assigns the render texture to a Texture2D.
	/// </summary>
	/// <returns>The render texture as Texture2D.</returns>
	private void assignRenderTexture ()
	{
		RenderTexture rt = GameObject.Find ("KeystonedTextureQuad").transform.GetComponent<Renderer> ().material.mainTexture as RenderTexture;
		RenderTexture.active = rt;
		hitTex = new Texture2D (rt.width, rt.height, TextureFormat.RGB24, false);
		hitTex.ReadPixels (new Rect (0, 0, rt.width, rt.height), 0, 0);
	}

	/// <summary>
	/// Sets the texture.
	/// </summary>
	private void setTexture ()
	{
		if (_useWebcam) {
          if (webcam.isPlaying())
          {
                _texture.SetPixels((GetComponent<Renderer>().material.mainTexture as WebCamTexture).GetPixels()); //for webcam 
          }
          else return;
        } else {
			_texture.SetPixels ((GetComponent<Renderer> ().material.mainTexture as Texture2D).GetPixels ()); // for texture map 
		}
		
		_texture.Apply ();
	}

	/// <summary>
	/// Finds the closest color to the given scan colors.
	/// </summary>
	/// <returns>The closest color's index in the colors array.</returns>
	/// <param name="pixel">Pixel.</param>
	private int closestColor (Color pixel)
	{
		Vector3 currPixel = new Vector3 (pixel.r, pixel.g, pixel.b);
		float minDistance = 1000;
		int minColor = -1;

		for (int i = 0; i < colors.Count (); i++) {
			float currDistance = Vector3.Distance (colors [i], currPixel);
			if (currDistance < minDistance) {
				minDistance = currDistance;
				minColor = i;
			}
		}
		return minColor;
	}

	/// <summary>
	/// Initialize scanners.
	/// </summary>
	private void scannersMaker ()
	{
		for (int x = 0; x < _numOfScannersX; x++) {
			for (int y = 0; y < _numOfScannersY; y++) {
				_scanner = GameObject.CreatePrimitive (PrimitiveType.Cube);
                _scanner.transform.parent = _gridParent.transform;

                _scanner.name = "grid_" + y + _numOfScannersX * x;
				_scanner.transform.localScale = new Vector3 (_scannerScale, _scannerScale, _scannerScale);  


				_scanner.transform.localPosition = 
					new Vector3 ((x * _scannerScale * 2),0, (y * _scannerScale * 2 + _zDistance * ((int)(y / _gridSize))));
				
				//_scanner.transform.Rotate (0, 0, 0); 
				scannersList [x, y] = this._scanner;
			}
		}
	}
}

