using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ResponseHandler
{
    public static void HandleResponse(string content)
    {
        var startIndex = content.IndexOf("```json", StringComparison.Ordinal) + 7;
        var endIndex = content.IndexOf("```", startIndex, StringComparison.Ordinal);
        if (startIndex >= 7 && endIndex > startIndex)
        {
            var diff = content.Substring(startIndex, endIndex - startIndex).Trim();
            DiffViewerEditorWindow.ShowWindow(BuildUidMap(), diff);
        }
        else
        {
            Debug.LogWarning("No valid JSON diff found in the response.");
        }
    }
    
    private static Dictionary<string, GameObject> BuildUidMap(GameObject[] selection = null)
    {
        if (selection == null || selection.Length == 0)
        {
            selection = Selection.gameObjects;
        }
        
        return selection
            .Select((obj, idx) => new { obj, index = idx })
            .ToDictionary(pair => pair.index.ToString(), pair => pair.obj);
    }
}