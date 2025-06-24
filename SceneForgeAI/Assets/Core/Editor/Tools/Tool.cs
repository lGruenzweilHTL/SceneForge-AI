using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Plastic.Newtonsoft.Json;

public class Tool
{
    /*
     * {
         "type": "function",
         "function": {
           "name": "get_current_weather",
           "description": "Get the current weather in a given location",
           "parameters": {
             "type": "object",
             "properties": {
               "location": {
                 "type": "string",
                 "description": "The city and state, e.g. San Francisco, CA"
               },
               "unit": {
                 "type": "string",
                 "enum": ["celsius", "fahrenheit"]
               }
             },
             "required": ["location"]
           }
         }
       }
     */
    public string type { get; set; } = "function";
    public ToolFunction function { get; set; }
    
    public class ToolFunction
    {
        public string name { get; set; }
        public string description { get; set; }
        public ToolFunctionParameters parameters { get; set; }
    }
    public class ToolFunctionParameters
    {
        public string type { get; set; } = "object";
        public Dictionary<string, ToolFunctionPropertyData> properties { get; set; }
        public List<string> required { get; set; }
    }
    public class ToolFunctionPropertyData
    {
        public string type { get; set; }
        public string description { get; set; }
        [CanBeNull, JsonProperty(NullValueHandling = NullValueHandling.Ignore)] 
        public List<string> @enum { get; set; } // For enum types
    }
}