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
        SendPostRequest(url, body, out var operation, headers, new DownloadHandlerBuffer());
        if (!operation.isDone) yield return null;
        OnRequestCompleted(operation.webRequest, onSuccess, onError);
    }
    private static void OnRequestCompleted(UnityWebRequest request, Action<UnityWebRequest> onSuccess, Action<string> onError)
    {
        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke(request);
        }
        else
        {
            onError?.Invoke(request.error);
        }
    }

    public static void SendPostRequest(string url, string body, out UnityWebRequestAsyncOperation operation, 
        Dictionary<string, string> headers = null, DownloadHandler downloadHandler = null)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(body);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = downloadHandler;
        if (headers != null)
        {
            foreach (var (key, value) in headers) 
            {
                request.SetRequestHeader(key, value);
            }
        }
        
        operation = request.SendWebRequest();
    }
}