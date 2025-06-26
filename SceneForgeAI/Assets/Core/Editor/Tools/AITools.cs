using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    
    [AITool("Returns the names and instance ids of all objects in the current active scene, including their hierarchy.")]
    public static object GetObjectsWithHierarchy()
    {
        var names = new List<string>();
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            names.Add($"{root.name} (ID: {root.GetInstanceID()})");
            names.AddRange(GetChildrenNamesAndIdsWithHierarchy(root.transform, "  "));
        }
        return names;
    }
    private static List<string> GetChildrenNamesAndIdsWithHierarchy(Transform parent, string indent)
    {
        var objects = new List<string>();
        foreach (Transform child in parent)
        {
            objects.Add($"{indent}{child.name} (ID: {child.GetInstanceID()})");
            objects.AddRange(GetChildrenNamesAndIdsWithHierarchy(child, indent + "  "));
        }
        return objects;
    }
    
    [AITool("Gets information about a game object by its instance ID.")]
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
    private static object SerializableVector3(Vector3 v)
    {
        return new[]
        {
            v.x,
            v.y,
            v.z
        };
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
}