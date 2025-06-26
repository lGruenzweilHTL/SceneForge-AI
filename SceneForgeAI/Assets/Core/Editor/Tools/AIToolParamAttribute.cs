using System;

public class AIToolParamAttribute : Attribute
{
    public string Description { get; set; } = "not available";
    public string[] EnumNames { get; set; } = null;

    public AIToolParamAttribute(string description)
    {
        Description = description;
    }

    public AIToolParamAttribute(string description, params string[] enumNames)
    {
        Description = description;
        EnumNames = enumNames;
    }
}
