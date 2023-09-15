using JJMasterData.Core.DataDictionary;
using System.Threading.Tasks;

namespace JJMasterData.Core.DataManager.Services;

public interface ILookupService
{
    string GetFormViewUrl(DataElementMap elementMap, FormStateData formStateData, string componentName);

    string GetDescriptionUrl(string elementName, string fieldName, string componentName, PageState pageState);
    
    string GetSelectedValue(string componentName);

    Task<string> GetDescriptionAsync(
        DataElementMap elementMap,
        FormStateData formStateData,
        object value,
        bool allowOnlyNumbers);
}