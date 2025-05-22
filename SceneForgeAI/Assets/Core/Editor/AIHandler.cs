using UnityEngine;
using UnityEngine.Networking;

public static class AIHandler
{
    private const string URL = "http://127.0.0.1:11434/";

    public static string Prompt(string prompt)
    {
        var body = new AIRequest
        {
            model = "gemma3:1b",
            prompt = prompt,
            stream = false
        };
        var json = JsonUtility.ToJson(body);
        const string url = URL + "api/generate";
        var request = new UnityWebRequest(url, "POST");
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

        var response = JsonUtility.FromJson<AIResponse>(request.downloadHandler.text);
        return response.response;
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