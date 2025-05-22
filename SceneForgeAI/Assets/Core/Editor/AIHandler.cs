using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public static class AIHandler
{
    private const string URL = "http://127.0.0.1:11434/";
    private const string GenerateURL = URL + "api/chat";
    
    private static List<AIMessage> messages = new();

    public static string Prompt(string prompt)
    {
        messages.Add(new AIMessage
        {
            role = "user",
            content = prompt
        });
        var body = new AIRequest
        {
            model = "gemma3:1b",
            messages = messages.ToArray(),
            stream = false
        };
        var json = JsonConvert.SerializeObject(body);
        var request = new UnityWebRequest(GenerateURL, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SendWebRequest();
        while (!request.isDone)
        {
            // Wait for the request to complete
        }

        if (request.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error: {request.error}");
            return null;
        }

        var response = JsonConvert.DeserializeObject<AIResponse>(request.downloadHandler.text);
        messages.Add(response.message);
        return response.message.content;
    }
    
    [MenuItem("Tools/List")]
    public static void List()
    {
        foreach (var message in messages)
        {

            Debug.Log($"{message.role}: {message.content}");
        }
    }

    public static bool CheckConnection()
    {
        var request = UnityWebRequest.Get(URL);
        request.SendWebRequest();
        while (!request.isDone)
        {
            // Wait for the request to complete
        }
        
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error: {request.error}");
            return false;
        }
        return true;
    }
}