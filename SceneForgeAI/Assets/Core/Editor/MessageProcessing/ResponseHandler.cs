using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

public static class ResponseHandler
{
    [CanBeNull]
    public static string GetJsonContent(string content)
    {
        var startIndex = content.IndexOf("```json", StringComparison.Ordinal) + 7;
        if (startIndex < 7) return null; // No valid start found
        
        var endIndex = content.IndexOf("```", startIndex, StringComparison.Ordinal);
        if (endIndex > startIndex)
            return content.Substring(startIndex, endIndex - startIndex).Trim();
        
        return null;
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
    
    public static SceneDiff[] GenerateDiffs(string json) => SceneDiffHandler.GetDiffFromScene(json, BuildUidMap());
    
    public static void ApplyDiff(SceneDiff diff)
    {
        switch (diff.DiffType)
        {
            case SceneDiffType.PropertyChange:
                var component = diff.Component ?? diff.GameObject.GetComponent(diff.ComponentType);
                if (!component)
                {
                    Debug.LogWarning(
                        $"Component {diff.ComponentType.Name} not found on GameObject {diff.GameObject.name}. " +
                        "This might be due to a missing AddComponent diff.");
                    return;
                }

                component.GetType()
                    .GetProperty(diff.Property.Name)
                    ?.SetValue(component, diff.NewValue);
                break;
            case SceneDiffType.AddComponent:
                diff.GameObject.AddComponent(diff.ComponentType);
                break;
            case SceneDiffType.RemoveComponent:
                UnityEngine.Object.DestroyImmediate(diff.GameObject.GetComponent(diff.ComponentType));
                break;
        }
    }
}