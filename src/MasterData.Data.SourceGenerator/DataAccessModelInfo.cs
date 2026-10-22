using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace JJMasterData.Data.SourceGenerators;

// Only value-equatable data crosses into the source pipeline. ImmutableArray itself uses reference equality,
// so Equals compares the properties element by element.
internal sealed class DataAccessModelInfo(
    string typeName,
    string namespaceName,
    string name,
    string accessibility,
    ImmutableArray<string> containers,
    bool isRecord,
    bool hasImplicitConstructor,
    ImmutableArray<PropertyData> properties,
    bool isValid) : IEquatable<DataAccessModelInfo>
{
    public string TypeName { get; } = typeName;
    public string Namespace { get; } = namespaceName;
    public string Name { get; } = name;
    public string Accessibility { get; } = accessibility;
    public ImmutableArray<string> Containers { get; } = containers;
    public bool IsRecord { get; } = isRecord;
    public bool HasImplicitConstructor { get; } = hasImplicitConstructor;
    public ImmutableArray<PropertyData> Properties { get; } = properties;
    public bool IsValid { get; } = isValid;

    public string HintName =>
        string.Concat(Encoding.UTF8.GetBytes(TypeName).Select(static value => value.ToString("x2"))) +
        ".g.cs";

    public bool Equals(DataAccessModelInfo? other) =>
        other != null &&
        TypeName == other.TypeName &&
        Namespace == other.Namespace &&
        Name == other.Name &&
        Accessibility == other.Accessibility &&
        Containers.SequenceEqual(other.Containers) &&
        IsRecord == other.IsRecord &&
        HasImplicitConstructor == other.HasImplicitConstructor &&
        IsValid == other.IsValid &&
        Properties.SequenceEqual(other.Properties);

    public override bool Equals(object? obj) => obj is DataAccessModelInfo other && Equals(other);
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(TypeName);
}

internal readonly struct PropertyData(
    string name,
    string column,
    string readerMethod,
    string typeName,
    string displayType,
    bool nullable) : IEquatable<PropertyData>
{
    public string Name { get; } = name;
    public string Column { get; } = column;
    public string ReaderMethod { get; } = readerMethod;
    public string TypeName { get; } = typeName;
    public string DisplayType { get; } = displayType;
    public bool Nullable { get; } = nullable;

    public bool Equals(PropertyData other) =>
        Name == other.Name &&
        Column == other.Column &&
        ReaderMethod == other.ReaderMethod &&
        TypeName == other.TypeName &&
        DisplayType == other.DisplayType &&
        Nullable == other.Nullable;

    public override bool Equals(object? obj) => obj is PropertyData other && Equals(other);
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Name);
}

