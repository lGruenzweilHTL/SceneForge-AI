using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class DiffViewerEditorWindow : EditorWindow
{
    private SceneDiff[] _diffs = { };
    private Dictionary<int, bool> _goFoldouts = new();
    private Dictionary<(int groupKey, string componentType), bool> _componentFoldouts = new();
    private Dictionary<SceneDiff, bool> _diffSelections = new();
    private Dictionary<string, Texture> _componentIcons = new();

    [MenuItem("Tools/Diff/Demo")]
    public static void ShowDemoWindow()
    {
        var diffs = new SceneDiff[]
        {
            new CreateObjectDiff { Name = "TestObject", TempId = "obj1" },
            new AddComponentDiff { TempId = "obj1", ComponentType = "BoxCollider" },
            new UpdatePropertyDiff { TempId = "obj1", ComponentType = "BoxCollider", PropertyName = "isTrigger", OldValue = false, NewValue = true },
            new AddComponentDiff { TempId = "obj1", ComponentType = "Health" }
        };
        ShowWindow(diffs);
    }

    public static void ShowWindow(SceneDiff[] diffs)
    {
        var window = GetWindow<DiffViewerEditorWindow>("Diff Viewer");
        window.minSize = new Vector2(350, 250);
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

        EditorGUILayout.LabelField("Scene Differences", HeaderStyles.SubheaderStyle);
        EditorGUILayout.Space();

        var diffsByObject = _diffs.GroupBy(diff => diff.InstanceId ?? diff.TempId?.GetHashCode() ?? 0);

        foreach (var group in diffsByObject)
        {
            int groupKey = group.Key;
            string label = GetLabelForGroup(group.First());

            _goFoldouts.TryAdd(groupKey, true);
            _goFoldouts[groupKey] = EditorGUILayout.Foldout(_goFoldouts[groupKey], label, true);

            if (_goFoldouts[groupKey])
            {
                EditorGUI.indentLevel++;

                var createDiff = group.FirstOrDefault(d => d is CreateObjectDiff);
                bool objectEnabled = createDiff == null || IsDiffSelected(createDiff);

                foreach (var compGroup in group
                    .GroupBy(d => (d as IComponentDiff)?.ComponentType ?? string.Empty))
                {
                    string compType = compGroup.Key;
                    var compCreate = compGroup.FirstOrDefault(d => d is AddComponentDiff);
                    bool compEnabled = compCreate == null || IsDiffSelected(compCreate);

                    bool isMultiDiffComponent = !string.IsNullOrEmpty(compType) && compGroup.Count() > 1;

                    if (isMultiDiffComponent)
                    {
                        var foldoutKey = (groupKey, compType);
                        _componentFoldouts.TryAdd(foldoutKey, true);
                        _componentFoldouts[foldoutKey] = EditorGUILayout.Foldout(_componentFoldouts[foldoutKey], compType, true);

                        if (_componentFoldouts[foldoutKey])
                        {
                            EditorGUI.indentLevel++;
                            foreach (var diff in compGroup)
                                DrawDiffLine(diff, objectEnabled, compEnabled);
                            EditorGUI.indentLevel--;
                        }
                    }
                    else
                    {
                        foreach (var diff in compGroup)
                            DrawDiffLine(diff, objectEnabled, compEnabled);
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Apply Selected Diffs"))
        {
            ApplySelectedDiffs();
            Close();
        }
    }

    #region UI Helpers

    private void DrawDiffLine(SceneDiff diff, bool parentEnabled, bool selfEnabled)
    {
        const int indentSize = 15;
        const int initialSpacing = 15;
        const int toggleDimension = 18;

        EditorGUILayout.BeginHorizontal();

        bool isRootToggle = diff is CreateObjectDiff or RemoveObjectDiff;
        bool isComponent = diff is AddComponentDiff or RemoveComponentDiff;
        bool allowToggle = (parentEnabled && selfEnabled) || isRootToggle || (parentEnabled && isComponent);

        EditorGUI.BeginDisabledGroup(!allowToggle);

        bool current = IsDiffSelected(diff);
        var toggleRect = GUILayoutUtility.GetRect(toggleDimension, toggleDimension,
            GUILayout.Width(EditorGUI.indentLevel * indentSize + initialSpacing));
        bool updated = EditorGUI.Toggle(toggleRect, current);
        if (updated != current) _diffSelections[diff] = updated;

        GUILayout.Label(GetIconForDiff(diff), GUILayout.Width(toggleDimension), GUILayout.Height(toggleDimension));

        GUI.color = GetColorForDiff(diff);
        EditorGUILayout.LabelField(diff.ToString());
        GUI.color = Color.white;

        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
    }

    private bool IsDiffSelected(SceneDiff diff) =>
        _diffSelections.TryGetValue(diff, out var selected) && selected;

    #endregion

    #region Init

    private void Initialize(SceneDiff[] diffs)
    {
        _diffs = diffs
            .Where(d => d is not UpdatePropertyDiff prop ||
                        (prop.OldValue != null && prop.NewValue != null && !prop.OldValue.Equals(prop.NewValue)))
            .ToArray();

        _goFoldouts.Clear();
        _componentFoldouts.Clear();
        _diffSelections = _diffs.ToDictionary(d => d, _ => true);
    }

    #endregion

    #region Utility

    private string GetLabelForGroup(SceneDiff diff)
    {
        if (diff.InstanceId.HasValue)
        {
            Object obj = EditorUtility.InstanceIDToObject(diff.InstanceId.Value);
            return obj ? obj.name : $"(Missing Object {diff.InstanceId.Value})";
        }

        return !string.IsNullOrEmpty(diff.TempId) ? $"(New Object: {diff.TempId})" : "(Unknown)";
    }

    private Texture GetIconForDiff(SceneDiff diff) => diff switch
    {
        AddComponentDiff addDiff => GetIconForComponent(addDiff),
        RemoveComponentDiff => EditorGUIUtility.IconContent("TreeEditor.Trash").image,
        UpdatePropertyDiff => EditorGUIUtility.IconContent("d_BuildSettings.N3DS").image,
        CreateObjectDiff => EditorGUIUtility.IconContent("Prefab Icon").image,
        RemoveObjectDiff => EditorGUIUtility.IconContent("TreeEditor.Trash").image,
        _ => EditorGUIUtility.IconContent("GameObject Icon").image
    };
    
    private Texture GetIconForComponent(AddComponentDiff diff) 
    {
        if (_componentIcons.TryGetValue(diff.ComponentType, out var icon))
            return icon;
        
        var componentType = ObjectUtility.FindType(diff.ComponentType);
        icon = EditorGUIUtility.ObjectContent(null, componentType).image;
        var texture = icon ?? EditorGUIUtility.IconContent("cs Script Icon").image;
        _componentIcons[diff.ComponentType] = texture;
        return texture;
    }

    private Color GetColorForDiff(SceneDiff diff) => diff switch
    {
        AddComponentDiff or CreateObjectDiff => Color.green,
        RemoveComponentDiff or RemoveObjectDiff => Color.red,
        UpdatePropertyDiff => Color.yellow,
        _ => Color.white,
    };

    private void ApplySelectedDiffs()
    {
        // TODO: filter out selected diffs with unselected parent diffs (aka. disabled diffs)
        var orderedDiffs = _diffs
            .Where(d => _diffSelections.TryGetValue(d, out var selected) && selected)
            .OrderBy(d => d.Priority)
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
