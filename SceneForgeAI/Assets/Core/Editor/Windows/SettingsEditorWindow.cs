using UnityEditor;
using UnityEngine;

public class SettingsEditorWindow : EditorWindow
{
    private readonly string[] tabs = { "General", "API Keys", "Advanced" };
    private int selectedTab = 0;
    
    #region Settings

    [SerializeField] private int maxErrorRetries = 3;
    [SerializeField] private AIType aiType = AIType.Ollama;
    [SerializeField] private string ollamaModel = "sceneforge",
        groqModel = "llama3-70b",
        openAiModel = "gpt-4o-mini";
    [SerializeField] private string openAiApiKey = string.Empty,
        groqApiKey = string.Empty,
        ollamaUrl = "http://localhost:11434";
    
    #endregion

    #region Styles

    private static readonly GUIStyle headerStyle = new GUIStyle(EditorStyles.label)
    {
        fontSize = 20,
        alignment = TextAnchor.MiddleCenter,
        fontStyle = FontStyle.Bold
    };
    private static readonly GUIStyle subheaderStyle = new GUIStyle(EditorStyles.label)
    {
        fontSize = 16,
        alignment = TextAnchor.MiddleLeft,
        fontStyle = FontStyle.Bold,
    };

    #endregion
    
    [MenuItem("Tools/SceneForge AI Settings")]
    public static void ShowWindow()
    {
        var window = GetWindow<SettingsEditorWindow>("Scene Forge AI Settings");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Scene Forge AI Settings", headerStyle);
        GUILayout.Space(20);
        
        selectedTab = GUILayout.Toolbar(selectedTab, tabs);
        switch (selectedTab)
        {
            case 0:
                DrawGeneralSettings();
                break;
            case 1:
                DrawApiKeysSettings();
                break;
            case 2:
                DrawAdvancedSettings();
                break;
        }
    }

    private void DrawGeneralSettings()
    {
        GUILayout.Space(10);
        GUILayout.Label("General Settings", subheaderStyle);
        
        aiType = (AIType)EditorGUILayout.EnumPopup("AI Type", aiType);
        switch (aiType)
        {
            case AIType.Ollama:
                ollamaModel = EditorGUILayout.TextField("Ollama Model", ollamaModel);
                break;
            case AIType.Groq:
                groqModel = EditorGUILayout.TextField("Groq Model", groqModel);
                break;
            case AIType.OpenAI:
                openAiModel = EditorGUILayout.TextField("OpenAI Model", openAiModel);
                break;
        }
    }
    
    private void DrawApiKeysSettings()
    {
        GUILayout.Space(10);
        GUILayout.Label("API Keys", subheaderStyle);
        
        EditorGUILayout.BeginVertical(GUI.skin.box);
        openAiApiKey = EditorGUILayout.TextField("OpenAI API Key", openAiApiKey);
        groqApiKey = EditorGUILayout.TextField("Groq API Key", groqApiKey);
        ollamaUrl = EditorGUILayout.TextField("Ollama URL", ollamaUrl);
        EditorGUILayout.EndVertical();
    }
    
    private void DrawAdvancedSettings()
    {
        GUILayout.Space(10);
        GUILayout.Label("Advanced Settings", subheaderStyle);
        
        maxErrorRetries = EditorGUILayout.IntField("Max Error Retries", maxErrorRetries);
    }
}