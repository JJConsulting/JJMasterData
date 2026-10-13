using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace JJConsulting.Html.SourceGenerators;

[Generator]
public sealed class HtmlBuilderExtensionsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var tagNamesProvider = context.CompilationProvider
            .Select((compilation, _) =>
            {
                var htmlTag = compilation.GetTypeByMetadataName("JJConsulting.Html.HtmlTag");
                if (htmlTag is null)
                {
                    return string.Empty;
                }

                return string.Join("\n", htmlTag.GetMembers()
                    .OfType<IFieldSymbol>()
                    .Where(field => field.IsStatic && SymbolEqualityComparer.Default.Equals(field.Type, htmlTag))
                    .Select(field => field.Name)
                    .OrderBy(name => name, System.StringComparer.Ordinal));
            });

        context.RegisterSourceOutput(tagNamesProvider, (productionContext, tagNames) =>
        {
            if (tagNames.Length == 0)
            {
                return;
            }

            productionContext.AddSource(
                "Extensions/HtmlBuilderAppendExtensions.g.cs",
                SourceText.From(GenerateAppendExtensions(tagNames), Encoding.UTF8));
            productionContext.AddSource(
                "Extensions/HtmlBuilderTagExtensions.g.cs",
                SourceText.From(GenerateTagExtensions(tagNames), Encoding.UTF8));
        });
    }

    private static string GenerateAppendExtensions(string tagNames)
    {
        var source = new StringBuilder();
        source.AppendLine("#nullable enable");
        source.AppendLine("using System;");
        source.AppendLine();
        source.AppendLine("namespace JJConsulting.Html.Extensions;");
        source.AppendLine();
        source.AppendLine("public static partial class HtmlBuilderAppendExtensions");
        source.AppendLine("{");
        source.AppendLine("    extension(HtmlBuilder htmlBuilder)");
        source.AppendLine("    {");

        foreach (var name in tagNames.Split('\n'))
        {
            source.AppendLine(
                $"        public HtmlBuilder Append{name}(Action<HtmlBuilder>? builderAction = null) => htmlBuilder.Append(HtmlTag.{name}, builderAction);");
        }

        source.AppendLine("    }");
        source.AppendLine("}");
        return source.ToString();
    }

    private static string GenerateTagExtensions(string tagNames)
    {
        var source = new StringBuilder();
        source.AppendLine("#nullable enable");
        source.AppendLine("using System.Collections.Generic;");
        source.AppendLine("using Microsoft.AspNetCore.Html;");
        source.AppendLine();
        source.AppendLine("namespace JJConsulting.Html.Extensions;");
        source.AppendLine();
        source.AppendLine("public static class HtmlBuilderTagExtensions");
        source.AppendLine("{");
        source.AppendLine("    extension(HtmlBuilder)");
        source.AppendLine("    {");

        foreach (var name in tagNames.Split('\n'))
        {
            source.AppendLine($"        public static HtmlBuilder {name}() => new(HtmlTag.{name});");
            source.AppendLine($"        public static HtmlBuilder {name}(params List<IHtmlContent?> children) => new(HtmlTag.{name}, children);");
            source.AppendLine($"        public static HtmlBuilder {name}(string rawText) => new(HtmlTag.{name}, rawText);");
        }

        source.AppendLine("    }");
        source.AppendLine("}");
        return source.ToString();
    }
}
