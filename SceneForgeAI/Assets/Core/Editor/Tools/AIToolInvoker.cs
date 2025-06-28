using System;
using System.Linq;
using System.Reflection;
using Unity.Plastic.Newtonsoft.Json.Linq;

public static class AIToolInvoker
{
    public static object InvokeTool(string toolName, JObject arguments)
    {
        if (!AIToolCollector.ToolRegistry.Any(t => t.Key.function.name.Equals(toolName, StringComparison.OrdinalIgnoreCase)))
            throw new Exception($"Tool '{toolName}' not found.");

        var tool = AIToolCollector.ToolRegistry.First(t => t.Key.function.name.Equals(toolName, StringComparison.OrdinalIgnoreCase));
        var method = tool.Value;
        var parameters = method.GetParameters();
        var args = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            var token = arguments.GetValue(param.Name, StringComparison.OrdinalIgnoreCase);

            if (token == null)
            {
                if (param.IsOptional)
                    args[i] = param.DefaultValue;
                else
                    throw new ArgumentException($"Missing argument: {param.Name}");
            }
            else
            {
                args[i] = token.ToObject(param.ParameterType);
            }
        }

        return method.Invoke(null, args); // Only static methods are supported
    }
}