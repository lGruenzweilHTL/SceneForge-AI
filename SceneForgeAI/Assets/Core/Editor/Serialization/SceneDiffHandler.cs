using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            }

            if (go)
            {
                ApplyDiffToObject(go, componentData.ToString());
                uidMap.Add(uid, go);
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
            var property = component.GetType().GetProperty(propertyName);
            if (property == null || !property.CanWrite) continue;

            if (property.PropertyType.IsPrimitive || property.PropertyType == typeof(string))
            {
                property.SetValue(component, Convert.ChangeType(value, property.PropertyType));
            }
            else if (property.PropertyType == typeof(Vector2))
            {
                var vector = DeserializeVector2(value.ToString());
                property.SetValue(component, vector);
            }
            else if (property.PropertyType == typeof(Vector3))
            {
                var vector = DeserializeVector3(value.ToString());
                property.SetValue(component, vector);
            }
            else if (property.PropertyType == typeof(Quaternion))
            {
                var eulerAngles = DeserializeVector3(value.ToString());
                property.SetValue(component, Quaternion.Euler(eulerAngles));
            }
            else if (property.PropertyType == typeof(Color))
            {
                var color = DeserializeColorRgba(value.ToString());
                property.SetValue(component, color);
            }
            else if (property.PropertyType.IsEnum)
            {
                var enumValue = Enum.Parse(property.PropertyType, value.ToString());
                property.SetValue(component, enumValue);
            }
            else if (property.PropertyType.IsArray)
            {
                var array = JsonConvert.DeserializeObject(value.ToString(), property.PropertyType);
                property.SetValue(component, array);
            }
            else if (property.PropertyType.IsGenericType &&
                     property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var listType = property.PropertyType.GetGenericArguments()[0];
                var list = JsonConvert.DeserializeObject(value.ToString(), typeof(List<>).MakeGenericType(listType));
                property.SetValue(component, list);
            }
            else if (property.PropertyType == typeof(Dictionary<string, object>))
            {
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(value.ToString());
                property.SetValue(component, dictionary);
            }
            else
            {
                Debug.LogWarning($"Unsupported property type: {property.PropertyType}");
            }
        }
    }

    private static Vector2 DeserializeVector2(string json)
    {
        var vectorArray = JsonConvert.DeserializeObject<float[]>(json);
        if (vectorArray.Length != 2)
        {
            throw new ArgumentException("Invalid Vector2 format");
        }

        return new Vector2(vectorArray[0], vectorArray[1]);
    }

    private static Vector3 DeserializeVector3(string json)
    {
        var vectorArray = JsonConvert.DeserializeObject<float[]>(json);
        if (vectorArray.Length != 3)
        {
            throw new ArgumentException("Invalid Vector3 format");
        }

        return new Vector3(vectorArray[0], vectorArray[1], vectorArray[2]);
    }

    private static Color DeserializeColorRgba(string json)
    {
        var colorArray = JsonConvert.DeserializeObject<float[]>(json);
        if (colorArray.Length != 4)
        {
            throw new ArgumentException("Invalid Color format");
        }
        
        return new Color(colorArray[0], colorArray[1], colorArray[2], colorArray[3]);
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