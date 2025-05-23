using System;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class SceneDiffHandler
{
    // Diff layers:
    // 1. UID (game object)
    // 2. Component (type, properties)
    // 3. Properties (name, value)
    public static void ApplyDiffToScene(Scene scene, string diff, Dictionary<string, GameObject> uidMap)
    {
        // Layer 1: Game Object
        var objectLayer = JsonConvert.DeserializeObject<Dictionary<string, object>>(diff);
        foreach ((string uid, object componentData) in objectLayer)
        {
            GameObject go = uidMap[uid];
            ApplyDiffToObject(go, componentData.ToString());
        }
    }

    private static void ApplyDiffToObject(GameObject go, string diff)
    {
        // Layer 2: Component
        var componentLayer = JsonConvert.DeserializeObject<Dictionary<string, object>>(diff);
        foreach ((string type, object propertyData) in componentLayer)
        {
            Component component = go.GetComponent(type);
            ApplyDiffToComponent(component, propertyData.ToString());
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
            else if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
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
}