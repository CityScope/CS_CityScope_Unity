using System;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


[System.Serializable]
public class ColorSettings {
	// Color sample objects
	public List<int> id;
	public List<Vector3> position;
	public Vector3 gridPosition;

	public ColorSettings() {
		position = new List<Vector3>();
		id = new List<int> ();
	}
}

public class Scanners : MonoBehaviour
{
	// webcam and scanner vars
	public static GameObject[,] scannersList;
	public int[,] currentIds;

	public GameObject _gridParent;
	public int _gridSizeX;
	public int _gridSizeY;
	private int numOfScannersX;
	private int numOfScannersY;

	private GameObject _scanner;
	RaycastHit hit;
	RenderTexture rTex;
	Texture2D _texture;
	GameObject keystonedQuad;

	public float _refreshRate = 1;
	public float _scannerScale = 0.5f;
	public bool _useWebcam;
	public bool _showRays = false;
	public bool _debug = true;
	public bool _isCalibrating;
	public bool _showDebugColors = false;
	int _gridSize = 2; // i.e. 2x2 reading for one cell

	private bool setup = true;

	// Color calibration
	ColorSettings colorSettings;
	ColorClassifier colorClassifier;

	private Dictionary<ColorClassifier.SampleColor, List<GameObject>> sampleCubes;

	public GameObject _colorSamplerParent;

	private string colorTexturedQuadName = "KeystonedTextureQuad";
	public string _colorSettingsFileName = "_sampleColorSettings.json";

	private Texture2D hitTex;

	private Color[] allColors;

	enum Brick { RL = 0, RM = 1, RS = 2, OL = 3, OM = 4, OS = 5, ROAD = 6 };

	private Dictionary<string, Brick> idList = new Dictionary<string, Brick>
	{
		{ "2000", Brick.RL },
		{ "2010", Brick.RM }, 
		{ "2001", Brick.RS },
		{ "2100", Brick.OL }, 
		{ "2011", Brick.OM },
		{ "2110", Brick.OS },
		{ "2101", Brick.ROAD }
	};

	IEnumerator Start ()
	{
		InitVariables ();
		EventManager.StartListening ("reload", OnReload);
	
		while (true) {
			yield return new WaitForEndOfFrame ();
			SetTexture ();
			yield return new WaitForSeconds (_refreshRate);

			// Assign render texture from keystoned quad texture copy & copy it to a Texture2D
			AssignRenderTexture();

			if (_isCalibrating || setup)
				CalibrateColors ();

			// Assign scanner colors
			ScanColors();

			if (_debug)
				PrintMatrix ();
			
			if (setup)
				setup = false;

			if (Time.frameCount % 60 == 0)
				System.GC.Collect();
		}
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update() {
		onKeyPressed();
	}

	/// <summary>
	/// Initializes the variables.
	/// </summary>
	private void InitVariables() {
		numOfScannersX = _gridSizeX * _gridSize;
		numOfScannersY = _gridSizeY * _gridSize;
		scannersList = new GameObject[numOfScannersX, numOfScannersY];
		allColors = new Color[numOfScannersX * numOfScannersY];
		currentIds = new int[numOfScannersX / _gridSize, numOfScannersY / _gridSize];
		colorClassifier = new ColorClassifier ();
		SetupSampleObjects ();
		MakeScanners ();

		// Find copy mesh with RenderTexture
		keystonedQuad = GameObject.Find (colorTexturedQuadName);
		if (!keystonedQuad)
			Debug.Log ("Keystoned quad not found.");

		_texture = new Texture2D (GetComponent<Renderer> ().material.mainTexture.width, 
			GetComponent<Renderer> ().material.mainTexture.height);

		LoadSamplers ();
	}

	/// <summary>
	/// Calibrates the colors based on sample points.
	/// </summary>
	private void CalibrateColors() {
		foreach (var colorCube in sampleCubes) {
			for (int i = 0; i < colorCube.Value.Count; i++) {
				if (Physics.Raycast (colorCube.Value[i].transform.position, Vector3.down, out hit, 60)) {
					int _locX = Mathf.RoundToInt (hit.textureCoord.x * hitTex.width);
					int _locY = Mathf.RoundToInt (hit.textureCoord.y * hitTex.height); 
					Color pixel = hitTex.GetPixel (_locX, _locY);
					colorCube.Value[i].GetComponent<Renderer> ().material.color = pixel;
					colorClassifier.SetSampledColors (colorCube.Key, i, pixel);
				}
			}
		}
	}

	/// <summary>
	/// Sets the sample objects.
	/// </summary>
	private void SetupSampleObjects() {
		sampleCubes = new Dictionary<ColorClassifier.SampleColor, List<GameObject>> ();

		GameObject redParent = GameObject.Find ("Red");
		sampleCubes [ColorClassifier.SampleColor.RED] = new List<GameObject>{};
		GameObject blackParent = GameObject.Find ("Black");
		sampleCubes [ColorClassifier.SampleColor.BLACK] = new List<GameObject>{};
		GameObject whiteParent = GameObject.Find ("White");
		sampleCubes [ColorClassifier.SampleColor.WHITE] = new List<GameObject>{};
			
		setChildren (blackParent, ColorClassifier.SampleColor.BLACK);
		setChildren (redParent, ColorClassifier.SampleColor.RED);
		setChildren (whiteParent, ColorClassifier.SampleColor.WHITE);

		foreach (var colorCube in sampleCubes) {
			for (int i = 0; i < colorCube.Value.Count; i++) {
				colorCube.Value[i].transform.localScale = new Vector3 (0.1f, 0.1f, 0.1f);
				colorCube.Value[i].transform.localPosition = new Vector3 (0, 0.1f, 0);
			}
		}
	}

	private void setChildren(GameObject parent, ColorClassifier.SampleColor sampleColor) {
		int children = parent.transform.childCount;
		for (int i = 0; i < children; i++) {
			sampleCubes [sampleColor].Add(parent.transform.GetChild (i).gameObject);
		}
	}

	/// <summary>
	/// Scans the colors.
	/// </summary>
	private void ScanColors() {
		string key = "";
		for (int i = 0; i < numOfScannersX; i += _gridSize) {
			for (int j = 0; j < numOfScannersY; j += _gridSize) {
				key = "";
				for (int k = 0; k < _gridSize; k++) {
					for (int m = 0; m < _gridSize; m++) {
						key += FindColor (i + k, j + m); 
					}
				} 
					
				// keys read counterclockwise
				key = new string(key.ToCharArray().Reverse().ToArray());

				if (idList.ContainsKey (key)) {
					currentIds [i / _gridSize, j / _gridSize] = (int)idList [key];
				} else { // check rotation independence
					bool isRotation = false;
					string keyConcat = key + key;
					foreach(string idKey in idList.Keys) {
						if (keyConcat.Contains (idKey)) {
							currentIds [i / _gridSize, j / _gridSize] = (int)idList [idKey];
							isRotation = true;
							break;
						}
					}
					if (!isRotation)
						currentIds [i / _gridSize, j / _gridSize] = -1;
				}
			}
		}

		if (_showDebugColors && setup)
			colorClassifier.SortColors (allColors);
	}

	/// <summary>
	/// Prints the ID matrix.
	/// </summary>
	private void PrintMatrix() {
		string matrix = "";

		if ((int)(currentIds.Length) <= 1) {
			Debug.Log ("Empty dictionary.");
			return;
		}
		for (int i = 0; i < currentIds.GetLength(0); i++) {
			for (int j = 0; j < currentIds.GetLength(1); j++) {
				if (currentIds [i, j] >= 0)
					matrix += " ";
				matrix += currentIds [i, j] + "";
				if (currentIds [i, j] >= 0)
					matrix += " ";
			}
			matrix += "\n";
		}
		Debug.Log (matrix);
	}

	public int[,] GetCurrentIds() {
		int[,] ids = currentIds.Clone () as int[,];
		return ids;
	}

	/// <summary>
	/// Finds the color below scanner item[i, j].
	/// </summary>
	/// <param name="i">The row index.</param>
	/// <param name="j">The column index.</param>
	private int FindColor(int i, int j) {
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
				allColors [i + numOfScannersX * j] = pixel;
				int currID = colorClassifier.GetClosestColorId (pixel);
				Color minColor = colorClassifier.GetColor (currID);

				//paint scanner with the found color 
				scannersList [i, j].GetComponent<Renderer> ().material.color = minColor;

				if (_showRays) {
					Debug.DrawLine (scannersList [i, j].transform.position, hit.point, pixel, 200, false);
					Debug.Log (hit.point);
				}
				return currID;
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
	private void AssignRenderTexture() {
		RenderTexture rt = GameObject.Find (colorTexturedQuadName).transform.GetComponent<Renderer> ().material.mainTexture as RenderTexture;
		RenderTexture.active = rt;
		if (!hitTex)
			hitTex = new Texture2D (rt.width, rt.height, TextureFormat.RGB24, false);
		hitTex.ReadPixels (new Rect (0, 0, rt.width, rt.height), 0, 0);
	}

	/// <summary>
	/// Sets the texture.
	/// </summary>
	private void SetTexture() {
		if (_useWebcam) {
			if (Webcam.isPlaying())
          {
                _texture.SetPixels((GetComponent<Renderer>().material.mainTexture as WebCamTexture).GetPixels()); //for webcam 
          }
          else return;
		}
		else {
			_texture.SetPixels ((GetComponent<Renderer> ().material.mainTexture as Texture2D).GetPixels ()); // for texture map 
		};
		_texture.Apply ();
	}

	/// <summary>
	/// Initialize scanners.
	/// </summary>
	private void MakeScanners ()
	{
		for (int x = 0; x < numOfScannersX; x++) {
			for (int y = 0; y < numOfScannersY; y++) {
				_scanner = GameObject.CreatePrimitive (PrimitiveType.Quad);
				_scanner.name = "grid_" + y + numOfScannersX * x;
				_scanner.transform.parent = _gridParent.transform;
				_scanner.transform.localScale = new Vector3 (_scannerScale, _scannerScale, _scannerScale);  
				float offset = GameObject.Find (colorTexturedQuadName).GetComponent<Renderer> ().bounds.size.x * 0.5f;
				_scanner.transform.localPosition = new Vector3 (x * _scannerScale * 2 - offset, 0.2f, y * _scannerScale * 2 - offset);
				_scanner.transform.Rotate (90, 0, 0); 
				scannersList[x, y] = this._scanner;
			}
		}
	}

	/// <summary>
	/// Loads the color sampler objects from a JSON.
	/// </summary>
	private void LoadSamplers() {
		Debug.Log ("Loading color sampling settings from  " + _colorSettingsFileName);

		string dataAsJson = JsonParser.loadJSON (_colorSettingsFileName, _debug);
		colorSettings = JsonUtility.FromJson<ColorSettings>(dataAsJson);

		if (colorSettings == null) return;
		if (colorSettings.position == null) return;

		int currId = 0;
		int index = 0;
		for (int i = 0; i < colorSettings.position.Count; i++) {
			if (currId != colorSettings.id [i]) {
				currId = colorSettings.id [i];
				index = 0;
			}
			sampleCubes [(ColorClassifier.SampleColor)currId] [index++].transform.position = colorSettings.position [i];
		}
			
		_gridParent.transform.position = colorSettings.gridPosition;
	}

	/// <summary>
	/// Saves the color sampler objects to a JSON.
	/// </summary>
	private void SaveSamplers() {
		Debug.Log ("Saving color sampling settings to " + _colorSettingsFileName);

		if (colorSettings == null || colorSettings.position == null) {
			colorSettings = new ColorSettings ();
		}

		int index = 0;
		foreach (var cube in sampleCubes) {
			for (int i = 0; i < cube.Value.Count; i++) {
				if (colorSettings.position.Count <= index) {
					colorSettings.position.Add(new Vector3(0, 0, 0));
					colorSettings.id.Add (0);
				}
				colorSettings.id [index] = (int)cube.Key;
				colorSettings.position [index++] = sampleCubes [cube.Key] [i].transform.position;
			}
		}

		colorSettings.gridPosition = _gridParent.transform.position;

		string dataAsJson = JsonUtility.ToJson (colorSettings);
		JsonParser.writeJSON (_colorSettingsFileName, dataAsJson);
	}

	/// <summary>
	/// Raises the scene control event.
	/// </summary>
	private void onKeyPressed ()
	{
		if (Input.GetKey (KeyCode.S) && _isCalibrating) {
			SaveSamplers ();
		} else if (Input.GetKey (KeyCode.L)) {
			LoadSamplers ();
		}
	}

	/// <summary>
	/// Reloads configuration / keystone settings when the scene is refreshed.
	/// </summary>
	void OnReload() {
		Debug.Log ("Color config was reloaded!");

		SetupSampleObjects ();
		LoadSamplers ();
	}

}