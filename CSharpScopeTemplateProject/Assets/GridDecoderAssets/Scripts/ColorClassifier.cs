using System;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ColorClassifier {
	
	public Vector2[] colorRanges;
	private GameObject[] debugColorObjectsH;
	private GameObject[] debugColorObjectsL;
	Dictionary<int, Vector3> hsvColors;

	private string colorScaleParentName = "Scale visualization";
	public GameObject colorScaleParent;

	// red, black, white
	// 0 - white
	// 1 - black
	// 2 - red
	private Color[] rgbColor = {
		new Color (1f, 1f, 1f),
		new Color (0f, 0f, 0f),
		new Color (1f, 0f, 0f)
	};

	public enum SampleColor { WHITE = 0, BLACK = 1, RED = 2 };
	private Dictionary<SampleColor, List<Vector3>> sampleColors = new Dictionary<SampleColor, List<Vector3>>();

	/// <summary>
	/// Finds the closest color to the given scan colors.
	/// </summary>
	/// <returns>The closest color's index in the colors array.</returns>
	/// <param name="pixel">Pixel.</param>
	public int GetClosestColorId(Color pixel) {
		Vector3 currPixel = new Vector3 (pixel.r, pixel.g, pixel.b);
		double minDistance = Double.PositiveInfinity;
		int minColorID = -1;

		foreach (var color in sampleColors) {
			for (int i = 0; i < color.Value.Count; i++) {
				double currDistance = Vector3.Distance (color.Value [i], currPixel);
				if (currDistance < minDistance) {
					minDistance = currDistance;
					minColorID = (int) color.Key;
				}
			}
		}

		return minColorID;
	}

	/// <summary>
	/// Sets the sampled colors.
	/// </summary>
	public void SetSampledColors(SampleColor color, int index, Color pixel) {
		if (!sampleColors.ContainsKey(color)) {
			sampleColors.Add (color, new List<Vector3>{});
		}
		if (sampleColors [color].Count <= index)
			sampleColors [color].Add (new Vector3 (pixel.r, pixel.g, pixel.b));
		else 
			sampleColors[color][index] = new Vector3 (pixel.r, pixel.g, pixel.b);
	}

	/// <summary>
	/// Gets the color.
	/// </summary>
	/// <returns>The color.</returns>
	public Color GetColor(int id) {
		Color currColor = rgbColor[id];
		return currColor;
	}


	/// <summary>
	/// Sorts the colors.
	/// </summary>
	public Vector3 ToCustomHSV(Color rgbColor) {
		float H, S, V;
		Color.RGBToHSV(rgbColor, out H, out S, out V);

		float lum = (float) Math.Sqrt (0.241f * rgbColor.r + 0.691f * rgbColor.g + 0.068f * rgbColor.b);
		//Vector3 hsvVector = new Vector3 ((int)(H * 7), (int)(lum * 7), (int)(V * 7));
		Vector3 hsvVector = new Vector3 ((H * 7), (lum * 7), (V * 7));
		//Vector3 hsvVector = new Vector3 (H, S, V);

		return hsvVector;
	}

	private void createDebugObjects(int length) {
		if (debugColorObjectsH == null) {
			float sizeX = 0.005f;
			float sizeY = 2f;
			float locationZ = -10;

			colorScaleParent = GameObject.Find (colorScaleParentName);
			debugColorObjectsH = new GameObject[length];
			debugColorObjectsL = new GameObject[length];
			float locationX = colorScaleParent.transform.position.x;

			for (int i = 0; i < length; i++) {
				debugColorObjectsH [i] = GameObject.CreatePrimitive (PrimitiveType.Quad);
				debugColorObjectsH [i].transform.localScale = new Vector3 (sizeX, sizeY, 1);  
				debugColorObjectsH [i].transform.position = new Vector3 (i * sizeX + locationX, 0, locationZ);
				debugColorObjectsH [i].transform.Rotate (90, 0, 0); 
				debugColorObjectsH [i].transform.parent = colorScaleParent.transform;

				debugColorObjectsL [i] = GameObject.CreatePrimitive (PrimitiveType.Quad);
				debugColorObjectsL [i].transform.localScale = new Vector3 (sizeX, sizeY, 1);  
				debugColorObjectsL [i].transform.position = new Vector3 (i * sizeX + locationX, 0, locationZ + sizeY * 2);
				debugColorObjectsL [i].transform.Rotate (90, 0, 0); 
				debugColorObjectsL [i].transform.parent = colorScaleParent.transform;
			}

			hsvColors = new Dictionary<int, Vector3> ();
		}
	}

	public void SortColors(Color[] colors) {
		createDebugObjects (colors.Length);

		for (int i = 0; i < colors.Length; i++) {
			hsvColors [i] = ToCustomHSV (colors [i]);
		}

		Dictionary<int, Vector3> hsvColorsSorted = hsvColors.OrderBy(x => x.Value.x).ToDictionary(x => x.Key, x => x.Value);

		int index = 0;
		foreach (var item in hsvColorsSorted) {
			Color rgbColor = colors [item.Key];
			debugColorObjectsH[index++].GetComponent<Renderer> ().material.color = rgbColor;
		}

		Dictionary<int, Vector3> hsvColorsSortedL = hsvColors.OrderBy(x => x.Value.y).ToDictionary(x => x.Key, x => x.Value);

		index = 0;
		foreach (var item in hsvColorsSortedL) {
			Color rgbColor = colors [item.Key];
			debugColorObjectsL[index++].GetComponent<Renderer> ().material.color = rgbColor;
		}
	}
}
