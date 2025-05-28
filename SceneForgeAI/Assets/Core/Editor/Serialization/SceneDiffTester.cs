using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDiffTester : EditorWindow
{
    private string diff = "";
    private GameObject[] objects = {};
    
    [MenuItem("Tools/Scene Diff Tester")]
    public static void ShowWindow()
    {
        GetWindow<SceneDiffTester>("Scene Diff Tester");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Scene Diff Tester", EditorStyles.boldLabel);
        
        objects = ArrayField("Select GameObjects", objects);
        var uidMap = objects
            .Where(o => o)
            .Select((o, i) => new { obj = o, idx = i})
            .ToDictionary(pair => pair.idx.ToString(), pair => pair.obj);
        
        GUILayout.Space(20);
        if (uidMap.Any()) EditorGUILayout.HelpBox("UID Map:\n" + 
            string.Join("\n", uidMap.Select(kv => $"{kv.Key}: {kv.Value.name}")), MessageType.Info);
        
        GUILayout.Label("Diff JSON:");
        diff = EditorGUILayout.TextArea(diff);

        if (GUILayout.Button("Apply Diff"))
        {
            SceneDiffHandler.ApplyDiffToScene(diff, uidMap);
        }
    }
    
    private T[] ArrayField<T>(string label, T[] array) where T : Object
    {
        EditorGUILayout.LabelField(label);
        int newSize = EditorGUILayout.IntField("Size", array.Length);
        if (newSize != array.Length)
        {
            System.Array.Resize(ref array, newSize);
        }
        
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = (T)EditorGUILayout.ObjectField($"Element {i}", array[i], typeof(T), true);
        }
        
        return array;
    }
}