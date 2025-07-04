using UnityEngine;

public static class ResponseHandler
{
    public static void ApplyDiff(SceneDiff diff)
    {
        var gameObject = ObjectUtility.FindByInstanceId(diff.InstanceId);
        if (diff is UpdatePropertyDiff propDiff)
        {
            var component = gameObject.GetComponent(propDiff.ComponentType);
            if (!component)
            {
                Debug.LogWarning(
                    $"Component {propDiff.ComponentType} not found on GameObject {gameObject.name}. " +
                    "This might be due to a missing AddComponent diff.");
                return;
            }
            
            component.GetType()
                .GetProperty(propDiff.PropertyName)
                ?.SetValue(component, propDiff.NewValue);
        }
        else if (diff is AddComponentDiff addDiff)
        {
            var type = ObjectUtility.FindType(addDiff.ComponentType);
            gameObject.AddComponent(type);
        }
        else if (diff is RemoveComponentDiff removeDiff)
        {
            var component = gameObject.GetComponent(removeDiff.ComponentType);
            Object.DestroyImmediate(component);
        }
        else if (diff is CreateObjectDiff createDiff)
        {
            var newObject = new GameObject(createDiff.Name);
            if (gameObject) newObject.transform.SetParent(gameObject.transform);
        }
        else if (diff is RemoveObjectDiff)
        {
            Object.DestroyImmediate(gameObject);
        }
    }
}