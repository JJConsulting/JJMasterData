using JJMasterData.Core.DataDictionary;

namespace JJMasterData.BlazorClient.Services;

public interface ISearchBoxService
{
    Task<IEnumerable<DataItemValue>> GetValues(FormElementDataItem dataItem,
        string? text,
        SearchBoxContext searchBoxContext);
}