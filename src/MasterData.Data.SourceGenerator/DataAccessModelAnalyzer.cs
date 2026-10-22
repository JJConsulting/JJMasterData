using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static JJMasterData.Data.SourceGenerators.DataAccessDiagnostics;

namespace JJMasterData.Data.SourceGenerators;

internal static class DataAccessModelAnalyzer
{
    private const string ModelAttribute = "JJMasterData.Commons.Data.DataAccessModelAttribute";
    private const string ColumnAttribute = "JJMasterData.Commons.Data.DataAccessColumnAttribute";
    private const string IgnoreAttribute = "JJMasterData.Commons.Data.DataAccessIgnoreAttribute";
    private const string JsonNameAttribute = "System.Text.Json.Serialization.JsonPropertyNameAttribute";

    internal static AnalysisResult AnalyzeModel(GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var symbol = (INamedTypeSymbol)context.TargetSymbol;
        var model = new ModelAnalysis { CancellationToken = cancellationToken };
        var compilation = context.SemanticModel.Compilation;
        AnalyzeModelConfiguration(symbol, model);
        AnalyzeProperties(symbol, compilation, model, cancellationToken);

        return new AnalysisResult(CreateModelData(symbol, model), model.Diagnostics.ToImmutableArray());
    }

    private static void AnalyzeModelConfiguration(INamedTypeSymbol symbol, ModelAnalysis model)
    {
        model.Name = symbol.Name;
        for (var type = symbol; type != null; type = type.ContainingType)
        {
            if (type.DeclaringSyntaxReferences.Any(reference =>
                    reference.GetSyntax(model.CancellationToken) is not TypeDeclarationSyntax declaration ||
                    !declaration.Modifiers.Any(SyntaxKind.PartialKeyword)))
                model.Report(PartialRequired, type, symbol.Name);
        }

        if (symbol.TypeKind != TypeKind.Class || symbol.IsAbstract || symbol.IsStatic || !IsAccessible(symbol) ||
            HasInvalidContainer(symbol))
            model.Report(InvalidModel, symbol, symbol.Name);
        if (!symbol.InstanceConstructors.Any(c => c.Parameters.Length == 0 && IsAccessible(c)))
            model.Report(InvalidConstructor, symbol, symbol.Name);
    }

    private static void AnalyzeProperties(
        INamedTypeSymbol symbol,
        Compilation compilation,
        ModelAnalysis model,
        CancellationToken cancellationToken)
    {
        var propertyNames = new Dictionary<string, IPropertySymbol>(StringComparer.Ordinal);
        var columnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var current = symbol;
             current != null && current.SpecialType != SpecialType.System_Object;
             current = current.BaseType)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var member in current.GetMembers().OrderBy(m => m.Name, StringComparer.Ordinal))
            {
                cancellationToken.ThrowIfCancellationRequested();
                // Required fields cannot be satisfied by the generated constructor either.
                if (member is IFieldSymbol field && IsRequired(field))
                    model.Report(InvalidProperty, field, field.Name);
                if (member is not IPropertySymbol property)
                    continue;
                var columnAttribute = GetPropertyAttribute(property, ColumnAttribute) ??
                                      GetPropertyAttribute(property, JsonNameAttribute);
                if (property.DeclaredAccessibility != Accessibility.Public && columnAttribute == null &&
                    !IsRequired(property))
                    continue;
                if (propertyNames.TryGetValue(property.Name, out var derived))
                {
                    // An override is mapped through the most-derived declaration; hidden properties are ambiguous.
                    if (!Overrides(derived, property))
                        model.Report(InvalidProperty, property, property.Name);
                    continue;
                }

                propertyNames.Add(property.Name, property);
                if (GetPropertyAttribute(property, IgnoreAttribute) != null)
                {
                    if (IsRequired(property))
                        model.Properties.Add(new PropertyAnalysis(property, "", "Ignore", property.Type, false));
                    continue;
                }

                AnalyzeProperty(property, columnAttribute, symbol, compilation, columnNames, model);
            }
        }
    }

    private static void AnalyzeProperty(
        IPropertySymbol property,
        AttributeData? columnAttribute,
        INamedTypeSymbol symbol,
        Compilation compilation,
        HashSet<string> columnNames,
        ModelAnalysis model)
    {
        if (property.IsStatic || property.IsIndexer || property.SetMethod == null ||
            !compilation.IsSymbolAccessibleWithin(property.SetMethod,
                compilation.Assembly) ||
            property.SetMethod.IsInitOnly || IsRequired(property) ||
            property.DeclaredAccessibility != Accessibility.Public)
        {
            model.Report(InvalidProperty, property, property.Name);
            return;
        }

        if (property.Type is INamedTypeSymbol nestedType && IsDataAccessModel(nestedType))
        {
            if (HasModelCycle(nestedType, new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default) { symbol },
                    model.CancellationToken))
                model.Report(RecursiveModel, property, property.Name);
            model.Properties.Add(new PropertyAnalysis(property, "", "Model", nestedType, false));
            return;
        }

        var column = columnAttribute == null
            ? property.Name
            : columnAttribute.ConstructorArguments.FirstOrDefault().Value as string;
        if (string.IsNullOrWhiteSpace(column))
        {
            model.Report(InvalidConfiguration, property, columnAttribute!.AttributeClass!.Name, property.Name,
                "column name cannot be empty");
            return;
        }

        if (!columnNames.Add(column!))
            model.Report(DuplicateColumn, property, symbol.Name, column!);
        var type = property.Type;
        var nullable = type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T };
        if (nullable)
            type = ((INamedTypeSymbol)type).TypeArguments[0];
        var enumType = type as INamedTypeSymbol;
        var conversionType = type.TypeKind == TypeKind.Enum ? enumType!.EnumUnderlyingType! : type;
        var readerMethod = GetReaderMethod(conversionType);
        if (readerMethod == null)
        {
            model.Report(UnsupportedType, property, property.Name, property.Type.ToDisplayString());
            return;
        }

        model.Properties.Add(new PropertyAnalysis(property, column!, readerMethod, type,
            nullable || (type.IsReferenceType && property.NullableAnnotation == NullableAnnotation.Annotated)));
    }

    private static DataAccessModelInfo CreateModelData(INamedTypeSymbol symbol, ModelAnalysis model)
    {
        return new DataAccessModelInfo(
            symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            symbol.ContainingNamespace.IsGlobalNamespace ? "" : symbol.ContainingNamespace.ToDisplayString(),
            model.Name,
            symbol.DeclaredAccessibility == Accessibility.Public ? "public" : "internal",
            GetContainers(symbol),
            symbol.IsRecord,
            symbol.InstanceConstructors.Any(c => c.IsImplicitlyDeclared && c.Parameters.Length == 0),
            model.Properties.Select(static p => new PropertyData(p.Symbol.Name, p.Column, p.ReaderMethod,
                p.ReaderMethod == "Model" || p.Type.TypeKind == TypeKind.Enum
                    ? p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    : "",
                p.Symbol.Type.ToDisplayString(), p.Nullable)).ToImmutableArray(),
            model.Diagnostics.Count == 0);
    }

    private static AttributeData? GetPropertyAttribute(IPropertySymbol property, string attributeName)
    {
        for (var current = property; current != null; current = current.OverriddenProperty)
        {
            var attribute = current.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == attributeName);
            if (attribute != null)
                return attribute;
        }

        return null;
    }

    private static bool IsDataAccessModel(INamedTypeSymbol type) =>
        type.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == ModelAttribute);

    private static bool HasModelCycle(INamedTypeSymbol type, HashSet<INamedTypeSymbol> path,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!path.Add(type))
            return true;
        var names = new HashSet<string>(StringComparer.Ordinal);
        for (var current = type; current != null; current = current.BaseType)
        {
            foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!names.Add(property.Name) || property.IsStatic || property.IsIndexer ||
                    property.DeclaredAccessibility != Accessibility.Public ||
                    GetPropertyAttribute(property, IgnoreAttribute) != null)
                    continue;
                if (property.Type is INamedTypeSymbol nested && IsDataAccessModel(nested) &&
                    HasModelCycle(nested, path, cancellationToken))
                    return true;
            }
        }
        path.Remove(type);
        return false;
    }

    private static bool Overrides(IPropertySymbol derived, IPropertySymbol ancestor)
    {
        for (var current = derived.OverriddenProperty; current != null; current = current.OverriddenProperty)
            if (SymbolEqualityComparer.Default.Equals(current, ancestor))
                return true;
        return false;
    }

    private static bool IsRequired(ISymbol symbol) =>
        symbol is IPropertySymbol { IsRequired: true } or IFieldSymbol { IsRequired: true };

    private static bool IsAccessible(ISymbol symbol) =>
        symbol.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal
            or Accessibility.ProtectedOrInternal;

    private static bool HasInvalidContainer(INamedTypeSymbol symbol)
    {
        for (var type = symbol; type != null; type = type.ContainingType)
            if (type.Arity != 0 || type.IsFileLocal || !IsAccessible(type))
                return true;
        return false;
    }

    private static string? GetReaderMethod(ITypeSymbol type)
    {
        switch (type.SpecialType)
        {
            case SpecialType.System_String:
                return "String";
            case SpecialType.System_Boolean:
                return "Boolean";
            case SpecialType.System_Char:
                return "Char";
            case SpecialType.System_Byte:
                return "Byte";
            case SpecialType.System_SByte:
                return "SByte";
            case SpecialType.System_Int16:
                return "Int16";
            case SpecialType.System_UInt16:
                return "UInt16";
            case SpecialType.System_Int32:
                return "Int32";
            case SpecialType.System_UInt32:
                return "UInt32";
            case SpecialType.System_Int64:
                return "Int64";
            case SpecialType.System_UInt64:
                return "UInt64";
            case SpecialType.System_Single:
                return "Single";
            case SpecialType.System_Double:
                return "Double";
            case SpecialType.System_Decimal:
                return "Decimal";
            case SpecialType.System_DateTime:
                return "DateTime";
        }

        return type.ToDisplayString() switch
        {
            "System.Guid" => "Guid",
            "System.DateTimeOffset" => "DateTimeOffset",
            "System.TimeSpan" => "TimeSpan",
            _ => null
        };
    }

    private static ImmutableArray<string> GetContainers(INamedTypeSymbol symbol)
    {
        var declarations = new Stack<string>();
        for (var type = symbol.ContainingType; type != null; type = type.ContainingType)
        {
            string kind;
            if (type.IsRecord)
                kind = type.TypeKind switch
                {
                    TypeKind.Struct => "record struct",
                    _ => "record class"
                };
            else
                kind = type.TypeKind switch
                {
                    TypeKind.Struct => "struct",
                    TypeKind.Interface => "interface",
                    _ => "class"
                };
            declarations.Push("partial " + kind + " @" + type.Name);
        }

        return [.. declarations];
    }

    // Symbols are retained only while analyzing one model, never in the cached source model.
    private sealed class ModelAnalysis
    {
        public CancellationToken CancellationToken { get; set; }
        public string Name { get; set; } = "";
        public List<PropertyAnalysis> Properties { get; } = new();
        public List<Diagnostic> Diagnostics { get; } = new();

        public void Report(DiagnosticDescriptor rule, ISymbol target, params object[] arguments) =>
            Diagnostics.Add(Diagnostic.Create(rule, target.Locations.FirstOrDefault(), arguments));
    }

    private sealed class PropertyAnalysis(
        IPropertySymbol symbol,
        string column,
        string readerMethod,
        ITypeSymbol type,
        bool nullable)
    {
        public IPropertySymbol Symbol { get; } = symbol;
        public string Column { get; } = column;
        public string ReaderMethod { get; } = readerMethod;
        public ITypeSymbol Type { get; } = type;
        public bool Nullable { get; } = nullable;
    }

    internal sealed class AnalysisResult(
        DataAccessModelInfo dataAccessModel,
        ImmutableArray<Diagnostic> diagnostics)
    {
        public DataAccessModelInfo DataAccessModel { get; } = dataAccessModel;
        public ImmutableArray<Diagnostic> Diagnostics { get; } = diagnostics;
    }
}
