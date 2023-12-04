using System;

namespace JJMasterData.Core.Events.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class CustomizedFieldsAttribute(params string[] fieldNames) : Attribute
{
    public string[] FieldNames { get; } = fieldNames;
}