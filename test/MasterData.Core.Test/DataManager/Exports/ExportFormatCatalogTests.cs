using System.Runtime.CompilerServices;
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
        var registration = new ExportFormatRegistration<JsonExportFormat, JsonExportOptions>(new JsonExportFormat());
        var catalog = new ExportFormatCatalog([registration]);
        var context = new ExportContext
        {
            FormElement = new FormElement { Name = "customers" },
            Columns = [],
            Rows = Rows(TestContext.Current.CancellationToken),
            UserValues = [],
            IncludeHeader = true,
            TotalRecords = 1,
            Progress = new Progress<ExportProgress>()
        };
        await using var output = new MemoryStream();

        await catalog.GetRequired("json").WriteAsync(context, new Dictionary<string, string?>(), output,
            TestContext.Current.CancellationToken);

        Assert.Equal("json", Assert.Single(catalog.Formats).Id);
        output.Position = 0;
        using var document = await JsonDocument.ParseAsync(output, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("Ada", document.RootElement[0].GetProperty("name").GetString());
    }

    [Fact]
    public void DuplicateIdentifiersAreRejected()
    {
        var first = new ExportFormatRegistration<JsonExportFormat, JsonExportOptions>(new JsonExportFormat());
        var second = new ExportFormatRegistration<JsonExportFormat, JsonExportOptions>(new JsonExportFormat());

        Assert.Throws<InvalidOperationException>(() => new ExportFormatCatalog([first, second]));
    }

    private static async IAsyncEnumerable<Dictionary<string, object?>> Rows(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        yield return new Dictionary<string, object?> { ["name"] = "Ada" };
        await Task.CompletedTask;
    }

    private sealed class JsonExportOptions
    {
        public bool Indented { get; set; }
    }

    private sealed class JsonExportFormat : IExportFormat<JsonExportOptions>
    {
        public ExportFormatConfiguration Configuration { get; } = new()
        {
            Id = "json",
            DisplayName = "JSON",
            FileExtension = "json",
            ContentType = "application/json",
            Options =
            [
                new ExportFormatOption
                {
                    Name = nameof(JsonExportOptions.Indented),
                    DisplayName = "Indented",
                    Kind = ExportFormatOptionKind.Boolean,
                    DefaultValue = "false"
                }
            ]
        };

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
