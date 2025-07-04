using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class ObjectUtility
{
    public static GameObject FindByInstanceId(int instanceId)
    {
        return Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(go => go.GetInstanceID() == instanceId);
    }

    public static Type FindType(string name)
    {
        // Try to find the type in all loaded assemblies
        var type = Type.GetType(name) 
                   ?? AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .FirstOrDefault(t => t.FullName == name || t.Name == name);
        return type;
    }
}