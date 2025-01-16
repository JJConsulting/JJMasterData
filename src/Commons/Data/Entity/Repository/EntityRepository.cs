#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Providers;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Data.Entity.Repository;

internal sealed class EntityRepository(
    IOptionsSnapshot<MasterDataCommonsOptions> commonsOptions,
    EntityProviderBase provider)
    : IEntityRepository
{
    public int Update(Element element, Dictionary<string, object?> values)
    {
        return provider.Update(element, values);
    }

    public Task<int> DeleteAsync(Element element, Dictionary<string, object> filters)
    {
        return provider.DeleteAsync(element, filters);
    }

    public int Delete(Element element, Dictionary<string, object> primaryKeys)
    {
        return provider.Delete(element, primaryKeys);
    }

    public void Insert(Element element, Dictionary<string, object?> values)
    {
        provider.Insert(element, values);
    }

    public Task InsertAsync(Element element, Dictionary<string, object?> values)
    {
        return provider.InsertAsync(element, values);
    }
    
    public int BulkInsert(Element element, IEnumerable<Dictionary<string, object?>> values, Guid? connectionId = null)
    {
        var dataAccess = GetDataAccess(connectionId);

        var commandList = GetInsertCommandList(element, values);

        return dataAccess.SetCommand(commandList);
    }
    
    public Task<int> BulkInsertAsync(Element element, IEnumerable<Dictionary<string, object?>> valueList, Guid? connectionId = null)
    {
        var dataAccess = GetDataAccess(connectionId);

        var commandList = GetInsertCommandList(element, valueList);

        return dataAccess.SetCommandListAsync(commandList);
    }

    private IEnumerable<DataAccessCommand> GetInsertCommandList(Element element, IEnumerable<Dictionary<string, object?>> values)
    {
        foreach (var valuesDictionary in values)
        {
            var command = provider.GetInsertCommand(element, valuesDictionary);
            yield return command;
        }
    }

    public Task<int> UpdateAsync(Element element, Dictionary<string, object?> values)
    {
        return provider.UpdateAsync(element, values);
    }

    public Task<CommandOperation> SetValuesAsync(Element element, Dictionary<string, object?> values,
        bool ignoreResults = false)
    {
        return provider.SetValuesAsync(element, values, ignoreResults);
    }

    public CommandOperation SetValues(Element element, Dictionary<string, object?> values, bool ignoreResults = false)
    {
        return provider.SetValues(element, values, ignoreResults);
    }

    public Task<Element> GetElementFromTableAsync(string tableName, Guid? connectionId = null)
    {
        return provider.GetElementFromTableAsync(tableName, connectionId);
    }

    public Task<object?> GetResultAsync(DataAccessCommand command, Guid? connectionId = null)
    {
        var dataAccess = GetDataAccess(connectionId);
        return dataAccess.GetResultAsync(command);
    }

    public Task<bool> TableExistsAsync(string tableName, Guid? connectionId = null)
    {
        return provider.TableExistsAsync(tableName, connectionId);
    }

    public bool TableExists(string tableName, Guid? connectionId = null)
    {
        return provider.TableExists(tableName, connectionId);
    }

    public async Task SetCommandAsync(DataAccessCommand command, Guid? connectionId = null)
    {
        var dataAccess = GetDataAccess(connectionId);
        await dataAccess.SetCommandAsync(command);
    }

    public Task<int> SetCommandListAsync(IEnumerable<DataAccessCommand> commandList, Guid? connectionId = null)
    {
        var dataAccess = GetDataAccess(connectionId);
        return dataAccess.SetCommandListAsync(commandList);
    }

    private Task<bool> ColumnExistsAsync(string tableName, string columnName, Guid? connectionId = null)
    {
        return provider.ColumnExistsAsync(tableName, columnName, connectionId);
    }

    public Task<bool> ExecuteBatchAsync(string script, Guid? connectionId = null)
    {
        var dataAccess = GetDataAccess(connectionId);
        return dataAccess.ExecuteBatchAsync(script);
    }

    public Dictionary<string, object?> GetFields(Element element, Dictionary<string, object> primaryKeys)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        if (primaryKeys.Count == 0)
            throw new ArgumentException(@"Your need at least one value at your primary keys.", nameof(primaryKeys));

        var totalOfRecords =
            new DataAccessParameter("@qtdtotal", 1, DbType.Int32, 0, ParameterDirection.InputOutput);
        var cmd = provider.GetReadCommand(element, new EntityParameters
        {
            Filters = primaryKeys!
        }, totalOfRecords);

        return dataAccess.GetDictionary(cmd) ?? new Dictionary<string, object?>();
    }

    public Dictionary<string, object?> GetFields(DataAccessCommand command, Guid? connectionId = null)
    {
        var dataAccess = GetDataAccess(connectionId);
        return dataAccess.GetDictionary(command) ?? new Dictionary<string, object?>();
    }

    public Task<Dictionary<string, object?>> GetFieldsAsync(DataAccessCommand command, Guid? connectionId = null)
    {
        var dataAccess = GetDataAccess(connectionId);
        return dataAccess.GetDictionaryAsync(command);
    }

    public Task<Dictionary<string, object?>> GetFieldsAsync(Element element,
        Dictionary<string, object> primaryKeys)
    {
        if (primaryKeys.Count == 0)
            throw new ArgumentException(@"Your need at least one value at your primary keys.", nameof(primaryKeys));

        var totalOfRecords =
            new DataAccessParameter("@qtdtotal", 1, DbType.Int32, 0, ParameterDirection.InputOutput);
        var cmd = provider.GetReadCommand(element, new EntityParameters
        {
            Filters = primaryKeys!
        }, totalOfRecords);
        
        var dataAccess = GetDataAccess(element.ConnectionId);

        return dataAccess.GetDictionaryAsync(cmd);
    }

    public Task CreateDataModelAsync(Element element, List<RelationshipReference>? relationships = null)
    {
        return provider.CreateDataModelAsync(element, relationships);
    }

    public void CreateDataModel(Element element, List<RelationshipReference>? relationships = null)
    {
        provider.CreateDataModel(element, relationships);
    }

    ///<inheritdoc cref="IEntityRepository.GetCreateTableScript"/>
    public string GetCreateTableScript(Element element, List<RelationshipReference>? relationships = null)
    {
        return provider.GetCreateTableScript(element, relationships);
    }

    ///<inheritdoc cref="IEntityRepository.GetWriteProcedureScript"/>
    public string? GetWriteProcedureScript(Element element)
    {
        return provider.GetWriteProcedureScript(element);
    }

    public async Task<string?> GetAlterTableScriptAsync(Element element)
    {
        List<ElementField> addedFields = [];

        addedFields.AddRange(await GetAddedFieldsAsync(element));
        
        return provider.GetAlterTableScript(element, addedFields);
    }

    private async Task<List<ElementField>> GetAddedFieldsAsync(Element element)
    {
        List<ElementField> addedFields = [];

        if (!await TableExistsAsync(element.TableName, element.ConnectionId))
            return addedFields;

        foreach (var field in element.Fields.Where(f => f.DataBehavior == FieldBehavior.Real))
        {
            if (!await ColumnExistsAsync(element.TableName, field.Name, element.ConnectionId))
            {
                addedFields.Add(field);
            }
        }

        return addedFields;
    }

    public string? GetReadProcedureScript(Element element)
    {
        return provider.GetReadProcedureScript(element);
    }


    public Task<string> GetListFieldsAsTextAsync(Element element, EntityParameters? parameters = null,
        bool showLogInfo = false,
        string delimiter = "|")
    {
        return provider.GetFieldsListAsTextAsync(element, parameters ?? new EntityParameters(), showLogInfo, delimiter);
    }

    public List<Dictionary<string, object?>> GetDictionaryList(Element element, EntityParameters? parameters = null)
    {
        var result = provider.GetDictionaryList(element, parameters ?? new EntityParameters(), false);

        return result.Data;
    }

    public async Task<List<Dictionary<string, object?>>> GetDictionaryListAsync(
        Element element,
        EntityParameters? parameters = null
    )
    {
        var result = await provider.GetDictionaryListAsync(element, parameters ?? new EntityParameters(), false);

        return result.Data;
    }

    public DataTable GetDataTable(DataAccessCommand dataAccessCommand, Guid? connectionId = null)
    {
        var dataAccess = GetDataAccess(connectionId);
        return dataAccess.GetDataTable(dataAccessCommand);
    }

    public Task<DataTable> GetDataTableAsync(DataAccessCommand dataAccessCommand, Guid? connectionId = null)
    {
        var dataAccess = GetDataAccess(connectionId);
        return dataAccess.GetDataTableAsync(dataAccessCommand);
    }

    public Task<DataTable> GetDataTableAsync(Element element, EntityParameters? parameters = null)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        var totalOfRecords =
            new DataAccessParameter("@qtdtotal", 1, DbType.Int32, 0, ParameterDirection.InputOutput);
        var command = provider.GetReadCommand(element, parameters ?? new EntityParameters(), totalOfRecords);

        return dataAccess.GetDataTableAsync(command);
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

    public Task<List<Dictionary<string, object?>>> GetDictionaryListAsync(DataAccessCommand command, Guid? connectionId = null)
    {
        var dataAccess = GetDataAccess(connectionId);
        return dataAccess.GetDictionaryListAsync(command);
    }

    public async Task<DictionaryListResult> GetDictionaryListResultAsync(
        Element element,
        EntityParameters? parameters = null,
        bool recoverTotalOfRecords = true
    )
    {
        var result =
            await provider.GetDictionaryListAsync(element, parameters ?? new EntityParameters(), recoverTotalOfRecords);

        return new DictionaryListResult(result.Data, result.TotalOfRecords);
    }

    public DictionaryListResult GetDictionaryListResult(
        Element element,
        EntityParameters? parameters = null,
        bool recoverTotalOfRecords = true
    )
    {
        var result = provider.GetDictionaryList(element, parameters ?? new EntityParameters(), recoverTotalOfRecords);

        return new DictionaryListResult(result.Data, result.TotalOfRecords);
    }
    
    public DataSet GetDataSet(DataAccessCommand command, Guid? connectionId = null)
    {
        var dataAccess = GetDataAccess(connectionId);
        return dataAccess.GetDataSet(command);
    }
    
    public Task<DataSet> GetDataSetAsync(DataAccessCommand command, Guid? connectionId = null, CancellationToken cancellationToken = default)
    {
        var dataAccess = GetDataAccess(connectionId);
        return dataAccess.GetDataSetAsync(command, cancellationToken);
    }
    public Task<string?> GetStoredProcedureDefinitionAsync(string procedureName, Guid? connectionId = null)
    {
        return provider.GetStoredProcedureDefinitionAsync(procedureName,connectionId);
    }

    public Task DropStoredProcedureAsync(string procedureName, Guid? connectionId = null)
    {
        return provider.DropStoredProcedureAsync(procedureName,connectionId);
    }
    
    public Task<List<string>> GetStoredProcedureListAsync(Guid? connectionId = null)
    {
        return provider.GetStoredProcedureListAsync(connectionId);
    }
    
    private DataAccess GetDataAccess(Guid? connectionId)
    {
        var connection = commonsOptions.Value.GetConnectionString(connectionId);
        return new DataAccess(connection.Connection, connection.ConnectionProvider);
    }
}