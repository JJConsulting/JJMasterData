#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Providers;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Logging;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Data.Entity.Repository;

internal sealed class EntityRepository(
    IEntityProvider provider,
    ILoggerFactory loggerFactory,
    IConnectionRepository connectionRepository
    ) : IEntityRepository
{
    private readonly ILogger<EntityRepository> _logger = loggerFactory.CreateLogger<EntityRepository>();

    public void Insert(Element element, Dictionary<string, object?> values)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        var command = provider.GetInsertCommand(element, values);

        using (_logger.BeginCommandScope(command))
        {
            _logger.LogInformation("Executing insert command for element {element}.", element.Name);
            var newFields = dataAccess.GetDictionary(command) ?? new Dictionary<string, object?>();

            foreach (var entry in newFields)
            {
                if (element.Fields.ContainsKey(entry.Key))
                {
                    values[entry.Key] = entry.Value;
                }
            }
        }
    }

    public async Task InsertAsync(Element element, Dictionary<string, object?> values)
    {
        var command = provider.GetInsertCommand(element, values);

        using (_logger.BeginCommandScope(command))
        {
            var dataAccess = GetDataAccess(element.ConnectionId);
            _logger.LogInformation("Executing insert command for element {element}.", element.Name);
            var newFields = await dataAccess.GetDictionaryAsync(command);

            foreach (var entry in newFields)
            {
                if (element.Fields.ContainsKey(entry.Key))
                {
                    values[entry.Key] = entry.Value;
                }
            }
        }
    }

    public int Insert(Element element, IEnumerable<Dictionary<string, object?>> values)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        var commandList = GetInsertCommandList(element, values);

        using (_logger.BeginCommandListScope(commandList))
        {
            _logger.LogInformation("Executing insert in batch for element {element}.", element.Name);
            return dataAccess.SetCommand(commandList);
        }
    }

    public async Task<int> InsertAsync(Element element, IEnumerable<Dictionary<string, object?>> values)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        var commandList = GetInsertCommandList(element, values);

        using (_logger.BeginCommandListScope(commandList))
        {
            _logger.LogInformation("Executing insert in batch for element {element}.", element.Name);
            return await dataAccess.SetCommandListAsync(commandList);
        }
    }
    
    public int Update(Element element, Dictionary<string, object?> values)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        var command = provider.GetUpdateCommand(element, values);

        using (_logger.BeginCommandScope(command))
        {
            _logger.LogInformation("Executing update command for element {element}.", element.Name);
            int numberRowsAffected = dataAccess.SetCommand(command);
            return numberRowsAffected;
        }
    }
    
    public async Task<int> UpdateAsync(Element element, Dictionary<string, object?> values)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        var command = provider.GetUpdateCommand(element, values);
        using (_logger.BeginCommandScope(command))
        {
            _logger.LogInformation("Executing update command for element {element}.", element.Name);
            int numberRowsAffected = await dataAccess.SetCommandAsync(command);
            return numberRowsAffected;
        }
    }

    public async Task<int> DeleteAsync(Element element, Dictionary<string, object> filters)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        var command = provider.GetDeleteCommand(element, filters);

        using (_logger.BeginCommandScope(command))
        {
            _logger.LogInformation("Executing delete command for element {element}.", element.Name);
            int numberRowsAffected = await dataAccess.SetCommandAsync(command);
            return numberRowsAffected;
        }
    }

    public int Delete(Element element, Dictionary<string, object> primaryKeys)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        var command = provider.GetDeleteCommand(element, primaryKeys);

        using (_logger.BeginCommandScope(command))
        {
            _logger.LogInformation("Executing delete command for element {element}.", element.Name);
            int numberRowsAffected = dataAccess.SetCommand(command);
            return numberRowsAffected;
        }
    }

    private List<DataAccessCommand> GetInsertCommandList(Element element, IEnumerable<Dictionary<string, object?>> values)
    {
        var commands = new List<DataAccessCommand>();
        
        foreach (Dictionary<string, object?> value in values) 
            commands.Add(provider.GetInsertCommand(element, value));

        return commands;
    }

    private List<DataAccessCommand> GetSetValuesCommands(Element element, IEnumerable<Dictionary<string, object?>> values)
    {
        var commands = new List<DataAccessCommand>();
        
        foreach (var value in values)
        {
            commands.Add(provider.GetInsertOrReplaceCommand(element, value));
        }
        
        return commands;
    }

    public Task<CommandOperation> SetValuesAsync(Element element, Dictionary<string, object?> values,
        bool ignoreResults = false)
    {
        if (ignoreResults)
            return SetValuesNoResultAsync(element, values);

        return SetValuesCoreAsync(element, values);
    }

    private async Task<CommandOperation> SetValuesCoreAsync(Element element, Dictionary<string,object?> values)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        const CommandOperation commandType = CommandOperation.None;
        var command = provider.GetInsertOrReplaceCommand(element, values);

        using (_logger.BeginCommandScope(command))
        {
            _logger.LogInformation("Executing SET command for element {element}.", element.Name);
            var newFields = await dataAccess.GetDictionaryAsync(command);
            return GetCommandOperation(element, values, command, commandType, newFields);
        }
    }

    public async Task<CommandOperation> SetValuesNoResultAsync(Element element, Dictionary<string,object?> values)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        const CommandOperation result = CommandOperation.None;
        var command = provider.GetInsertOrReplaceCommand(element, values);

        using (_logger.BeginCommandScope(command))
        {
            _logger.LogInformation("Executing SET command for element {element}.", element.Name);
            await dataAccess.SetCommandAsync(command);
            return GetCommandFromValuesNoResult(element, command, result);
        }
    }

    private static CommandOperation GetCommandFromValuesNoResult(Element element, DataAccessCommand command, CommandOperation ret)
    {
        var retParameter = command.Parameters.First(x => x.Name.Equals("@RET"));
        if (retParameter.Value != DBNull.Value)
        {
            if (!int.TryParse(retParameter.Value.ToString(), out var result))
            {
                var errorMessage = $"Element {element.Name}: Invalid return of @RET variable in procedure";
                throw new JJMasterDataException(errorMessage);
            }

            ret = (CommandOperation)result;
        }

        return ret;
    }

    public CommandOperation SetValues(Element element, Dictionary<string, object?> values, bool ignoreResults = false)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        const CommandOperation commandType = CommandOperation.None;
        var command = provider.GetInsertOrReplaceCommand(element, values);

        using (_logger.BeginCommandScope(command))
        {
            _logger.LogInformation("Executing SET command for element {element}.", element.Name);
            var newFields = dataAccess.GetDictionary(command);
            return GetCommandOperation(element, values, command, commandType, newFields);
        }
    }

    public async Task SetValuesAsync(Element element, IEnumerable<Dictionary<string, object?>> values)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        
        var commandList = GetSetValuesCommands(element, values);

        using (_logger.BeginCommandListScope(commandList))
        {
            _logger.LogInformation("Executing SET command list for element {element}.", element.Name);

            await dataAccess.SetCommandListAsync(commandList);
        }
    }

    public Task<Element> GetElementFromTableAsync(string? schemaName, string tableName, Guid? connectionId = null)
    {
        return provider.GetElementFromTableAsync(schemaName, tableName, connectionId);
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

    public Task<bool> TableExistsAsync(string? schema, string tableName, Guid? connectionId = null)
    {
        return provider.TableExistsAsync(schema ?? "dbo", tableName, connectionId);
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

        var totalOfRecords = new DataAccessParameter("@qtdtotal", 1, DbType.Int32, 0, ParameterDirection.InputOutput);
        var cmd = provider.GetReadCommand(element, new EntityParameters
        {
            Filters = primaryKeys!
        }, totalOfRecords);

        using (_logger.BeginCommandScope(cmd))
        {
            _logger.LogInformation("Executing GET command for element {element}.", element.Name);
            return dataAccess.GetDictionary(cmd) ?? new Dictionary<string, object?>();
        }
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

    public async Task<Dictionary<string, object?>> GetFieldsAsync(Element element, Dictionary<string, object> primaryKeys)
    {
        if (primaryKeys.Count == 0)
            throw new ArgumentException("Your need at least one value at your primary keys.", nameof(primaryKeys));

        var totalOfRecords = new DataAccessParameter("@qtdtotal", 1, DbType.Int32, 0, ParameterDirection.InputOutput);
        var command = provider.GetReadCommand(element, new EntityParameters
        {
            Filters = primaryKeys!
        }, totalOfRecords);

        using (_logger.BeginCommandScope(command))
        {
            var dataAccess = GetDataAccess(element.ConnectionId);
            _logger.LogInformation("Executing GET command for element {element}.", element.Name);
            return await dataAccess.GetDictionaryAsync(command);
        }
    }

    public Task CreateDataModelAsync(Element element, List<RelationshipReference>? relationships = null)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        var sqlScripts = GetDataModelScripts(element, relationships);
        return dataAccess.ExecuteBatchAsync(sqlScripts);
    }

    public void CreateDataModel(Element element, List<RelationshipReference>? relationships = null)
    {
        var dataAccess = GetDataAccess(element.ConnectionId);
        var sqlScripts = GetDataModelScripts(element, relationships);
        dataAccess.ExecuteBatch(sqlScripts);
    }

    public string GetCreateTableScript(Element element, List<RelationshipReference>? relationships = null)
    {
        return provider.GetCreateTableScript(element, relationships);
    }

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
        if (!await TableExistsAsync(element.Schema ?? "dbo", element.TableName, element.ConnectionId))
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
        EntityParameters entityParameters = parameters ?? new EntityParameters();
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (!entityParameters.OrderBy.Validate(element.Fields))
            throw new ArgumentException("[order by] clause is not valid");

        var plainTextWriter = new PlainTextReader(provider, connectionRepository, loggerFactory.CreateLogger<PlainTextReader>())
        {
            ShowLogInfo = showLogInfo,
            Delimiter = delimiter
        };

        return plainTextWriter.GetFieldsListAsTextAsync(element, entityParameters);
    }

    public List<Dictionary<string, object?>> GetDictionaryList(Element element, EntityParameters? parameters = null)
    {
        EntityParameters entityParameters = parameters ?? new EntityParameters();
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (!entityParameters.OrderBy.Validate(element.Fields))
            throw new ArgumentException("[order by] clause is not valid");

        var totalParameter = new DataAccessParameter($"{provider.VariablePrefix}qtdtotal", false ? 0 : -1, DbType.Int32, 0, ParameterDirection.InputOutput);
        var command = provider.GetReadCommand(element, entityParameters, totalParameter);

        var dataAccess = GetDataAccess(element.ConnectionId);
        var list = dataAccess.GetDictionaryList(command);

        int totalRecords = 0;
        if (totalParameter is { Value: not null } && totalParameter.Value != DBNull.Value)
            totalRecords = (int)totalParameter.Value;

        var result = new DictionaryListResult(list, totalRecords);
        return result.Data;
    }

    public async Task<List<Dictionary<string, object?>>> GetDictionaryListAsync(Element element, EntityParameters? parameters = null)
    {
        var result = await GetDictionaryListResultAsync(element, parameters ?? new EntityParameters(), false);
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
        var totalOfRecords = new DataAccessParameter("@qtdtotal", 1, DbType.Int32, 0, ParameterDirection.InputOutput);
        var command = provider.GetReadCommand(element, parameters ?? new EntityParameters(), totalOfRecords);
        return dataAccess.GetDataTableAsync(command);
    }

    public int GetCount(Element element, Dictionary<string, object?> values)
    {
        var result = GetDictionaryListResult(element, new EntityParameters { Filters = values });
        return result.Count;
    }

    public async Task<int> GetCountAsync(Element element, Dictionary<string, object?> filters)
    {
        var result = await GetDictionaryListResultAsync(element, new EntityParameters { Filters = filters });
        return result.Count;
    }

    public Task<List<Dictionary<string, object?>>> GetDictionaryListAsync(DataAccessCommand command, Guid? connectionId = null)
    {
        var dataAccess = GetDataAccess(connectionId);
        return dataAccess.GetDictionaryListAsync(command);
    }

    public DictionaryListResult GetDictionaryListResult(Element element, EntityParameters? parameters = null, bool recoverTotalOfRecords = true)
    {
        EntityParameters entityParameters = parameters ?? new EntityParameters();
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (!entityParameters.OrderBy.Validate(element.Fields))
            throw new ArgumentException("[order by] clause is not valid");

        var totalParameter = new DataAccessParameter($"{provider.VariablePrefix}qtdtotal", recoverTotalOfRecords ? 0 : -1, DbType.Int32, 0, ParameterDirection.InputOutput);
        var command = provider.GetReadCommand(element, entityParameters, totalParameter);

        var dataAccess = GetDataAccess(element.ConnectionId);

        using (_logger.BeginCommandScope(command))
        {
            _logger.LogInformation("Executing GET command for element {element}.", element.Name);
            var list = dataAccess.GetDictionaryList(command);

            int totalRecords = 0;
            if (totalParameter is { Value: not null } && totalParameter.Value != DBNull.Value)
                totalRecords = (int)totalParameter.Value;

            var result = new DictionaryListResult(list, totalRecords);
            return new DictionaryListResult(result.Data, result.TotalOfRecords);
        }
    }

    public async Task<DictionaryListResult> GetDictionaryListResultAsync(Element element, EntityParameters? parameters = null, bool recoverTotalOfRecords = true)
    {
        EntityParameters entityParameters = parameters ?? new EntityParameters();
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (!entityParameters.OrderBy.Validate(element.Fields))
            throw new ArgumentException("[order by] clause is not valid");

        var totalParameter = new DataAccessParameter($"{provider.VariablePrefix}qtdtotal", recoverTotalOfRecords ? 0 : -1, DbType.Int32, 0, ParameterDirection.InputOutput);
        var command = provider.GetReadCommand(element, entityParameters, totalParameter);

        var dataAccess = GetDataAccess(element.ConnectionId);

        using (_logger.BeginCommandScope(command))
        {
            _logger.LogInformation("Executing GET command for element {element}.", element.Name);
            var list = await dataAccess.GetDictionaryListAsync(command);

            int totalRecords = 0;
            if (totalParameter is { Value: not null } && totalParameter.Value != DBNull.Value)
                totalRecords = (int)totalParameter.Value;

            var result = new DictionaryListResult(list, totalRecords);
            return new DictionaryListResult(result.Data, result.TotalOfRecords);
        }
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
        return provider.GetStoredProcedureDefinitionAsync(procedureName, connectionId);
    }

    public Task DropStoredProcedureAsync(string procedureName, Guid? connectionId = null)
    {
        return provider.DropStoredProcedureAsync(procedureName, connectionId);
    }

    public Task<List<string>> GetStoredProcedureListAsync(Guid? connectionId = null)
    {
        return provider.GetStoredProcedureListAsync(connectionId);
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

    private static CommandOperation GetCommandOperation(
        Element element,
        Dictionary<string, object?> values,
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

    internal DataAccess GetDataAccess(Guid? connectionId)
    {
        var connection = connectionRepository.Get(connectionId);
        return new DataAccess(connection.Connection, connection.ConnectionProvider);
    }
}
