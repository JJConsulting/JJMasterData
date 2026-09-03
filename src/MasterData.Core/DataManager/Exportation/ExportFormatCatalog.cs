using System;
using System.Collections.Generic;
using System.Linq;

namespace JJMasterData.Core.DataManager.Exportation;

public sealed class ExportFormatCatalog
{
    private readonly Dictionary<string, IExportFormatRegistration> _registrations;

    internal ExportFormatCatalog(IEnumerable<IExportFormatRegistration> registrations)
    {
        _registrations = Build(registrations.ToList());
    }

    public List<ExportFormatMetadata> Formats => _registrations.Values
        .Select(registration => registration.Metadata)
        .OrderBy(definition => definition.DisplayName, StringComparer.CurrentCultureIgnoreCase)
        .ToList();

    internal IExportFormatRegistration GetRequired(string id)
    {
        if (_registrations.TryGetValue(id, out var registration))
            return registration;
        throw new InvalidOperationException($"Export format '{id}' is not registered.");
    }

    private static Dictionary<string, IExportFormatRegistration> Build(
        List<IExportFormatRegistration> registrations)
    {
        var result = new Dictionary<string, IExportFormatRegistration>(StringComparer.OrdinalIgnoreCase);
        foreach (var registration in registrations)
        {
            var definition = registration.Metadata;
            if (string.IsNullOrWhiteSpace(definition.Id))
                throw new InvalidOperationException("Export format identifiers cannot be empty.");
            if (!result.TryAdd(definition.Id, registration))
                throw new InvalidOperationException($"Export format '{definition.Id}' is registered more than once.");
        }
        return result;
    }
}
