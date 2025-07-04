using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

public static class ResponseHandler
{
    public static void ApplyDiff(SceneDiff diff)
    {
        if (diff is UpdatePropertyDiff propDiff)
        {
            var component = diff.GameObject.GetComponent(diff.ComponentType);
            if (!component)
            {
                Debug.LogWarning(
                    $"Component {diff.ComponentType.Name} not found on GameObject {diff.GameObject.name}. " +
                    "This might be due to a missing AddComponent diff.");
                return;
            }
        }
        switch (diff.DiffType)
        {
            case SceneDiffType.PropertyChange:
                

                component.GetType()
                    .GetProperty(diff.Property.Name)
                    ?.SetValue(component, diff.NewValue);
                break;
            case SceneDiffType.AddComponent:
                diff.GameObject.AddComponent(diff.ComponentType);
                break;
            case SceneDiffType.RemoveComponent:
                UnityEngine.Object.DestroyImmediate(diff.GameObject.GetComponent(diff.ComponentType));
                break;
        }
    }
}