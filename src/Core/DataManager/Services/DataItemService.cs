#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Logging;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Expressions.Providers;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Logging;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.Services;

public class DataItemService(IEntityRepository entityRepository,
    ExpressionParser expressionParser,
    ElementMapService elementMapService,
    ILogger<DataItemService> logger)
{
    private IEntityRepository EntityRepository { get; } = entityRepository;
    private ExpressionParser ExpressionParser { get; } = expressionParser;
    private ElementMapService ElementMapService { get; } = elementMapService;
    private ILogger<DataItemService> Logger { get; } = logger;

    public async Task<List<DataItemValue>> GetValuesAsync(
        FormElementDataItem dataItem,
        FormStateData formStateData,
        string? searchText = null,
        string? searchId = null)
    {
        var dataItemType = GetDataItemType(dataItem);
        
        switch (dataItemType)
        {
            case DataItemType.Manual:
                return GetItemsValues(dataItem, searchId, searchText).ToList();
            case DataItemType.SqlCommand:
                return await GetSqlCommandValues(dataItem, formStateData, searchId, searchText).ToListAsync();
            case DataItemType.ElementMap:
                return await GetElementMapValues(dataItem, formStateData, searchId,searchText).ToListAsync();
            default:
                throw new JJMasterDataException("Invalid DataItemType.");
        }
    }

    private static DataItemType GetDataItemType(FormElementDataItem dataItem)
    {
        if (dataItem.HasItems())
            return DataItemType.Manual;
        if (dataItem.HasSqlCommand())
            return DataItemType.SqlCommand;
        if (dataItem.HasElementMap())
            return DataItemType.ElementMap;
        
        return DataItemType.Manual;
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
                Id = value[elementMap!.IdFieldName]!.ToString()!
            };

            if (elementMap.DescriptionFieldName != null) 
                item.Description = value[elementMap.DescriptionFieldName]?.ToString()!;
            
            if (dataItem.ShowIcon)
            {
                if (elementMap.IconIdFieldName != null)
                    item.Icon = (IconType)int.Parse(value[elementMap.IconIdFieldName]?.ToString() ?? string.Empty);
                
                if (elementMap.IconColorFieldName != null)
                    item.IconColor = value[elementMap.IconColorFieldName]?.ToString();
            }
            
            if (elementMap.GroupFieldName != null)
                item.Group = value[elementMap.GroupFieldName]?.ToString();

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
        
        DataTable result;
        
        try
        {
             result = await EntityRepository.GetDataTableAsync(command);
        }
        catch (Exception ex)
        {
            Logger.LogDataAccessCommandException(ex, command);
            throw;
        }

        foreach (DataRow row in result.Rows)
        {
            var item = new DataItemValue();
            item.Id = row[0].ToString()!;

            if (row.Table.Columns.Count == 1)
            {
                item.Description = item.Id;
            }
            else
            {
                item.Description = row[1].ToString()!;
            }
            
            if (dataItem.ShowIcon)
            {
                item.Icon = (IconType)int.Parse(row[2].ToString() ?? string.Empty);
                item.IconColor = row[3].ToString();
                if (row.Table.Columns.Count >= 5)
                {
                    item.Group = row[4]?.ToString();
                }
            }
            else
            {
                if (row.Table.Columns.Count >= 3)
                {
                    item.Group = row[2]?.ToString();
                }
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