using JJMasterData.Core.DataDictionary;
using System.Threading.Tasks;

namespace JJMasterData.Core.DataManager.Services;

public interface ILookupService
{
    string GetLookupUrl(DataElementMap elementMap, FormStateData formStateData, string componentName);

    object GetSelectedValue(string componentName);

    Task<string> GetDescriptionAsync(
        DataElementMap elementMap,
        FormStateData formStateData,
        object value,
        bool allowOnlyNumbers);
}