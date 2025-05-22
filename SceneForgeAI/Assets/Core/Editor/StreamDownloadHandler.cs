using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine.Networking;

public class StreamDownloadHandler : DownloadHandlerScript
{
    private readonly Queue<string> tokens = new();

    protected override bool ReceiveData(byte[] data, int dataLength)
    {
        var chunk = System.Text.Encoding.UTF8.GetString(data, 0, dataLength);
        foreach (var line in chunk.Split('\n'))
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                var response = JsonConvert.DeserializeObject<AIStreamResponse>(line);
                tokens.Enqueue(response.message.content);
            }
        }
        return true;
    }

    public bool HasNewToken() => tokens.Count > 0;
    public string GetNextToken() => tokens.Dequeue();
}