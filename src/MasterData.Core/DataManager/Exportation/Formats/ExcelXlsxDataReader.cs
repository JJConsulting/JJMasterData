using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using MiniExcelLibs;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

internal sealed class ExcelXlsxDataReader(
    ExportContext context,
    CancellationToken cancellationToken) : MiniExcelDataReaderBase
{
    private IAsyncEnumerator<ExportRow>? _enumerator;
    private ExportRow? _current;
    private long _processed;
    private bool _disposed;

    public override int FieldCount => context.Columns.Count;

    public override bool IsClosed => _disposed;

    public override string GetName(int i) => context.Columns[i].DisplayName;

    public override int GetOrdinal(string name) => context.Columns.FindIndex(column =>
        string.Equals(column.DisplayName, name, StringComparison.OrdinalIgnoreCase));

    public override object? GetValue(int i)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        if (_current is null)
            throw new InvalidOperationException("No export row is currently available.");

        var column = context.Columns[i];
        return _current.Values.GetValueOrDefault(column.Name);
    }

    public override Task<object> GetValueAsync(int i, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        return Task.FromResult(GetValue(i)!);
    }

    public override bool Read() => throw new NotSupportedException("Synchronous XLSX export is not supported.");

    public override async Task<bool> ReadAsync(CancellationToken token = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        token.ThrowIfCancellationRequested();
        cancellationToken.ThrowIfCancellationRequested();

        _enumerator ??= context.Rows.GetAsyncEnumerator(cancellationToken);
        if (!await _enumerator.MoveNextAsync())
        {
            _current = null;
            return false;
        }

        _current = _enumerator.Current;
        _processed++;
        context.Progress.Report(new ExportProgress(
            _processed,
            context.TotalRecords,
            $"Exporting {_processed:N0} records..."));
        return true;
    }

    public override void Close()
    {
        _current = null;
    }

    public override async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;
        _current = null;
        if (_enumerator is not null)
            await _enumerator.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
