using System;
using System.Linq;
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
        GUILayout.Label("Scene Forge AI", HeaderStyles.HeaderStyle);
        
        GUILayout.BeginHorizontal();
        var chats = AIHandler.Chats;
        _currentChatIndex = EditorGUILayout.Popup(_currentChatIndex, chats.Select(c => c.Name).ToArray());
        AIHandler.SetCurrentChat(_currentChatIndex);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("New Chat", GUILayout.Width(125)))
        {
            AIHandler.NewChat("Chat" + chats.Length);
            _currentChatIndex = chats.Length; // Set to the new chat
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
            GUILayout.BeginHorizontal(new GUIStyle
            {
                margin = new RectOffset(0, 0, 0, 3),
            });
            GUILayout.Label(message.Role.ToUpper());
            GUILayout.FlexibleSpace();
            if (message.Json != null)
            {
                if (GUILayout.Button("Accept", GUILayout.Width(90)))
                    foreach (var diff in message.Diffs)
                        ResponseHandler.ApplyDiff(diff);
                    
                if (GUILayout.Button("Review Changes", GUILayout.Width(120)))
                    DiffViewerEditorWindow.ShowWindow(message.Diffs);
            }

            GUILayout.EndHorizontal();
            var style = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true,
                richText = true,
                margin = new RectOffset(0, 0, 0, 5),
            };
            DynamicHeightSelectableLabel(GetLabelContent(message), style);
            GUILayout.EndVertical();
            GUILayout.Space(10);
        }
        GUILayout.EndScrollView();
        
        GUILayout.Space(20);
        
        const string controlName = "TextArea";
        GUI.SetNextControlName(controlName);
        _prompt = GUILayout.TextArea(_prompt, new GUIStyle(EditorStyles.textArea)
        {
            wordWrap = true,
        }, GUILayout.Height(50));
        EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Text);
        
        bool enterPressed = Event.current.type == EventType.KeyUp &&
                            Event.current.keyCode == KeyCode.Return &&
                            GUI.GetNameOfFocusedControl() == controlName &&
                            !Event.current.shift;

        bool shouldSendPrompt = GUILayout.Button("Send Prompt") || enterPressed;

        if (shouldSendPrompt)
        {
            if (enterPressed)
                Event.current.Use();

            if (string.IsNullOrWhiteSpace(_prompt))
            {
                EditorUtility.DisplayDialog("Error", "Prompt cannot be empty.", "OK");
                return;
            }

            var p = _prompt.TrimEnd('\n', '\r');
            _prompt = string.Empty;
            AIHandler.SendMessageInChat(p);
        }
    }

    private static string GetLabelContent(ChatMessage message) =>
        message.Name == null
            ? (message.Reasoning != null ? $"<i>Reasoning: {message.Reasoning}</i>\n" : "") +  message.Content
            : "Executing tool: " + message.Name;

    private static void DynamicHeightSelectableLabel(string text, GUIStyle style)
    {
        // TODO: take scrollbar into account
        float height = style.CalcHeight(new GUIContent(text), EditorGUIUtility.currentViewWidth - 50);
        EditorGUILayout.SelectableLabel(text, style, GUILayout.Height(height));
    }
}
