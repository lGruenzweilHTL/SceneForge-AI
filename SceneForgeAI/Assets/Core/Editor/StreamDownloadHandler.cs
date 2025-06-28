using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class StreamDownloadHandler<T> : DownloadHandlerScript
{
    private readonly Queue<T> tokens = new();
    private string incompleteLine = "";

    protected override bool ReceiveData(byte[] data, int dataLength)
    {
        var chunk = System.Text.Encoding.UTF8.GetString(data, 0, dataLength);
        var lines = chunk.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // Insert the incomplete line from the last packet
            if (!string.IsNullOrEmpty(incompleteLine))
            {
                line = incompleteLine + line;
                incompleteLine = "";
            }

            // Find incomplete line at the end of the chunk
            if (i == lines.Length - 1 && !string.IsNullOrWhiteSpace(line) && !line.TrimEnd().EndsWith("}"))
            {
                incompleteLine = line;
                continue;
            }

            if (!string.IsNullOrWhiteSpace(line))
            {
                bool err = false;
                var cleaned = string.Join("", line.Trim().SkipWhile(c => c != '{'));
                var response = JsonConvert.DeserializeObject<T>(cleaned, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    Error = (sender, args) =>
                    {
                        Debug.LogWarning($"Error deserializing JSON: {args.ErrorContext.Error.Message}");
                        args.ErrorContext.Handled = true;
                        err = true;
                    }
                });
                if (response != null && !err) tokens.Enqueue(response);
            }
        }
        return true;
    }

    public bool HasNewToken() => tokens.Count > 0;
    public T GetNextToken() => tokens.Dequeue();
}