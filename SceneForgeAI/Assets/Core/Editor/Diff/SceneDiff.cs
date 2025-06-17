using System;
using System.Reflection;
using UnityEngine;

public enum SceneDiffType
{
    AddComponent,
    RemoveComponent,
    PropertyChange
}

public class SceneDiff
{
    public SceneDiffType DiffType;
    public GameObject GameObject;
    public Type ComponentType;
    public Component Component;
    public PropertyInfo Property;
    public object OldValue;
    public object NewValue;
}