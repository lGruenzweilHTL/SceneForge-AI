using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;

public static class AIToolCollector
{
    // Keep a registry of tools for invocation
    public static Dictionary<Tool, MethodInfo> ToolRegistry { get; private set; } = new();
    public static void UpdateRegistry()
    {
        ToolRegistry.Clear();

        var methods = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(m => m.GetCustomAttribute<AIToolAttribute>() != null);

        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<AIToolAttribute>();
            ToolRegistry.Add(new Tool
            {
                type = "function",
                function = new Tool.ToolFunction
                {
                    name = method.Name,
                    description = attr.Description,
                    parameters = new Tool.ToolFunctionParameters
                    {
                        type = "object",
                        properties = method.GetParameters().ToDictionary(
                            p => p.Name,
                            p => new Tool.ToolFunctionPropertyData
                            {
                                type = GetTypeName(p.ParameterType, out var enumNames),
                                description = "not available", // TODO: provide better descriptions
                                @enum = enumNames?.ToList()
                            }),
                        required = method.GetParameters()
                            .Select(p => p.Name)
                            .ToList()
                    }
                }
            }, method);
        }
    }

    private static string GetTypeName(Type type, out string[] enumNames)
    {
        enumNames = null;
        if (type == typeof(string)) return "string";
        if (type == typeof(int)) return "integer";
        if (type == typeof(float) || type == typeof(double)) return "number";
        if (type == typeof(bool)) return "boolean";
        if (type.IsEnum)
        {
            enumNames = type.GetEnumNames();
            return "string"; // Enums are treated as strings in JSON schema
        }
        return null;
    }
}