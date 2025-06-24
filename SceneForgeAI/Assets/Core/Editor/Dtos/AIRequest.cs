using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;

public class AIRequest
{
    public string model;
    public AIMessage[] messages;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<Tool> tools;
    public bool stream;
}