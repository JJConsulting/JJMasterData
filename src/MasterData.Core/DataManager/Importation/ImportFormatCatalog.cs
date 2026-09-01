using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Importation.Abstractions;

namespace JJMasterData.Core.DataManager.Importation;

internal interface IImportReaderRegistration
{
    ImportFormatDefinition Definition { get; }
    void ValidateOptions(Dictionary<string, string?> options);
    IAsyncEnumerable<ImportRecord> ReadAsync(
        ImportContext context,
        Dictionary<string, string?> options,
        Stream input,
        CancellationToken cancellationToken);
}

internal sealed class ImportReaderRegistration<TReader, TOptions>(TReader reader) : IImportReaderRegistration
    where TReader : class, IImportReader<TOptions>
    where TOptions : class, new()
{
    public ImportFormatDefinition Definition => reader.Definition;

    public IAsyncEnumerable<ImportRecord> ReadAsync(
        ImportContext context,
        Dictionary<string, string?> options,
        Stream input,
        CancellationToken cancellationToken)
    {
        return reader.ReadAsync(
            context,
            FormatOptionsBinder.Bind<TOptions>(Definition.Options, options),
            input,
            cancellationToken);
    }

    public void ValidateOptions(Dictionary<string, string?> options) =>
        _ = FormatOptionsBinder.Bind<TOptions>(Definition.Options, options);
}

public sealed class ImportFormatCatalog
{
    private readonly Dictionary<string, IImportReaderRegistration> _registrations;

    internal ImportFormatCatalog(IEnumerable<IImportReaderRegistration> registrations)
    {
        _registrations = Build(registrations.ToList());
    }

    public List<ImportFormatDefinition> Formats => _registrations.Values
        .Select(registration => registration.Definition)
        .OrderBy(definition => definition.DisplayName, StringComparer.CurrentCultureIgnoreCase)
        .ToList();

    internal IImportReaderRegistration Resolve(string? id, string fileName, string? contentType)
    {
        if (!string.IsNullOrWhiteSpace(id))
        {
            if (_registrations.TryGetValue(id, out var selected))
                return selected;
            throw new InvalidOperationException($"Import format '{id}' is not registered.");
        }

        var extension = Path.GetExtension(fileName);
        var matches = _registrations.Values.Where(registration =>
                registration.Definition.FileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(contentType) &&
                 registration.Definition.ContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase)))
            .ToArray();
        return matches.Length switch
        {
            1 => matches[0],
            0 => throw new InvalidOperationException($"No import reader accepts '{fileName}'."),
            _ => throw new InvalidOperationException($"More than one import reader accepts '{fileName}'. Select a format explicitly.")
        };
    }

    private static Dictionary<string, IImportReaderRegistration> Build(
        List<IImportReaderRegistration> registrations)
    {
        var result = new Dictionary<string, IImportReaderRegistration>(StringComparer.OrdinalIgnoreCase);
        foreach (var registration in registrations)
        {
            if (string.IsNullOrWhiteSpace(registration.Definition.Id))
                throw new InvalidOperationException("Import format identifiers cannot be empty.");
            if (!result.TryAdd(registration.Definition.Id, registration))
                throw new InvalidOperationException($"Import format '{registration.Definition.Id}' is registered more than once.");
        }
        return result;
    }
}
