#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Expressions.Providers;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.Services;

public class DataItemService(IEntityRepository entityRepository,
    ExpressionParser expressionParser,
    IFormValues formValues,
    ElementMapService elementMapService,
    ILogger<DataItemService> logger)
{
    private IEntityRepository EntityRepository { get; } = entityRepository;
    private ExpressionParser ExpressionParser { get; } = expressionParser;
    private IFormValues FormValues { get; } = formValues;
    private ElementMapService ElementMapService { get; } = elementMapService;
    private ILogger<DataItemService> Logger { get; } = logger;

    public async Task<string?> GetSelectedValueAsync(FormElementField field, FormStateData formStateData,
        string? searchText = null, string? searchId = null)
    {
        var value = FormValues[field.Name];
        if (value is not null)
            return value;

        var first = await GetValuesAsync(field.DataItem!, formStateData, searchText, searchId).FirstOrDefaultAsync();

        return first?.Id;
    }

    public static IEnumerable<DataItemResult> GetItems(FormElementDataItem dataItem, IEnumerable<DataItemValue> values)
    {
        foreach (var i in values.ToArray())
        {
            string? description;
            
            if (dataItem.ShowIcon)
                description = $"{i.Description}|{i.Icon.GetCssClass()}|{i.IconColor}";
            else
                description = i.Description;

            yield return new DataItemResult(i.Id, description);
        }
    }

    public async IAsyncEnumerable<DataItemValue> GetValuesAsync(
        FormElementDataItem dataItem,
        FormStateData formStateData,
        string? searchText = null,
        string? searchId = null)
    {
        switch (dataItem.DataItemType)
        {
            case DataItemType.Manual:
            {
                foreach (var item in GetItemsValues(dataItem, searchId, searchText))
                    yield return item;
                yield break;
            }
            case DataItemType.SqlCommand:
                await foreach (var value in GetSqlCommandValues(dataItem, formStateData,searchId, searchText))
                    yield return value;
                yield break;
            case DataItemType.ElementMap:
                await foreach (var value in GetElementMapValues(dataItem, formStateData, searchId,searchText))
                    yield return value;
                yield break;
            default:
                throw new JJMasterDataException("Invalid DataItemType.");
        }
    }

    private static IEnumerable<DataItemValue> GetItemsValues(FormElementDataItem dataItem, string? searchId, string? searchText)
    {
        if (dataItem.Items != null)
            foreach (var item in dataItem.Items)
            {
                if (searchId is not null)
                {
                    if (item.Id == searchId)
                    {
                        yield return item;
                    }
                }
                else if (searchText is not null)
                {
                    if (item.Description?.ToLower().Contains(searchText.ToLower()) ?? false)
                    {
                        yield return item;
                    }
                }
                else
                {
                    yield return item;
                }
            }
    }

    private async IAsyncEnumerable<DataItemValue> GetElementMapValues(FormElementDataItem dataItem, FormStateData formStateData, string? searchId, string? searchText)
    {
        var elementMap = dataItem.ElementMap;
        var values = await ElementMapService.GetDictionaryList(elementMap!, searchId, formStateData);
            
        foreach(var value in values)
        {
            var item = new DataItemValue
            {
                Id = value[elementMap!.FieldId]?.ToString()
            };

            if (elementMap.FieldDescription != null) 
                item.Description = value[elementMap.FieldDescription]?.ToString();
            
            if (dataItem.ShowIcon)
            {
                if (elementMap.FieldIconId != null)
                    item.Icon = (IconType)int.Parse(value[elementMap.FieldIconId]?.ToString() ?? string.Empty);
                
                if (elementMap.FieldIconColor != null)
                    item.IconColor = value[elementMap.FieldIconColor]?.ToString();
            }

            if (searchText == null || item.Description!.ToLower().Contains(searchText.ToLower()))
            {
                yield return item;
            }
        }
    }

    private async IAsyncEnumerable<DataItemValue> GetSqlCommandValues(FormElementDataItem dataItem,
        FormStateData formStateData,
        string? searchId,
        string? searchText)
    {
        var command = GetDataItemCommand(dataItem, formStateData, searchText, searchId);
        
        List<Dictionary<string, object?>> result;
        
        try
        {
             result = await EntityRepository.GetDictionaryListAsync(command);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error at DataItemService while recovering SqlCommand values. Sql: {Sql}", command?.Sql);
            throw;
        }

        foreach (var row in result)
        {
            var item = new DataItemValue();
            item.Id = row.ElementAt(0).Value?.ToString();

            if (row.Count == 1)
            {
                item.Description = item.Id;
            }
            else
            {
                item.Description = row.ElementAt(1).Value?.ToString();
            }
            
            if (dataItem.ShowIcon)
            {
                item.Icon = (IconType)int.Parse(row.ElementAt(2).Value?.ToString() ?? string.Empty);
                item.IconColor = row.ElementAt(3).Value?.ToString();
            }

            if (searchText == null || (item.Description?.ToLower().Contains(searchText.ToLower()) ?? false))
            {
                yield return item;
            }
        }
    }

    private DataAccessCommand GetDataItemCommand(FormElementDataItem dataItem, FormStateData formStateData, string? searchText,
        string? searchId)
    {
        var sql = dataItem.Command!.Sql;
        if (sql.Contains("{"))
        {
            if (searchId != null)
            {
                if (formStateData.UserValues != null && !formStateData.UserValues.ContainsKey("SearchId"))
                    formStateData.UserValues.Add("SearchId", searchId);
            }

            if (searchText != null)
            {
                if (formStateData.UserValues != null && !formStateData.UserValues.ContainsKey("SearchText"))
                    formStateData.UserValues.Add("SearchText", searchText);
            }
        }

        var parsedValues = ExpressionParser.ParseExpression(sql, formStateData);

        return SqlExpressionProvider.GetParsedDataAccessCommand(sql, parsedValues);
    }
}