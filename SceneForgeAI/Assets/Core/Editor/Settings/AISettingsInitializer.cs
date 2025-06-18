using UnityEditor;

[InitializeOnLoad]
public static class AISettingsInitializer
{
    static AISettingsInitializer()
    {
        // Access properties to ensure they are loaded (optional, as EditorPrefs loads on demand)
        _ = AISettings.OpenAIApiKey;
        _ = AISettings.GroqApiKey;
        _ = AISettings.OllamaUrl;
        _ = AISettings.AIType;
        _ = AISettings.MaxErrorRetries;
        _ = AISettings.OllamaModel;
        _ = AISettings.GroqModel;
        _ = AISettings.OpenAIModel;
    }
}