using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.DataManager.Services.Abstractions;

public interface IDataItemService
{
    IEnumerable<DataItemResult> GetItems(FormElementDataItem dataItem, IEnumerable<DataItemValue> values);
    
    Task<IEnumerable<DataItemValue>> GetValues(FormElementDataItem dataItem,
        string searchText,
        string searchId,
        SearchBoxContext searchBoxContext);

    Task<string> GetSelectedValue(FormElementField field,string searchText, IDictionary<string,dynamic> values, PageState pageState);
}