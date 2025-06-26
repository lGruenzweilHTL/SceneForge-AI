using System;
using System.Collections;

public interface IMessageHandler
{
    string Model { get; set; }
    bool StreamSupported { get; }
    bool ReasoningSupported { get; }
    
    IEnumerator FetchModels(Action<string[]> onModelsFetched);
    
    IEnumerator GetChatCompletion(AIMessage[] history, Tool[] tools, Action<string, ToolCall[]> onMessageCompleted);
    IEnumerator GetChatCompletionWithReasoning(AIMessage[] history, Tool[] tools, Action<string, string, ToolCall[]> onMessageCompleted);
    IEnumerator GetChatCompletionWithStream(AIMessage[] history, Tool[] tools, Action<string> onNewToken, Action<ToolCall[]> onMessageCompleted);
    IEnumerator GetChatCompletionWithStreamAndReasoning(AIMessage[] history, Tool[] tools, Action<string> onNewToken, Action<string> onNewReasoningToken, Action<ToolCall[]> onMessageCompleted);
}