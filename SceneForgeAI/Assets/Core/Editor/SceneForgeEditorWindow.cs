using UnityEditor;
using UnityEngine;

public class SceneForgeEditorWindow : EditorWindow
{
    private string _prompt;
    private Vector2 _scrollPosition;
    private int _currentChatIndex = 0;
    
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
        
        GUILayout.BeginHorizontal();
        _currentChatIndex = EditorGUILayout.Popup(_currentChatIndex, AIHandler.GetChatNames());
        AIHandler.SetCurrentChat(_currentChatIndex);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("New Chat", GUILayout.Width(125)))
        {
            AIHandler.NewChat("Chat" + AIHandler.GetChatNames().Length);
            _currentChatIndex = AIHandler.GetChatNames().Length - 1; // Set to the new chat
            _scrollPosition = Vector2.zero; // Reset scroll position
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, new GUIStyle(GUI.skin.window)
            {
                padding = new RectOffset(5, 5, 10, 10),
                margin = new RectOffset(5, 5, 0, 0),
            }, 
            GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
        foreach (var message in AIHandler.GetCurrentChatHistory())
        {
            GUILayout.BeginVertical(GUI.skin.button);
            GUILayout.Label(message.role.ToUpper());
            GUILayout.Box(message.content, GUI.skin.textArea);
            GUILayout.Space(5); // Space inside vertical for a little padding
            GUILayout.EndVertical();
            GUILayout.Space(10);
        }
        GUILayout.EndScrollView();
        
        GUILayout.Space(20);

        _prompt = EditorGUILayout.TextArea(_prompt, GUILayout.Height(50));
        
        if (GUILayout.Button("Send Prompt"))
        {
            AIHandler.PromptStream(_prompt);
            _prompt = string.Empty;
        }
    }
}
