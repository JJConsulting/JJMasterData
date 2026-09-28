#nullable disable warnings
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using JJConsulting.FontAwesome;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Logging;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.Services;

public class HierarchyService(
    IEntityRepository entityRepository,
    ExpressionParser expressionParser,
    ElementMapService elementMapService,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    ILogger<HierarchyService> logger)
{
    public async Task<List<DataItemValue>> GetValuesAsync(
        FormElementDataItem dataItem,
        DataQuery dataQuery)
    {
        var values = dataItem.DataItemType switch
        {
            DataItemType.Manual => GetManualValues(dataItem, dataQuery),
            DataItemType.SqlCommand => await GetSqlValuesAsync(dataItem, dataQuery),
            DataItemType.ElementMap => await GetElementMapValuesAsync(dataItem, dataQuery),
            _ => throw new JJMasterDataException("Invalid DataItemType.")
        };

        if (dataItem.EnableLocalization)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrEmpty(value.Description))
                    value.Description = stringLocalizer[value.Description];
            }
        }

        return values;
    }

    public async Task<List<DataItemValue>> GetPathAsync(
        FormElementDataItem dataItem,
        DataQuery dataQuery,
        int maximumDepth = 100)
    {
        if (string.IsNullOrEmpty(dataQuery.SearchId))
            return [];

        var path = new List<DataItemValue>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var currentId = dataQuery.SearchId;

        while (!string.IsNullOrEmpty(currentId) && path.Count < maximumDepth)
        {
            if (!visited.Add(currentId))
                throw new JJMasterDataException("A cycle was found while resolving the hierarchy path.");

            var values = await GetValuesAsync(dataItem,
                new DataQuery(dataQuery.FormStateData, dataQuery.ConnectionId) { SearchId = currentId });
            var current = values.FirstOrDefault(value =>
                value.Id.Equals(currentId, StringComparison.OrdinalIgnoreCase));
            if (current is null)
                return [];

            path.Insert(0, current);
            currentId = current.ParentId;
        }

        if (!string.IsNullOrEmpty(currentId))
            throw new JJMasterDataException($"The hierarchy path exceeds the maximum depth of {maximumDepth}.");

        return path;
    }

    private static List<DataItemValue> GetManualValues(FormElementDataItem dataItem, DataQuery dataQuery)
    {
        var items = dataItem.Items ?? [];
        return (!string.IsNullOrEmpty(dataQuery.SearchId)
            ? items.Where(item => item.Id.Equals(dataQuery.SearchId, StringComparison.OrdinalIgnoreCase))
            : items.Where(item => ParentIdsEqual(item.ParentId, dataQuery.ParentId))).ToList();
    }

    private async Task<List<DataItemValue>> GetElementMapValuesAsync(
        FormElementDataItem dataItem,
        DataQuery dataQuery)
    {
        var elementMap = dataItem.ElementMap
                         ?? throw new JJMasterDataException("Hierarchy ElementMap is not defined.");
        if (string.IsNullOrEmpty(elementMap.ParentIdFieldName))
            throw new JJMasterDataException("Hierarchy ElementMap requires a ParentId field.");

        var values = await elementMapService.GetHierarchyDictionaryList(
            elementMap,
            dataQuery.SearchId,
            dataQuery.ParentId,
            dataQuery.FormStateData);
        var result = new List<DataItemValue>();

        foreach (var value in values)
        {
            var item = new DataItemValue
            {
                Id = value[elementMap.IdFieldName]?.ToString() ?? string.Empty,
                Description = elementMap.DescriptionFieldName is null
                    ? null
                    : value[elementMap.DescriptionFieldName]?.ToString(),
                ParentId = value[elementMap.ParentIdFieldName]?.ToString()
            };

            if (dataItem.ShowIcon)
            {
                if (elementMap.IconIdFieldName is not null &&
                    int.TryParse(value[elementMap.IconIdFieldName]?.ToString(), out var icon))
                    item.Icon = (FontAwesomeIcon)icon;
                if (elementMap.IconColorFieldName is not null)
                    item.IconColor = value[elementMap.IconColorFieldName]?.ToString();
            }

            if ((!string.IsNullOrEmpty(dataQuery.SearchId) &&
                 item.Id.Equals(dataQuery.SearchId, StringComparison.OrdinalIgnoreCase)) ||
                (string.IsNullOrEmpty(dataQuery.SearchId) && ParentIdsEqual(item.ParentId, dataQuery.ParentId)))
                result.Add(item);
        }

        return result;
    }

    private async Task<List<DataItemValue>> GetSqlValuesAsync(
        FormElementDataItem dataItem,
        DataQuery dataQuery)
    {
        var command = GetDataItemCommand(dataItem, dataQuery);
        DataTable dataTable;
        using (logger.BeginCommandScope(command))
        {
            dataTable = await entityRepository.GetDataTableAsync(command, dataQuery.ConnectionId);
        }

        if (dataTable.Columns.Count < 3)
            throw new JJMasterDataException("Hierarchy SQL must return Id, Description and ParentId.");

        var result = new List<DataItemValue>(dataTable.Rows.Count);
        foreach (DataRow row in dataTable.Rows)
        {
            var item = new DataItemValue
            {
                Id = row[0].ToString() ?? string.Empty,
                Description = row[1].ToString(),
                ParentId = row.IsNull(2) ? null : row[2].ToString()
            };

            if (dataItem.ShowIcon && dataTable.Columns.Count >= 5)
            {
                if (int.TryParse(row[3].ToString(), out var icon))
                    item.Icon = (FontAwesomeIcon)icon;
                item.IconColor = row[4].ToString();
            }

            result.Add(item);
        }

        return result;
    }

    private DataAccessCommand GetDataItemCommand(FormElementDataItem dataItem, DataQuery dataQuery)
    {
        var sql = dataItem.Command?.Sql
                  ?? throw new JJMasterDataException("Hierarchy SQL command is not defined.");
        var parsedValues = expressionParser.ParseExpression(sql, dataQuery.FormStateData);
        parsedValues["SearchId"] = dataQuery.SearchId;
        parsedValues["ParentId"] = dataQuery.ParentId;
        return ExpressionDataAccessCommandFactory.Create(sql, parsedValues);
    }

    private static bool ParentIdsEqual(string? left, string? right) =>
        string.Equals(left?.Trim() ?? string.Empty, right?.Trim() ?? string.Empty,
            StringComparison.OrdinalIgnoreCase);
}
