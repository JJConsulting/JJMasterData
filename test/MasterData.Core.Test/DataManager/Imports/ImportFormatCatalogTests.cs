using System.Runtime.CompilerServices;
using JJMasterData.Core.DataManager.Importation;
using JJMasterData.Core.DataManager.Importation.Abstractions;

namespace JJMasterData.Core.Test.DataManager.Imports;

public sealed class ImportFormatCatalogTests
{
    [Fact]
    public void ReaderIsResolvedByExtensionWithoutCoreChanges()
    {
        var registration = new ImportReaderRegistration<JsonReader, JsonImportOptions>(new JsonReader());
        var catalog = new ImportFormatCatalog([registration]);

        var resolved = catalog.Resolve(null, "customers.json", "application/octet-stream");

        Assert.Equal("json", resolved.Metadata.Id);
        Assert.Equal("json", Assert.Single(catalog.Formats).Id);
    }

    [Fact]
    public void DuplicateIdentifiersAreRejected()
    {
        var first = new ImportReaderRegistration<JsonReader, JsonImportOptions>(new JsonReader());
        var second = new ImportReaderRegistration<JsonReader, JsonImportOptions>(new JsonReader());

        Assert.Throws<InvalidOperationException>(() => new ImportFormatCatalog([first, second]));
    }

    public sealed class JsonImportOptions : ImportFormatOptions
    {
        protected internal override string Id => "json";
        protected internal override string DisplayName => "JSON";
        protected internal override IReadOnlyList<string> FileExtensions => [".json"];
        protected internal override IReadOnlyList<string> ContentTypes => ["application/json"];
    }

    private sealed class JsonReader : IImportReader<JsonImportOptions>
    {
        public async IAsyncEnumerable<ImportRecord> ReadAsync(
            ImportContext context,
            JsonImportOptions options,
            Stream input,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            yield break;
        }
    }
}
