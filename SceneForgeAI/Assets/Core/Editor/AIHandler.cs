using System;
using System.Collections.Generic;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using Unity.Plastic.Newtonsoft.Json;

public static class AIHandler
{
    public static void SendMessageInChat(string prompt, string[] images = null)
    {
        var msg = new ChatMessage
        {
            Role = "user",
            Content = prompt,
            Images = images
        };
        ChatManager.AddMessageToHistory(msg);
        var handler = ChatManager.CurrentChat.MessageHandler;
        
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
        ChatManager.AddMessageToHistory(response);
        
        var messages = SerializeMessages(ChatManager.CurrentChat.History);
        var tools = useTools ? AIToolCollector.ToolRegistry.Keys.ToArray() : null;
        EditorCoroutineUtility.StartCoroutineOwnerless(AISettings.PreferStream && handler.StreamSupported
            ? handler.GetChatCompletionStreamed(messages,
                tools,
                t => response.Content += t,
                rt => response.Reasoning += rt,
                (toolCalls, reprompt) => ProcessToolCalls(response, toolCalls, reprompt))
            : handler.GetChatCompletion(messages,
                tools,
                (content, reasoning, toolCalls, reprompt) =>
                {
                    response.Content = content;
                    response.Reasoning = reasoning;
                    ProcessToolCalls(response, toolCalls, reprompt);
                }));
    }

    private static AIMessage[] SerializeMessages(List<ChatMessage> messages)
    {
        return messages
            .SkipLast(1) // Skip the last message which is the response being built
            .Select(m => new AIMessage
            {
                role = m.Role,
                content = m.Content,
                name = m.Name,
                tool_call_id = m.ToolCallId,
                image_urls = m.Images
            })
            .ToArray();
    }
    
    private static void ProcessToolCalls(ChatMessage responseMessage, ToolCall[] toolCalls, bool reprompt)
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

                if (reprompt)
                    ChatManager.AddMessageToHistory(new ChatMessage
                    {
                        Display = true,
                        Role = "tool",
                        Name = toolCall.ToolName,
                        Content = JsonConvert.SerializeObject(result, Formatting.Indented),
                        ToolCallId = toolCall.Id
                    });
            }
        }
        
        if (reprompt) SendMessage(ChatManager.CurrentChat.MessageHandler);
    }
}