using UnityEditor;
using UnityEngine;

public class DiffViewerEditorWindow : EditorWindow
{
    private GameObject _current;
    private string _diffString = string.Empty;
    
    [MenuItem("Tools/Diff Viewer")]
    public static void ShowWindow()
    {
        DiffViewerEditorWindow window = GetWindow<DiffViewerEditorWindow>("Diff Viewer");
        window.minSize = new UnityEngine.Vector2(400, 300);
        window.Show();
    }
    
    private void OnGUI()
    {
        _current = (GameObject)EditorGUILayout.ObjectField("Current GameObject", _current, typeof(GameObject), true);
        _diffString = EditorGUILayout.TextArea(_diffString, GUILayout.Height(position.height - 50));
        
        if (GUILayout.Button("Generate Diff"))
        {
            GenerateDiff();
        }
    }

    private void GenerateDiff()
    {
        throw new System.NotImplementedException();
    }
}