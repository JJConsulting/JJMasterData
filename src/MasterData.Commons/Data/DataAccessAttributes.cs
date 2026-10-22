#nullable enable
using System;

namespace JJMasterData.Commons.Data;


/// <summary>Generates typed database reading for a partial model class.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class DataAccessModelAttribute : Attribute;

/// <summary>Overrides the SQL column name matched to a model property.</summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class DataAccessColumnAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}

/// <summary>Excludes a property from database mapping and mapping validation.</summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class DataAccessIgnoreAttribute : Attribute;
