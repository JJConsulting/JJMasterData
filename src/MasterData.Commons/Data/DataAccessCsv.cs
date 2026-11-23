#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using JJMasterData.Commons.Data.Csv;

namespace JJMasterData.Commons.Data;

public partial class DataAccess
{
    /// <summary>Writes SQL rows directly to CSV without materializing them. Returns rows written,
    /// excluding the header. Leaves the destination open; the caller controls its final flush.</summary>
    public Task<long> WriteCsvAsync(
        DataAccessCommand command,
        TextWriter destination,
        IReadOnlyList<CsvColumn> columns,
        CsvWriteOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(columns);
        
        if (columns.Count == 0)
            throw new ArgumentException(@"At least one CSV column is required.", nameof(columns));
        
        foreach (var column in columns)
        {
            ArgumentNullException.ThrowIfNull(column);
            ArgumentException.ThrowIfNullOrEmpty(column.SourceName);
        }
        
        options ??= new CsvWriteOptions();
        
        ArgumentNullException.ThrowIfNull(options.Culture);
        ArgumentException.ThrowIfNullOrEmpty(options.Delimiter);
        ArgumentException.ThrowIfNullOrEmpty(options.NewLine);
        ArgumentException.ThrowIfNullOrEmpty(options.DateTimeFormat);
        
        var configuration = new CsvConfiguration(options.Culture)
        {
            Delimiter = options.Delimiter,
            HasHeaderRecord = options.IncludeHeader,
            NewLine = options.NewLine
        };
        
        configuration.Validate();
        cancellationToken.ThrowIfCancellationRequested();
        
        return ReadAsync(command, async (reader, token) =>
        {
            var ordinals = new int[columns.Count];
            for (var i = 0; i < columns.Count; i++)
                ordinals[i] = reader.GetOrdinal(columns[i].SourceName);
            
            await using var csv = new CsvWriter(destination, configuration, leaveOpen: true);
            csv.Context.TypeConverterOptionsCache.GetOptions<DateTime>().Formats = [options.DateTimeFormat];
            if (options.IncludeHeader)
            {
                foreach (var column in columns)
                    csv.WriteField(column.Header ?? column.SourceName);
                await csv.NextRecordAsync();
            }
            long count = 0;
            while (await reader.ReadAsync(token))
            {
                token.ThrowIfCancellationRequested();
                foreach (var ordinal in ordinals)
                    csv.WriteField(reader.IsDBNull(ordinal) ? null : reader.GetValue(ordinal));
                
                await csv.NextRecordAsync();
                count++;
            }
            await csv.FlushAsync();
            return count;
            
        }, cancellationToken: cancellationToken);
    }
}
