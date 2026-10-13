#nullable enable
using System;

namespace JJMasterData.Commons.Data;


/// <summary>Generates DataAccess query extensions for a concrete, non-generic model with public writable properties.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class DataAccessModelAttribute : Attribute
{
    /// <summary>Optional method-name suffix, for example SalesCustomer generates GetSalesCustomerAsync.</summary>
    public string? ExtensionName { get; set; }
}

/// <summary>Overrides the SQL column name matched to a model property.</summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class DataAccessColumnAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
