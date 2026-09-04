using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Exportation.Abstractions;

namespace JJMasterData.Core.Test.DataManager.Exports;

public sealed class ExportFormatCatalogTests
{
    [Fact]
    public async Task CustomJsonFormatIsDiscoveredAndWritesWithoutCoreChanges()
    {
        var catalog = new ExportFormatCatalog([new JsonExportFormat()]);
        var context = new ExportContext
        {
            FormElement = new FormElement { Name = "customers" },
            Columns = [],
            Rows = Rows(TestContext.Current.CancellationToken),
            UserValues = [],
            TotalRecords = 1,
            Progress = new Progress<ExportProgress>()
        };
        await using var output = new MemoryStream();

        await catalog.GetRequired("json").WriteAsync(context, new JsonExportOptions(), output,
            TestContext.Current.CancellationToken);

        var metadata = Assert.Single(catalog.GetFormats());
        Assert.Equal("json", metadata.Format.Id);
        var option = Assert.Single(metadata.Options, o => o.Name == nameof(JsonExportOptions.Indented));
        Assert.Equal("Pretty JSON", option.DisplayName);
        Assert.Equal(ExportFormatOptionKind.Boolean, option.Kind);
        output.Position = 0;
        using var document = await JsonDocument.ParseAsync(output, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("Ada", document.RootElement[0].GetProperty("name").GetString());
    }

    [Fact]
    public void DuplicateIdentifiersAreRejected()
    {
        Assert.Throws<InvalidOperationException>(() => new ExportFormatCatalog([new JsonExportFormat(), new JsonExportFormat()]));
    }

    private static async IAsyncEnumerable<Dictionary<string, object?>> Rows(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        yield return new Dictionary<string, object?> { ["name"] = "Ada" };
        await Task.CompletedTask;
    }

    private sealed class JsonExportOptions : ExportFormatOptions
    {
        [Display(Name = "Pretty JSON")]
        public bool Indented { get; set; }
    }

    private sealed class JsonExportFormat : IExportFormat<JsonExportOptions>
    {
        public string Id => "json";
        public string DisplayName => "JSON";
        public string FileExtension => "json";

        public async Task WriteAsync(
            ExportContext context,
            JsonExportOptions options,
            Stream output,
            CancellationToken cancellationToken)
        {
            var rows = new List<Dictionary<string, object?>>();
            await foreach (var row in context.Rows.WithCancellation(cancellationToken))
                rows.Add(row);
            await JsonSerializer.SerializeAsync(output, rows, new JsonSerializerOptions
            {
                WriteIndented = options.Indented
            }, cancellationToken);
        }
    }
}
