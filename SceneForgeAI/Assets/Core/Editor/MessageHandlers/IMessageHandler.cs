using System;
using System.Collections;

public interface IMessageHandler
{
    string Model { get; set; }

    /// <summary>
    /// Sends a message to the AI and streams the response token by token.
    /// </summary>
    /// <param name="history">The chat history to complete.</param>
    /// <param name="tools">The tools the AI can use to interact with the scene</param>
    /// <param name="onNewToken">The callback invoked when a new token is received.</param>
    /// <param name="onMessageCompleted">The callback invoked when the request is finished</param>
    /// <returns>A coroutine</returns>
    IEnumerator GetChatCompletionWithStream(AIMessage[] history, Tool[] tools, Action<string> onNewToken, Action<ToolCall[]> onMessageCompleted);
}