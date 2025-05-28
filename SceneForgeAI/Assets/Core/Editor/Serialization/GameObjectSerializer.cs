using System;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

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
                    .Where(p => p.PropertyType.IsPrimitive || p.PropertyType == typeof(string) || p.PropertyType == typeof(Vector3))
                    .ToDictionary(p => p.Name, p => p.PropertyType != typeof(Vector3) ? p.GetValue(c)
                        : p.GetValue(c) is Vector3 vector3 ? SerializableVector3(vector3) : null)
            }).ToArray(),
            transform = new
            {
                position = SerializableVector3(obj.transform.position),
                rotation = SerializableVector3(obj.transform.rotation.eulerAngles),
                local_scale = SerializableVector3(obj.transform.localScale)
            }
        };
    }
    
    private static object SerializableVector3(Vector3 vector)
    {
        return new[]
        {
            vector.x, vector.y, vector.z
        };
    }
}