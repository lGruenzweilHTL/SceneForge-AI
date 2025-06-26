using System.Collections.Generic;

public class GroqResponse
{
    public string Id { get; set; }
    public string Object { get; set; }
    public long Created { get; set; }
    public string Model { get; set; }
    public List<Choice> Choices { get; set; }
    public UsageData Usage { get; set; }

    public class Choice
    {
        public int Index { get; set; }
        public Message Message { get; set; }
        public string FinishReason { get; set; }
    }

    public class Message
    {
        public string Role { get; set; }
        public string Content { get; set; }
        public string Reasoning { get; set; }
        public List<ToolCall> tool_calls { get; set; }
    }
    
    public class ToolCall
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public ToolCallFunction Function { get; set; }
    }

    public class ToolCallFunction
    {
        public string Name { get; set; }
        public string Arguments { get; set; }
    }

    public class UsageData
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }
}