using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace JJMasterData.Data.SourceGenerators;

[Generator(LanguageNames.CSharp)]
public sealed class DataAccessExtensionsGenerator : IIncrementalGenerator
{
    private const string ModelAttribute = "JJMasterData.Commons.Data.DataAccessModelAttribute";
    private const string ColumnAttribute = "JJMasterData.Commons.Data.DataAccessColumnAttribute";

    private static readonly DiagnosticDescriptor InvalidModel = Rule("JJMDG001", "Invalid model",
        "Model '{0}' must be a concrete, accessible, non-generic class");

    private static readonly DiagnosticDescriptor InvalidConstructor = Rule("JJMDG002", "Invalid constructor",
        "Model '{0}' needs a public or internal parameterless constructor");

    private static readonly DiagnosticDescriptor UnsupportedType = Rule("JJMDG003", "Unsupported property type",
        "Property '{0}' has unsupported type '{1}'");

    private static readonly DiagnosticDescriptor InvalidProperty = Rule("JJMDG004", "Property cannot be mapped",
        "Property '{0}' must have an accessible setter and cannot be static, an indexer, init-only or required");

    private static readonly DiagnosticDescriptor DuplicateColumn = Rule("JJMDG005", "Duplicate column",
        "Model '{0}' maps column '{1}' more than once (case-insensitive)");

    private static readonly DiagnosticDescriptor NameConflict = Rule("JJMDG006", "Extension name conflict",
        "Model '{0}' has conflicting extension names; set a unique DataAccessModel.ExtensionName");

    private static readonly DiagnosticDescriptor InvalidConfiguration =
        Rule("JJMDG007", "Invalid attribute configuration", "Invalid {0} on '{1}': {2}");

    private static DiagnosticDescriptor Rule(string id, string title, string message) =>
        new(id, title, message, "JJMasterData.DataAccess", DiagnosticSeverity.Error, true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var analysis = context.SyntaxProvider.ForAttributeWithMetadataName(ModelAttribute,
            static (node, _) => node is TypeDeclarationSyntax,
            static (attributeContext, token) => AnalyzeModel(attributeContext, token));

        // Symbols are used only during semantic analysis. Source models have value equality,
        // and diagnostic locations are kept on a separate branch from source generation.
        var models = analysis.Select(static (result, _) => result.Model)
            .WithTrackingName("Models");
        var conflicts = models.Select(static (model, _) => model.Name).Collect()
            .Select(static (names, token) => FindConflicts(names, token));
        var diagnostics = analysis.Combine(conflicts)
            .SelectMany(static (pair, _) => GetDiagnostics(pair.Left, pair.Right));
        context.RegisterSourceOutput(diagnostics, static (output, diagnostic) => output.ReportDiagnostic(diagnostic));

        var sources = models.Combine(conflicts)
            .Select(static (pair, _) => (Model: pair.Left, Conflict: HasConflict(pair.Right, pair.Left.Name)))
            .Where(static pair => pair.Model.IsValid && !pair.Conflict)
            .Select(static (pair, token) => (pair.Model.HintName, Source: Emit(pair.Model, token)))
            .WithTrackingName("Sources");
        context.RegisterSourceOutput(sources, static (output, source) =>
            output.AddSource(source.HintName, SourceText.From(source.Source, Encoding.UTF8)));
    }

    private static AnalysisResult AnalyzeModel(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var symbol = (INamedTypeSymbol)context.TargetSymbol;
        var model = new ModelAnalysis();
        var compilation = context.SemanticModel.Compilation;
        AnalyzeModelConfiguration(symbol, context.Attributes[0], model);
        AnalyzeProperties(symbol, compilation, model, cancellationToken);

        var extensionMetadataName =
            (symbol.ContainingNamespace.IsGlobalNamespace ? "" : symbol.ContainingNamespace.ToDisplayString() + ".") +
            model.Name + "DataAccessExtensions";
        if (compilation.GetTypeByMetadataName(extensionMetadataName) != null)
            model.Report(NameConflict, symbol, symbol.Name);
        return new AnalysisResult(CreateModelData(symbol, model), model.Diagnostics.ToImmutableArray(),
            Diagnostic.Create(NameConflict, symbol.Locations.FirstOrDefault(), symbol.Name));
    }

    private static void AnalyzeModelConfiguration(INamedTypeSymbol symbol, AttributeData attribute, ModelAnalysis model)
    {
        model.Name = symbol.Name;
        foreach (var argument in attribute.NamedArguments)
            if (argument.Key == "ExtensionName")
                model.Name = argument.Value.Value as string ?? "";
        if (!SyntaxFacts.IsValidIdentifier(model.Name) || SyntaxFacts.GetKeywordKind(model.Name) != SyntaxKind.None)
            model.Report(InvalidConfiguration, symbol, "ExtensionName", symbol.Name,
                "expected a non-keyword C# identifier");

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
        var propertyNames = new HashSet<string>(StringComparer.Ordinal);
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
                var columnAttribute = GetColumnAttribute(property);
                if (property.DeclaredAccessibility != Accessibility.Public && columnAttribute == null &&
                    !IsRequired(property))
                    continue;
                if (!propertyNames.Add(property.Name))
                {
                    // An override is mapped through the most-derived declaration; hidden properties are ambiguous.
                    if (!model.Properties.Any(p => Overrides(p.Symbol, property)))
                        model.Report(InvalidProperty, property, property.Name);
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

        var column = columnAttribute == null
            ? property.Name
            : columnAttribute.ConstructorArguments.FirstOrDefault().Value as string;
        if (string.IsNullOrWhiteSpace(column))
        {
            model.Report(InvalidConfiguration, property, "DataAccessColumn", property.Name,
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

    private static ModelData CreateModelData(INamedTypeSymbol symbol, ModelAnalysis model)
    {
        return new ModelData(
            symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            symbol.ContainingNamespace.IsGlobalNamespace ? "" : symbol.ContainingNamespace.ToDisplayString(),
            model.Name,
            symbol.DeclaredAccessibility == Accessibility.Public && HasPublicContainingTypes(symbol) ? "public" : "internal",
            model.Properties.Select(static p => new PropertyData(p.Symbol.Name, p.Column, p.ReaderMethod,
                p.Type.TypeKind == TypeKind.Enum
                    ? p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    : "",
                p.Symbol.Type.ToDisplayString(), p.Nullable)).ToImmutableArray(),
            model.Diagnostics.Count == 0);
    }

    private static AttributeData? GetColumnAttribute(IPropertySymbol property)
    {
        for (var current = property; current != null; current = current.OverriddenProperty)
        {
            var attribute = current.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == ColumnAttribute);
            if (attribute != null)
                return attribute;
        }

        return null;
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
        symbol.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal;

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

        switch (type.ToDisplayString())
        {
            case "System.Guid": return "Guid";
            case "System.DateTimeOffset": return "DateTimeOffset";
            case "System.TimeSpan": return "TimeSpan";
            default: return null;
        }
    }

    private static string FindConflicts(ImmutableArray<string> names, CancellationToken cancellationToken)
    {
        var methodNames = new Dictionary<string, string>(StringComparer.Ordinal);
        var conflicting = new HashSet<string>(StringComparer.Ordinal);
        foreach (var name in names)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // FooList's single-result method conflicts with Foo's list method.
            foreach (var method in new[] { "Get" + name + "Async", "Get" + name + "ListAsync" })
            {
                if (methodNames.TryGetValue(method, out var previous))
                {
                    conflicting.Add(previous);
                    conflicting.Add(name);
                }
                else
                {
                    methodNames.Add(method, name);
                }
            }
        }

        // A canonical string provides value equality for this global dependency.
        return conflicting.Count == 0
            ? ""
            : "\n" + string.Join("\n", conflicting.OrderBy(static n => n, StringComparer.Ordinal)) + "\n";
    }

    private static bool HasConflict(string conflicts, string name) =>
        conflicts.IndexOf("\n" + name + "\n", StringComparison.Ordinal) >= 0;

    private static ImmutableArray<Diagnostic> GetDiagnostics(AnalysisResult result, string conflicts) =>
        HasConflict(conflicts, result.Model.Name)
            ? result.Diagnostics.Add(result.ConflictDiagnostic)
            : result.Diagnostics;

    private static string Literal(string text) => SymbolDisplay.FormatLiteral(text, true);

    private static string Emit(ModelData model, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var builder = new StringBuilder("// <auto-generated/>\n#nullable enable\n");
        if (model.Namespace.Length != 0)
            builder.Append("namespace ").Append(model.Namespace).Append(" {\n");
        builder.Append(model.Accessibility).Append(" static class ").Append(model.Name).Append("DataAccessExtensions\n{\n");
        AppendQueryMethods(builder, model);
        AppendMapper(builder, model, cancellationToken);
        builder.Append("}\n");
        if (model.Namespace.Length != 0)
            builder.Append("}\n");
        return builder.ToString();
    }

    private static void AppendQueryMethods(StringBuilder builder, ModelData model)
    {
        // Raw strings mirror the generated code; fully qualified types avoid consumer using directives.
        builder.Append($"""
                public static global::System.Threading.Tasks.Task<{model.TypeName}?> Get{model.Name}Async(this global::JJMasterData.Commons.Data.DataAccess dataAccess, global::JJMasterData.Commons.Data.DataAccessCommand command, global::System.Threading.CancellationToken cancellationToken = default)
                    => dataAccess.QueryFirstOrDefaultAsync<{model.TypeName}>(command, static reader => new Mapper(reader), cancellationToken);
                public static global::System.Threading.Tasks.Task<global::System.Collections.Generic.List<{model.TypeName}>> Get{model.Name}ListAsync(this global::JJMasterData.Commons.Data.DataAccess dataAccess, global::JJMasterData.Commons.Data.DataAccessCommand command, global::System.Threading.CancellationToken cancellationToken = default)
                    => dataAccess.QueryListAsync<{model.TypeName}>(command, static reader => new Mapper(reader), cancellationToken);
            """).Append('\n');
    }

    private static void AppendMapper(StringBuilder builder, ModelData model, CancellationToken cancellationToken)
    {
        var type = model.TypeName;
        builder.Append("    private sealed class Mapper : global::JJMasterData.Commons.Data.IDataAccessMapper<").Append(type)
            .Append(">\n    {\n");
        for (var i = 0; i < model.Properties.Length; i++)
            builder.Append("        private readonly int ordinal").Append(i).Append(";\n");
        AppendMapperConstructor(builder, model);
        builder.Append("        public ").Append(type)
            .Append(" Map(global::System.Data.Common.DbDataReader reader)\n        {\n            var item = new ")
            .Append(type).Append("();\n");
        for (var i = 0; i < model.Properties.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            AppendPropertyMapping(builder, model.Properties[i], i);
        }

        builder.Append("            return item;\n        }\n    }\n");
    }

    private static void AppendMapperConstructor(StringBuilder builder, ModelData model)
    {
        builder.Append("        public Mapper(global::System.Data.Common.DbDataReader reader)\n        {\n");
        for (var i = 0; i < model.Properties.Length; i++)
            builder.Append("            ordinal").Append(i)
                .Append(" = global::JJMasterData.Commons.Data.DataAccessValueReader.FindOrdinal(reader, ")
                .Append(Literal(model.Properties[i].Column)).Append(");\n");
        builder.Append("        }\n");
    }

    private static void AppendPropertyMapping(StringBuilder builder, PropertyData property, int propertyIndex)
    {
        var target = "item.@" + property.Name;
        var nullAssignment = property.Nullable
            ? target + " = null;"
            : "throw global::JJMasterData.Commons.Data.DataAccessValueReader.NullValue(" +
              Literal(property.Column) + ", " + Literal(property.DisplayType) + ");";
        var enumCast = property.EnumType.Length == 0 ? "" : "(" + property.EnumType + ")";

        builder.Append($$"""
                        if (ordinal{{propertyIndex}} >= 0)
                        {
                            if (reader.IsDBNull(ordinal{{propertyIndex}}))
                                {{nullAssignment}}
                            else
                                {{target}} = {{enumCast}}global::JJMasterData.Commons.Data.DataAccessValueReader.Read{{property.ReaderMethod}}(reader, ordinal{{propertyIndex}});
                        }
            """).Append('\n');
    }

    private static bool HasPublicContainingTypes(INamedTypeSymbol symbol)
    {
        for (var current = symbol.ContainingType; current != null; current = current.ContainingType)
            if (current.DeclaredAccessibility != Accessibility.Public)
                return false;
        return true;
    }

    private sealed class AnalysisResult(
        ModelData model,
        ImmutableArray<Diagnostic> diagnostics,
        Diagnostic conflictDiagnostic)
    {
        public ModelData Model { get; } = model;
        public ImmutableArray<Diagnostic> Diagnostics { get; } = diagnostics;
        public Diagnostic ConflictDiagnostic { get; } = conflictDiagnostic;
    }

    // Only value-equatable data crosses into the source pipeline. ImmutableArray itself uses reference equality,
    // so Equals compares the properties element by element.
    private sealed class ModelData(
        string typeName,
        string namespaceName,
        string name,
        string accessibility,
        ImmutableArray<PropertyData> properties,
        bool isValid) : IEquatable<ModelData>
    {
        public string TypeName { get; } = typeName;
        public string Namespace { get; } = namespaceName;
        public string Name { get; } = name;
        public string Accessibility { get; } = accessibility;
        public ImmutableArray<PropertyData> Properties { get; } = properties;
        public bool IsValid { get; } = isValid;

        public string HintName => string.Concat(Encoding.UTF8.GetBytes(TypeName).Select(static value => value.ToString("x2"))) +
                                  ".g.cs";

        public bool Equals(ModelData? other) =>
            other != null &&
            TypeName == other.TypeName &&
            Namespace == other.Namespace &&
            Name == other.Name &&
            Accessibility == other.Accessibility &&
            IsValid == other.IsValid &&
            Properties.SequenceEqual(other.Properties);

        public override bool Equals(object? obj) => obj is ModelData other && Equals(other);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(TypeName);
    }

    private readonly struct PropertyData(
        string name,
        string column,
        string readerMethod,
        string enumType,
        string displayType,
        bool nullable) : IEquatable<PropertyData>
    {
        public string Name { get; } = name;
        public string Column { get; } = column;
        public string ReaderMethod { get; } = readerMethod;
        public string EnumType { get; } = enumType;
        public string DisplayType { get; } = displayType;
        public bool Nullable { get; } = nullable;

        public bool Equals(PropertyData other) =>
            Name == other.Name &&
            Column == other.Column &&
            ReaderMethod == other.ReaderMethod &&
            EnumType == other.EnumType &&
            DisplayType == other.DisplayType &&
            Nullable == other.Nullable;

        public override bool Equals(object? obj) => obj is PropertyData other && Equals(other);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Name);
    }

    // Symbols are retained only while analyzing one model, never in the cached source model.
    private sealed class ModelAnalysis
    {
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
}