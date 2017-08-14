/// <summary>
/// Scanners samples a 2D quad with a set of objects on a grid. 
/// 
/// </summary>

using System;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


[System.Serializable]
public class ColorSettings {
	// Color sample objects
	public List<int> id;
	public List<Color> color;
	public Vector3 gridPosition;
	public Vector3 dockPosition;

	public ColorSettings() {
		color = new List<Color>();
		id = new List<int> ();
	}
}

public class Scanners : MonoBehaviour
{
	private Thread scannerThread;

	public int _bufferSize = 50;
	public bool _useBuffer;

	// webcam and scanner vars
	public static GameObject[,] scannersList;
	public int[,] currentIds;

	public GameObject _gridParent;
	public GameObject _colorSpaceParent;

	public int _gridSizeX;
	public int _gridSizeY;
	private int numOfScannersX;
	private int numOfScannersY;
	private Queue<int>[] idBuffer;

	// Scanner objects
	private GameObject _scanner;

	// UI scanners
	public bool _enableUI = false;
	private Dock dock;
	private LegoSlider slider;
	public int _sliderRange = 30;

	RaycastHit hit;
	RenderTexture rTex;
	Texture2D _texture;
	GameObject keystonedQuad;
	GameObject cameraKeystonedQuad;

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

	private Dictionary<ColorClassifier.SampleColor, GameObject> colorRefSpheres;
	public Color[] sampleColors;
	public int _numColors = 3;

	private string colorTexturedQuadName = "KeystonedTextureQuad";
	public string _colorSettingsFileName = "_sampleColorSettings.json";

	private Texture2D hitTex;

	private Color[] allColors;

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
		if (_useWebcam) {
			if (!GetComponent<Webcam> ().enabled)
				GetComponent<Webcam> ().enabled = true;
		}
		 
		scannerThread = new Thread(UpdateScanners);
		scannerThread.Start ();

		InitVariables ();

		EventManager.StartListening ("reload", OnReload);
		EventManager.StartListening ("save", OnSave);
			
		while (true) {
			////
			//// Wait one frame for GPU
			//// http://answers.unity3d.com/questions/465409/reading-from-a-rendertexture-is-slow-how-to-improv.html
			////
			yield return new WaitForSeconds (_refreshRate);
			// Assign render texture from keystoned quad texture copy & copy it to a Texture2D
			AssignRenderTexture();
			yield return new WaitForEndOfFrame ();

			UpdateScanners ();
		}
	}

	public void ToggleCalibration() {
		this._isCalibrating = !this._isCalibrating;
	}

	private void UpdateScanners() {

		if (_isCalibrating || setup)
			CalibrateColors ();

		// Assign scanner colors
		ScanColors();

		// Update slider & dock readings
		if (_enableUI) {
			dock.UpdateDock();
			slider.UpdateSlider ();
		}

		if (_debug)
			PrintMatrix ();

		if (setup)
			setup = false;

		if (Time.frameCount % 60 == 0)
			System.GC.Collect();
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
		idBuffer = new Queue<int>[numOfScannersX * numOfScannersY];

		MakeScanners ();
		SetupSampleObjects ();

		// Create UX scanners
		dock = new Dock (this.gameObject, _gridSize, _scannerScale);
		slider = new LegoSlider (this.gameObject, _scannerScale, _sliderRange);

		// Original keystoned object with webcam texture / video
		cameraKeystonedQuad = GameObject.Find("CameraKeystoneQuad");

		// Copy mesh with RenderTexture
		keystonedQuad = GameObject.Find (colorTexturedQuadName);

		_texture = new Texture2D (cameraKeystonedQuad.GetComponent<Renderer> ().material.mainTexture.width, 
			cameraKeystonedQuad.GetComponent<Renderer> ().material.mainTexture.height);

		LoadScannerSettings ();

		EventManager.TriggerEvent ("scannersInitialized");
	}

	/// <summary>
	/// Calibrates the colors based on sample points.
	/// </summary>
	private void CalibrateColors() {
		foreach (var colorSphere in colorRefSpheres) {
			UpdateSphereColor (colorSphere.Value);
			sampleColors [(int)colorSphere.Key] = colorSphere.Value.GetComponent<Renderer> ().material.color;
		}
			
		colorClassifier.SetSampledColors (ColorClassifier.SampleColor.RED, 0, sampleColors[(int)ColorClassifier.SampleColor.RED]);
		colorClassifier.SetSampledColors (ColorClassifier.SampleColor.BLACK, 0, sampleColors[(int)ColorClassifier.SampleColor.BLACK]);
		colorClassifier.SetSampledColors (ColorClassifier.SampleColor.WHITE, 0, sampleColors[(int)ColorClassifier.SampleColor.WHITE]);
	}


	private void UpdateSphereColor(GameObject sphere) {
		sphere.GetComponent<Renderer> ().material.color = new Color(sphere.transform.localPosition.x, sphere.transform.localPosition.y, sphere.transform.localPosition.z);
	}

	/// <summary>
	/// Sets the sample spheres.
	/// </summary>
	private void SetupSampleObjects() {
		sampleColors = new Color[_numColors];
		sampleColors[(int)ColorClassifier.SampleColor.RED] = Color.red;
		sampleColors[(int)ColorClassifier.SampleColor.BLACK] = Color.black;
		sampleColors[(int)ColorClassifier.SampleColor.WHITE] = Color.white;

		colorRefSpheres = new Dictionary<ColorClassifier.SampleColor, GameObject>();

		CreateColorSphere (ColorClassifier.SampleColor.RED, Color.red);
		CreateColorSphere (ColorClassifier.SampleColor.BLACK, Color.black);
		CreateColorSphere (ColorClassifier.SampleColor.WHITE, Color.white);
	}

	/// <summary>
	/// Creates the color spheres for sampling the 3D color space.
	/// </summary>
	/// <param name="color">Color.</param>
	/// <param name="c">C.</param>
	private void CreateColorSphere(ColorClassifier.SampleColor color, Color c) {
		float scale = 0.1f;
		colorRefSpheres[color] = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		colorRefSpheres[color].name = "sphere_" + color;
		colorRefSpheres[color].transform.parent = _colorSpaceParent.transform;
		colorRefSpheres[color].GetComponent<Renderer> ().material.color = c;
		colorRefSpheres[color].transform.localScale = new Vector3 (scale, scale, scale);  
		colorRefSpheres[color].transform.localPosition = new Vector3 (c.r, c.g, c.b);
	}

	/// <summary>
	/// Scans the colors.
	/// </summary>
	private void ScanColors() {
		string key = "";
		for (int i = 0; i < numOfScannersX; i += _gridSize) {
			for (int j = 0; j < numOfScannersY; j += _gridSize) {
				currentIds [i / _gridSize, j / _gridSize] = FindCurrentId(key, i, j, ref scannersList, true);
			}
		}

		if (_showDebugColors && setup)
			colorClassifier.SortColors (allColors, _colorSpaceParent);

		if (setup)
			colorClassifier.Create3DColorPlot (allColors, _colorSpaceParent);
	}


	/// <summary>
	/// Finds the current id for a block at i, j in the grid or for the dock module.
	/// </summary>
	/// <returns>The current identifier.</returns>
	/// <param name="key">Key.</param>
	/// <param name="i">The index.</param>
	/// <param name="j">J.</param>
	public int FindCurrentId(string key, int i, int j, ref GameObject[,] currScanners, bool isGrid = true) {
		key = "";
		for (int k = 0; k < _gridSize; k++) {
			for (int m = 0; m < _gridSize; m++) {
				key += FindColor (i + k, j + m, ref currScanners, isGrid); 
			}
		} 

		// keys read counterclockwise
		key = new string(key.ToCharArray().Reverse().ToArray());

		if (idList.ContainsKey (key)) {
			return (int)idList [key];
		} 
		else { // check rotation independence & return key if it is a rotation
			string keyConcat = key + key;
			foreach(string idKey in idList.Keys) {
				if (keyConcat.Contains (idKey))
					return (int)idList [idKey];
			}
		}
		return -1;
	}

	/// <summary>
	/// Finds the color below scanner item[i, j].
	/// </summary>
	/// <param name="i">The row index.</param>
	/// <param name="j">The column index.</param>
	public int FindColor(int i, int j, ref GameObject[,] currScanners, bool isGrid = true) {
		if (Physics.Raycast (currScanners [i, j].transform.position, Vector3.down, out hit, 6)) {
			// Get local tex coords w.r.t. triangle
			if (!hitTex) {
				Debug.Log ("No hit texture");
				currScanners [i, j].GetComponent<Renderer> ().material.color = Color.magenta;
				return -1;
			} else {
				int _locX = Mathf.RoundToInt (hit.textureCoord.x * hitTex.width);
				int _locY = Mathf.RoundToInt (hit.textureCoord.y * hitTex.height); 
				Color pixel = hitTex.GetPixel (_locX, _locY);
				int currID = colorClassifier.GetClosestColorId (pixel);

				if (isGrid) {
					if (_useBuffer)
						currID = GetIdAverage (i, j, currID);

					// Save colors for 3D visualization
					if (setup)
						allColors [i + numOfScannersX * j] = pixel;
				}

				Color minColor;

				// Display 3D colors & use scanned colors for scanner color
				if (_isCalibrating && isGrid) {
					minColor = pixel;
					if (_showDebugColors) {
						// Could improve by drawing only if sphere locations change
						Vector3 origin = _colorSpaceParent.transform.position;
						Debug.DrawLine (origin + new Vector3 (pixel.r, pixel.g, pixel.b), origin + new Vector3 (sampleColors [currID].r, sampleColors [currID].g, sampleColors [currID].b), pixel, 1, false);
					}
				} else 
					minColor = colorClassifier.GetColor (currID);

				// Display rays cast at the keystoned quad
				if (_showRays) {
					Debug.DrawLine (scannersList [i, j].transform.position, hit.point, pixel, 200, false);
					Debug.Log (hit.point);
				}

				// Paint scanner with the found color 
				currScanners [i, j].GetComponent<Renderer> ().material.color = minColor;

				return currID;
			}
		} else { 
			currScanners [i, j].GetComponent<Renderer> ().material.color = Color.magenta; //paint scanner with Out of bounds / invalid  color 
			return -1;
		}
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

	/// <summary>
	/// Gets the average color ID from a given number of readings defined by _bufferSize
	/// to reduce flickering in reading of video stream.
	/// </summary>
	private int GetIdAverage (int i, int j, int currID) {
		int index = i * numOfScannersX + j;

		if (idBuffer [index] == null)
			idBuffer [index] = new Queue<int> ();

		if (idBuffer [index].Count < _bufferSize)
			idBuffer [index].Enqueue (currID);
		else {
			idBuffer [index].Dequeue ();
			idBuffer [index].Enqueue (currID);
		}

		return (int) (idBuffer [index].Average ());
	}

	/// <summary>
	/// Assigns the render texture to a Texture2D.
	/// </summary>
	/// <returns>The render texture as Texture2D.</returns>
	private void AssignRenderTexture() {
		RenderTexture rt = keystonedQuad.transform.GetComponent<Renderer> ().material.mainTexture as RenderTexture;
		RenderTexture.active = rt;
		if (!hitTex)
			hitTex = new Texture2D (rt.width, rt.height, TextureFormat.RGB24, false);
		hitTex.ReadPixels (new Rect (0, 0, rt.width, rt.height), 0, 0);
	}

	/// <summary>
	/// Sets the texture from a static image or from the webcam.
	/// </summary>
	private void SetTexture() {
		if (_useWebcam) {
			if (Webcam.isPlaying())
	          {
				_texture.SetPixels((cameraKeystonedQuad.GetComponent<Renderer>().material.mainTexture as WebCamTexture).GetPixels()); //for webcam 
	          }
          	else return;
		}
		else {
			_texture.SetPixels ((cameraKeystonedQuad.GetComponent<Renderer> ().material.mainTexture as Texture2D).GetPixels ()); // for texture map 
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
	private void LoadScannerSettings() {
		Debug.Log ("Loading color sampling settings from  " + _colorSettingsFileName);

		string dataAsJson = JsonParser.loadJSON (_colorSettingsFileName, _debug);
		if (String.IsNullOrEmpty(dataAsJson)) {
			Debug.Log ("No such file: " + _colorSettingsFileName);
			return;
		}

		colorSettings = JsonUtility.FromJson<ColorSettings>(dataAsJson);

		if (colorSettings == null) return;
		if (colorSettings.color == null) return;

		for (int i = 0; i < colorSettings.color.Count; i++) {
			sampleColors [i] = colorSettings.color [i];
			colorRefSpheres [(ColorClassifier.SampleColor)i].GetComponent<Renderer> ().material.color = colorSettings.color [i];
			colorRefSpheres [(ColorClassifier.SampleColor)i].transform.localPosition = new Vector3 (colorSettings.color [i].r, colorSettings.color [i].g, colorSettings.color [i].b); 
		}
			
		_gridParent.transform.position = colorSettings.gridPosition;

		dock.SetDockPosition (colorSettings.dockPosition);
	}

	/// <summary>
	/// Saves the sampler objects (color & dock etc positions) to a JSON.
	/// </summary>
	private void SaveScannerSettings() {
		Debug.Log ("Saving scanner settings to " + _colorSettingsFileName);

		if (colorSettings == null || colorSettings.color == null) {
			colorSettings = new ColorSettings ();
		}

		for (int i = 0; i < sampleColors.Length; i++) {
			if (colorSettings.id.Count <= i) {
				colorSettings.id.Add (i);
				colorSettings.color.Add (sampleColors [i]);
			} else {
				colorSettings.id [i] = i;
				colorSettings.color [i] = sampleColors [i];
			}
		}

		colorSettings.gridPosition = _gridParent.transform.position;
		colorSettings.dockPosition = dock.GetDockPosition ();

		string dataAsJson = JsonUtility.ToJson (colorSettings);
		JsonParser.writeJSON (_colorSettingsFileName, dataAsJson);
	}

	/// <summary>
	/// Raises the scene control event.
	/// </summary>
	private void onKeyPressed ()
	{
		if (Input.GetKey (KeyCode.S)) {
			SaveScannerSettings ();
		} else if (Input.GetKey (KeyCode.L)) {
			LoadScannerSettings ();
		}
	}

	/// <summary>
	/// Reloads configuration / keystone settings when the scene is refreshed.
	/// </summary>
	public void OnReload() {
		Debug.Log ("Scanner config was reloaded!");
		SetupSampleObjects ();
		LoadScannerSettings ();
	}

	public void OnSave() {
		SaveScannerSettings ();
	}

	/////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////
	/// 
	/// GETTERS
	/// 
	/////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////

	/// <summary>
	/// Gets the current identifiers.
	/// </summary>
	/// <returns>The current identifiers.</returns>
	public int[,] GetCurrentIds() {
		int[,] ids = currentIds.Clone () as int[,];
		return ids;
	}

	public Vector2 GetGridDimensions() {
		return (new Vector2 (numOfScannersX * 0.5f, numOfScannersY * 0.5f));
	}

	public int GetDockId() {
		return this.dock.GetDockId ();
	}

	public int GetSliderValue() {
		return this.slider.GetSliderValue ();
	}
}