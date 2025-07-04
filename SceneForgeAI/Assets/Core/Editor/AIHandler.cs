using System;
using System.Collections.Generic;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using Unity.Plastic.Newtonsoft.Json;

public static class AIHandler
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

    private static Chat _currentChat = _chats[0];

    public static void SendMessageInChat(string prompt, string[] images = null)
    {
        var msg = new ChatMessage
        {
            Role = "user",
            Content = prompt,
            Images = images
        };
        _currentChat.History.Add(msg);
        var handler = _currentChat.MessageHandler;
        
        SendMessage(handler);
    }

    private static void SendMessage(IMessageHandler handler, bool useTools = true)
    {
        if (useTools && AIToolCollector.ToolRegistry.Count == 0)
            AIToolCollector.UpdateRegistry();
        
        var response = new ChatMessage
        {
            Role = "assistant",
            Content = "",
        };
        _currentChat.History.Add(response);
        EditorCoroutineUtility.StartCoroutineOwnerless(AISettings.PreferStream && handler.StreamSupported
            ? handler.GetChatCompletionStreamed(_currentChat.History
                    .SkipLast(1) // Skip the just added response message
                    .Select(m => new AIMessage
                    {
                        role = m.Role,
                        content = m.Content,
                        name = m.Name,
                        tool_call_id = m.ToolCallId,
                        image_urls = m.Images
                    })
                    .ToArray(),
                useTools ? AIToolCollector.ToolRegistry.Keys.ToArray() : null,
                t => response.Content += t,
                rt => response.Reasoning += rt,
                toolCalls => OnResponseReceived(response, toolCalls))
            : handler.GetChatCompletion(_currentChat.History
                .SkipLast(1). // Skip the just added response message
                Select(m => new AIMessage
                    {
                        role = m.Role,
                        content = m.Content,
                        name = m.Name,
                        tool_call_id = m.ToolCallId,
                        image_urls = m.Images
                    })
                .ToArray(),
                useTools ? AIToolCollector.ToolRegistry.Keys.ToArray() : null,
                (content, reasoning, toolCalls) =>
                {
                    response.Content = content;
                    response.Reasoning = reasoning;
                    OnResponseReceived(response, toolCalls);
                }));
    }
    
    private static void OnResponseReceived(ChatMessage responseMessage, ToolCall[] toolCalls)
    {
        responseMessage.Diffs = Array.Empty<SceneDiff>();

        if (toolCalls.Length > 0)
        {
            foreach (var toolCall in toolCalls)
            {
                var result = AIToolInvoker.InvokeTool(toolCall.ToolName, toolCall.Arguments);

                if (result is SceneDiff diff)
                {
                    responseMessage.Diffs = responseMessage.Diffs.Append(diff).ToArray();
                    result = "Tool call successful. Scene diff created";
                }
                
                _currentChat.History.Add(new ChatMessage
                {
                    Display = true,
                    Role = "tool",
                    Name = toolCall.ToolName,
                    Content = JsonConvert.SerializeObject(result, Formatting.Indented),
                    ToolCallId = toolCall.Id
                });
            }

            SendMessage(_currentChat.MessageHandler);
        }
    }

    public static ChatMessage[] GetCurrentChatHistory()
    {
        return _currentChat.History
            .Where(m => m.Display)
            .ToArray();
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
            AIType.Ollama => throw new NotImplementedException("Ollama does not support tool calls."),
            AIType.Groq => new GroqMessageHandler(AISettings.GroqApiKey, AISettings.GroqModel),
            AIType.OpenAI => throw new NotImplementedException("OpenAI support is not implemented yet."),
            _ => throw new ArgumentOutOfRangeException("Unsupported AI type: " + AISettings.AIType)
        };
    }
}