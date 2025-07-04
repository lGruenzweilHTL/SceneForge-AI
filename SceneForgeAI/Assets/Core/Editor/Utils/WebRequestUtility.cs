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
        var operation = SendPostRequest(url, body, headers, new DownloadHandlerBuffer());
        while (!operation.isDone) yield return null;
        OnRequestCompleted(operation.webRequest, onSuccess, onError);
    }

    public static UnityWebRequestAsyncOperation SendPostRequest(string url, string body,
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
        
        return request.SendWebRequest();
    }
    
    public static IEnumerator SendGetRequest(string url, Dictionary<string, string> headers = null,
        Action<UnityWebRequest> onSuccess = null, Action<string> onError = null)
    {
        var operation = SendGetRequest(url, headers, new DownloadHandlerBuffer());
        while (!operation.isDone) yield return null;
        OnRequestCompleted(operation.webRequest, onSuccess, onError);
    }

    public static UnityWebRequestAsyncOperation SendGetRequest(string url,
        Dictionary<string, string> headers = null, DownloadHandler downloadHandler = null)
    {
        var request = new UnityWebRequest(url, "GET");
        request.downloadHandler = downloadHandler ?? new DownloadHandlerBuffer();
        if (headers != null)
        {
            foreach (var (key, value) in headers) 
            {
                request.SetRequestHeader(key, value);
            }
        }
        
        return request.SendWebRequest();
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
}