using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Parse a txt File from sting to int.
/// </summary>
public class AsciiParser : MonoBehaviour
{

    /// <param name="txtFile">Text file.</param>
    public static List<int> AsciiParserMethod(TextAsset txtFile)
    {
        List<int> _tmpList = new List<int>();
        string _txtRemovedNewlines = txtFile.text.Replace('\n', ' ');
        string[] _strValues = _txtRemovedNewlines.Split(' ');
        foreach (string str in _strValues)
        {
            var _outVal = 0; // must have out value for this method
            if (int.TryParse(str, out _outVal))
            {
                _tmpList.Add(System.Int32.Parse(str));
            }
        }
        return _tmpList; // the temp list that this method returns
    }
}