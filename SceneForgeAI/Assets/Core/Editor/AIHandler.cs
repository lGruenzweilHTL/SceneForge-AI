using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;

public static class AIHandler
{
    private static readonly AIMessage Greeting = new()
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
            History = new List<AIMessage> { Greeting },
            MessageHandler = new OllamaMessageHandler()
        }
    };

    private static Chat _currentChat = _chats[0];

    public static void SendMessageInChat(string prompt)
    {
        var sceneJson = JsonConvert.SerializeObject(GameObjectSerializer.SerializeSelection(), Formatting.None);
        string requestContent = "Scene JSON: " + sceneJson + "\n\nUser Prompt: " + prompt;
        var msg = new AIMessage
        {
            role = "user",
            content = requestContent,
        };
        _currentChat.History.Add(msg);
        var handler = _currentChat.MessageHandler;
        EditorCoroutineRunner.StartCoroutine(handler.GetChatCompletion(_currentChat.History.ToArray(),
            responseText =>
            {
                var response = new AIMessage
                {
                    role = "assistant",
                    content = responseText,
                };
                _currentChat.History.Add(response);
                
                ResponseHandler.HandleResponse(responseText);
            }));
    }
    
    public static AIMessage[] GetCurrentChatHistory()
    {
        return _currentChat.History.ToArray();
    }

    public static void SetCurrentChat(int index)
    {
        if (index >= _chats.Count || index < 0) return;
        _currentChat = _chats[index];
    }

    public static Chat NewChat(IMessageHandler messageHandler, string name = null, bool updateCurrent = true)
    {
        var c = new Chat
        {
            Name = name ?? "New Chat",
            History = new List<AIMessage> { Greeting },
            MessageHandler = messageHandler
        };
        _chats.Add(c);
        
        if (updateCurrent)
        {
            _currentChat = c;
        }

        return c;
    }
}