#nullable disable warnings
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataManager.Services;

public class LookupService(
    ElementMapService elementMapService)
{
    public async Task<string?> GetDescriptionAsync(
        DataElementMap elementMap,
        FormStateData? formStateData,
        object? value,
        bool allowOnlyNumbers)
    {
        if (string.IsNullOrEmpty(value?.ToString()))
            return null;
        
        if (allowOnlyNumbers)
        {
            bool isNumeric = int.TryParse(value?.ToString(), out _);
            if (!isNumeric)
                return null;
        }

        Dictionary<string, object?> values;

        try
        {
            values = await elementMapService.GetFieldsAsync(elementMap, value, formStateData);
        }
        catch
        {
            return null;
        }


        if (string.IsNullOrEmpty(elementMap.DescriptionFieldName) &&
            values.TryGetValue(elementMap.IdFieldName, out var id))
            return id?.ToString();

        if (elementMap.DescriptionFieldName != null &&
            values.TryGetValue(elementMap.DescriptionFieldName, out var description))
            return description?.ToString();

        return null;
    }

}
