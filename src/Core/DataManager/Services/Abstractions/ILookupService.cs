using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.DataManager.Services;

public interface ILookupService
{
    string GetLookupUrl(FormElementDataItem dataItem, string componentName, PageState pageState, IDictionary<string,dynamic> formValues);
    object GetSelectedValue(string componentName);

    Task<string> GetDescriptionAsync(
        FormElementDataItem dataItem,
        string selectedValue,
        PageState pageState, 
        IDictionary<string,dynamic> formValues,
        bool allowOnlyNumbers);
}