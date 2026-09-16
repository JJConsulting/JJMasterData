using System;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Core.DataManager.Exportation.Abstractions;

namespace JJMasterData.Core.DataManager.Exportation;

public sealed class ExportFormatCatalog
{
    private readonly Dictionary<string, IExportFormat> _formats;

    public ExportFormatCatalog(IEnumerable<IExportFormat> registrations)
    {
        _formats = Build(registrations.ToList());
    }

    public IEnumerable<ExportFormatMetadata> GetFormats()
    {
        foreach (var format in _formats.Values)
        {
            var options = ExportFormatOptionsMetadataFactory.CreateOptions(format);

            yield return new ExportFormatMetadata
            {
                Format = format,
                Options = options
            };
        }
    }

    internal IExportFormat GetRequired(string id)
    {
        if (_formats.TryGetValue(id, out var registration))
            return registration;
        throw new InvalidOperationException($"Export format '{id}' is not registered.");
    }

    private static Dictionary<string, IExportFormat> Build(List<IExportFormat> registrations)
    {
        var result = new Dictionary<string, IExportFormat>(StringComparer.OrdinalIgnoreCase);
        foreach (var registration in registrations)
        {
            if (string.IsNullOrWhiteSpace(registration.Id))
                throw new InvalidOperationException("Export format identifiers cannot be empty.");
            if (!result.TryAdd(registration.Id, registration))
                throw new InvalidOperationException($"Export format '{registration.Id}' is registered more than once.");
        }
        return result;
    }
}
