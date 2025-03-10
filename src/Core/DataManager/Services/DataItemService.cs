#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Extensions;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.Services;

public class DataItemService(
    IEntityRepository entityRepository,
    ExpressionParser expressionParser,
    ElementMapService elementMapService,
    ILogger<DataItemService> logger)
{
    public async Task<List<DataItemValue>> GetValuesAsync(
        FormElementDataItem dataItem,
        DataQuery dataQuery)
    {
        var dataItemType = GetDataItemType(dataItem);

        return dataItemType switch
        {
            DataItemType.Manual => GetItemsValues(dataItem, dataQuery.SearchId, dataQuery.SearchText).ToList(),
            DataItemType.SqlCommand => await GetSqlCommandValues(dataItem, dataQuery),
            DataItemType.ElementMap => await GetElementMapValues(dataItem, dataQuery),
            _ => throw new JJMasterDataException("Invalid DataItemType.")
        };
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
        if (dataItem.Items == null)
            yield break;
        
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
                if (item.Description?.Contains(searchText, StringComparison.InvariantCultureIgnoreCase) is true)
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

    private async Task<List<DataItemValue>> GetElementMapValues(FormElementDataItem dataItem, DataQuery dataQuery)
    {
        var formStateData = dataQuery.FormStateData;
        var searchId = dataQuery.SearchId;
        var searchText = dataQuery.SearchText;
        
        var elementMap = dataItem.ElementMap;
        var values = await elementMapService.GetDictionaryList(elementMap!, searchId, formStateData);

        List<DataItemValue> result = [];
        
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

            if (searchText == null || item.Description?.Contains(searchText, StringComparison.OrdinalIgnoreCase) is true)
            {
                result.Add(item);
            }
        }

        return result;
    }

    private async Task<List<DataItemValue>> GetSqlCommandValues(
        FormElementDataItem dataItem,
        DataQuery dataQuery)
    {
        var command = GetDataItemCommand(dataItem, dataQuery);
        
        DataTable dataTable;
        
        try
        {
             dataTable = await entityRepository.GetDataTableAsync(command, dataQuery.ConnectionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error recovering SQL DataItem.");
            throw;
        }
        
        var searchText = dataQuery.SearchText;
        
        List<DataItemValue> result = [];
        
        foreach (DataRow row in dataTable.Rows)
        {
            var item = new DataItemValue
            {
                Id = row[0].ToString()
            };

            if (row.Table.Columns.Count == 1)
            {
                item.Description = item.Id;
            }
            else
            {
                item.Description = row[1].ToString()!;
                
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
            }

            if (searchText == null || item.Description?.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) is true)
            {
                result.Add(item);
            }
        }

        return result;
    }

    private DataAccessCommand GetDataItemCommand(FormElementDataItem dataItem, DataQuery dataQuery)
    {
        var sql = dataItem.Command!.Sql;
        var parsedValues = expressionParser.ParseExpression(sql, dataQuery.FormStateData);
        
        if (dataQuery.SearchId != null)
            parsedValues["SearchId"] = dataQuery.SearchId;

        if (dataQuery.SearchText != null)
            parsedValues["SearchText"] = dataQuery.SearchText;
        
        return ExpressionDataAccessCommandFactory.Create(sql, parsedValues);
    }
}