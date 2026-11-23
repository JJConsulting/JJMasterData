using System.Data;
using System.Globalization;
using System.Text;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Csv;
using Microsoft.Data.Sqlite;

namespace JJMasterData.Commons.Test.Data;

public class DataAccessCsvTests
{
    private static DataAccess Access() => new("Data Source=:memory:", SqliteFactory.Instance);
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public async Task WritesSelectedColumnsInOrderWithEscapingAndLeavesWriterOpen()
    {
        using var writer = new StringWriter();
        var command = new DataAccessCommand("SELECT 'ignored' AS extra, @text AS name, 7 AS id, NULL AS missing");
        command.AddParameter("@text", " A|B\r\nC\"  ", DbType.String);
        var count = await Access().WriteCsvAsync(command, writer,
            new[] { new CsvColumn("id"), new CsvColumn("name"), new CsvColumn("missing") }, cancellationToken: Token);
        Assert.Equal(1, count);
        Assert.Equal("7|\" A|B\r\nC\"\"  \"|\r\n", writer.ToString());
        writer.Write("still open");
    }

    [Theory]
    [InlineData(true, "ID;Nome\n")]
    [InlineData(false, "")]
    public async Task EmptyResultHasOptionalHeader(bool header, string expected)
    {
        using var writer = new StringWriter();
        var count = await Access().WriteCsvAsync(new("SELECT 1 AS id, 'name' AS name WHERE 0"), writer,
            new[] { new CsvColumn("id", "ID"), new CsvColumn("name", "Nome") },
            new CsvWriteOptions { IncludeHeader = header, Delimiter = ";", NewLine = "\n" }, Token);
        Assert.Equal(0, count);
        Assert.Equal(expected, writer.ToString());
    }

    [Fact]
    public async Task MissingColumnFailsBeforeWritingHeader()
    {
        using var writer = new StringWriter();
        await Assert.ThrowsAnyAsync<Exception>(() => Access().WriteCsvAsync(new("SELECT 1 AS id"), writer,
            new[] { new CsvColumn("missing") }, new CsvWriteOptions { IncludeHeader = true }, Token));
        Assert.Empty(writer.ToString());
    }

    [Fact]
    public async Task CancellationRemainsCancellationAndLeavesDestinationOpen()
    {
        using var cts = new CancellationTokenSource();
        using var writer = new CancellingWriter(cts);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Access().WriteCsvAsync(
            new("SELECT 1 AS id UNION ALL SELECT 2"), writer, new[] { new CsvColumn("id") },
            cancellationToken: cts.Token));
        writer.Write("still open");
    }

    [Fact]
    public async Task WritesWithoutSynchronousIoAndUsesRequestedCulture()
    {
        using var writer = new AsyncOnlyWriter();
        await Access().WriteCsvAsync(new("SELECT 12.5 AS amount"), writer,
            new[] { new CsvColumn("amount") }, new CsvWriteOptions { Culture = CultureInfo.GetCultureInfo("pt-BR") }, Token);
        Assert.Equal("12,5\r\n", writer.Output.ToString());
    }

    [Fact]
    public async Task InvalidFormatFailsBeforeWriting()
    {
        using var writer = new StringWriter();
        await Assert.ThrowsAsync<FormatException>(() => Access().WriteCsvAsync(new("SELECT 1 AS id"), writer,
            new[] { new CsvColumn("id") }, new CsvWriteOptions { IncludeHeader = true, DateTimeFormat = "%" }, Token));
        Assert.Empty(writer.ToString());
    }

    private sealed class CancellingWriter(CancellationTokenSource cts) : StringWriter
    {
        public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
        {
            cts.Cancel();
            cancellationToken.ThrowIfCancellationRequested();
            return base.WriteAsync(buffer, cancellationToken);
        }
    }

    private sealed class AsyncOnlyWriter : TextWriter
    {
        public StringBuilder Output { get; } = new();
        public override Encoding Encoding => Encoding.UTF8;
        public override void Write(char value) => throw new InvalidOperationException("Synchronous write");
        public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Output.Append(buffer.Span);
            return Task.CompletedTask;
        }
    }
}
