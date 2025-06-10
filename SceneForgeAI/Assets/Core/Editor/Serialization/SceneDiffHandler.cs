using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

public static class SceneDiffHandler
{
    // Diff layers:
    // 1. UID (game object)
    // 2. Component (type, properties)
    // 3. Properties (name, value)
    public static void ApplyDiffToScene(string diff, Dictionary<string, GameObject> uidMap)
    {
        // Layer 1: Game Object
        var objectLayer = JsonConvert.DeserializeObject<Dictionary<string, object>>(diff);
        foreach ((string uid, object componentData) in objectLayer)
        {
            if (!uidMap.TryGetValue(uid, out GameObject go))
            {
                go = CreateGameObjectFromComponentData(componentData, uidMap);
                uidMap.Add(uid, go);
            }

            if (go)
            {
                ApplyDiffToObject(go, componentData.ToString());
            }
            else Debug.LogWarning($"GameObject with UID '{uid}' could not be created or found in the scene.");
        }
    }

    private static void ApplyDiffToObject(GameObject go, string diff)
    {
        // Layer 2: Component
        var componentLayer = JsonConvert.DeserializeObject<Dictionary<string, object>>(diff);
        foreach ((string type, object propertyData) in componentLayer)
        {
            if (type is "name" or "parent")
            {
                // Skip name and parent properties, they are handled separately
                continue;
            }
            
            var componentType = FindType(type);
            if (!go.TryGetComponent(componentType, out Component component))
            {
                // Create component if it doesn't exist
                if (componentType == null)
                {
                    Debug.LogWarning($"Component type '{type}' not found.");
                    continue;
                }

                component = go.AddComponent(componentType);
            }

            if (component) ApplyDiffToComponent(component, propertyData.ToString());
        }
    }

    private static void ApplyDiffToComponent(Component component, string diff)
    {
        // Layer 3: Properties
        var propertyLayer = JsonConvert.DeserializeObject<Dictionary<string, object>>(diff);
        foreach ((string propertyName, object value) in propertyLayer)
        {
            var property = component.GetType().GetProperty(propertyName)
                           ?? throw new Exception($"Property '{propertyName}' not found.");
            var deserializedValue = Deserializers.Property(property.PropertyType, value);
            if (property.CanWrite)
            {
                try
                {
                    property.SetValue(component, deserializedValue);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to set property '{propertyName}' on component '{component.GetType().Name}': {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"Property '{propertyName}' not found or not writable on component '{component.GetType().Name}'.");
            }
        }
    }

    private static GameObject CreateGameObjectFromComponentData(object data, Dictionary<string, GameObject> uidMap)
    {
        var dataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.ToString());
        if (dataDict == null || !dataDict.ContainsKey("name"))
        {
            Debug.LogWarning("Invalid component data, cannot create GameObject.");
            return null;
        }

        string name = dataDict.TryGetValue("name", out var nameObj) ? nameObj.ToString() : "New GameObject";
        GameObject go = new GameObject(name);

        if (dataDict.TryGetValue("parent", out var parentObj))
        {
            string parentUid = parentObj.ToString();
            if (!uidMap.TryGetValue(parentUid, out GameObject parentGo))
            {
                Debug.LogWarning($"Parent GameObject with UID '{parentUid}' not found.");
            }
            else
            {
                go.transform.SetParent(parentGo.transform);
            }
        }
        else
        {
            Debug.Log($"No parent specified for GameObject with name {name}, it will be a root object.");
            go.transform.SetParent(null);
        }

        return go;
    }
    
    private static Type FindType(string typeName)
    {
        Type type = Type.GetType(typeName, false, true);
        if (type == null)
        {
            type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.FullName == typeName || t.Name == typeName);
        }

        return type;
    } 
}