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
    private const string IgnoreAttribute = "JJMasterData.Commons.Data.DataAccessIgnoreAttribute";
    private const string JsonNameAttribute = "System.Text.Json.Serialization.JsonPropertyNameAttribute";

    private static readonly DiagnosticDescriptor RecursiveModel = Rule("JJMDG006", "Recursive model mapping",
        "Property '{0}' creates a cycle between DataAccess models");

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

    private static readonly DiagnosticDescriptor PartialRequired = Rule("JJMDG008", "Partial declaration required",
        "Model '{0}' and its containing types must be partial");

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
        var diagnostics = analysis.SelectMany(static (result, _) => result.Diagnostics);
        context.RegisterSourceOutput(diagnostics, static (output, diagnostic) => output.ReportDiagnostic(diagnostic));

        var sources = models.Where(static model => model.IsValid)
            .Select(static (model, token) => (model.HintName, Source: Emit(model, token)))
            .WithTrackingName("Sources");
        context.RegisterSourceOutput(sources, static (output, source) =>
            output.AddSource(source.HintName, SourceText.From(source.Source, Encoding.UTF8)));
    }

    private static AnalysisResult AnalyzeModel(GeneratorAttributeSyntaxContext context,
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
                    reference.GetSyntax() is not TypeDeclarationSyntax declaration ||
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

    private static ModelData CreateModelData(INamedTypeSymbol symbol, ModelAnalysis model)
    {
        return new ModelData(
            symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            symbol.ContainingNamespace.IsGlobalNamespace ? "" : symbol.ContainingNamespace.ToDisplayString(),
            model.Name,
            symbol.DeclaredAccessibility == Accessibility.Public ? "public" : "internal",
            GetContainerStart(symbol),
            string.Concat(Enumerable.Repeat("}\n", GetContainerCount(symbol))),
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

        switch (type.ToDisplayString())
        {
            case "System.Guid": return "Guid";
            case "System.DateTimeOffset": return "DateTimeOffset";
            case "System.TimeSpan": return "TimeSpan";
            default: return null;
        }
    }

    private static string Literal(string text) => SymbolDisplay.FormatLiteral(text, true);

    private static string GetContainerStart(INamedTypeSymbol symbol)
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
            declarations.Push("partial " + kind + " @" + type.Name + "\n{\n");
        }

        return string.Concat(declarations);
    }

    private static int GetContainerCount(INamedTypeSymbol symbol)
    {
        var count = 0;
        for (var type = symbol.ContainingType; type != null; type = type.ContainingType)
            count++;
        return count;
    }

    private static string Emit(ModelData model, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var builder = new StringBuilder("// <auto-generated/>\n#nullable enable\n\n")
            .Append("using Data = global::JJMasterData.Commons.Data;\n")
            .Append("using Db = global::System.Data.Common;\n")
            .Append("using System = global::System;\n\n");
        if (model.Namespace.Length != 0)
            builder.Append("namespace ").Append(model.Namespace).Append(" {\n");
        builder.Append(model.ContainerStart);
        builder.Append(model.Accessibility).Append(model.IsRecord ? " partial record class @" : " partial class @")
            .Append(model.Name).Append(" : Data::IDataAccessModel<")
            .Append(model.TypeName).Append(">\n{\n");
        var hasIgnoredRequired = model.Properties.Any(p => p.ReaderMethod == "Ignore");
        if (hasIgnoredRequired)
        {
            // A constructor marked SetsRequiredMembers lets ignored required properties keep their initializers.
            // Declaring an overload removes the implicit constructor, so reproduce it when necessary.
            if (model.HasImplicitConstructor)
                builder.Append("    ").Append(model.Accessibility).Append(" @").Append(model.Name).Append("() { }\n");
            builder.Append("    private readonly struct __DataAccessConstruction { }\n")
                .Append("#pragma warning disable CS8618 // Ignored required members retain their constructor defaults.\n")
                .Append("    [global::System.Diagnostics.CodeAnalysis.SetsRequiredMembers]\n")
                .Append("    private @").Append(model.Name)
                .Append("(__DataAccessConstruction _) : this() { }\n")
                .Append("#pragma warning restore CS8618\n");
        }
        var contract = "Data::IDataAccessModel<" + model.TypeName + ">";
        builder.Append("    static int[] ").Append(contract)
            .Append(".ResolveOrdinals(Db::DbDataReader reader)\n    {\n")
            .Append("        var ordinals = new global::System.Collections.Generic.List<int>();\n");
        foreach (var property in model.Properties)
        {
            if (property.ReaderMethod == "Ignore")
                continue;
            if (property.ReaderMethod == "Model")
                builder.Append("        AppendModelOrdinals<").Append(property.TypeName)
                    .Append(">(reader, ordinals);\n");
            else
                builder.Append("        ordinals.Add(Data::DataAccessValueReader.FindOrdinal(reader, ")
                    .Append(Literal(property.Column)).Append("));\n");
        }
        builder.Append("        return ordinals.ToArray();\n    }\n\n");
        builder.Append("    static ").Append(model.TypeName).Append(' ').Append(contract)
            .Append(".Read(Db::DbDataReader reader, System::ReadOnlySpan<int> ordinals)\n    {\n")
            .Append("        var item = new ").Append(model.TypeName)
            .Append(hasIgnoredRequired ? "(default(__DataAccessConstruction));\n" : "();\n");
        if (model.Properties.Any(p => p.ReaderMethod != "Ignore"))
            builder.Append("        var index = 0;\n");
        foreach (var property in model.Properties)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (property.ReaderMethod == "Ignore")
                continue;
            builder.AppendLine();
            if (property.ReaderMethod == "Model")
            {
                builder.Append("        var length_").Append(property.Name).Append(" = ordinals[index++];\n")
                    .Append("        item.@").Append(property.Name).Append(" = ReadModel<")
                    .Append(property.TypeName).Append(">(reader, ordinals.Slice(index, length_")
                    .Append(property.Name).Append("));\n")
                    .Append("        index += length_").Append(property.Name).Append(";\n");
            }
            else
            {
                builder.Append("        {\n            var ordinal = ordinals[index++];\n");
                AppendPropertyMapping(builder, property);
                builder.Append("        }\n");
            }
        }

        builder.Append("\n        return item;\n    }\n");
        if (model.Properties.Any(p => p.ReaderMethod == "Model"))
            builder.Append("""
                    private static void AppendModelOrdinals<T>(Db::DbDataReader reader,
                        global::System.Collections.Generic.List<int> ordinals)
                        where T : class, Data::IDataAccessModel<T>
                    {
                        var nested = T.ResolveOrdinals(reader);
                        ordinals.Add(nested.Length);
                        ordinals.AddRange(nested);
                    }

                    private static T ReadModel<T>(Db::DbDataReader reader, System::ReadOnlySpan<int> ordinals)
                        where T : class, Data::IDataAccessModel<T>
                        => T.Read(reader, ordinals);
                """).Append('\n');
        builder.Append("}\n");
        builder.Append(model.ContainerEnd);
        if (model.Namespace.Length != 0)
            builder.Append("}\n");
        return CSharpSyntaxTree.ParseText(builder.ToString(), cancellationToken: cancellationToken)
            .GetRoot(cancellationToken).NormalizeWhitespace(eol: "\n").ToFullString() + "\n";
    }

    private static void AppendPropertyMapping(StringBuilder builder, PropertyData property)
    {
        var target = "item.@" + property.Name;
        var nullAssignment = property.Nullable
            ? target + " = null;"
            : "throw Data::DataAccessValueReader.NullValue(" +
              Literal(property.Column) + ", " + Literal(property.DisplayType) + ");";
        var enumCast = property.TypeName.Length == 0 ? "" : "(" + property.TypeName + ")";

        builder.Append($$"""
                                 if (ordinal >= 0)
                                 {
                                     if (reader.IsDBNull(ordinal))
                                         {{nullAssignment}}
                                     else
                                         {{target}} = {{enumCast}}Data::DataAccessValueReader.Read{{property.ReaderMethod}}(reader, ordinal);
                                 }
                         """).Append('\n');
    }

    private sealed class AnalysisResult(
        ModelData model,
        ImmutableArray<Diagnostic> diagnostics)
    {
        public ModelData Model { get; } = model;
        public ImmutableArray<Diagnostic> Diagnostics { get; } = diagnostics;
    }

    // Only value-equatable data crosses into the source pipeline. ImmutableArray itself uses reference equality,
    // so Equals compares the properties element by element.
    private sealed class ModelData(
        string typeName,
        string namespaceName,
        string name,
        string accessibility,
        string containerStart,
        string containerEnd,
        bool isRecord,
        bool hasImplicitConstructor,
        ImmutableArray<PropertyData> properties,
        bool isValid) : IEquatable<ModelData>
    {
        public string TypeName { get; } = typeName;
        public string Namespace { get; } = namespaceName;
        public string Name { get; } = name;
        public string Accessibility { get; } = accessibility;
        public string ContainerStart { get; } = containerStart;
        public string ContainerEnd { get; } = containerEnd;
        public bool IsRecord { get; } = isRecord;
        public bool HasImplicitConstructor { get; } = hasImplicitConstructor;
        public ImmutableArray<PropertyData> Properties { get; } = properties;
        public bool IsValid { get; } = isValid;

        public string HintName =>
            string.Concat(Encoding.UTF8.GetBytes(TypeName).Select(static value => value.ToString("x2"))) +
            ".g.cs";

        public bool Equals(ModelData? other) =>
            other != null &&
            TypeName == other.TypeName &&
            Namespace == other.Namespace &&
            Name == other.Name &&
            Accessibility == other.Accessibility &&
            ContainerStart == other.ContainerStart &&
            ContainerEnd == other.ContainerEnd &&
            IsRecord == other.IsRecord &&
            HasImplicitConstructor == other.HasImplicitConstructor &&
            IsValid == other.IsValid &&
            Properties.SequenceEqual(other.Properties);

        public override bool Equals(object? obj) => obj is ModelData other && Equals(other);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(TypeName);
    }

    private readonly struct PropertyData(
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
}