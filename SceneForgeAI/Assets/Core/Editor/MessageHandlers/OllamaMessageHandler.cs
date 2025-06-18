using System.Linq;
using System.Threading.Tasks;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class OllamaMessageHandler : IMessageHandler
{
    public OllamaMessageHandler(string model = "sceneforge")
    {
        Model = model;
    }

    private const string Endpoint = "http://127.0.0.1:11434/api/chat";

    public string Model { get; set; }

    public async Task<string> GetChatCompletion(AIMessage[] history)
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
        var streamHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        await request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonConvert.DeserializeObject<GroqResponse>(request.downloadHandler.text);
            return response.Choices.FirstOrDefault()?.Message.Content ?? "No response from AI.";
        }
       
        Debug.LogError($"Error sending message: {request.error}");
        return $"Error: {request.error}";
    }
}