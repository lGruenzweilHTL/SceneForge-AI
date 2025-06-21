using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine.Networking;

public class StreamDownloadHandler<T> : DownloadHandlerScript
{
    private readonly Queue<T> tokens = new();

    protected override bool ReceiveData(byte[] data, int dataLength)
    {
        var chunk = System.Text.Encoding.UTF8.GetString(data, 0, dataLength);
        foreach (var line in chunk.Split('\n'))
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                var cleaned = string.Join("", line.Trim().SkipWhile(c => c != '{'));
                var response = JsonConvert.DeserializeObject<T>(cleaned);
                if (response != null) tokens.Enqueue(response);
            }
        }
        return true;
    }

    public bool HasNewToken() => tokens.Count > 0;
    public T GetNextToken() => tokens.Dequeue();
}