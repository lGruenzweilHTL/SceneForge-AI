using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;

public static class AIToolCollector
{
    public class AIToolMethod
    {
        public MethodInfo Method { get; set; }
        public string Name => Method.Name;
        public AIToolAttribute Attribute { get; set; }
    }

    // Keep a registry of tools for invocation
    public static Dictionary<string, AIToolMethod> ToolRegistry { get; private set; } = new();

    public static List<object> CollectAITools()
    {
        ToolRegistry.Clear();
        var tools = new List<object>();

        var methods = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(m => m.GetCustomAttribute<AIToolAttribute>() != null);

        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<AIToolAttribute>();
            var parameters = method.GetParameters();

            var properties = new JObject();
            var required = new List<string>();

            foreach (var param in parameters)
            {
                var typeSchema = GetJsonSchemaType(param.ParameterType);
                if (typeSchema != null)
                {
                    properties[param.Name] = typeSchema;
                    required.Add(param.Name);
                }
            }

            var toolObject = new
            {
                name = method.Name,
                description = attr.Description,
                parameters = new
                {
                    type = "object",
                    properties = properties,
                    required = required
                }
            };

            tools.Add(toolObject);
            ToolRegistry[method.Name] = new AIToolMethod { Method = method, Attribute = attr };
        }

        return tools;
    }

    private static JObject GetJsonSchemaType(Type type)
    {
        if (type == typeof(string)) return new JObject { ["type"] = "string" };
        if (type == typeof(int)) return new JObject { ["type"] = "integer" };
        if (type == typeof(float) || type == typeof(double)) return new JObject { ["type"] = "number" };
        if (type == typeof(bool)) return new JObject { ["type"] = "boolean" };
        if (type.IsEnum)
        {
            return new JObject
            {
                ["type"] = "string",
                ["enum"] = new JArray(Enum.GetNames(type))
            };
        }
        return null;
    }

    public static string GetFunctionsJson()
    {
        var tools = CollectAITools();
        return JsonConvert.SerializeObject(tools, Formatting.Indented);
    }
}