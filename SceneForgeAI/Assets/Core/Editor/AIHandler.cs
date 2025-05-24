using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public static class AIHandler
{
    private const string URL = "http://127.0.0.1:11434/";
    private const string GenerateURL = URL + "api/chat";
    private const string Model = "sceneforge:1b";

    private static List<AIMessage> history = new()
    {
        new AIMessage
        {
            role = "assistant",
            content = "Hello! I am Scene Forge AI. How can I assist you today?"
        }
    };
    
    public static void PromptStream(string prompt)
    {
        EditorCoroutineRunner.StartCoroutine(StreamCoroutine(prompt));
    }

    private static IEnumerator StreamCoroutine(string prompt)
    {
        history.Add(new AIMessage { role = "user", content = prompt });
        var body = new AIRequest
        {
            model = Model,
            messages = history.ToArray(),
            stream = true
        };
        var json = JsonConvert.SerializeObject(body);
        var request = new UnityWebRequest(GenerateURL, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);

        var streamHandler = new StreamDownloadHandler();
        request.downloadHandler = streamHandler;
        request.SetRequestHeader("Content-Type", "application/json");
        request.SendWebRequest();

        var msg = new AIMessage { role = "assistant", content = "" };
        history.Add(msg);

        while (!request.isDone)
        {
            while (streamHandler.HasNewToken())
            {
                var token = streamHandler.GetNextToken();
                msg.content += token;
            }
            yield return null;
        }
    }
    
    public static AIMessage[] GetHistory() => history.ToArray();
}