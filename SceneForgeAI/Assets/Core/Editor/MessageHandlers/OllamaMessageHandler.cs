using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class OllamaMessageHandler : IMessageHandler
{
    public OllamaMessageHandler(string url = "http://127.0.0.1:11434", string model = "deepseek-coder:6.7b")
    {
        Model = model;
        _endpoint = url.EndsWith("/") ? url + "api/chat" : url + "/api/chat";
    }

    private readonly string _endpoint;

    public string Model { get; set; }
    
    public IEnumerator GetChatCompletion(AIMessage[] history, Action<string> callback)
    {
        var body = new AIRequest
        {
            model = Model,
            messages = history,
            stream = false
        };

        var json = JsonConvert.SerializeObject(body);
        yield return WebRequestUtility.SendPostRequest(_endpoint, json, new Dictionary<string, string>
        {
            ["Content-Type"] = "application/json"
        }, request => OnRequestSuccess(request, callback), error => OnRequestError(error, callback));
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