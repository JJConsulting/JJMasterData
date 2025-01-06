using System;
using System.Collections.Generic;

namespace JJMasterData.Core.Events.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class CustomizedFieldsAttribute(params string[] fieldNames) : Attribute
{
    public IEnumerable<string> FieldNames { get; } = fieldNames;
}