using System;
using System.Collections;

public interface IMessageHandler
{
    string Model { get; set; }
    
    /// <summary>
    /// Sends a message to the AI and returns the response.
    /// </summary>
    /// <param name="history">The chat history to complete.</param>
    /// <param name="callback">The callback invoked when the request is finished</param>
    /// <returns>A coroutine</returns>
    IEnumerator GetChatCompletion(AIMessage[] history, Action<string> callback);

    /// <summary>
    /// Sends a message to the AI and streams the response token by token.
    /// </summary>
    /// <param name="history">The chat history to complete.</param>
    /// <param name="onNewToken">The callback invoked when a new token is recieved.</param>
    /// <param name="onMessageCompleted">The callback invoked when the request is finished</param>
    /// <returns>A coroutine</returns>
    IEnumerator GetChatCompletionWithStream(AIMessage[] history, Action<string> onNewToken, Action onMessageCompleted);
}