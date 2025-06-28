using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class AITools
{
    [AITool("Returns the names and instance ids of all objects in the current active scene.")]
    public static object GetObjects()
    {
        var names = new List<string>();
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            names.Add($"{root.name} (ID: {root.GetInstanceID()})");
            names.AddRange(GetChildrenNamesAndIds(root.transform));
        }
        return names;
    }
    
    [AITool("Gets information such as tag, layer, position and rotation about a game-object by its instance ID.")]
    public static object GetObjectById([AIToolParam("The instance id of the object")] int instanceId)
    {
        var obj = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .FirstOrDefault(o => o.GetInstanceID() == instanceId);
        
        if (obj == null)
            return $"No object found with ID {instanceId}";

        var info = new
        {
            name = obj.name,
            id = obj.GetInstanceID(),
            active = obj.activeSelf,
            tag = obj.tag,
            layer = LayerMask.LayerToName(obj.layer),
            position = SerializableVector3(obj.transform.position),
            rotation = SerializableVector3(obj.transform.rotation.eulerAngles),
            scale = SerializableVector3(obj.transform.localScale),
            components = obj.GetComponents<Component>()
                .Select(c => new { type = c.GetType().Name, enabled = c is not Behaviour b || b.enabled })
                .ToList()
        };
        
        return info;
    }
    
    [AITool("Returns the names of all scenes currently loaded in the editor.")]
    public static object GetLoadedScenes()
    {
        var scenes = new List<string>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var loadedScene = SceneManager.GetSceneAt(i);
            if (loadedScene.isLoaded)
            {
                scenes.Add(loadedScene.name);
            }
        }
        return scenes;
    }
    
    [AITool("Creates a primitive object in the current active scene.\n" +
            "You can specify the type of primitive and its position, rotation, and scale.")]
    public static object CreatePrimitive(
        [AIToolParam("The type of primitive to create", false, "cube", "sphere", "capsule", "cylinder", "plane", "quad")] string primitiveType,
        [AIToolParam("The name of the primitive", true)] string name = "NewPrimitive",
        [AIToolParam("The position of the primitive as a comma-separated string (e.g. \"0,0,0\")", true)] string position = "0,0,0",
        [AIToolParam("The rotation of the primitive as a comma-separated string (e.g. \"0,0,0\")", true)] string rotation = "0,0,0",
        [AIToolParam("The scale of the primitive as a comma-separated string (e.g. \"1,1,1\")", true)] string scale = "1,1,1")
    {
        Vector3 pos;
        Vector3 rot;
        Vector3 scl;

        try
        {
            pos = ParseVector3(position);
            rot = ParseVector3(rotation);
            scl = ParseVector3(scale);
        }
        catch (FormatException e)
        {
            return $"Invalid vector format: {e.Message}";
        }

        GameObject primitive = null;
        
        switch (primitiveType.ToLower())
        {
            case "cube":
                primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
            case "sphere":
                primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                break;
            case "capsule":
                primitive = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                break;
            case "cylinder":
                primitive = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                break;
            case "plane":
                primitive = GameObject.CreatePrimitive(PrimitiveType.Plane);
                break;
            case "quad":
                primitive = GameObject.CreatePrimitive(PrimitiveType.Quad);
                break;
            default:
                return $"Unknown primitive type: {primitiveType}";
        }

        if (primitive != null)
        {
            primitive.name = name;
            primitive.transform.position = pos;
            primitive.transform.rotation = Quaternion.Euler(rot);
            primitive.transform.localScale = scl;

            return $"Created {primitiveType} at {pos} with rotation {rot} and scale {scl}. ID: {primitive.GetInstanceID()}";
        }

        return "Failed to create primitive.";
    }
    
    [AITool("Adds a component to a game object by its instance ID.")]
    public static object AddComponentToObject(
        [AIToolParam("The instance ID of the game object")] int instanceId,
        [AIToolParam("The type of component to add (e.g. Rigidbody, BoxCollider)")] string componentType) // TODO: maybe add enum for all available components
    {
        var obj = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .FirstOrDefault(o => o.GetInstanceID() == instanceId);
        
        if (obj == null)
            return $"No object found with ID {instanceId}";

        Type type = FindType(componentType);
        if (type == null)
            return $"Type '{componentType}' not found.";

        if (!typeof(Component).IsAssignableFrom(type))
            return $"'{componentType}' is not a valid component type.";

        if (!obj.AddComponent(type))
            return $"Failed to add component '{componentType}' to {obj.name} (ID: {obj.GetInstanceID()})\n" +
                   "Maybe the component is already present";
        
        return $"Added {componentType} to {obj.name} (ID: {obj.GetInstanceID()})";
    }
    
    [AITool("Gets the names of all prefabs in the project")]
    public static object GetAllPrefabs()
    {
        var prefabs = new List<string>();
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                prefabs.Add($"{prefab.name} (Path: {path})");
            }
        }
        
        return prefabs;
    }

    [AITool("Spawns a prefab in the current active scene by name\n" +
            "Returns the instance ID of the spawned object.")]
    public static object SpawnPrefabByName([AIToolParam("The name of the prefab")] string name)
    {
        string[] guids = AssetDatabase.FindAssets($"t:Prefab {name}", new[] { "Assets" });
        
        if (guids.Length == 0)
            return $"No prefab found with name '{name}'";

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        
        if (prefab == null)
            return $"Failed to load prefab at path '{path}'";

        GameObject instance = Object.Instantiate(prefab);
        instance.name = prefab.name; // Ensure the instance has the same name as the prefab

        return $"Spawned {prefab.name} at position {instance.transform.position}. ID: {instance.GetInstanceID()}";
    }

    private static List<string> GetChildrenNamesAndIds(Transform parent)
    {
        var objects = new List<string>();
        foreach (Transform child in parent)
        {
            objects.Add($"{child.name} (ID: {child.GetInstanceID()})");
            objects.AddRange(GetChildrenNamesAndIds(child));
        }
        return objects;
    }
    private static object SerializableVector3(Vector3 v)
    {
        return new[]
        {
            v.x,
            v.y,
            v.z
        };
    }
    private static Vector3 ParseVector3(string vec)
    {
        if (string.IsNullOrWhiteSpace(vec))
            throw new FormatException("Vector cannot be empty");

        var parts = vec.Split(',');
        if (parts.Length != 3)
            throw new FormatException("Vector must have exactly 3 components");

        if (!float.TryParse(parts[0].Trim(), out float x) ||
            !float.TryParse(parts[1].Trim(), out float y) ||
            !float.TryParse(parts[2].Trim(), out float z))
        {
            throw new FormatException("Invalid vector format");
        }

        return new Vector3(x, y, z);
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