using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SceneForgeEditorWindow : EditorWindow
{
    private string _prompt;
    private Vector2 _scrollPosition;
    private List<string> _images = new();
    private List<Texture> _cachedPreviewTextures = new();
    private Dictionary<ChatMessage, bool> _reasoningFoldouts = new();
    
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
        var chats = ChatManager.Chats;
        ChatManager.CurrentChatIndex = EditorGUILayout.Popup(ChatManager.CurrentChatIndex, chats.Select(c => c.Name).ToArray());
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("New Chat", GUILayout.Width(125)))
        {
            ChatManager.NewChat("Chat" + chats.Length);
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
        foreach (var message in ChatManager.GetDisplayedMessages())
        {
            GUILayout.BeginVertical(GUI.skin.button);
            GUILayout.BeginHorizontal(new GUIStyle
            {
                margin = new RectOffset(0, 0, 0, 3),
            });
            GUILayout.Label(message.Role.ToUpper());
            GUILayout.FlexibleSpace();
            if (message.Diffs.Any())
            {
                if (GUILayout.Button("Accept", GUILayout.Width(90)))
                    foreach (var diff in message.Diffs)
                        ResponseHandler.ApplyDiff(diff);
                    
                if (GUILayout.Button("Review Changes", GUILayout.Width(120)))
                    DiffViewerEditorWindow.ShowWindow(message.Diffs);
            }

            GUILayout.EndHorizontal();
            
            // Attached Images
            if (message.Images != null && message.Images.Length > 0)
            {
                GUILayout.BeginHorizontal();
                
                // Cache textures if not already cached to avoid decoding every time
                if (message.CachedTextures == null || message.CachedTextures.Length != message.Images.Length)
                    message.CachedTextures = message.Images.Select(img => ImageUtility.DecodeBase64Image(img, true)).ToArray();
                
                foreach (var image in message.CachedTextures)
                    RenderImage(image);
                
                GUILayout.EndHorizontal();
            }
            
            // Reasoning Foldout
            if (!string.IsNullOrEmpty(message.Reasoning))
            {
                _reasoningFoldouts.TryAdd(message, false);
                _reasoningFoldouts[message] = EditorGUILayout.Foldout(_reasoningFoldouts[message], "Reasoning", true);
                if (_reasoningFoldouts[message])
                {
                    var reasoningStyle = new GUIStyle(EditorStyles.helpBox)
                    {
                        wordWrap = true,
                        fontStyle = FontStyle.Italic,
                        margin = new RectOffset(0, 0, 0, 0),
                    };
                    EditorGUILayout.LabelField(message.Reasoning, reasoningStyle);
                }
            }

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
        
        // Attachments and prompt input
        if (_images.Count > 0)
        {
            if (_images.Count > _cachedPreviewTextures.Count)
            {
                // Cache textures for preview
                _cachedPreviewTextures.AddRange(_images.Skip(_cachedPreviewTextures.Count)
                    .Select(img => ImageUtility.DecodeBase64Image(img, true)));
            }
            GUILayout.Label("Attached Images:", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            foreach (var image in _cachedPreviewTextures.ToList()) // Use ToList() to avoid modifying the collection while iterating
            {
                GUILayout.BeginVertical();
                if (image) RenderImage(image);
                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                {
                    int index = _cachedPreviewTextures.IndexOf(image);
                    if (index >= 0 && index < _images.Count)
                    {
                        _images.RemoveAt(index);
                        _cachedPreviewTextures.RemoveAt(index);
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }
        
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

        GUILayout.BeginHorizontal();
        bool shouldSendPrompt = GUILayout.Button("Send Prompt") || enterPressed;
        EditorGUI.BeginDisabledGroup(!ChatManager.CurrentChat.MessageHandler.ImagesSupported);
        if (GUILayout.Button("Attach Image", GUILayout.Width(120)))
            _images.Add(ImageUtility.SelectAndEncodeImage());
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();

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
            var img = _images.ToArray();
            _prompt = string.Empty;
            _images.Clear();
            _cachedPreviewTextures.Clear();
            AIHandler.SendMessageInChat(p, img);
        }
    }

    private static string GetLabelContent(ChatMessage message) =>
        message.Name == null
            ? message.Content
            : "Executing tool: " + message.Name;

    private static void DynamicHeightSelectableLabel(string text, GUIStyle style)
    {
        // TODO: take scrollbar into account
        float height = style.CalcHeight(new GUIContent(text), EditorGUIUtility.currentViewWidth - 50);
        EditorGUILayout.SelectableLabel(text, style, GUILayout.Height(height));
    }

    private static void RenderImage(Texture image)
    {
        // Calculate width and height so that the largest dimension is 100px
        float aspectRatio = (float)image.width / image.height;
        float width = 100f;
        float height = 100f / aspectRatio;
        if (height > 100f)
        {
            height = 100f;
            width = 100f * aspectRatio;
        }
        GUILayout.Label(image, GUILayout.Width(width), GUILayout.Height(height));
    }
}