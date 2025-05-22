using System;
using UnityEditor;
using UnityEngine;

public class SceneForgeEditorWindow : EditorWindow
{
    private string _prompt;
    private string _response;
    
    [MenuItem("Tools/SceneForge AI")]
    public static void ShowWindow()
    {
        GetWindow<SceneForgeEditorWindow>("Scene Forge AI");
    }

    private void OnGUI()
    {
        GUILayout.Label("Scene Forge AI", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        _prompt = EditorGUILayout.TextField("Prompt", _prompt);

        if (GUILayout.Button("Check Connection"))
        {
            _response = AIHandler.CheckConnection() ? "Connected" : "Not Connected";
        }
        if (GUILayout.Button("Send Prompt"))
        {
            _response = AIHandler.Prompt(_prompt);
        }
        
        if (!string.IsNullOrEmpty(_response))
        {
            GUILayout.Label("Response", EditorStyles.boldLabel);
            GUILayout.TextArea(_response, GUILayout.Height(200));
        }
    }
}
