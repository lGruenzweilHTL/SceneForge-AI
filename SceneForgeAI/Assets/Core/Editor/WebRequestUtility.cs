using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class WebRequestUtility
{
    public static IEnumerator SendPostRequest(string url, string body, Dictionary<string, string> headers = null,
        Action<UnityWebRequest> onSuccess = null, Action<string> onError = null)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(body);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        if (headers != null)
        {
            foreach (var (key, value) in headers) 
            {
                request.SetRequestHeader(key, value);
            }
        }

        var op = request.SendWebRequest();
        while (!op.isDone)
        {
            yield return null; // Wait for the request to complete
        }

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke(request);
            yield break; // Exit the coroutine on success
        }

        Debug.LogError($"Error sending request: {request.error}");
        onError?.Invoke(request.error);
    }
}