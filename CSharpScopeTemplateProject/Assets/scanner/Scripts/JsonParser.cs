using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;  

public class JsonParser {

	/// <summary>
	/// Loads a JSON file from fileName.
	/// Following https://unity3d.com/learn/tutorials/topics/scripting/loading-game-data-json
	/// </summary>
	/// <returns><c>true</c>, if JSO was loaded, <c>false</c> otherwise.</returns>
	/// <param name="fileName">File name.</param>
	public static string loadJSON(string _settingsFileName, bool debug) {
		string filePath = "Assets/Settings/" +  _settingsFileName;

		if (File.Exists(filePath))
		{
			// Read the json from the file into a string
			string dataAsJson = File.ReadAllText(filePath); 

			if (debug) 
				Debug.Log("Data loaded.");
			return dataAsJson;
		}
		else
		{
			Debug.LogError("Cannot load data from " + filePath);
			return "";
		}
	}

	/// <summary>
	/// Writes the JSON
	/// </summary>
	/// <returns><c>true</c>, if JSO was writed, <c>false</c> otherwise.</returns>
	public static bool writeJSON(string _settingsFileName, string dataAsJson) {
		string filePath = "Assets/Settings/" + _settingsFileName;
		File.WriteAllText (filePath, dataAsJson);
		return true;
	}
}
