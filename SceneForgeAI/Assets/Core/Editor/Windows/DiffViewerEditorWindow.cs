using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DiffViewerEditorWindow : EditorWindow
{
    private Dictionary<string, GameObject> _uidMap;
    private string _diffString = string.Empty;
    private SceneDiff[] _diffs = { };
    private Dictionary<GameObject, bool> _goFoldouts = new();
    private Dictionary<(GameObject, Type), bool> _compFoldouts = new();
    private Dictionary<int, bool> _selectedDiffs = new();
    private Dictionary<GameObject, bool> _goEnabled = new();
    private Dictionary<(GameObject, Type), bool> _compEnabled = new();
    
    private Rect ToggleRect() => GUILayoutUtility.GetRect(18, 18, GUILayout.Width(18));
    private Rect FoldoutRect(string name) => GUILayoutUtility.GetRect(
        new GUIContent(name), EditorStyles.foldout, GUILayout.ExpandWidth(true)
    );

    #region Demo

    [MenuItem("Tools/Demo/Diff Viewer Demo")]
    public static void ShowDemoWindow()
    {
        // Demo GameObjects (not added to the scene)
        var go1 = new GameObject("Player");
        var go2 = new GameObject("Enemy");

        // Demo UID map
        var uidMap = new Dictionary<string, GameObject>
        {
            { "uid_player", go1 },
            { "uid_enemy", go2 }
        };

        // Demo diffs
        var diffs = new[]
        {
            new SceneDiff
            {
                GameObject = go1,
                ComponentType = typeof(Transform),
                DiffType = SceneDiffType.PropertyChange,
                Property = typeof(Transform).GetProperty("position"),
                OldValue = new Vector3(0, 0, 0),
                NewValue = new Vector3(1, 2, 3)
            },
            new SceneDiff
            {
                GameObject = go1,
                ComponentType = typeof(BoxCollider),
                DiffType = SceneDiffType.AddComponent
            },
            new SceneDiff
            {
                GameObject = go1,
                ComponentType = typeof(BoxCollider),
                DiffType = SceneDiffType.PropertyChange,
                Property = typeof(BoxCollider).GetProperty("size"),
                OldValue = new Vector3(1, 1, 1),
                NewValue = new Vector3(2, 2, 2)
            },
            new SceneDiff
            {
                GameObject = go2,
                ComponentType = typeof(Transform),
                DiffType = SceneDiffType.PropertyChange,
                Property = typeof(Transform).GetProperty("localScale"),
                OldValue = new Vector3(1, 1, 1),
                NewValue = new Vector3(2, 2, 2)
            }
        };

        // Create and show the window
        var window = GetWindow<DiffViewerEditorWindow>("Diff Viewer Demo");
        window.minSize = new Vector2(500, 400);
        window._uidMap = uidMap;
        window._diffString = ""; // Not used in demo
        window._diffs = diffs;
        window._selectedDiffs = diffs.Select((_, i) => new { i }).ToDictionary(x => x.i, x => true);
        window._goFoldouts.Clear();
        window._compFoldouts.Clear();
        window.Show();
    }

    #endregion

    public static void ShowWindow(Dictionary<string, GameObject> uidMap, string diffString)
    {
        var window = GetWindow<DiffViewerEditorWindow>("Diff Viewer");
        window.minSize = new Vector2(500, 400);

        window._uidMap = uidMap;
        window._diffString = diffString;
        window.GenerateDiff();

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

    private void GenerateDiff()
    {
        var diff = SceneDiffHandler.GetDiffFromScene(_diffString, _uidMap);
        _diffs = diff
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
            ApplyDiff(diff);
    }

    private static void ApplyDiff(SceneDiff diff)
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
                DestroyImmediate(diff.GameObject.GetComponent(diff.ComponentType));
                break;
        }
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