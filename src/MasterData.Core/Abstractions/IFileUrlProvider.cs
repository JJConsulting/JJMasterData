using System.Collections.Generic;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.Abstractions;

public interface IFileUrlProvider
{
    string? GetFileUrl(
        FormElement formElement,
        FormElementField field,
        IReadOnlyDictionary<string, object?> values,
        string fileName);
}

internal sealed class EmptyFileUrlProvider : IFileUrlProvider
{
    public string? GetFileUrl(
        FormElement formElement,
        FormElementField field,
        IReadOnlyDictionary<string, object?> values,
        string fileName) => null;
}
