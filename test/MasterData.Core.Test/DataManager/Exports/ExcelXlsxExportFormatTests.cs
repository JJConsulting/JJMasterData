using System.IO.Compression;
using System.Runtime.CompilerServices;
using JJMasterData.Core.Configuration;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Core.DataManager.Exportation.Formats;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniExcelLibs;

namespace JJMasterData.Core.Test.DataManager.Exports;

public sealed class ExcelXlsxExportFormatTests
{
    [Fact]
    public void XlsxFormatIsRegisteredAlongsideLegacyExcelFormat()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMemoryCache();
        services.AddLocalization();
        services.AddRouting();
        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JJMasterData:ConnectionString"] = "Server=(local);Database=master;Integrated Security=true"
            })
            .Build());
        services.AddJJMasterDataCore();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var formats = scope.ServiceProvider.GetRequiredService<ExportFormatCatalog>().Formats;

        var legacy = Assert.Single(formats, format => format.Id == "excel");
        Assert.Equal("xls", legacy.FileExtension);
        Assert.Equal("application/vnd.ms-excel", legacy.ContentType);
        Assert.Equal(2, legacy.Options?.Count);

        var xlsx = Assert.Single(formats, format => format.Id == "xlsx");
        Assert.Equal("Excel (.xlsx)", xlsx.DisplayName);
        Assert.Equal("xlsx", xlsx.FileExtension);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", xlsx.ContentType);
        var option = Assert.Single(xlsx.Options!);
        Assert.Equal(nameof(ExcelXlsxExportOptions.ShowTableStyle), option.Name);
        Assert.Equal("Show table style", option.DisplayName);
        Assert.Equal(ExportFormatOptionKind.Boolean, option.Kind);
        Assert.Equal("true", option.DefaultValue);
    }

    [Fact]
    public async Task WriteAsyncWritesHeadersAndNativeValuesAndDisposesRows()
    {
        var disposed = false;
        var progress = new RecordingProgress();
        var identifier = Guid.Parse("c32fcb0d-04d4-436f-b5b4-771ff605fec8");
        var timestamp = new DateTime(2026, 9, 1, 13, 45, 0);
        var duration = TimeSpan.FromHours(9.5);
        var columns = CreateColumns(
            ("name", "Name"),
            ("count", "Count"),
            ("amount", "Amount"),
            ("timestamp", "Timestamp"),
            ("active", "Active"),
            ("identifier", "Identifier"),
            ("duration", "Duration"));
        var rows = Rows(
            [
                new Dictionary<string, object?>
                {
                    ["name"] = "Ada",
                    ["count"] = 42,
                    ["amount"] = 12.34m,
                    ["timestamp"] = timestamp,
                    ["active"] = true,
                    ["identifier"] = identifier,
                    ["duration"] = duration
                }
            ],
            () => disposed = true,
            TestContext.Current.CancellationToken);
        var context = CreateContext(columns, rows, true, 1, progress);

        var bytes = await WriteAsync(context, TestContext.Current.CancellationToken);
        var row = Assert.Single(Query(bytes, true));

        Assert.Equal(["Name", "Count", "Amount", "Timestamp", "Active", "Identifier", "Duration"], row.Keys);
        Assert.Equal("Ada", row["Name"]);
        Assert.Equal(42D, row["Count"]);
        Assert.Equal(12.34D, row["Amount"]);
        Assert.Equal(timestamp, row["Timestamp"]);
        Assert.True(Assert.IsType<bool>(row["Active"]));
        Assert.Equal(identifier.ToString(), row["Identifier"]);
        Assert.Equal(duration, row["Duration"]);
        Assert.True(disposed);

        var report = Assert.Single(progress.Reports);
        Assert.Equal(1, report.Processed);
        Assert.Equal(1, report.Total);
        Assert.Equal(100, report.Percentage);

        var sheetXml = ReadSheetXml(bytes);
        Assert.Contains("autoFilter", sheetXml);
        Assert.Contains("state=\"frozen\"", sheetXml);
    }

    [Fact]
    public async Task WriteAsyncOmitsHeaderFeaturesWhenHeaderIsDisabled()
    {
        var context = CreateContext(
            CreateColumns(("name", "Name"), ("count", "Count")),
            Rows(
                [new Dictionary<string, object?> { ["name"] = "Ada", ["count"] = 42 }],
                cancellationToken: TestContext.Current.CancellationToken),
            false,
            1);

        var bytes = await WriteAsync(context, TestContext.Current.CancellationToken);
        var row = Assert.Single(Query(bytes, false));

        Assert.Equal("Ada", row["A"]);
        Assert.Equal(42D, row["B"]);
        var sheetXml = ReadSheetXml(bytes);
        Assert.DoesNotContain("autoFilter", sheetXml);
        Assert.DoesNotContain("state=\"frozen\"", sheetXml);
    }

    [Fact]
    public async Task WriteAsyncOmitsTableStyleWhenDisabled()
    {
        var context = CreateContext(
            CreateColumns(("name", "Name")),
            Rows(
                [new Dictionary<string, object?> { ["name"] = "Ada" }],
                cancellationToken: TestContext.Current.CancellationToken),
            true,
            1);

        var bytes = await WriteAsync(
            context,
            TestContext.Current.CancellationToken,
            new ExcelXlsxExportOptions { ShowTableStyle = false });

        using var stream = new MemoryStream(bytes);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        Assert.DoesNotContain(archive.Entries, entry => entry.FullName.StartsWith("xl/tables/"));
    }

    [Fact]
    public async Task WriteAsyncPreservesHeadersForAnEmptyExport()
    {
        var context = CreateContext(
            CreateColumns(("name", "Name"), ("count", "Count")),
            Rows([], cancellationToken: TestContext.Current.CancellationToken),
            true,
            0);

        var bytes = await WriteAsync(context, TestContext.Current.CancellationToken);
        var header = Assert.Single(Query(bytes, false));

        Assert.Equal("Name", header["A"]);
        Assert.Equal("Count", header["B"]);
    }

    [Fact]
    public async Task WriteAsyncWritesNullForNullAndMissingValues()
    {
        var context = CreateContext(
            CreateColumns(("name", "Name"), ("note", "Note")),
            Rows(
            [
                new Dictionary<string, object?> { ["name"] = "Ada", ["note"] = null },
                new Dictionary<string, object?> { ["name"] = "Grace" }
            ], cancellationToken: TestContext.Current.CancellationToken),
            true,
            2);

        var bytes = await WriteAsync(context, TestContext.Current.CancellationToken);
        var rows = Query(bytes, true);

        Assert.Equal(2, rows.Count);
        Assert.Null(rows[0]["Note"]);
        Assert.Null(rows[1]["Note"]);
    }

    [Fact]
    public async Task WriteAsyncHonorsCancellation()
    {
        using var cancellationSource = new CancellationTokenSource();
        await cancellationSource.CancelAsync();
        var context = CreateContext(
            CreateColumns(("name", "Name")),
            Rows(
                [new Dictionary<string, object?> { ["name"] = "Ada" }],
                cancellationToken: TestContext.Current.CancellationToken),
            true,
            1);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            WriteAsync(context, cancellationSource.Token));
    }

    [Fact]
    public async Task WriteAsyncSupportsProductionFileStream()
    {
        var filePath = Path.GetTempFileName();
        try
        {
            var context = CreateContext(
                CreateColumns(("name", "Name")),
                Rows(
                    [new Dictionary<string, object?> { ["name"] = "Ada" }],
                    cancellationToken: TestContext.Current.CancellationToken),
                true,
                1);

            await using (var output = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None,
                81920,
                true))
            {
                await new ExcelXlsxExportFormat().WriteAsync(
                    context,
                    new ExcelXlsxExportOptions(),
                    output,
                    TestContext.Current.CancellationToken);
            }

            var bytes = await File.ReadAllBytesAsync(filePath, TestContext.Current.CancellationToken);
            var row = Assert.Single(Query(bytes, true));
            Assert.Equal("Ada", row["Name"]);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    private static ExportContext CreateContext(
        List<ExportColumn> columns,
        IAsyncEnumerable<Dictionary<string, object?>> rows,
        bool includeHeader,
        long totalRecords,
        IProgress<ExportProgress>? progress = null)
    {
        return new ExportContext
        {
            FormElement = new FormElement { Name = "customers" },
            Columns = columns,
            Rows = rows,
            UserValues = [],
            IncludeHeader = includeHeader,
            TotalRecords = totalRecords,
            Progress = progress ?? new RecordingProgress()
        };
    }

    private static List<ExportColumn> CreateColumns(params (string Name, string DisplayName)[] columns) =>
        columns.Select(column => new ExportColumn(column.Name, column.DisplayName, new FormElementField
        {
            Name = column.Name
        })).ToList();

    private static async Task<byte[]> WriteAsync(
        ExportContext context,
        CancellationToken cancellationToken,
        ExcelXlsxExportOptions? options = null)
    {
        await using var output = new MemoryStream();
        await new ExcelXlsxExportFormat().WriteAsync(
            context,
            options ?? new ExcelXlsxExportOptions(),
            output,
            cancellationToken);
        return output.ToArray();
    }

    private static List<IDictionary<string, object>> Query(byte[] bytes, bool useHeaderRow)
    {
        using var stream = new MemoryStream(bytes);
        return stream.Query(useHeaderRow: useHeaderRow, excelType: ExcelType.XLSX)
            .Cast<IDictionary<string, object>>()
            .ToList();
    }

    private static string ReadSheetXml(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        var entry = archive.GetEntry("xl/worksheets/sheet1.xml");
        Assert.NotNull(entry);
        using var reader = new StreamReader(entry.Open());
        return reader.ReadToEnd();
    }

    private static async IAsyncEnumerable<Dictionary<string, object?>> Rows(
        IReadOnlyList<Dictionary<string, object?>> values,
        Action? onDisposed = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var value in values)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return value;
                await Task.Yield();
            }
        }
        finally
        {
            onDisposed?.Invoke();
        }
    }

    private sealed class RecordingProgress : IProgress<ExportProgress>
    {
        public List<ExportProgress> Reports { get; } = [];

        public void Report(ExportProgress value) => Reports.Add(value);
    }
}
