using System;

public class AIToolParamAttribute : Attribute
{
    public string Description { get; set; }
    public bool IsOptional { get; set; }
    public string[] EnumNames { get; set; }

    public AIToolParamAttribute(string description)
    {
        Description = description;
    }

    public AIToolParamAttribute(string description, bool optional = false, params string[] enumNames)
    {
        Description = description;
        EnumNames = enumNames;
        IsOptional = optional;
    }
}
