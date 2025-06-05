using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public static class AIHandler
{
    private const string URL = "http://127.0.0.1:11434/";
    private const string Endpoint = URL + "api/chat";
    private const string DefaultModel = "sceneforge";

    private static readonly AIMessage Greeting = new()
    {
        role = "assistant",
        content = "Hello! I am Scene Forge AI. How can I assist you today?"
    };
    
    private static string _mostRecentDiff = "";

    private static List<Chat> _chats = new()
    {
        new Chat
        {
            Name = "New Chat",
            History = new List<AIMessage> { Greeting },
        }
    };

    private static Chat _currentChat = _chats[0];
    
    public static void PromptStream(string prompt)
    {
        EditorCoroutineRunner.StartCoroutine(StreamCoroutine(prompt, Selection.gameObjects));
    }

    private static IEnumerator StreamCoroutine(string prompt, GameObject[] selection = null)
    {
        var sceneJson = JsonConvert.SerializeObject(new
        {
            uidMap = GameObjectSerializer.SerializeSelection()
        }, Formatting.None);
        _currentChat.History.Add(new AIMessage
        {
            role = "user", 
            content = "Scene JSON: " 
            + sceneJson + "\n\nUser Prompt: " + prompt
        });
        var body = new AIRequest
        {
            model = DefaultModel,
            messages = _currentChat.History.ToArray(),
            stream = true
        };
        var json = JsonConvert.SerializeObject(body);
        var request = new UnityWebRequest(Endpoint, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);

        var streamHandler = new StreamDownloadHandler();
        request.downloadHandler = streamHandler;
        request.SetRequestHeader("Content-Type", "application/json");
        request.SendWebRequest();

        var msg = new AIMessage { role = "assistant", content = "" };
        _currentChat.History.Add(msg);

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
            SceneDiffHandler.ApplyDiffToScene(_mostRecentDiff, BuildUidMap(selection));
        }
        else
        {
            Debug.LogWarning("No valid JSON diff found in the response.");
        }
    }
    
    private static Dictionary<string, GameObject> BuildUidMap(GameObject[] selection = null)
    {
        if (selection == null || selection.Length == 0)
        {
            selection = Selection.gameObjects;
        }
        
        return selection
            .Select((obj, idx) => new { obj, index = idx })
            .ToDictionary(pair => pair.index.ToString(), pair => pair.obj);
    }
    
    public static AIMessage[] GetCurrentChatHistory()
    {
        return _currentChat.History.ToArray();
    }
    public static AIMessage[] GetHistory(string chat)
    {
        var foundChat = _chats.FirstOrDefault(c => c.Name == chat);
        return !string.IsNullOrEmpty(foundChat.Name) ? foundChat.History.ToArray() : Array.Empty<AIMessage>();
    }
    public static void SetCurrentChat(int idx)
    {
        if (idx < 0 || idx >= _chats.Count)
        {
            Debug.LogError("Invalid chat index.");
            return;
        }
        _currentChat = _chats[idx];
    }

    public static Chat NewChat([CanBeNull] string name = null, bool updateCurrent = true)
    {
        Chat c = new Chat
        {
            Name = name ?? "New Chat",
            History = new List<AIMessage> { Greeting }
        };
        _chats.Add(c);
        
        if (updateCurrent)
        {
            _currentChat = c;
        }

        return c;
    }
    public static void DeleteChat(string chatName)
    {
        _chats = _chats.Where(c => c.Name != chatName).ToList();
    }
    public static string[] GetChatNames()
    {
        return _chats.Select(c => c.Name).ToArray();
    }
}