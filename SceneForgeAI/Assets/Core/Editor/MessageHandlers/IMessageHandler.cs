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
}