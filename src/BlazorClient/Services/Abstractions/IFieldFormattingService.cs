using JJMasterData.Core.DataDictionary;

namespace JJMasterData.BlazorClient.Services.Abstractions;

public interface IFieldFormattingService
{
    Task<string> FormatGridValue(FormElementField field, IDictionary<string,dynamic?> values);
}