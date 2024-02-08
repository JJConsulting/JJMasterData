#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
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
        var filters = await GetFilters(childElement,elementMap, value, formStateData);
        return await EntityRepository.GetFieldsAsync(childElement, filters);
    }
    
    public async Task<List<Dictionary<string, object?>>> GetDictionaryList(DataElementMap elementMap, object? value, FormStateData formStateData)
    {
        var childElement = await DataDictionaryRepository.GetFormElementAsync(elementMap.ElementName);
        var filters = await GetFilters(childElement,elementMap, value, formStateData);
        return await EntityRepository.GetDictionaryListAsync(childElement, new EntityParameters
        {
            Filters = filters!
        });
    }
    
    private async Task<Dictionary<string, object>> GetFilters(
        FormElement childElement,
        DataElementMap elementMap, 
        object? value, 
        FormStateData? formStateData)
    {
        var filters = new Dictionary<string, object>();

        if (elementMap.Filters.Count > 0)
        {
            foreach (var filter in elementMap.Filters)
            {
                if (formStateData != null)
                {
                    var field = childElement.Fields[filter.Key];
                    var filterParsed =
                        await ExpressionsService.GetExpressionValueAsync(filter.Value.ToString(), field,formStateData) ?? string.Empty;
                    filters[filter.Key] = filterParsed;
                }
            }
        }
        else
        {
            filters[elementMap.IdFieldName] = value?.ToString()!;
        }
       
        return filters;
    }
}