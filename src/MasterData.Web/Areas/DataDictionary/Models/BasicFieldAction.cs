using JJMasterData.Core.DataDictionary.Models.Actions;

namespace JJMasterData.Web.Areas.DataDictionary.Models;

public sealed class BasicFieldAction
{
    public required string FieldName { get; init; }
    public required BasicAction Action { get; init; }
}