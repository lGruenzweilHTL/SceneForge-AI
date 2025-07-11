using UnityEditor;
using UnityEngine;

public static class AISettings
{
    private const string OpenAIKey = "SceneForgeAI_OpenAIKey";
    private const string GroqKey = "SceneForgeAI_GroqKey";
    private const string OllamaUrlKey = "SceneForgeAI_OllamaUrl";
    private const string AITypeKey = "SceneForgeAI_AIType";
    private const string OllamaModelKey = "SceneForgeAI_OllamaModel";
    private const string GroqModelKey = "SceneForgeAI_GroqModel";
    private const string OpenAIModelKey = "SceneForgeAI_OpenAIModel";
    private const string AllowObjectCreationKey = "SceneForgeAI_AllowObjectCreation";
    private const string AllowComponentCreationKey = "SceneForgeAI_AllowComponentCreation";
    private const string PreferStreamKey = "SceneForgeAI_PreferStream";

    public static string OpenAIApiKey
    {
        get => EditorPrefs.GetString(OpenAIKey, "");
        set => EditorPrefs.SetString(OpenAIKey, value);
    }

    public static string GroqApiKey
    {
        get => EditorPrefs.GetString(GroqKey, "");
        set => EditorPrefs.SetString(GroqKey, value);
    }

    public static string OllamaUrl
    {
        get => EditorPrefs.GetString(OllamaUrlKey, "http://localhost:11434");
        set => EditorPrefs.SetString(OllamaUrlKey, value);
    }

    public static AIType AIType
    {
        get => (AIType)EditorPrefs.GetInt(AITypeKey, 0);
        set => EditorPrefs.SetInt(AITypeKey, (int)value);
    }
    
    public static string OllamaModel
    {
        get => EditorPrefs.GetString(OllamaModelKey, "deepseek-coder:6.7b");
        set => EditorPrefs.SetString(OllamaModelKey, value);
    }
    
    public static string GroqModel
    {
        get => EditorPrefs.GetString(GroqModelKey, "gemma2-9b-it");
        set => EditorPrefs.SetString(GroqModelKey, value);
    }
    
    public static string OpenAIModel
    {
        get => EditorPrefs.GetString(OpenAIModelKey, "gpt-3.5-turbo");
        set => EditorPrefs.SetString(OpenAIModelKey, value);
    }
    
    public static bool AllowObjectCreation
    {
        get => EditorPrefs.GetBool(AllowObjectCreationKey, true);
        set => EditorPrefs.SetBool(AllowObjectCreationKey, value);
    }
    
    public static bool AllowComponentCreation
    {
        get => EditorPrefs.GetBool(AllowComponentCreationKey, true);
        set => EditorPrefs.SetBool(AllowComponentCreationKey, value);
    }
    
    public static bool PreferStream
    {
        get => EditorPrefs.GetBool(PreferStreamKey, true);
        set => EditorPrefs.SetBool(PreferStreamKey, value);
    }
}