using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DiffViewerEditorWindow : EditorWindow
{
    private SceneDiff[] _diffs = { };
    private Dictionary<GameObject, bool> _goFoldouts = new();
    private Dictionary<(GameObject, Type), bool> _compFoldouts = new();
    private Dictionary<int, bool> _selectedDiffs = new();
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

        var goGroups = _diffs
            .Select((d, idx) => (diff: d, idx))
            .GroupBy(pair => pair.diff.GameObject ?? pair.diff.Component?.gameObject)
            .ToList();

        foreach (var goGroup in goGroups)
        {
            DrawGameObjectDiff(goGroup);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Apply Selected Diffs"))
        {
            ApplySelectedDiffs();
            Close();
        }
    }

    #region Diff Drawing
    
    private void DrawGameObjectDiff(IGrouping<GameObject, (SceneDiff diff, int)> goGroup)
    {
        var go = goGroup.Key;
        _goFoldouts.TryAdd(go, true);
        _goEnabled.TryAdd(go, true);

        EditorGUILayout.BeginHorizontal();
        bool prevGoEnabled = _goEnabled[go];
        _goEnabled[go] = GUI.Toggle(ToggleRect(), _goEnabled[go], GUIContent.none);
        _goFoldouts[go] = EditorGUI.Foldout(FoldoutRect(go.name), _goFoldouts[go], go.name, true);
        EditorGUILayout.EndHorizontal();

        // If GameObject checkbox changed, update all children
        if (_goEnabled[go] != prevGoEnabled)
        {
            foreach (var compGroup in goGroup.GroupBy(pair => pair.diff.ComponentType))
            {
                var compKey = (go, compGroup.Key);
                _compEnabled[compKey] = _goEnabled[go];
                foreach (var (diff, idx) in compGroup)
                    _selectedDiffs[idx] = _goEnabled[go];
            }
        }

        if (_goFoldouts[go])
        {
            EditorGUI.indentLevel++;
            var compGroups = goGroup.GroupBy(pair => pair.diff.ComponentType);
            foreach (var compGroup in compGroups)
            {
                DrawComponentDiff(go, compGroup);
            }

            EditorGUI.indentLevel--;
        }
    }

    private void DrawComponentDiff(GameObject go, IGrouping<Type, (SceneDiff, int)> compGroup)
    {
        var compType = compGroup.Key;
        var compKey = (go, compType);
        _compFoldouts.TryAdd(compKey, true);
        _compEnabled.TryAdd(compKey, _goEnabled[go]);

        EditorGUILayout.BeginHorizontal();
        bool prevCompEnabled = _compEnabled[compKey];
        EditorGUI.BeginDisabledGroup(!_goEnabled[go]);
        _compEnabled[compKey] = GUI.Toggle(ToggleRect(), _compEnabled[compKey], GUIContent.none);
        EditorGUI.EndDisabledGroup();
        _compFoldouts[compKey] = EditorGUI.Foldout(FoldoutRect(go.name), _compFoldouts[compKey], compType.Name, true);
        EditorGUILayout.EndHorizontal();

        // If component checkbox changed, update all children
        if (_compEnabled[compKey] != prevCompEnabled)
        {
            foreach (var (diff, idx) in compGroup)
                _selectedDiffs[idx] = _compEnabled[compKey] && _goEnabled[go];
        }

        if (_compFoldouts[compKey])
        {
            EditorGUI.indentLevel++;
            foreach (var (diff, idx) in compGroup)
            {
                DrawDiff(go, compKey, idx, diff, compType);
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(10);
    }

    private void DrawDiff(GameObject go, (GameObject, Type) compKey, int idx, SceneDiff diff, Type compType)
    {
        bool enabled = _goEnabled[go] && _compEnabled[compKey];
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        EditorGUI.BeginDisabledGroup(!enabled);
        _selectedDiffs[idx] =
            GUI.Toggle(ToggleRect(), _selectedDiffs[idx], GUIContent.none) && enabled;
        EditorGUI.EndDisabledGroup();

        switch (diff.DiffType)
        {
            case SceneDiffType.PropertyChange:
                EditorGUILayout.LabelField(diff.Property.Name, GUILayout.Width(120));
                EditorGUILayout.LabelField("Old:", GUILayout.Width(30));
                GUI.contentColor = Color.yellow;
                EditorGUILayout.LabelField(diff.OldValue?.ToString() ?? "null",
                    GUILayout.MaxWidth(175));
                GUI.contentColor = Color.white;
                EditorGUILayout.LabelField("New:", GUILayout.Width(30));
                GUI.contentColor = Color.green;
                EditorGUILayout.LabelField(diff.NewValue?.ToString() ?? "null",
                    GUILayout.MaxWidth(175));
                GUI.contentColor = Color.white;
                break;
            case SceneDiffType.AddComponent:
                GUI.contentColor = Color.cyan;
                EditorGUILayout.LabelField($"Add Component: {compType.Name}");
                GUI.contentColor = Color.white;
                break;
            case SceneDiffType.RemoveComponent:
                GUI.contentColor = Color.red;
                EditorGUILayout.LabelField($"Remove Component: {compType.Name}");
                GUI.contentColor = Color.white;
                break;
        }

        EditorGUILayout.EndHorizontal();
    }

    #endregion
    
    #region Diff System

    private void Initialize(SceneDiff[] diffs)
    {
        _diffs = diffs
            .Where(d => d.OldValue != d.NewValue || d.OldValue == null || d.NewValue == null)
            .ToArray();
        _selectedDiffs = _diffs
            .Select((_, i) => i)
            .ToDictionary(i => i, i => true);
        _goFoldouts.Clear();
        _compFoldouts.Clear();
        
        // Initialize game object and componentType for each diff
        foreach (var sceneDiff in _diffs)
        {
            sceneDiff.GameObject ??= sceneDiff.Component?.gameObject;
            sceneDiff.ComponentType ??= sceneDiff.Component?.GetType();
        }
    }

    private void ApplySelectedDiffs()
    {
        var orderedDiffs = _diffs
            .Select((diff, idx) => (diff, idx))
            .Where(pair => _selectedDiffs.ContainsKey(pair.idx) && _selectedDiffs[pair.idx])
            .OrderBy(pair => pair.diff.DiffType == SceneDiffType.AddComponent ? 0 : 1)
            .Select(pair => pair.diff)
            .ToArray();

        foreach (var diff in orderedDiffs)
            ResponseHandler.ApplyDiff(diff);
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