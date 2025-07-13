using System;
using System.Collections.Generic;
using System.Linq;

public static class ChatManager
{
    private const string SystemPrompt =
        "You are a helpful Unity scene-editing assistant\n" +
        "You can answer user prompts using some predefined tools\n" +
        "When you have everything you need, do NOT use any more tool calls\n" +
        "Only use tools when you need to use them to answer the user's question\n" +
        "Try to complete tasks using as few tool calls as possible\n";

    private static readonly ChatMessage SystemMessage = new()
    {
        Role = "system",
        Content = SystemPrompt,
        Display = false
    };
    private static readonly ChatMessage Greeting = new()
    {
        Role = "assistant",
        Content = "Hello! I am Scene Forge AI. How can I assist you today?"
    };

    public static Chat[] Chats => _chats.ToArray();
    private static List<Chat> _chats = new()
    {
        CreateChat()
    };

    public static int CurrentChatIndex { get; set; }
    public static Chat CurrentChat => _chats[CurrentChatIndex];
    
    public static ChatMessage[] GetDisplayedMessages()
    {
        return CurrentChat.History
            .Where(m => m.Display)
            .ToArray();
    }

    public static void SetCurrentChat(int index)
    {
        if (index >= _chats.Count || index < 0) return;
        CurrentChatIndex = index;
    }

    public static void AddMessageToHistory(ChatMessage message)
    {
        if (CurrentChat.History.Count > 0 && CurrentChat.History.Last().Role == message.Role)
        {
            // If the last message has the same role, append to its content
            CurrentChat.History.Last().Content += "\n" + message.Content;
            CurrentChat.History.Last().Images = message.Images;
        }
        else
        {
            // Otherwise, add a new message
            CurrentChat.History.Add(message);
        }
    }

    public static Chat NewChat(string name = null, IMessageHandler messageHandler = null, bool updateCurrent = true)
    {
        var c = CreateChat(name, messageHandler);
        _chats.Add(c);

        if (updateCurrent)
        {
            CurrentChatIndex = _chats.Count - 1;
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
            AIType.Ollama => throw new NotImplementedException("Ollama does not support tool calls."),
            AIType.Groq => new GroqMessageHandler(AISettings.GroqApiKey, AISettings.GroqModel),
            AIType.OpenAI => throw new NotImplementedException("OpenAI support is not implemented yet."),
            _ => throw new ArgumentOutOfRangeException("Unsupported AI type: " + AISettings.AIType)
        };
    }
}