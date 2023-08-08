using JJMasterData.Core.DataDictionary;
using System.Threading.Tasks;

namespace JJMasterData.Core.DataManager.Services;

public interface ILookupService
{
    string GetLookupUrl(FormElementDataItem dataItem, FormStateData formStateData, string componentName);

    object GetSelectedValue(string componentName);

    Task<string> GetDescriptionAsync(
        FormElementDataItem dataItem,
        FormStateData formStateData,
        string searchId,
        bool allowOnlyNumbers);
}