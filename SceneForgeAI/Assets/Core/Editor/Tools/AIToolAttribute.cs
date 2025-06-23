using System;

[AttributeUsage(AttributeTargets.Method)]
public class AIToolAttribute : Attribute
{
    public string Description { get; }

    public AIToolAttribute(string description)
    {
        Description = description;
    }
}