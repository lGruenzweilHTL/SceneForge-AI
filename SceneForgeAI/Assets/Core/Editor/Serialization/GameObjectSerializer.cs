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
                    .Where(p => p.Name != "material" && p.Name != "materials") // Instance materials should not be serialized in edit mode
                    .Where(p => p.CanRead && p.CanWrite)    
                    .Select(p => new
                    {
                        Name = p.Name,
                        Value = Serializers.Property(p.PropertyType, p.GetValue(c))
                    })
                    .Where(p => p?.Value != null)
                    .ToDictionary(p => p.Name, p => p.Value)
            }).ToArray(),
            Transform = Serializers.Transform(obj.transform)
        };
    }
}