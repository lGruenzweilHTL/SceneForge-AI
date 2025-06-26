using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

public static class AIToolCollector
{
    // Keep a registry of tools for invocation
    public static Dictionary<Tool, MethodInfo> ToolRegistry { get; private set; } = new();
    
    [MenuItem("Tools/Update AI Tool Registry")]
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
                        properties = method
                            .GetParameters()
                            .Select(p => new {param = p, paramAttr = p.GetCustomAttribute<AIToolParamAttribute>() 
                                                                     ?? throw new Exception($"Could not find AIToolParamAttribute for parameter {p.Name} in method {method.Name}")})
                            .ToDictionary(
                            p => p.param.Name,
                            p => new Tool.ToolFunctionPropertyData
                            {
                                type = GetTypeName(p.param.ParameterType),
                                description = p.paramAttr.Description,
                                @enum = p.paramAttr.EnumNames?.ToList()
                            }),
                        required = method.GetParameters()
                            .Select(p => p.Name)
                            .ToList()
                    }
                }
            }, method);
        }
    }

    private static string GetTypeName(Type type)
    {
        if (type == typeof(string) || type.IsEnum) return "string";
        if (type == typeof(int)) return "integer";
        if (type == typeof(float) || type == typeof(double)) return "number";
        if (type == typeof(bool)) return "boolean";
        return "object";
    }
}