using System.Collections.Generic;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.DataManager.Exportation.Abstractions;

public interface IExportFileLinkProvider
{
    string? GetLink(
        FormElement formElement,
        FormElementField field,
        IReadOnlyDictionary<string, object?> values,
        string fileName);
}

internal sealed class EmptyExportFileLinkProvider : IExportFileLinkProvider
{
    public string? GetLink(
        FormElement formElement,
        FormElementField field,
        IReadOnlyDictionary<string, object?> values,
        string fileName) => null;
}
