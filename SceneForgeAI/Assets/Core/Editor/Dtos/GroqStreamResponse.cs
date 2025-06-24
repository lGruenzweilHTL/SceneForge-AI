using System.Collections.Generic;

public class GroqStreamResponse
{
    // data: {"id":"chatcmpl-22a515a4-ab53-4406-b9e6-f50cf68f470a","object":"chat.completion.chunk","created":1750510346,"model":"gemma2-9b-it","system_fingerprint":"fp_10c08bf97d","choices":[{"index":0,"delta":{"role":"assistant","content":""},"logprobs":null,"finish_reason":null}],"x_groq":{"id":"req_01jy98r8cwfgqb86a6ndm3n4jf"}}

    public string Id { get; set; }
    public string Object { get; set; }
    public ulong Created { get; set; }
    public string Model { get; set; }
    public string SystemFingerprint { get; set; }
    public List<Choice> Choices { get; set; }
    public GroqXGroq XGroq { get; set; }


    public class Choice
    {
        public int Index { get; set; }
        public Delta Delta { get; set; }
        public object Logprobs { get; set; }
        public object FinishReason { get; set; }
    }

    public class Delta
    {
        public string Role { get; set; }
        public string Content { get; set; }
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

    public class GroqXGroq
    {
        public string Id { get; set; }
    }
}