using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

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

    public IEnumerator GetChatCompletion(AIMessage[] history, Action<string> callback)
    {
        var body = new AIRequest
        {
            model = Model,
            messages = history,
            stream = false
        };

        var json = JsonConvert.SerializeObject(body);
        yield return WebRequestUtility.SendPostRequest(Endpoint, json, new Dictionary<string, string> {
            ["Content-Type"] = "application/json",
            ["Authorization"] = "Bearer " + _key
        }, request => OnRequestSuccess(request, callback), error => OnRequestError(error, callback));
    }

    
    private void OnRequestSuccess(UnityWebRequest request, Action<string> callback)
    {
        var response = JsonConvert.DeserializeObject<GroqResponse>(request.downloadHandler.text);
        callback(response.Choices.FirstOrDefault()?.Message.Content ?? "No response from AI.");
    }
    private void OnRequestError(string error, Action<string> callback)
    {
        Debug.LogError($"Error sending message: {error}");
        callback($"Error: {error}");
    }
}