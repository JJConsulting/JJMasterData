#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;

namespace JJMasterData.Core.DataManager.Services;

public class ElementMapService
{
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IEntityRepository EntityRepository { get; }
    private IExpressionsService ExpressionsService { get; }

    public ElementMapService(IDataDictionaryRepository dataDictionaryRepository,IEntityRepository entityRepository,IExpressionsService expressionsService)
    {
        DataDictionaryRepository = dataDictionaryRepository;
        EntityRepository = entityRepository;
        ExpressionsService = expressionsService;
    }
    
    public async Task<IDictionary<string, object?>> GetFieldsAsync(DataElementMap elementMap, object? value, FormStateData formStateData)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(elementMap.ElementName);
        var filters = GetFilters(elementMap, value, formStateData);
        return await EntityRepository.GetFieldsAsync(formElement, filters);
    }
    
    public async Task<List<Dictionary<string, object?>>> GetDictionaryList(DataElementMap elementMap, object? value, FormStateData formStateData)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(elementMap.ElementName);
        var filters = GetFilters(elementMap, value, formStateData);
        return await EntityRepository.GetDictionaryListAsync(formElement, new EntityParameters()
        {
            Filters = filters!
        });
    }
    
    private IDictionary<string, object> GetFilters(DataElementMap elementMap, object? value, FormStateData formStateData)
    {
        var filters = new Dictionary<string, object>();

        if (elementMap.Filters.Count > 0)
        {
            foreach (var filter in elementMap.Filters)
            {
                string? filterParsed =
                    ExpressionsService.ParseExpression(filter.Value?.ToString(), formStateData, false);
                filters[filter.Key] = StringManager.ClearText(filterParsed);
            }
        }
        else
        {
            filters[elementMap.FieldKey] = StringManager.ClearText(value?.ToString());
        }
       
        return filters;
    }
}