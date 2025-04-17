#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Data.Entity.Providers;

public abstract class EntityProviderBase(
    IOptionsSnapshot<MasterDataCommonsOptions> options,
    ILoggerFactory loggerFactory)
{
    protected internal MasterDataCommonsOptions Options { get; } = options.Value;

    public abstract string VariablePrefix { get; }
    public abstract string GetCreateTableScript(Element element, List<RelationshipReference>? relationships = null);
    public abstract string? GetWriteProcedureScript(Element element);
    public abstract string? GetReadProcedureScript(Element element);
    public abstract string? GetAlterTableScript(Element element, IEnumerable<ElementField> addedFields);
    public abstract Task<Element> GetElementFromTableAsync(string tableName, Guid? connectionId = null);
    public abstract Task<Element> GetElementFromTableAsync(string schemaName, string connectionId, Guid? guid);
    
    public abstract DataAccessCommand GetInsertCommand(Element element, Dictionary<string,object?> values);
    public abstract DataAccessCommand GetUpdateCommand(Element element, Dictionary<string,object?> values);
    public abstract DataAccessCommand GetDeleteCommand(Element element, Dictionary<string,object> primaryKeys);
    public abstract DataAccessCommand GetReadCommand(Element element, EntityParameters parameters, DataAccessParameter totalOfRecordsParameter);
    protected internal abstract DataAccessCommand GetInsertOrReplaceCommand(Element element, Dictionary<string,object?> values);
    public abstract bool TableExists(string tableName, Guid? connectionId = null);
    
    public abstract Task<bool> TableExistsAsync(string schema, string tableName, Guid? connectionId = null, CancellationToken cancellationToken = default);
    
    public abstract Task<bool> TableExistsAsync(string tableName, Guid? connectionId = null, CancellationToken cancellationToken = default);

    public abstract Task<bool> ColumnExistsAsync(
        string tableName, 
        string columnName,
        Guid? connectionId = null,
        CancellationToken cancellationToken = default);
    
    public abstract Task<string?> GetStoredProcedureDefinitionAsync(string procedureName, Guid? connectionId = null);
    public abstract Task DropStoredProcedureAsync(string procedureName, Guid? connectionId = null);
    public abstract Task<List<string>> GetStoredProcedureListAsync(Guid? connectionId = null);
    
    public async Task InsertAsync(Element element, Dictionary<string,object?> values)
    {
        var command = GetInsertCommand(element, values);
        var dataAccess = GetDataAccess(element.ConnectionId);
        var newFields = await dataAccess.GetDictionaryAsync(command);

        foreach (var entry in newFields)
        {
            if (element.Fields.ContainsKey(entry.Key))
            {
                values[entry.Key] = entry.Value;
            }
        }
    }
    
    public  void Insert(Element element, Dictionary<string,object?> values)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        var command = GetInsertCommand(element, values);
        var newFields =  dataAccess.GetDictionary(command) ?? new Dictionary<string, object?>();

        foreach (var entry in newFields)
        {
            if (element.Fields.ContainsKey(entry.Key))
            {
                values[entry.Key] = entry.Value;
            }
        }
    }
    
    public int Update(Element element, Dictionary<string, object?> values)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        var cmd = GetUpdateCommand(element, values);
        int numberRowsAffected = dataAccess.SetCommand(cmd);
        return numberRowsAffected;
    }
    
    public async Task<int> UpdateAsync(Element element, Dictionary<string,object?> values)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        var cmd = GetUpdateCommand(element, values);
        int numberRowsAffected = await dataAccess.SetCommandAsync(cmd);
        return numberRowsAffected;
    }
    
    public async Task<CommandOperation> SetValuesAsync(Element element, Dictionary<string,object?> values)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        const CommandOperation commandType = CommandOperation.None;
        var command = GetInsertOrReplaceCommand(element, values);
        var newFields = await dataAccess.GetDictionaryAsync(command);

        return GetCommandOperation(element, values, command, commandType, newFields);
    }
    
    public CommandOperation SetValues(Element element, Dictionary<string, object?> values, bool ignoreResults)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        const CommandOperation commandType = CommandOperation.None;
        var command = GetInsertOrReplaceCommand(element, values);
        var newFields =  dataAccess.GetDictionary(command);

        return GetCommandOperation(element, values, command, commandType, newFields);
    }
    
    private static CommandOperation GetCommandOperation(
        Element element, 
        Dictionary<string,object?> values,
        DataAccessCommand command,
        CommandOperation commandType,
        Dictionary<string, object?>? newFields)
    {
        var resultParameter = command.Parameters.First(x => x.Name.Equals("@RET"));

        if (resultParameter.Value != DBNull.Value)
        {
            if (!int.TryParse(resultParameter.Value.ToString(), out var commandOperation))
            {
                var err = "Element";
                err += $" {element.Name}";
                err += ": Invalid return of @RET variable in procedure";
                throw new JJMasterDataException(err);
            }

            commandType = (CommandOperation)commandOperation;
        }

        if (newFields == null)
            return commandType;

        foreach (var entry in newFields)
        {
            if (element.Fields.ContainsKey(entry.Key))
            {
                values[entry.Key] = entry.Value;
            }
        }

        return commandType;
    }
    
    public Task<CommandOperation> SetValuesAsync(Element element, Dictionary<string,object?> values, bool ignoreResults)
    {
        if (ignoreResults)
            return SetValuesNoResultAsync(element, values);

        return SetValuesAsync(element, values);
    }
    
    
    public int Delete(Element element, Dictionary<string, object> primaryKeys)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        var cmd = GetDeleteCommand(element, primaryKeys);
        int numberRowsAffected = dataAccess.SetCommand(cmd);
        return numberRowsAffected;
    }
    
    public async Task<int> DeleteAsync(Element element, Dictionary<string,object> primaryKeys)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        var cmd = GetDeleteCommand(element, primaryKeys);
        int numberRowsAffected = await dataAccess.SetCommandAsync(cmd);
        return numberRowsAffected;
    }
    
    
    public async Task<DictionaryListResult> GetDictionaryListAsync(
        Element element,
        EntityParameters entityParameters,
        bool recoverTotalOfRecords = true)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (!entityParameters.OrderBy.Validate(element.Fields))
            throw new ArgumentException("[order by] clause is not valid");

        var totalParameter = new DataAccessParameter($"{VariablePrefix}qtdtotal", recoverTotalOfRecords ? 0 : -1, DbType.Int32, 0, ParameterDirection.InputOutput);
        
        var dataAccess = GetDataAccess(element.ConnectionId);
        
        var command = GetReadCommand(element, entityParameters, totalParameter);
        
        var list = await dataAccess.GetDictionaryListAsync(command);

        int totalRecords = 0;
        
        if (totalParameter is { Value: not null } && totalParameter.Value != DBNull.Value)
            totalRecords = (int)totalParameter.Value;

        return new DictionaryListResult(list, totalRecords);
    }
    
    public void CreateDataModel(Element element, List<RelationshipReference>? relationships = null)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        var sqlScripts = GetDataModelScripts(element, relationships);
        dataAccess.ExecuteBatch(sqlScripts);
    }
    
    public async Task CreateDataModelAsync(Element element, List<RelationshipReference>? relationships = null)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        var sqlScripts = GetDataModelScripts(element, relationships);
        await dataAccess.ExecuteBatchAsync(sqlScripts);
    }

    private string GetDataModelScripts(Element element, List<RelationshipReference>? relationships = null)
    {
        var sqlScripts = new StringBuilder();
        sqlScripts.AppendLine(GetCreateTableScript(element, relationships));
        sqlScripts.AppendLine("GO");
        
        if (element.UseReadProcedure)
        {   
            sqlScripts.AppendLine(GetReadProcedureScript(element));
            sqlScripts.AppendLine("GO");
        }

        if (element.UseWriteProcedure)
        {
            sqlScripts.AppendLine(GetWriteProcedureScript(element));
            sqlScripts.AppendLine("GO");
        }

        return sqlScripts.ToString();
    }
    
    public Task<string> GetFieldsListAsTextAsync(Element element, EntityParameters entityParameters,
        bool showLogInfo, string delimiter = "|")
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (!entityParameters.OrderBy.Validate(element.Fields))
            throw new ArgumentException("[order by] clause is not valid");

        var plainTextWriter = new PlainTextReader(this, loggerFactory.CreateLogger<PlainTextReader>())
        {
            ShowLogInfo = showLogInfo,
            Delimiter = delimiter
        };

        return plainTextWriter.GetFieldsListAsTextAsync(element, entityParameters);
    }
    
    private async Task<CommandOperation> SetValuesNoResultAsync(Element element, Dictionary<string,object?> values)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        const CommandOperation result = CommandOperation.None;
        var command = GetInsertOrReplaceCommand(element, values);
        await dataAccess.SetCommandAsync(command);

        return GetCommandFromValuesNoResult(element, command, result);
    }
    
    private static CommandOperation GetCommandFromValuesNoResult(Element element, DataAccessCommand command, CommandOperation ret)
    {
        var retParameter = command.Parameters.First(x => x.Name.Equals("@RET"));
        if (retParameter.Value != DBNull.Value)
        {
            if (!int.TryParse(retParameter.Value.ToString(), out var result))
            {
                string err = "Element";
                err += $" {element.Name}";
                err += ": Invalid return of @RET variable in procedure";
                throw new JJMasterDataException(err);
            }

            ret = (CommandOperation)result;
        }

        return ret;
    }

    public DictionaryListResult GetDictionaryList(
        Element element,
        EntityParameters entityParameters,
        bool recoverTotalOfRecords = true)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (!entityParameters.OrderBy.Validate(element.Fields))
            throw new ArgumentException("[order by] clause is not valid");

        var totalParameter = new DataAccessParameter($"{VariablePrefix}qtdtotal", recoverTotalOfRecords ? 0 : -1, DbType.Int32, 0, ParameterDirection.InputOutput);
        
        var command = GetReadCommand(element, entityParameters, totalParameter);

        var dataAccess = GetDataAccess(element.ConnectionId);
        
        var list =  dataAccess.GetDictionaryList(command);

        int totalRecords = 0;
        
        if (totalParameter is { Value: not null } && totalParameter.Value != DBNull.Value)
            totalRecords = (int)totalParameter.Value;

        return new DictionaryListResult(list, totalRecords);
    }
    
    internal DataAccess GetDataAccess(Guid? connectionId)
    {
        var connection = Options.GetConnectionString(connectionId);
        return new DataAccess(connection.Connection, connection.ConnectionProvider);
    }
}
