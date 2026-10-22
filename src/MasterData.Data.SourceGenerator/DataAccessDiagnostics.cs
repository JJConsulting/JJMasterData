using Microsoft.CodeAnalysis;

namespace JJMasterData.Data.SourceGenerators;

internal static class DataAccessDiagnostics
{
    internal static readonly DiagnosticDescriptor RecursiveModel = Rule("JJMDG006", "Recursive model mapping",
        "Property '{0}' creates a cycle between DataAccess models");

    internal static readonly DiagnosticDescriptor InvalidModel = Rule("JJMDG001", "Invalid model",
        "Model '{0}' must be a concrete, accessible, non-generic class");

    internal static readonly DiagnosticDescriptor InvalidConstructor = Rule("JJMDG002", "Invalid constructor",
        "Model '{0}' needs a public or internal parameterless constructor");

    internal static readonly DiagnosticDescriptor UnsupportedType = Rule("JJMDG003", "Unsupported property type",
        "Property '{0}' has unsupported type '{1}'");

    internal static readonly DiagnosticDescriptor InvalidProperty = Rule("JJMDG004", "Property cannot be mapped",
        "Property '{0}' must have an accessible setter and cannot be static, an indexer, init-only or required");

    internal static readonly DiagnosticDescriptor DuplicateColumn = Rule("JJMDG005", "Duplicate column",
        "Model '{0}' maps column '{1}' more than once (case-insensitive)");

    internal static readonly DiagnosticDescriptor PartialRequired = Rule("JJMDG008", "Partial declaration required",
        "Model '{0}' and its containing types must be partial");

    internal static readonly DiagnosticDescriptor InvalidConfiguration =
        Rule("JJMDG007", "Invalid attribute configuration", "Invalid {0} on '{1}': {2}");

    private static DiagnosticDescriptor Rule(string id, string title, string message) =>
        new(id, title, message, "JJMasterData.DataAccess", DiagnosticSeverity.Error, true);

}
