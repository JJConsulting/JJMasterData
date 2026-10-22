using JJMasterData.Commons.Data;
using JJMasterData.Data.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace JJMasterData.Commons.Test.Data;

public class DataAccessSourceGeneratorTests
{
    private static (Compilation Compilation, GeneratorDriverRunResult Result) Generate(string source)
    {
        var compilation = CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new DataAccessSourceGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out _, TestContext.Current.CancellationToken);
        return (output, driver.GetRunResult());
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(DataAccess).Assembly.Location));
        return CSharpCompilation.Create("GeneratorConsumer",
            [CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken)], references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static CSharpGeneratorDriver CreateTrackedDriver() =>
        CSharpGeneratorDriver.Create(
            [new DataAccessSourceGenerator().AsSourceGenerator()],
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None,
                trackIncrementalGeneratorSteps: true));

    [Fact]
    public void ReusesSourcesWhenUnrelatedCodeChanges()
    {
        var compilation = CreateCompilation("""
            using JJMasterData.Commons.Data;
            [DataAccessModel] public partial class Product { public int Id { get; set; } }
            """);
        var driver = CreateTrackedDriver().RunGenerators(compilation, TestContext.Current.CancellationToken);
        var edited = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("public class Unrelated { }", cancellationToken: TestContext.Current.CancellationToken));
        driver = driver.RunGenerators(edited, TestContext.Current.CancellationToken);

        var result = Assert.Single(driver.GetRunResult().Results);
        Assert.Empty(result.Diagnostics);
        var step = Assert.Single(result.TrackedSteps["Sources"]);
        Assert.Equal(IncrementalStepRunReason.Cached, Assert.Single(step.Outputs).Reason);
    }

    [Fact]
    public void RegeneratesOnlyTheChangedModel()
    {
        const string source = """
            using JJMasterData.Commons.Data;
            [DataAccessModel] public partial class Product { public int Id { get; set; } }
            [DataAccessModel] public partial class Customer { public int Code { get; set; } }
            """;
        var compilation = CreateCompilation(source);
        var driver = CreateTrackedDriver().RunGenerators(compilation, TestContext.Current.CancellationToken);
        var edited = compilation.ReplaceSyntaxTree(compilation.SyntaxTrees.Single(),
            CSharpSyntaxTree.ParseText(source.Replace("int Id", "long Id"), cancellationToken: TestContext.Current.CancellationToken));
        driver = driver.RunGeneratorsAndUpdateCompilation(edited, out var output, out _, TestContext.Current.CancellationToken);

        var result = Assert.Single(driver.GetRunResult().Results);
        Assert.Empty(result.Diagnostics);
        Assert.Empty(output.GetDiagnostics(TestContext.Current.CancellationToken)
            .Where(d => d.Severity == DiagnosticSeverity.Error));
        var reasons = result.TrackedSteps["Sources"].SelectMany(step => step.Outputs).Select(item => item.Reason).ToArray();
        Assert.Equal(2, reasons.Length);
        Assert.Contains(IncrementalStepRunReason.Modified, reasons);
        Assert.Contains(IncrementalStepRunReason.Cached, reasons);
    }

    [Fact]
    public void UpdatesDiagnosticLocationsAfterTriviaChanges()
    {
        const string source = """
            using JJMasterData.Commons.Data;
            [DataAccessModel] public partial class Product { public object Value { get; set; } }
            """;
        var compilation = CreateCompilation(source);
        var driver = CreateTrackedDriver().RunGenerators(compilation, TestContext.Current.CancellationToken);
        var original = Assert.Single(driver.GetRunResult().Diagnostics);
        var edited = compilation.ReplaceSyntaxTree(compilation.SyntaxTrees.Single(),
            CSharpSyntaxTree.ParseText("\n\n" + source, cancellationToken: TestContext.Current.CancellationToken));
        driver = driver.RunGenerators(edited, TestContext.Current.CancellationToken);

        var diagnostic = Assert.Single(driver.GetRunResult().Diagnostics);
        Assert.Equal("JJMDG003", diagnostic.Id);
        Assert.Equal(original.Location.SourceSpan.Start + 2, diagnostic.Location.SourceSpan.Start);
        Assert.Same(edited.SyntaxTrees.Single(), diagnostic.Location.SourceTree);
    }

    [Fact]
    public void EmitsEscapedIdentifiersAndLiteralsInsideRecordStructContainers()
    {
        var (compilation, result) = Generate(""""
            #nullable enable
            using JJMasterData.Commons.Data;
            namespace @event;
            public partial record struct Container
            {
                public enum Kind { None }
                [DataAccessModel] public partial record @class
                {
                    [DataAccessColumn("quoted\"column\\name\n")] public string? @string { get; set; }
                    public Kind? @enum { get; set; }
                }
            }
            """");
        Assert.Empty(result.Diagnostics);
        Assert.Single(Assert.Single(result.Results).GeneratedSources);
        Assert.Empty(compilation.GetDiagnostics(TestContext.Current.CancellationToken)
            .Where(d => d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning));
    }

    [Fact]
    public void SupportsIdenticalNamesNestedModelsAndGenericCalls()
    {
        var (compilation, result) = Generate("""
            using JJMasterData.Commons.Data;
            namespace Sales
            {
                [DataAccessModel] public partial class Product { public int Id { get; set; } }
                [DataAccessModel] public partial class ProductList { public int Id { get; set; } }
            }
            namespace Inventory
            {
                public static partial class Models
                {
                    [DataAccessModel] public partial class Product { public int Id { get; set; } }
                }
                [DataAccessModel] public partial class Product { public int Id { get; set; } }
                internal partial class Container
                {
                    [DataAccessModel] public partial class Nested { public int Id { get; set; } }
                }
                public class Queries
                {
                    public async System.Threading.Tasks.Task Run(DataAccess access, DataAccessCommand command)
                    {
                        await access.GetAsync<Sales.Product>(command);
                        await access.GetListAsync<Sales.ProductList>(command);
                        await access.GetAsync<Models.Product>(command);
                        await access.GetListAsync<Product>(command);
                    }
                }
            }
            """);

        Assert.Empty(result.Diagnostics);
        Assert.Equal(5, Assert.Single(result.Results).GeneratedSources.Length);
        Assert.Empty(compilation.GetDiagnostics(TestContext.Current.CancellationToken).Where(d => d.Severity == DiagnosticSeverity.Error));
    }

    [Theory]
    [InlineData("[DataAccessModel] public class Product { }", "JJMDG008")]
    [InlineData("public class Container { [DataAccessModel] public partial class Product { } }", "JJMDG008")]
    [InlineData("[DataAccessModel] public partial class Product<T> { }", "JJMDG001")]
    [InlineData("[DataAccessModel] public partial class Product { public object Value { get; set; } }", "JJMDG003")]
    public void ReportsInvalidModels(string declaration, string diagnosticId)
    {
        var (_, result) = Generate("using JJMasterData.Commons.Data; " + declaration);
        Assert.Contains(result.Diagnostics, d => d.Id == diagnosticId);
        Assert.Empty(Assert.Single(result.Results).GeneratedSources);
    }

    [Fact]
    public void HonorsInheritedAliasesAndIgnoresBeforeValidatingProperties()
    {
        var (compilation, result) = Generate("""
            using JJMasterData.Commons.Data;
            using System.Text.Json.Serialization;
            public class Base
            {
                [JsonPropertyName("base_id")] public virtual int Id { get; set; }
                [DataAccessIgnore] public virtual object Computed => new object();
                [DataAccessIgnore] public required object Context { get; init; }
            }
            [DataAccessModel] public partial class Product : Base
            {
                public override int Id { get; set; }
                public override object Computed => new object();
                [DataAccessIgnore, DataAccessColumn("")] public int Ignored { get; }
                [DataAccessIgnore] public static object Static => new object();
                [DataAccessIgnore] public object this[int index] => new object();
            }
            """);
        Assert.Empty(result.Diagnostics);
        Assert.Empty(compilation.GetDiagnostics(TestContext.Current.CancellationToken)
            .Where(d => d.Severity == DiagnosticSeverity.Error));
        var generated = Assert.Single(Assert.Single(result.Results).GeneratedSources).SourceText.ToString();
        Assert.Contains("\"base_id\"", generated);
        Assert.DoesNotContain("item.@Computed", generated);
    }

    [Theory]
    [InlineData("[JsonPropertyName(\"\")] public int Id { get; set; }", "JJMDG007")]
    [InlineData("[JsonPropertyName(\"id\")] public int Id { get; set; } public int ID { get; set; }", "JJMDG005")]
    [InlineData("public Product Child { get; set; }", "JJMDG006")]
    [InlineData("public required string Name { get; init; }", "JJMDG004")]
    public void ReportsInvalidPropertyMappings(string properties, string diagnosticId)
    {
        var (_, result) = Generate("""
            using JJMasterData.Commons.Data;
            using System.Text.Json.Serialization;
            [DataAccessModel] public partial class Product {
            """ + properties + "}");
        Assert.Contains(result.Diagnostics, d => d.Id == diagnosticId);
        Assert.Empty(Assert.Single(result.Results).GeneratedSources);
    }

    [Theory]
    [InlineData("public partial class Product { [DataAccessIgnore] public required string Value { get; init; } }")]
    [InlineData("internal partial record Product { [DataAccessIgnore] public required int Value { get; init; } }")]
    [InlineData("public partial class Product { internal Product() { } [DataAccessIgnore] public required string Value { get; set; } }")]
    public void SupportsModelsContainingOnlyIgnoredRequiredProperties(string declaration)
    {
        var (compilation, result) = Generate("using JJMasterData.Commons.Data; [DataAccessModel] " + declaration);
        Assert.Empty(result.Diagnostics);
        Assert.Empty(compilation.GetDiagnostics(TestContext.Current.CancellationToken)
            .Where(d => d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning));
    }

    [Fact]
    public void ReportsIndirectModelCycles()
    {
        var (_, result) = Generate("""
            using JJMasterData.Commons.Data;
            [DataAccessModel] public partial class Parent { public Child Child { get; set; } }
            [DataAccessModel] public partial class Child { public Parent Parent { get; set; } }
            """);
        Assert.Equal(2, result.Diagnostics.Count(d => d.Id == "JJMDG006"));
        Assert.Empty(Assert.Single(result.Results).GeneratedSources);
    }

    [Fact]
    public void RejectsGenericQueriesWithoutModelContract()
    {
        var (compilation, _) = Generate("""
            using JJMasterData.Commons.Data;
            public class Product { }
            public class Queries
            {
                public void Run(DataAccess access, DataAccessCommand command)
                    => _ = access.GetAsync<Product>(command);
            }
            """);
        Assert.Contains(compilation.GetDiagnostics(TestContext.Current.CancellationToken), d => d.Id == "CS0311");
    }
}
