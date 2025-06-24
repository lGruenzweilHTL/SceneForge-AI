using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;

public class GroqMessageHandler : IMessageHandler
{
    public GroqMessageHandler(string key, string model = "gemma2-9b-it")
    {
        Model = model;
        _key = key;
    }

    private const string Endpoint = "https://api.groq.com/openai/v1/chat/completions";

    public string Model { get; set; }
    private readonly string _key;

    public IEnumerator GetChatCompletionWithStream(AIMessage[] history, Tool[] tools, Action<string> onNewToken, Action<ToolCall[]> onMessageCompleted)
    {
        var body = new AIRequest
        {
            model = Model,
            messages = history,
            tools = tools?.ToList(),
            stream = true
        };

        var json = JsonConvert.SerializeObject(body);
        var downloadHandler = new StreamDownloadHandler<GroqStreamResponse>();
        WebRequestUtility.SendPostRequest(Endpoint, json, out var operation, new Dictionary<string, string>
        {
            ["Content-Type"] = "application/json",
            ["Authorization"] = "Bearer " + _key
        }, downloadHandler);
        while (!operation.isDone || downloadHandler.HasNewToken())
        {
            while (downloadHandler.HasNewToken())
            {
                var choices = downloadHandler.GetNextToken().Choices;
                if (choices == null || choices.Count == 0)
                {
                    continue; // Skip if no choices are available
                }
                var token = choices[0]; // Assuming we only care about the first choice
                var content = token.Delta.Content;
                var toolCalls = GetToolCalls(token);
                if (toolCalls != null && toolCalls.Length > 0)
                {
                    onMessageCompleted?.Invoke(toolCalls);
                    yield break; // Exit early if tool calls are present
                }
                if (content != null)
                {
                    onNewToken?.Invoke(content);
                }
            }
            yield return null;
        }
        
        onMessageCompleted?.Invoke(Array.Empty<ToolCall>()); // Final call with any remaining text
    }

    private ToolCall[] GetToolCalls(GroqStreamResponse.Choice c)
    {
        var calls = c.Delta.tool_calls ?? new List<GroqStreamResponse.ToolCall>();
        return calls.Select(call => new ToolCall
        {
            ToolName = call.Function.Name,
            Arguments = JObject.Parse(call.Function.Arguments),
            Id = call.Id
        }).ToArray();
    }
}