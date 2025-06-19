using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

public class ResponseHandler
{
    [CanBeNull]
    public static string GetJsonContent(string content)
    {
        var startIndex = content.IndexOf("```json", StringComparison.Ordinal) + 7;
        var endIndex = content.IndexOf("```", startIndex, StringComparison.Ordinal);
        if (startIndex >= 7 && endIndex > startIndex)
            return content.Substring(startIndex, endIndex - startIndex).Trim();
        return null;
    }

    public static void ShowDiffViewer(string diffJson)
    {
        DiffViewerEditorWindow.ShowWindow(BuildUidMap(), diffJson);
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