#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataManager.Services;

public class ElementMapService(IDataDictionaryRepository dataDictionaryRepository,IEntityRepository entityRepository,ExpressionsService expressionsService)
{
    private IDataDictionaryRepository DataDictionaryRepository { get; } = dataDictionaryRepository;
    private IEntityRepository EntityRepository { get; } = entityRepository;
    private ExpressionsService ExpressionsService { get; } = expressionsService;

    public async Task<Dictionary<string, object?>> GetFieldsAsync(DataElementMap elementMap, object? value, FormStateData? formStateData)
    {
        var childElement = await DataDictionaryRepository.GetFormElementAsync(elementMap.ElementName);
        var filters = GetFilters(elementMap, value, formStateData);
        return await EntityRepository.GetFieldsAsync(childElement, filters);
    }
    
    public async Task<List<Dictionary<string, object?>>> GetDictionaryList(DataElementMap elementMap, object? value, FormStateData formStateData)
    {
        var childElement = await DataDictionaryRepository.GetFormElementAsync(elementMap.ElementName);
        var filters = GetFilters(elementMap, value, formStateData);
        return await EntityRepository.GetDictionaryListAsync(childElement, new EntityParameters
        {
            Filters = filters!
        });
    }
    
    private Dictionary<string, object> GetFilters(
        DataElementMap elementMap, 
        object? value, 
        FormStateData? formStateData)
    {
        var filters = new Dictionary<string, object>
        {
            [elementMap.IdFieldName] = value?.ToString()!
        };
        if (elementMap.Filters.Count > 0)
        {
            foreach (var filter in elementMap.Filters)
            {
                if (formStateData != null)
                {
                    var filterParsed = ExpressionsService.GetExpressionValue(filter.Value.ToString(), formStateData) ?? string.Empty;
                    filters[filter.Key] = filterParsed;
                }
            }
        }
       
        return filters;
    }
}