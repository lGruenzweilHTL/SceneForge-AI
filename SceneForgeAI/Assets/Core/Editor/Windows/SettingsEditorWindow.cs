// SceneForgeAI/Assets/Core/Editor/Windows/SettingsEditorWindow.cs

using UnityEditor;
using UnityEngine;

public class SettingsEditorWindow : EditorWindow
{
    private readonly string[] tabs = { "General", "API Keys", "Advanced" };
    private int selectedTab = 0;

    [MenuItem("Tools/SceneForge AI Settings")]
    public static void ShowWindow()
    {
        var window = GetWindow<SettingsEditorWindow>("Scene Forge AI Settings");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Scene Forge AI Settings", HeaderStyles.HeaderStyle);
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
        GUILayout.Label("General Settings", HeaderStyles.SubheaderStyle);

        AISettings.AIType = (AIType)EditorGUILayout.EnumPopup("AI Type", AISettings.AIType);
        switch (AISettings.AIType)
        {
            case AIType.Ollama:
                AISettings.OllamaModel = EditorGUILayout.TextField("Ollama Model", AISettings.OllamaModel);
                break;
            case AIType.Groq:
                AISettings.GroqModel = EditorGUILayout.TextField("Groq Model", AISettings.GroqModel);
                break;
            case AIType.OpenAI:
                AISettings.OpenAIModel = EditorGUILayout.TextField("OpenAI Model", AISettings.OpenAIModel);
                break;
        }
    }

    private void DrawApiKeysSettings()
    {
        GUILayout.Space(10);
        GUILayout.Label("API Keys", HeaderStyles.SubheaderStyle);

        EditorGUILayout.BeginVertical(GUI.skin.box);
        AISettings.OpenAIApiKey = EditorGUILayout.TextField("OpenAI API Key", AISettings.OpenAIApiKey);
        AISettings.GroqApiKey = EditorGUILayout.TextField("Groq API Key", AISettings.GroqApiKey);
        AISettings.OllamaUrl = EditorGUILayout.TextField("Ollama URL", AISettings.OllamaUrl);
        EditorGUILayout.EndVertical();
    }

    private void DrawAdvancedSettings()
    {
        GUILayout.Space(10);
        GUILayout.Label("Advanced Settings", HeaderStyles.SubheaderStyle);
        
        AISettings.AllowObjectCreation = EditorGUILayout.Toggle("Allow Object Creation", AISettings.AllowObjectCreation);
        AISettings.AllowComponentCreation = EditorGUILayout.Toggle("Allow Component Creation", AISettings.AllowComponentCreation);
    }
}