﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Providers;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;

namespace JJMasterData.Commons.Data.Entity.Repository;

public class EntityRepository(DataAccess dataAccess, EntityProviderBase provider)
    : IEntityRepository
{
    private DataAccess DataAccess { get; } = dataAccess;
    private EntityProviderBase Provider { get; } = provider;

    public int Update(Element element, Dictionary<string, object?> values)
    {
        return Provider.Update(element, values);
    }

    public Task<int> DeleteAsync(Element element, Dictionary<string, object> filters) =>
        Provider.DeleteAsync(element, filters);

    public int Delete(Element element, Dictionary<string, object> primaryKeys)
    {
        return Provider.Delete(element, primaryKeys);
    }

    public void Insert(Element element, Dictionary<string, object?> values)
    {
        Provider.Insert(element, values);
    }

    public Task InsertAsync(Element element, Dictionary<string, object?> values) =>
        Provider.InsertAsync(element, values);

    public Task<int> UpdateAsync(Element element, Dictionary<string, object?> values) =>
        Provider.UpdateAsync(element, values);

    public Task<CommandOperation> SetValuesAsync(Element element, Dictionary<string, object?> values,
        bool ignoreResults = false) =>
        Provider.SetValuesAsync(element, values, ignoreResults);

    public CommandOperation SetValues(Element element, Dictionary<string, object?> values, bool ignoreResults = false)
    {
        return Provider.SetValues(element, values, ignoreResults);
    }

    public Task<Element> GetElementFromTableAsync(string tableName) => Provider.GetElementFromTableAsync(tableName);

    public Task<object?> GetResultAsync(DataAccessCommand command)
    {
        return DataAccess.GetResultAsync(command);
    }

    public Task<bool> TableExistsAsync(string tableName) => DataAccess.TableExistsAsync(tableName);

    public bool TableExists(string tableName) => DataAccess.TableExists(tableName);

    public async Task SetCommandAsync(DataAccessCommand command)
    {
        await DataAccess.SetCommandAsync(command);
    }

    public Task<int> SetCommandListAsync(IEnumerable<DataAccessCommand> commandList) =>
        DataAccess.SetCommandListAsync(commandList);

    public Task<bool> ColumnExistsAsync(string tableName, string columnName) =>
        DataAccess.ColumnExistsAsync(tableName, columnName);


    public Task<bool> ExecuteBatchAsync(string script) => DataAccess.ExecuteBatchAsync(script);

    public Dictionary<string, object?> GetFields(Element element, Dictionary<string, object> primaryKeys)
    {
        if (!primaryKeys.Any())
            throw new ArgumentException("Your need at least one value at your primary keys.", nameof(primaryKeys));

        var totalOfRecords =
            new DataAccessParameter("@qtdtotal", 1, DbType.Int32, 0, ParameterDirection.InputOutput);
        var cmd = Provider.GetReadCommand(element, new EntityParameters
        {
            Filters = primaryKeys!
        }, totalOfRecords);

        return DataAccess.GetDictionary(cmd) ?? new Dictionary<string, object?>();
    }

    public Dictionary<string, object?> GetFields(DataAccessCommand command)
    {
        return DataAccess.GetDictionary(command) ?? new Dictionary<string, object?>();
    }

    public async Task<Dictionary<string, object?>> GetFieldsAsync(DataAccessCommand command)
    {
        return await DataAccess.GetDictionaryAsync(command);
    }

    public async Task<Dictionary<string, object?>> GetFieldsAsync(Element element,
        Dictionary<string, object> primaryKeys)
    {
        if (!primaryKeys.Any())
            throw new ArgumentException("Your need at least one value at your primary keys.", nameof(primaryKeys));

        var totalOfRecords =
            new DataAccessParameter("@qtdtotal", 1, DbType.Int32, 0, ParameterDirection.InputOutput);
        var cmd = Provider.GetReadCommand(element, new EntityParameters
        {
            Filters = primaryKeys!
        }, totalOfRecords);

        return await DataAccess.GetDictionaryAsync(cmd);
    }

    public Task CreateDataModelAsync(Element element, List<RelationshipReference>? relationships = null) =>
        Provider.CreateDataModelAsync(element, relationships);

    public void CreateDataModel(Element element, List<RelationshipReference>? relationships = null)
    {
        Provider.CreateDataModel(element, relationships);
    }

    ///<inheritdoc cref="IEntityRepository.GetCreateTableScript"/>
    public string GetCreateTableScript(Element element, List<RelationshipReference>? relationships = null) =>
        Provider.GetCreateTableScript(element, relationships);

    ///<inheritdoc cref="IEntityRepository.GetWriteProcedureScript"/>
    public string? GetWriteProcedureScript(Element element) => Provider.GetWriteProcedureScript(element);

    public async Task<string> GetAlterTableScriptAsync(Element element)
    {
        var addedFields = await GetAddedFieldsAsync(element).ToListAsync();
        return Provider.GetAlterTableScript(element, addedFields);
    }

    private async IAsyncEnumerable<ElementField> GetAddedFieldsAsync(Element element)
    {
        if (!await TableExistsAsync(element.TableName))
            yield break;

        foreach (var field in element.Fields.Where(f => f.DataBehavior == FieldBehavior.Real))
        {
            if (!await ColumnExistsAsync(element.TableName, field.Name))
            {
                yield return field;
            }
        }
    }

    public string? GetReadProcedureScript(Element element) => Provider.GetReadProcedureScript(element);


    public Task<string> GetListFieldsAsTextAsync(Element element, EntityParameters? parameters = null,
        bool showLogInfo = false,
        string delimiter = "|")
    {
        return Provider.GetFieldsListAsTextAsync(element, parameters ?? new EntityParameters(), showLogInfo, delimiter);
    }

    public List<Dictionary<string, object?>> GetDictionaryList(Element element, EntityParameters? parameters = null)
    {
        var result = Provider.GetDictionaryList(element, parameters ?? new EntityParameters(), false);

        return result.Data;
    }

    public async Task<List<Dictionary<string, object?>>> GetDictionaryListAsync(
        Element element,
        EntityParameters? parameters = null
    )
    {
        var result = await Provider.GetDictionaryListAsync(element, parameters ?? new EntityParameters(), false);

        return result.Data;
    }

    public DataTable GetDataTable(DataAccessCommand dataAccessCommand)
    {
        return DataAccess.GetDataTable(dataAccessCommand);
    }

    public Task<DataTable> GetDataTableAsync(DataAccessCommand dataAccessCommand)
    {
        return DataAccess.GetDataTableAsync(dataAccessCommand);
    }

    public Task<DataTable> GetDataTableAsync(Element element, EntityParameters? parameters = null)
    {
        var totalOfRecords =
            new DataAccessParameter("@qtdtotal", 1, DbType.Int32, 0, ParameterDirection.InputOutput);
        var command = Provider.GetReadCommand(element, parameters ?? new EntityParameters(), totalOfRecords);

        return DataAccess.GetDataTableAsync(command);
    }

    public int GetCount(Element element, Dictionary<string, object?> values)
    {
        var result = GetDictionaryListResult(element, new EntityParameters
        {
            Filters = values
        });

        return result.Count;
    }

    public async Task<int> GetCountAsync(Element element, Dictionary<string, object?> filters)
    {
        var result = await GetDictionaryListResultAsync(element, new EntityParameters
        {
            Filters = filters
        });

        return result.Count;
    }

    public Task<List<Dictionary<string, object?>>> GetDictionaryListAsync(DataAccessCommand command)
    {
        return DataAccess.GetDictionaryListAsync(command);
    }

    public async Task<DictionaryListResult> GetDictionaryListResultAsync(
        Element element,
        EntityParameters? parameters = null,
        bool recoverTotalOfRecords = true
    )
    {
        var result =
            await Provider.GetDictionaryListAsync(element, parameters ?? new EntityParameters(), recoverTotalOfRecords);

        return new DictionaryListResult(result.Data, result.TotalOfRecords);
    }

    public DictionaryListResult GetDictionaryListResult(
        Element element,
        EntityParameters? parameters = null,
        bool recoverTotalOfRecords = true
    )
    {
        var result = Provider.GetDictionaryList(element, parameters ?? new EntityParameters(), recoverTotalOfRecords);

        return new DictionaryListResult(result.Data, result.TotalOfRecords);
    }
}