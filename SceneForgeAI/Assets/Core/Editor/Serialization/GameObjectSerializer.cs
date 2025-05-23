using System;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public static class GameObjectSerializer
{
    public static string SerializeSelection()
    {
        var selectedObjects = Selection.gameObjects;
        var serializedObject = selectedObjects
            .Select(SerializeObject);
        
        return JsonConvert.SerializeObject(serializedObject);
    }

    private static object SerializeObject(GameObject obj)
    {
        return new
        {
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
        return new
        {
            x = vector.x,
            y = vector.y,
            z = vector.z
        };
    }
}