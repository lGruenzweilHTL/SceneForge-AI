using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class GroqMessageHandler : IMessageHandler
{
    public GroqMessageHandler(string key, string model)
    {
        Model = model;
        _key = key;
    }

    private const string Endpoint = "https://api.groq.com/openai/v1/chat/completions";
    private const string ModelsEndpoint = "https://api.groq.com/openai/v1/models";
    private readonly string _key;
    private IMessageHandler _messageHandlerImplementation;
    private string AuthString => "Bearer " + _key;

    public string Model { get; set; }
    public bool StreamSupported => true;
    public bool ReasoningSupported => true;
    public bool ImagesSupported => Model is "meta-llama/llama-4-scout-17b-16e-instruct" or "meta-llama/llama-4-maverick-17b-128e-instruct";

    public IEnumerator FetchModels(Action<string[]> onModelsFetched)
    {
        // GET https://api.groq.com/openai/v1/models
        var downloadHandler = new DownloadHandlerBuffer();
        var operation = WebRequestUtility.SendGetRequest(ModelsEndpoint, new Dictionary<string, string>
        {
            ["Authorization"] = AuthString
        }, downloadHandler);
        while (!operation.isDone) yield return null;
        
        if (operation.webRequest.result == UnityWebRequest.Result.Success)
        {
            var responses = JsonConvert.DeserializeObject<GroqModelResponse[]>(downloadHandler.text);
            onModelsFetched?.Invoke(responses.SelectMany(m => m.Data.Select(d => d.Id)).ToArray());
        }
        else
        {
            Debug.LogError($"Error fetching models: {operation.webRequest.error}");
            onModelsFetched?.Invoke(Array.Empty<string>());
        }
    }

    public IEnumerator GetChatCompletion(AIMessage[] history, Tool[] tools, Action<string, ToolCall[]> onMessageCompleted)
    {
        yield return GetChatCompletionWithReasoning(history, tools, (content, reasoning, toolCalls) =>
        {
            onMessageCompleted?.Invoke(content, toolCalls);
        });
    }

    public IEnumerator GetChatCompletionWithReasoning(AIMessage[] history, Tool[] tools, Action<string, string, ToolCall[]> onMessageCompleted)
    {
        var body = new AIRequest
        {
            model = Model,
            messages = history,
            tools = tools?.ToList(),
            stream = false
        };
        
        var json = JsonConvert.SerializeObject(body);
        var downloadHandler = new DownloadHandlerBuffer();
        var operation = WebRequestUtility.SendPostRequest(Endpoint, json, new Dictionary<string, string>
        {
            ["Content-Type"] = "application/json",
            ["Authorization"] = AuthString
        }, downloadHandler);
        
        while (!operation.isDone) yield return null;

        if (operation.webRequest.result == UnityWebRequest.Result.Success)
        {
            var response = JsonConvert.DeserializeObject<GroqResponse>(downloadHandler.text);
            onMessageCompleted?.Invoke(response.Choices[0].Message.Content, response.Choices[0].Message.Reasoning, GetToolCalls(response.Choices[0]));
        }
        else
        {
            Debug.LogError($"Error fetching chat completion: {operation.webRequest.error}");
            onMessageCompleted?.Invoke(null, null, Array.Empty<ToolCall>());
        }
    }

    public IEnumerator GetChatCompletionWithStream(AIMessage[] history, Tool[] tools, Action<string> onNewToken, Action<ToolCall[]> onMessageCompleted)
    {
        yield return GetChatCompletionWithStreamAndReasoning(history, tools, onNewToken, null, onMessageCompleted);
    }

    public IEnumerator GetChatCompletionWithStreamAndReasoning(AIMessage[] history, Tool[] tools, Action<string> onNewToken,
        Action<string> onNewReasoningToken, Action<ToolCall[]> onMessageCompleted)
    {
        var body = new
        {
            model = Model,
            messages = history.Select(GetVisionMessage).ToArray(),
            tools = tools?.ToList(),
            stream = true
        };

        var json = JsonConvert.SerializeObject(body, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });
        var downloadHandler = new StreamDownloadHandler<GroqStreamResponse, GroqErrorResponse>(
            line => !line.StartsWith("data"));
        downloadHandler.OnError += err =>
        {
            Debug.LogError($"Error in request: {err.error.message}");
            onMessageCompleted?.Invoke(Array.Empty<ToolCall>());
        };
        
        var operation = WebRequestUtility.SendPostRequest(Endpoint, json, new Dictionary<string, string>
        {
            ["Content-Type"] = "application/json",
            ["Authorization"] = AuthString
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
                string content = token.Delta.Content, 
                    reasoning = token.Delta.Reasoning;
                var toolCalls = GetToolCalls(token);
                if (toolCalls != null && toolCalls.Length > 0)
                {
                    onMessageCompleted?.Invoke(toolCalls);
                    yield break; // Exit early if tool calls are present
                }
                
                if (content != null) onNewToken?.Invoke(content);
                if (reasoning != null) onNewReasoningToken?.Invoke(reasoning);
                
            }
            yield return null;
        }
        
        onMessageCompleted?.Invoke(Array.Empty<ToolCall>());
    }

    private object GetVisionMessage(AIMessage msg)
    {
        return new
        {
            msg.role,
            msg.tool_call_id,
            msg.name,
            content = (object)(msg.image_urls == null || msg.image_urls.Length == 0 || !ImagesSupported
                ? msg.content
                : new List<object>
                {
                    new
                    {
                        type = "text",
                        text = msg.content
                    }
                }
                .Concat(msg.image_urls.Select(img => new
                {
                    type = "image_url",
                    image_url = new { url = img }
                })))
        };
    }

    private static ToolCall[] GetToolCalls(GroqStreamResponse.Choice c)
    {
        var calls = c.Delta.tool_calls ?? new List<GroqStreamResponse.ToolCall>();
        return calls.Select(call => new ToolCall
        {
            ToolName = call.Function.Name,
            Arguments = JObject.Parse(call.Function.Arguments),
            Id = call.Id
        }).ToArray();
    }
    private static ToolCall[] GetToolCalls(GroqResponse.Choice c)
    {
        var calls = c.Message.tool_calls ?? new List<GroqResponse.ToolCall>();
        return calls.Select(call => new ToolCall
        {
            ToolName = call.Function.Name,
            Arguments = JObject.Parse(call.Function.Arguments),
            Id = call.Id
        }).ToArray();
    }
}