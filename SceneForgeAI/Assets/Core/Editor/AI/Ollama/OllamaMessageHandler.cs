using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class OllamaMessageHandler : IMessageHandler
{
    public OllamaMessageHandler(string url = "http://127.0.0.1:11434", string model = "deepseek-coder:6.7b")
    {
        Model = model;
        _endpoint = url.EndsWith("/") ? url + "api/chat" : url + "/api/chat";
        _modelEndpoint = url.EndsWith("/") ? url + "api/tags" : url + "/api/tags";
    }

    private readonly string _endpoint;
    private readonly string _modelEndpoint;

    public string Model { get; set; }
    public bool StreamSupported => true;
    public bool ReasoningSupported => false; // Ollama does not support reasoning in the same way as some other providers
    public bool ImagesSupported => false;

    public IEnumerator FetchModels(Action<string[]> onModelsFetched)
    {
        var downloadHandler = new DownloadHandlerBuffer();
        var operation = WebRequestUtility.SendGetRequest(_modelEndpoint, new Dictionary<string, string>(), downloadHandler);
        while (!operation.isDone) yield return null;
        
        if (operation.webRequest.result == UnityWebRequest.Result.Success)
        {
            var responses = JsonConvert.DeserializeObject<OllamaModelResponse[]>(downloadHandler.text);
            onModelsFetched?.Invoke(responses.SelectMany(r => r.Models.Select(m => m.Model)).ToArray());
        }
        else
        {
            Debug.LogError($"Error fetching models: {operation.webRequest.error}");
            onModelsFetched?.Invoke(Array.Empty<string>());
        }
    }

    public IEnumerator GetChatCompletion(AIMessage[] history, Tool[] tools, Action<string, ToolCall[]> onMessageCompleted)
    {
        throw new NotImplementedException();
    }

    public IEnumerator GetChatCompletionWithReasoning(AIMessage[] history, Tool[] tools, Action<string, string, ToolCall[]> onMessageCompleted)
    {
        throw new NotImplementedException();
    }

    // TODO: emulate tool calls somehow, Ollama does not support tool calls natively like OpenAI or Groq
    public IEnumerator GetChatCompletionWithStream(AIMessage[] history, Tool[] tools, Action<string> onNewToken, Action<ToolCall[]> onMessageCompleted)
    {
        var body = new AIRequest
        {
            model = Model,
            messages = history,
            stream = true
        };

        var json = JsonConvert.SerializeObject(body);
        var downloadHandler = new StreamDownloadHandler<AIStreamResponse>();
        var operation = WebRequestUtility.SendPostRequest(_endpoint, json, new Dictionary<string, string>
        {
            ["Content-Type"] = "application/json"
        }, downloadHandler);
        while (!operation.isDone)
        {
            while (downloadHandler.HasNewToken())
            {
                var token = downloadHandler.GetNextToken();
                onNewToken?.Invoke(token.message.content);
            }
            yield return null;
        }
        onMessageCompleted?.Invoke(null);
    }

    public IEnumerator GetChatCompletionWithStreamAndReasoning(AIMessage[] history, Tool[] tools, Action<string> onNewToken,
        Action<string> onNewReasoningToken, Action<ToolCall[]> onMessageCompleted)
    {
        throw new NotImplementedException();
    }

    private void OnRequestSuccess(UnityWebRequest request, Action<string> callback)
    {
        var response = JsonConvert.DeserializeObject<AIResponse>(request.downloadHandler.text);
        callback(response.message.content);
    }

    private void OnRequestError(string error, Action<string> callback)
    {
        Debug.LogError($"Error sending message: {error}");
        callback($"Error: {error}");
    }
}