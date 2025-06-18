using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
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
        var request = new UnityWebRequest(Endpoint, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + _key);

        var op = request.SendWebRequest();
        while (!op.isDone)
        {
            yield return null; // Wait for the request to complete
        }
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonConvert.DeserializeObject<GroqResponse>(request.downloadHandler.text);
            callback(response.Choices.FirstOrDefault()?.Message.Content ?? "No response from AI.");
            yield break; // Exit the coroutine on success
        }
       
        Debug.LogError($"Error sending message: {request.error}");
        callback($"Error: {request.error}");
    }
}