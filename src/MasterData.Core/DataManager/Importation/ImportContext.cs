using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.DataManager.Importation;

public sealed class ImportContext(FormElement formElement, string fileName, string? contentType)
{
    public FormElement FormElement { get; init; } = formElement;
    public string FileName { get; init; } = fileName;
    public string? ContentType { get; init; } = contentType;
}
