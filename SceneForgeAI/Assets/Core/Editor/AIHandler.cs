using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public static class AIHandler
{
    private const string URL = "http://127.0.0.1:11434/";
    private const string GenerateURL = URL + "api/chat";
    private const string Model = "sceneforge";
    
    private static string _mostRecentDiff = "";

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
        var sceneJson = JsonConvert.SerializeObject(new
        {
            uidMap = GameObjectSerializer.SerializeSelection()
        }, Formatting.None);
        history.Add(new AIMessage
        {
            role = "user", 
            content = "Scene JSON: " 
            + sceneJson + "\n\nUser Prompt: " + prompt
        });
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
        
        var startIndex = msg.content.IndexOf("```json", StringComparison.Ordinal) + 7;
        var endIndex = msg.content.IndexOf("```", startIndex, StringComparison.Ordinal);
        if (startIndex >= 0 && endIndex > startIndex)
        {
            _mostRecentDiff = msg.content.Substring(startIndex, endIndex - startIndex).Trim();
            ApplyDiff(_mostRecentDiff);
        }
        else
        {
            Debug.LogWarning("No valid JSON diff found in the response.");
        }
    }

    private static void ApplyDiff(string diff)
    {
        SceneDiffHandler.ApplyDiffToScene(diff, GetUidMap());
    }
    private static Dictionary<string, GameObject> GetUidMap()
    {
        return Selection.gameObjects
            .Select((obj, idx) => new { obj, index = idx })
            .ToDictionary(pair => pair.index.ToString(), pair => pair.obj);
    }
    
    public static AIMessage[] GetHistory() => history.ToArray();
}