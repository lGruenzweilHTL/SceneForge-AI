using System;
using System.Reflection;
using Unity.Plastic.Newtonsoft.Json.Linq;

public static class AIToolInvoker
{
    public static object InvokeTool(string toolName, JObject arguments)
    {
        if (!AIToolCollector.ToolRegistry.TryGetValue(toolName, out var tool))
            throw new Exception($"Tool '{toolName}' not found.");

        var method = tool.Method;
        var parameters = method.GetParameters();
        var args = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            var token = arguments.GetValue(param.Name, StringComparison.OrdinalIgnoreCase);

            if (token == null)
                throw new ArgumentException($"Missing argument: {param.Name}");

            args[i] = token.ToObject(param.ParameterType);
        }

        return method.Invoke(null, args); // Static methods only
    }
}