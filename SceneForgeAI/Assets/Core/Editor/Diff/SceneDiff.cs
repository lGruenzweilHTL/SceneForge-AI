using System.Reflection;
using UnityEngine;

public struct SceneDiff
{
    public Component Component;
    public PropertyInfo Property;
    public object OldValue;
    public object NewValue;
}