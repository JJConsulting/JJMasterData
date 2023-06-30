using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.DataManager.Services.Abstractions;

public interface ISearchBoxService
{
    IEnumerable<SearchBoxItem> GetSearchBoxItems(FormElementDataItem dataItem, IEnumerable<DataItemValue> values);
    
    Task<IEnumerable<DataItemValue>> GetValues(FormElementDataItem dataItem,
        string searchText,
        string searchId,
        SearchBoxContext searchBoxContext);
}