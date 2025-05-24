using System;
using System.Linq;
using UnityEngine;

public class test : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string[] typeNames = {
            "string",
            "String",
            "System.String",
            "UnityEngine.Transform",
            "UnityEngine.Rigidbody",
            "UnityEngine.MeshRenderer",
            "UnityEngine.BoxCollider",
            "UnityEngine.SphereCollider",
            "UnityEngine.CapsuleCollider",
            "UnityEngine.Collider2D",
            "UnityEngine.Rigidbody2D",
            "UnityEngine.SpriteRenderer",
            "Transform",
            "Rigidbody",
            "UnityEngine.Transform, UnityEngine"
        };
        
        foreach (var typeName in typeNames)
        {
            Type type = Type.GetType(typeName, false, true);
            if (type == null)
            {
                type = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.FullName == typeName || t.Name == typeName);
            }
            Debug.Log($"Type {typeName} exists: {type != null}");
        }
    }
}
