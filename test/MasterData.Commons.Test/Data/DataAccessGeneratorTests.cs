using JJMasterData.Commons.Data;
using JJMasterData.Data.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace JJMasterData.Commons.Test.Data;

public class DataAccessGeneratorTests
{
    private static (Compilation Compilation, GeneratorDriverRunResult Result) Generate(string source)
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(DataAccess).Assembly.Location));
        var compilation = CSharpCompilation.Create("GeneratorConsumer",
            [CSharpSyntaxTree.ParseText(source)], references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new DataAccessExtensionsGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out _);
        return (output, driver.GetRunResult());
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
