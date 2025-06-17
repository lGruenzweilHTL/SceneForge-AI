using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DiffViewerEditorWindow : EditorWindow
{
    private Dictionary<string, GameObject> _uidMap;
    private string _diffString = string.Empty;
    private SceneDiff[] _diffs = { };
    private bool[] _selectedDiffs = { };
    
    public static void ShowWindow(Dictionary<string, GameObject> uidMap, string diffString)
    {
        DiffViewerEditorWindow window = GetWindow<DiffViewerEditorWindow>("Diff Viewer");
        window.minSize = new Vector2(400, 300);
        
        window._uidMap = uidMap;
        window._diffString = diffString;
        window.GenerateDiff();
        
        window.Show();
    }

    private void OnGUI()
    {
        if (_diffs != null && _diffs.Length > 0)
        {
            EditorGUILayout.LabelField("Detected Diffs:", EditorStyles.boldLabel);
            for (int i = 0; i < _diffs.Length; i++)
            {
                var sceneDiff = _diffs[i];
                _selectedDiffs[i] = EditorGUILayout.ToggleLeft(
                    $"{sceneDiff.DiffType} - {sceneDiff.ComponentType?.Name ?? sceneDiff.Component?.GetType().Name} " +
                    $"{(sceneDiff.Property != null ? $"Property: {sceneDiff.Property.Name}" : "")} " +
                    $"{(sceneDiff.OldValue != null ? $"Old: {sceneDiff.OldValue}" : "")} " +
                    $"{(sceneDiff.NewValue != null ? $"New: {sceneDiff.NewValue}" : "")}",
                    _selectedDiffs[i]);
            }

            if (GUILayout.Button("Apply Selected Diffs"))
            {
                ApplySelectedDiffs();
            }
        }
        else 
        {
            EditorGUILayout.LabelField("No diffs detected or available.");
        }
    }

    private void GenerateDiff()
    {
        var diff = SceneDiffHandler.GetDiffFromScene(_diffString, _uidMap);
        // Filter out diffs where OldValue and NewValue are the same
        _diffs = diff
            .Where(d => d.OldValue != d.NewValue || d.OldValue == null || d.NewValue == null)
            .ToArray();
        _selectedDiffs = Enumerable.Repeat(true, _diffs.Length).ToArray(); // Default all to selected
    }

    private void ApplySelectedDiffs()
    {
        // Ensure AddComponent diffs are applied before PropertyChange diffs
        var orderedDiffs = _diffs
            .Select((diff, idx) => (diff, idx))
            .Where(pair => _selectedDiffs[pair.idx])
            .OrderBy(pair => pair.diff.DiffType == SceneDiffType.AddComponent ? 0 : 1)
            .Select(pair => pair.diff)
            .ToArray();

        foreach (var diff in orderedDiffs)
            ApplyDiff(diff);
    }

    private static void ApplyDiff(SceneDiff diff)
    {
        switch (diff.DiffType)
        {
            case SceneDiffType.PropertyChange:
                // If component is null, try to get it (it may have just been added)
                var component = diff.Component ?? diff.GameObject.GetComponent(diff.ComponentType);
                if (!component)
                {
                    Debug.LogWarning($"Component {diff.ComponentType.Name} not found on GameObject {diff.GameObject.name}. " +
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
                DestroyImmediate(diff.GameObject.GetComponent(diff.ComponentType));
                break;
        }
    }
    
    #region Auto-Close
    
    private void OnEnable()
    {
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
    }

    private void OnDisable()
    {
        AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
    }

    private void OnBeforeAssemblyReload()
    {
        Close();
    }
    
    #endregion
}