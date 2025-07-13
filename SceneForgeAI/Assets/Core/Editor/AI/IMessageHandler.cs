using System;
using System.Collections;

public interface IMessageHandler
{
    string Model { get; set; }
    bool StreamSupported { get; }
    bool ImagesSupported { get; }
    
    /// <summary>
    /// Fetches all available models from the AI service.
    /// </summary>
    /// <param name="onModelsFetched">Callback invoked when the models are fetched. Returns null if said service endpoint is unavailable</param>
    /// <returns>A coroutine that completes when the request is done</returns>
    IEnumerator FetchModels(Action<string[]> onModelsFetched);
    

    /// <summary>
    /// Gets a chat completion from the AI service without streaming.
    /// </summary>
    /// <param name="history">The chat history to complete</param>
    /// <param name="tools">The available tools for the AI to use</param>
    /// <param name="onMessageCompleted">Callback invoked when the request is completed.
    /// The first parameter is the response text, the second is the reasoning text (if any), the third is an array of tool calls made by the AI.
    /// The fourth parameter indicates if the Model should be reprompted e.g. because of a tool call</param>
    /// <returns>A coroutine that completes when the request is done.</returns>
    IEnumerator GetChatCompletion(AIMessage[] history, Tool[] tools, Action<string, string, ToolCall[], bool> onMessageCompleted);
    
    /// <summary>
    /// Gets a chat completion from the AI service with streaming support. Only used if StreamSupported is true.
    /// </summary>
    /// <param name="history">The chat history to complete</param>
    /// <param name="tools">The available tools for the AI to use</param>
    /// <param name="onNewToken">Callback invoked when a new response token is received</param>
    /// <param name="onNewReasoningToken">Callback invoked when a new reasoning token is received (if supported)</param>
    /// <param name="onMessageCompleted">Callback invoked when the request is done. The parameters represent the tool calls the AI would like to make and if the Model should be reprompted e.g. because of tool calls</param>
    /// <returns>A coroutine that completes when the request is done</returns>
    IEnumerator GetChatCompletionStreamed(AIMessage[] history, Tool[] tools, Action<string> onNewToken, Action<string> onNewReasoningToken, Action<ToolCall[], bool> onMessageCompleted);
}