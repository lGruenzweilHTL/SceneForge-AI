using System;
using System.Collections.Generic;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using Unity.Plastic.Newtonsoft.Json;

public static class AIHandler
{
    private const string SystemPrompt =
        "You are a Unity scene-editing assistant. You are given two things:\n" +
        "1. A JSON object representing a subset of a Unity scene (only relevant objects, with assigned UIDs).\n" +
        "2. A user instruction describing what should change.\n\n" +
        "Your task is to output ONLY the changes that need to be applied to the scene in JSON format.\n" +
        "You must NOT output the full scene, only a minimal diff containing the modified values.\n\n" +
        "Output Format Rules:\n" +
        "Your JSON output must be in a fenced code block starting with ```json.\n" +
        "- The top-level keys of your output must be object UIDs (as strings).\n" +
        "- Under each UID, specify the component or section being modified. Use capitalized keys like \"Transform\" or the component type (e.g., \"Light2D\", \"Camera\").\n" +
        "- Under each component, only include the fields that were changed, not the full component.\n" +
        "- Represent vectors (Vector2, Vector3) and Color as arrays:\n" +
        "  - Vector2: [x, y]\n" +
        "  - Vector3: [x, y, z]\n" +
        "  - Color: [r, g, b, a]\n" +
        "- Do NOT modify the UID or component names.\n" +
        "- If nothing needs to change, don't output any JSON.\n\n" +
        "You may also include a summary of what you changed in your Response.\n\n" +
        "Example input:\n" +
        "(scene JSON excerpt)\n" +
        "{\n" +
        "  \"0\": {\n" +
        "    \"uid\": \"0\",\n" +
        "    \"name\": \"Global Light 2D\",\n" +
        "    \"components\": [ ... ]\n" +
        "    ...\n" +
        "  }\n" +
        "}\n\n" +
        "User prompt:\n" +
        "\"Increase the light intensity and move the light up by 2 units.\"\n\n" +
        "Your output:\n" +
        "I have moved to light up 2 Units and increased the light intensity to 2.\n\n" +
        "```json\n" +
        "{\n" +
        "  \"0\": {\n" +
        "    \"Light2D\": {\n" +
        "      \"intensity\": 2.0\n" +
        "    },\n" +
        "    \"Transform\": {\n" +
        "      \"position\": [0.0, 2.0, 0.0]\n" +
        "    }\n" +
        "  }\n" +
        "}\n" +
        "```\n\n";
    
    private static readonly ChatMessage SystemMessage = new()
    {
        role = "system",
        content = SystemPrompt
    };
    private static readonly ChatMessage Greeting = new()
    {
        role = "assistant",
        content = "Hello! I am Scene Forge AI. How can I assist you today?"
    };

    public static Chat[] Chats => _chats.ToArray();
    private static List<Chat> _chats = new()
    {
        CreateChat()
    };

    private static Chat _currentChat = _chats[0];

    public static void SendMessageInChat(string prompt)
    {
        var sceneJson = JsonConvert.SerializeObject(GameObjectSerializer.SerializeSelection(), Formatting.None);
        string requestContent = "Scene JSON: " + sceneJson + "\n\nUser Prompt: " + prompt;
        var msg = new ChatMessage()
        {
            role = "user",
            content = requestContent,
        };
        _currentChat.History.Add(msg);
        var handler = _currentChat.MessageHandler;
        EditorCoroutineUtility.StartCoroutineOwnerless(handler.GetChatCompletion(_currentChat.History
                .Select(m => new AIMessage
                {
                    role = m.role,
                    content = m.content,
                })
                .ToArray(),
            responseText =>
            {
                var jsonContent = ResponseHandler.GetJsonContent(responseText);
                var response = new ChatMessage
                {
                    role = "assistant",
                    content = responseText,
                    json = jsonContent,
                    diffs = ResponseHandler.GenerateDiffs(jsonContent ?? "{ }")
                };
                _currentChat.History.Add(response);
            }));
    }
    
    public static ChatMessage[] GetCurrentChatHistory()
    {
        return _currentChat.History.Skip(1).ToArray(); // Skip the system message
    }

    public static void SetCurrentChat(int index)
    {
        if (index >= _chats.Count || index < 0) return;
        _currentChat = _chats[index];
    }

    public static Chat NewChat(string name = null, IMessageHandler messageHandler = null, bool updateCurrent = true)
    {
        var c = CreateChat(name, messageHandler);
        _chats.Add(c);
        
        if (updateCurrent)
        {
            _currentChat = c;
        }

        return c;
    }
    private static Chat CreateChat(string name = null, IMessageHandler handler = null)
    {
        return new Chat
        {
            Name = name ?? "New Chat",
            History = new List<ChatMessage> { SystemMessage, Greeting },
            MessageHandler = handler ?? GetPreferredMessageHandler()
        };
    }

    private static IMessageHandler GetPreferredMessageHandler()
    {
        return AISettings.AIType switch
        {
            AIType.Ollama => new OllamaMessageHandler(AISettings.OllamaUrl, AISettings.OllamaModel),
            AIType.Groq => new GroqMessageHandler(AISettings.GroqApiKey, AISettings.GroqModel),
            AIType.OpenAI => throw new NotImplementedException("OpenAI support is not implemented yet."),
            _ => throw new ArgumentOutOfRangeException("Unsupported AI type: " + AISettings.AIType)
        };
    }
}