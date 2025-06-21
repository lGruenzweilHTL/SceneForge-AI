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
    public static SceneDiff[] GetDiffFromScene(string diff, Dictionary<string, GameObject> uidMap)
    {
        SceneDiff[] diffs = { };
        // Layer 1: Game Object
        var objectLayer = JsonConvert.DeserializeObject<Dictionary<string, object>>(diff);
        foreach ((string uid, object componentData) in objectLayer)
        {
            if (!uidMap.TryGetValue(uid, out GameObject go) && AISettings.AllowObjectCreation)
            {
                go = CreateGameObjectFromComponentData(componentData, uidMap);
                uidMap.Add(uid, go);
            }

            if (go)
            {
                diffs = diffs.Concat(GetDiffFromObject(go, componentData.ToString())).ToArray();
            }
            else Debug.LogWarning($"GameObject with UID '{uid}' could not be created or found in the scene.");
        }
        return diffs;
    }

    private static SceneDiff[] GetDiffFromObject(GameObject go, string diff)
    {
        List<SceneDiff> diffs = new();
        var componentLayer = JsonConvert.DeserializeObject<Dictionary<string, object>>(diff);
        foreach ((string type, object propertyData) in componentLayer)
        {
            if (type is "name" or "parent") continue;

            var componentType = FindType(type);
            bool hasComponent = go.TryGetComponent(componentType, out Component component);

            if (!hasComponent)
            {
                if (!AISettings.AllowComponentCreation) 
                {
                    Debug.LogWarning($"Component of type '{type}' cannot be created on GameObject '{go.name}' as component creation is disabled.");
                    continue;
                }
                
                // AddComponent diff
                diffs.Add(new SceneDiff
                {
                    DiffType = SceneDiffType.AddComponent,
                    GameObject = go,
                    ComponentType = componentType
                });

                // Generate PropertyChange diffs for the soon-to-be-created component
                var propertyLayer = JsonConvert.DeserializeObject<Dictionary<string, object>>(propertyData.ToString());
                foreach ((string propertyName, object value) in propertyLayer)
                {
                    var property = componentType.GetProperty(propertyName);
                    if (property != null && property.CanWrite)
                    {
                        var deserializedValue = Deserializers.Property(property.PropertyType, value);
                        diffs.Add(new SceneDiff
                        {
                            DiffType = SceneDiffType.PropertyChange,
                            Component = null, // Will be resolved on apply
                            ComponentType = componentType,
                            GameObject = go,
                            Property = property,
                            OldValue = null,
                            NewValue = deserializedValue,
                        });
                    }
                }
                continue;
            }

            // Property diffs for existing component
            diffs.AddRange(GetDiffFromComponent(component, propertyData.ToString()));
        }
        return diffs.ToArray();
    }

    private static SceneDiff[] GetDiffFromComponent(Component component, string diff)
    {
        List<SceneDiff> diffs = new List<SceneDiff>();
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
                    diffs.Add(new SceneDiff
                    {
                        DiffType = SceneDiffType.PropertyChange,
                        Component = component,
                        Property = property,
                        OldValue = property.GetValue(component),
                        NewValue = deserializedValue,
                    });
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
        
        return diffs.ToArray();
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