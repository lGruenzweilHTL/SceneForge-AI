using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Antlr3.Runtime.Misc;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class StreamDownloadHandler<T, TError> : DownloadHandlerScript
{
    public System.Action<TError> OnError;
    private readonly Queue<T> tokens = new();
    private string incompleteLine = "";
    private System.Func<string, bool> errorOnLine;
    
    public StreamDownloadHandler(System.Func<string, bool> detectError = null)
    {
        errorOnLine = detectError;
    }

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
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    Error = (sender, args) =>
                    {
                        Debug.LogWarning($"Error deserializing JSON: {args.ErrorContext.Error.Message}");
                        args.ErrorContext.Handled = true;
                        err = true;
                    }
                };
                if (err) continue;
                
                var cleaned = string.Join("", line.Trim().SkipWhile(c => c != '{'));
                if (errorOnLine != null && errorOnLine(line))
                {
                    var errorResponse = JsonConvert.DeserializeObject<TError>(cleaned, settings);
                    if (errorResponse != null)
                    {
                        OnError?.Invoke(errorResponse);
                        continue;
                    }
                    Debug.LogWarning($"Failed to deserialize error response: {cleaned}");
                    continue;
                }
                
                var response = JsonConvert.DeserializeObject<T>(cleaned, settings);
                if (response != null) tokens.Enqueue(response);
            }
        }
        return true;
    }

    public bool HasNewToken() => tokens.Count > 0;
    public T GetNextToken() => tokens.Dequeue();
}