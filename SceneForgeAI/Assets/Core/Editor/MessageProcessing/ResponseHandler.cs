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
        switch (diff.DiffType)
        {
            case SceneDiffType.PropertyChange:
                var component = diff.Component ?? diff.GameObject.GetComponent(diff.ComponentType);
                if (!component)
                {
                    Debug.LogWarning(
                        $"Component {diff.ComponentType.Name} not found on GameObject {diff.GameObject.name}. " +
                        "This might be due to a missing AddComponent diff.");
                    return;
                }

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