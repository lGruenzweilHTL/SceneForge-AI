using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;

public static class AIHandler
{
    private static readonly ChatMessage Greeting = new()
    {
        role = "assistant",
        content = "Hello! I am Scene Forge AI. How can I assist you today?"
    };

    public static Chat[] Chats => _chats.ToArray();
    private static List<Chat> _chats = new()
    {
        new Chat
        {
            Name = "New Chat",
            History = new List<ChatMessage> { Greeting },
            MessageHandler = GetPreferredMessageHandler()
        }
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
        EditorCoroutineRunner.StartCoroutine(handler.GetChatCompletion(_currentChat.History
                .Select(m => new AIMessage
                {
                    role = m.role,
                    content = m.content,
                })
                .ToArray(),
            responseText =>
            {
                var response = new ChatMessage
                {
                    role = "assistant",
                    content = responseText,
                    json = ResponseHandler.GetJsonContent(responseText)
                };
                _currentChat.History.Add(response);
            }));
    }
    
    public static ChatMessage[] GetCurrentChatHistory()
    {
        return _currentChat.History.ToArray();
    }

    public static void SetCurrentChat(int index)
    {
        if (index >= _chats.Count || index < 0) return;
        _currentChat = _chats[index];
    }

    public static Chat NewChat(string name = null, IMessageHandler messageHandler = null, bool updateCurrent = true)
    {
        var c = new Chat
        {
            Name = name ?? "New Chat",
            History = new List<ChatMessage> { Greeting },
            MessageHandler = messageHandler ?? GetPreferredMessageHandler()
        };
        _chats.Add(c);
        
        if (updateCurrent)
        {
            _currentChat = c;
        }

        return c;
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