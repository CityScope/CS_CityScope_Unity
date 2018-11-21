using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SiteData : MonoBehaviour {

	/// <summary>
	/// The ASCII types txt files.
	/// </summary>
	public TextAsset _asciiTypes;
	/// <summary>
	/// The ASCII floors txt files.
	/// </summary>
	public TextAsset _asciiFloors;
	/// <summary>
	/// The ASCII masks txt files.
	/// </summary>
	public TextAsset _asciiMasks;
	private List<int> _typesList = new List<int>();
	private List<int> _floorsList = new List<int>();
	private List<int> _masksList = new List<int>();

	private int interactiveIndex;
	private Vector2 interactiveGridLocation;
	private Vector2 interactiveGridDim;

	/// <summary>
	/// to be replaced with x,y dim from ascii parsing
	/// </summary>
	public int _gridX;
	public int _gridY;

	private bool setup;

	private enum Mask { INTERACTIVE = 0, GRID = 1, FULL_SITE = 2, OUTSIDE = 3 };

	// Use this for initialization
	void Awake () {
		setup = true;
		_floorsList = AsciiParser.AsciiParserMethod(_asciiFloors);
		_typesList = AsciiParser.AsciiParserMethod(_asciiTypes);
		_masksList = AsciiParser.AsciiParserMethod(_asciiMasks);

		GameObject scannersParent = GameObject.Find ("ScannersParent");
		if (scannersParent != null)
			EventManager.StartListening ("scannersInitialized", FindInteractiveZone);
		else
			FindInteractiveZone ();

	}

	void Update() {
		if (_masksList.Count > 0 && setup) {
			setup = false;
			EventManager.TriggerEvent ("siteInitialized");
		}
	}

	private void FindInteractiveZone() {
		interactiveIndex = -1;
		int index = 0;
		interactiveGridLocation = new Vector2 (-1, -1);
		interactiveGridDim = new Vector2 (0, 0);

		// Find location of interactive grid
		for (int i = 0; i < _gridX; i++) {
			for (int j = 0; j < _gridY; j++) {
				if (_masksList[index] == (int) Mask.GRID && interactiveIndex < 0) {
					interactiveIndex = index;
					interactiveGridLocation.x = j;
					interactiveGridLocation.y = i;
					Debug.Log ("Mask index is " + interactiveIndex);
				}
				if (interactiveGridLocation.x == j && _masksList[index] == (int) Mask.GRID) {
					interactiveGridDim.y++;
				}
				if (interactiveGridLocation.y == i && _masksList[index] == (int) Mask.GRID) {
					interactiveGridDim.x++;
				}
					
				index++;
			}
		}

		// Assume square grid
		interactiveGridDim.y = interactiveGridDim.x;

		Debug.Log ("Interactive grid starts at " + interactiveGridLocation + "   and has dimensions:   " + interactiveGridDim + " with index " + interactiveIndex);
	}

	/////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////
	/// 
	/// GETTERS
	/// 
	/////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////

	public Vector2 GetInteractiveGridLocation() {
		return this.interactiveGridLocation;
	}

	public Vector2 GetGridSize() {
		return new Vector2 (this._gridX, this._gridY);
	}

	public Vector2 GetInteractiveGridDim() {
		return interactiveGridDim;
	}

	public int GetInteractiveIndex() {
		return interactiveIndex;
	}

	public int GetFloor(int index) {
		if (_floorsList.Count > index)
			return _floorsList [index];
		else
			return -1;
	}

	public int GetMaxFloor() {
		return Mathf.Abs (_floorsList.Max ());
	}

	public int GetMinFloor() {
		return Mathf.Abs (_floorsList.Min ());
	}

	public int GetType(int index) {
		if (_typesList.Count > index)
			return _typesList [index];
		else
			return -1;
	}

	/// <summary>
	/// Determines whether the mask shows that the module is interactive at the specified index.
	/// </summary>
	/// <returns><c>true</c> if this instance is interactive the specified index; otherwise, <c>false</c>.</returns>
	/// <param name="index">Index.</param>
	public bool IsIndexInsideInteractiveArea(int index) {
		return (_masksList [index] == (int)Mask.INTERACTIVE);
	}

	public bool IsInInteractive(int id) {
		return (id == (int)Mask.INTERACTIVE);
	}

	/// <summary>
	/// Returns the value of the mask (from the ASCII file) at the given location in the Table grid.
	/// </summary>
	/// <returns>The mask.</returns>
	/// <param name="index">Index.</param>
	public int GetMask(int index) {
        if (interactiveGridDim.y == 0)
            return -1;

		// I and J are flipped for the full site!
		int currI = index % (int) interactiveGridDim.y;
		int currJ = (int) (index / interactiveGridDim.y);
		int i = (int) interactiveGridLocation.x + currI;
		int j = (int) interactiveGridLocation.y + currJ;
		int remappedIndex = j * (int)(_gridY) + i;

		return _masksList[remappedIndex];
	}

	public List<int> GetFloors() {
		return this._floorsList;
	}

	public List<int> GetTypes() {
		return this._typesList;
	}

}
