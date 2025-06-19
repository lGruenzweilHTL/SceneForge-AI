using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class OllamaMessageHandler : IMessageHandler
{
    public OllamaMessageHandler(string url = "http://127.0.0.1:11434", string model = "sceneforge")
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
        var request = new UnityWebRequest(_endpoint, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        var op = request.SendWebRequest();
        while (!op.isDone)
        {
            yield return null; // Wait for the request to complete
        }
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonConvert.DeserializeObject<AIResponse>(request.downloadHandler.text);
            callback(response.message.content);
            yield break; // Exit the coroutine on success
        }
       
        Debug.LogError($"Error sending message: {request.error}");
        callback($"Error: {request.error}");
    }
}