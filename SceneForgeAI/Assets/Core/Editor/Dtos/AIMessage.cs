using Unity.Plastic.Newtonsoft.Json;

public class AIMessage
{
    public string role { get; set; }
    public string content { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string name { get; set; } = null; // Optional, used for tool calls
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string tool_call_id { get; set; } = null; // Optional, used to track tool calls
}