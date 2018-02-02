
/// <summary> 
/// Class that mirrors data format from cityIO server JSON files (or local source) and allow parsing of data across project
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Brick { RL = 0, RM = 1, RS = 2, OL = 3, OM = 4, OS = 5, ROAD = 6, AMENITIES = 7, PARK = 8, STREET = 9, INVALID = -1, MASK = -2 };

[System.Serializable]  // have to have this in every JSON class!
public class Grid
{
    public int type;
    public int x;
    public int y;
    public int rot;
	private bool update;

	public bool ShouldUpdate() {
		return this.update;
	}

	public void SetUpdate(bool up) {
		this.update = up;
	}
}

[System.Serializable] // have to have this in every JSON class!
public class Objects
{
    public float slider1;
	public float slider2;
    public int toggle1;
    public int toggle2;
    public int toggle3;
    public int toggle4;
    public int dockID;
    public int dockRotation;
    public int IDMax;
    public List<int> density;
    public int pop_young;
    public int pop_mid;
    public int pop_old;

	public void SetDockId(int newDockId) {
		if (this.dockID == newDockId)
			return;
		
		this.dockID = newDockId;
		UpdateDensity ();
	}

	public void SetSlider(int newSliderVal) {
		slider1 = newSliderVal;
		UpdateDensity ();
	}

	private void UpdateDensity() {
		if (this.dockID >= 0 && this.dockID < density.Count)
			this.density [dockID] = (int) slider1;
	}
}

/// <summary> class end </summary>
