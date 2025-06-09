using System;
using System.Linq;
using System.Reflection;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class GameObjectSerializer
{
    [MenuItem("Tools/Serialize Selection")]
    public static void SerializeSelectionMenu()
    {
        var serializedData = SerializeSelection();
        var json = JsonConvert.SerializeObject(serializedData, Formatting.Indented);
        Debug.Log(json);
    }
    
    public static object SerializeSelection()
    {
        var selectedObjects = Selection.gameObjects;
        var serializedObject = selectedObjects
            .Select((obj, idx) => new {obj = SerializeObject(obj, idx.ToString()), uid = idx.ToString()});
        var dictionary = serializedObject
            .ToDictionary(pair => pair.uid, pair => pair.obj);

        return dictionary;
    }

    private static object SerializeObject(GameObject obj, string uid)
    {
        return new
        {
            uid = uid,
            name = obj.name,
            active = obj.activeSelf,
            tag = obj.tag,
            layer = obj.layer,
            components = obj.GetComponents<Component>()
                .Where(c => c.GetType() != typeof(Transform))
                .Select(c => new
            {
                type = c.GetType().Name,
                properties = c
                    .GetType()
                    .GetProperties()
                    .Where(p => p.CustomAttributes.All(a => a.AttributeType != typeof(ObsoleteAttribute)))
                    .Where(p => p.CanRead && p.CanWrite)
                    .ToDictionary(p => p.Name, p => SerializeProperty(p, c))
            }).ToArray(),
            Transform = new
            {
                position = SerializableVector3(obj.transform.position),
                rotation = SerializableVector3(obj.transform.rotation.eulerAngles),
                local_scale = SerializableVector3(obj.transform.localScale)
            }
        };
    }

    private static object SerializeProperty(PropertyInfo prop, object obj)
    {
        if (prop.PropertyType == typeof(Vector3))
        {
            return SerializableVector3((Vector3)prop.GetValue(obj));
        }
        if (prop.PropertyType == typeof(Vector2))
        {
            return SerializableVector2((Vector2)prop.GetValue(obj));
        }
        if (prop.PropertyType == typeof(Quaternion))
        {
            return SerializableQuaternion((Quaternion)prop.GetValue(obj));
        }
        if (prop.PropertyType == typeof(Color))
        {
            return SerializableColor((Color)prop.GetValue(obj));
        }
        if (prop.PropertyType.IsEnum)
        {
            return prop.GetValue(obj).ToString();
        }
        if (prop.PropertyType == typeof(LayerMask))
        {
            return ((LayerMask)prop.GetValue(obj)).value;
        }
        if (prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string))
        {
            return prop.GetValue(obj);
        }
        Debug.LogWarning($"Property {prop.Name} of type {prop.PropertyType} is not currently serializable.");
        return "Unsupported Type";
    }
    private static object SerializableVector3(Vector3 vector)
    {
        return new[]
        {
            vector.x, vector.y, vector.z
        };
    }
    private static object SerializableVector2(Vector2 vector)
    {
        return new[]
        {
            vector.x, vector.y
        };
    }
    private static object SerializableQuaternion(Quaternion quaternion)
    {
        return SerializableVector3(quaternion.eulerAngles);
    }
    private static object SerializableColor(Color color)
    {
        return new[]
        {
            color.r, color.g, color.b, color.a
        };
    }
}