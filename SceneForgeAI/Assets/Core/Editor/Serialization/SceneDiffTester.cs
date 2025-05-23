using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDiffTester : EditorWindow
{
    private string diff;
    private string selectionUid;
    
    [MenuItem("Tools/Scene Diff Tester")]
    public static void ShowWindow()
    {
        GetWindow<SceneDiffTester>("Scene Diff Tester");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Scene Diff Tester", EditorStyles.boldLabel);
        
        GUILayout.Label("Diff JSON:");
        diff = EditorGUILayout.TextArea(diff);
        selectionUid = EditorGUILayout.TextField("Selection UID", selectionUid);

        if (GUILayout.Button("Apply Diff"))
        {
            var uidMap = new Dictionary<string, GameObject>() {
                { selectionUid, Selection.activeGameObject }
            };
            var scene = SceneManager.GetActiveScene();
            SceneDiffHandler.ApplyDiffToScene(scene, diff, uidMap);
        }
    }
}