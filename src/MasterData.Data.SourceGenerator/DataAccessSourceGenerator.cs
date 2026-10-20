using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace JJMasterData.Data.SourceGenerators;

[Generator(LanguageNames.CSharp)]
public sealed class DataAccessSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var analysis = context.SyntaxProvider.ForAttributeWithMetadataName(
            "JJMasterData.Commons.Data.DataAccessModelAttribute",
            static (node, _) => node is TypeDeclarationSyntax,
            static (attributeContext, token) => DataAccessModelAnalyzer.AnalyzeModel(attributeContext, token));

        // Symbols are used only during semantic analysis. Source models have value equality,
        // and diagnostic locations are kept on a separate branch from source generation.
        var models = analysis.Select(static (result, _) => result.DataAccessModel)
            .WithTrackingName("Models");
        var diagnostics = analysis.SelectMany(static (result, _) => result.Diagnostics);
        context.RegisterSourceOutput(diagnostics, static (output, diagnostic) => output.ReportDiagnostic(diagnostic));

        var sources = models.Where(static model => model.IsValid)
            .Select(static (model, token) => (model.HintName, Source: DataAccessSourceEmitter.Emit(model, token)))
            .WithTrackingName("Sources");
        context.RegisterSourceOutput(sources, static (output, source) =>
            output.AddSource(source.HintName, SourceText.From(source.Source, Encoding.UTF8)));
    }
}
