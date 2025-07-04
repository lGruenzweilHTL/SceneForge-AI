using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class DiffViewerEditorWindow : EditorWindow
{
    private (SceneDiff diff, bool selected)[] _diffs = { };
    private Dictionary<GameObject, bool> _goFoldouts = new();
    private Dictionary<(GameObject, Type), bool> _compFoldouts = new();
    private Dictionary<GameObject, bool> _goEnabled = new();
    private Dictionary<(GameObject, Type), bool> _compEnabled = new();
    
    private static Rect ToggleRect() => GUILayoutUtility.GetRect(18, 18, GUILayout.Width(18));
    private static Rect FoldoutRect(string name) => GUILayoutUtility.GetRect(
        new GUIContent(name), EditorStyles.foldout, GUILayout.ExpandWidth(true)
    );
    
    public static void ShowWindow(SceneDiff[] diffs)
    {
        var window = GetWindow<DiffViewerEditorWindow>("Diff Viewer");
        window.minSize = new Vector2(200, 100);
        window.Initialize(diffs);

        window.Show();
    }


    private void OnGUI()
    {
        if (_diffs == null || _diffs.Length == 0)
        {
            EditorGUILayout.LabelField("No differences detected.", EditorStyles.boldLabel);
            if (GUILayout.Button("Close")) Close();
            return;
        }

        EditorGUILayout.LabelField("Scene Differences", HeaderStyles.HeaderStyle);
        EditorGUILayout.Space();

        var diffsByObject = _diffs
            .GroupBy(pair => pair.diff.InstanceId)
            .ToList();

        foreach (var grouping in diffsByObject)
        {
            var gameObject = ObjectUtility.FindByInstanceId(grouping.Key);
            GUILayout.Label(gameObject?.name ?? "Undefined", EditorStyles.boldLabel);

            for (int i = 0; i < grouping.Count(); i++)
            {
                var diff = grouping.ElementAt(i);
                GUILayout.BeginHorizontal();
                diff.selected = GUI.Toggle(ToggleRect(), diff.selected, "");
                GUILayout.Label(diff.diff.ToString(), EditorStyles.boldLabel);
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Apply Selected Diffs"))
        {
            ApplySelectedDiffs();
            Close();
        }
    }
    
    #region Diff System

    private void Initialize(SceneDiff[] diffs)
    {
        // Filter out all diffs that don't actually change anything, default selections to true
        _diffs = diffs
            .Where(d => d is not UpdatePropertyDiff propDiff || 
                        (propDiff.OldValue != null && propDiff.NewValue != null &&
                         !propDiff.OldValue.Equals(propDiff.NewValue)))
            .Select(diff => (diff, true))
            .ToArray();
        _goFoldouts.Clear();
        _compFoldouts.Clear();
    }

    private void ApplySelectedDiffs()
    {
        var orderedDiffs = _diffs
            .Select((diff, idx) => (diff, idx))
            .Where(pair => _diffs[pair.idx].selected)
            .OrderBy(pair => pair.diff.diff.Priority)
            .Select(pair => pair.diff)
            .ToArray();

        foreach (var diff in orderedDiffs)
            ResponseHandler.ApplyDiff(diff.diff);
    }

    #endregion

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