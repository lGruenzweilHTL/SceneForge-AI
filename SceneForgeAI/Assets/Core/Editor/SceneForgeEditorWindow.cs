using UnityEditor;
using UnityEngine;

public class SceneForgeEditorWindow : EditorWindow
{
    private string _prompt;
    private Vector2 _scrollPosition;
    
    [MenuItem("Tools/SceneForge AI")]
    public static void ShowWindow()
    {
        var window = GetWindow<SceneForgeEditorWindow>("Scene Forge AI");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }

    private void OnGUI()
    {
        var headerStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        GUILayout.Label("Scene Forge AI", headerStyle);
        GUILayout.Space(10);

        var history = AIHandler.GetHistory();
        
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, new GUIStyle(GUI.skin.window)
            {
                padding = new RectOffset(5, 5, 10, 10),
                margin = new RectOffset(5, 5, 0, 0)
            }, 
            GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
        foreach (var message in history)
        {
            GUILayout.BeginVertical(GUI.skin.button);
            GUILayout.Label(message.role.ToUpper());
            GUILayout.Box(message.content, GUI.skin.textArea);
            GUILayout.Space(5);
            GUILayout.EndVertical();
            GUILayout.Space(10);
        }
        GUILayout.EndScrollView();
        
        GUILayout.Space(20);

        _prompt = EditorGUILayout.TextArea(_prompt, GUILayout.Height(50));
        
        if (GUILayout.Button("Send Prompt"))
        {
            AIHandler.Prompt(_prompt);
            _prompt = string.Empty;
        }
    }
}
