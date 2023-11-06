using System;

namespace JJMasterData.Core.Events.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class CustomizedFieldsAttribute : Attribute
{
    public string[] FieldNames { get; }

    public CustomizedFieldsAttribute(params string[] fieldNames)
    {
        FieldNames = fieldNames;
    }
}