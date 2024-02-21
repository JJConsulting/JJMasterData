#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Data.Entity.Providers;

public abstract class EntityProviderBase(
    DataAccess dataAccess,
    IOptionsSnapshot<MasterDataCommonsOptions> options,
    ILoggerFactory loggerFactory)
{
    internal DataAccess DataAccess { get; set; } = dataAccess;
    protected MasterDataCommonsOptions Options { get; } = options.Value;
    private ILoggerFactory LoggerFactory { get; } = loggerFactory;

    public abstract string VariablePrefix { get; }
    public abstract string GetCreateTableScript(Element element, List<RelationshipReference>? relationships = null);
    public abstract string? GetWriteProcedureScript(Element element);
    public abstract string? GetReadProcedureScript(Element element);
    public abstract string GetAlterTableScript(Element element, IEnumerable<ElementField> addedFields);
    public abstract Task<Element> GetElementFromTableAsync(string tableName);
    public abstract DataAccessCommand GetInsertCommand(Element element, Dictionary<string,object?> values);
    public abstract DataAccessCommand GetUpdateCommand(Element element, Dictionary<string,object?> values);
    public abstract DataAccessCommand GetDeleteCommand(Element element, Dictionary<string,object> primaryKeys);
    public abstract DataAccessCommand GetReadCommand(Element element, EntityParameters parameters, DataAccessParameter totalOfRecordsParameter);
    protected abstract DataAccessCommand GetInsertOrReplaceCommand(Element element, Dictionary<string,object?> values);
    
    public async Task InsertAsync(Element element, Dictionary<string,object?> values)
    {
        var command = GetInsertCommand(element, values);
        var newFields = await DataAccess.GetDictionaryAsync(command);

        foreach (var entry in newFields.Where(entry => element.Fields.ContainsKey(entry.Key)))
        {
            values[entry.Key] = entry.Value;
        }
    }
    
    public  void Insert(Element element, Dictionary<string,object?> values)
    {
        var command = GetInsertCommand(element, values);
        var newFields =  DataAccess.GetDictionary(command) ?? new Dictionary<string, object?>();

        foreach (var entry in newFields.Where(entry => element.Fields.ContainsKey(entry.Key)))
        {
            values[entry.Key] = entry.Value;
        }
    }
    
    public int Update(Element element, Dictionary<string, object?> values)
    {
        var cmd = GetUpdateCommand(element, values);
        int numberRowsAffected = DataAccess.SetCommand(cmd);
        return numberRowsAffected;
    }
    
    public async Task<int> UpdateAsync(Element element, Dictionary<string,object?> values)
    {
        var cmd = GetUpdateCommand(element, values);
        int numberRowsAffected = await DataAccess.SetCommandAsync(cmd);
        return numberRowsAffected;
    }
    
    public async Task<CommandOperation> SetValuesAsync(Element element, Dictionary<string,object?> values)
    {
        const CommandOperation commandType = CommandOperation.None;
        var command = GetInsertOrReplaceCommand(element, values);
        var newFields = await DataAccess.GetDictionaryAsync(command);

        return GetCommandOperation(element, values, command, commandType, newFields);
    }
    
    public CommandOperation SetValues(Element element, Dictionary<string, object?> values, bool ignoreResults)
    {
        const CommandOperation commandType = CommandOperation.None;
        var command = GetInsertOrReplaceCommand(element, values);
        var newFields =  DataAccess.GetDictionary(command);

        return GetCommandOperation(element, values, command, commandType, newFields);
    }
    
    private static CommandOperation GetCommandOperation(Element element, Dictionary<string,object?> values, DataAccessCommand command,
        CommandOperation commandType, Dictionary<string, object?>? newFields)
    {
        var resultParameter = command.Parameters.ToList().First(x => x.Name.Equals("@RET"));

        if (resultParameter.Value != DBNull.Value)
        {
            if (!int.TryParse(resultParameter.Value.ToString(), out var commandOperation))
            {
                var err = "Element";
                err += $" {element.Name}";
                err += ": " + "Invalid return of @RET variable in procedure";
                throw new JJMasterDataException(err);
            }

            commandType = (CommandOperation)commandOperation;
        }

        if (newFields == null)
            return commandType;
        
        foreach (var entry in newFields.Where(entry => element.Fields.ContainsKey(entry.Key.ToString())))
        {
            values[entry.Key] = entry.Value;
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
        var cmd = GetDeleteCommand(element, primaryKeys);
        int numberRowsAffected = DataAccess.SetCommand(cmd);
        return numberRowsAffected;
    }
    
    public async Task<int> DeleteAsync(Element element, Dictionary<string,object> primaryKeys)
    {
        var cmd = GetDeleteCommand(element, primaryKeys);
        int numberRowsAffected = await DataAccess.SetCommandAsync(cmd);
        return numberRowsAffected;
    }
    
    
    
    public async Task<DictionaryListResult> GetDictionaryListAsync(
        Element element,
        EntityParameters entityParameters,
        bool recoverTotalOfRecords = true)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (!ValidateOrderByClause(element, entityParameters.OrderBy.ToQueryParameter()))
            throw new ArgumentException("[order by] clause is not valid");

        var totalParameter = new DataAccessParameter($"{VariablePrefix}qtdtotal", recoverTotalOfRecords ? 0 : -1, DbType.Int32, 0, ParameterDirection.InputOutput);
        
        var command = GetReadCommand(element, entityParameters, totalParameter);
        
        var list = await DataAccess.GetDictionaryListAsync(command);

        int totalRecords = 0;
        
        if (totalParameter is { Value: not null } && totalParameter.Value != DBNull.Value)
            totalRecords = (int)totalParameter.Value;

        return new DictionaryListResult(list, totalRecords);
    }
    
    public void CreateDataModel(Element element, List<RelationshipReference>? relationships = null)
    {
        var sqlScripts = GetDataModelScripts(element, relationships);
        DataAccess.ExecuteBatch(sqlScripts);
    }
    
    public async Task CreateDataModelAsync(Element element, List<RelationshipReference>? relationships = null)
    {
        var sqlScripts = GetDataModelScripts(element, relationships);
        await DataAccess.ExecuteBatchAsync(sqlScripts);
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

        if (!ValidateOrderByClause(element, entityParameters.OrderBy.ToQueryParameter()))
            throw new ArgumentException("[order by] clause is not valid");

        var plainTextWriter = new PlainTextReader(this,LoggerFactory.CreateLogger<PlainTextReader>())
        {
            ShowLogInfo = showLogInfo,
            Delimiter = delimiter
        };

        return plainTextWriter.GetFieldsListAsTextAsync(element, entityParameters);
    }

    private static bool ValidateOrderByClause(Element element, string? orderBy)
    {
        if (orderBy == null || string.IsNullOrWhiteSpace(orderBy))
            return true;

        var clauses = orderBy.Split(',');
        foreach (var clause in clauses)
        {
            if (!string.IsNullOrEmpty(clause))
            {
                string temp = clause.ToLower();
                temp = temp.Replace(" desc", "");
                temp = temp.Replace(" asc", "");
                temp = temp.Replace(" ", "");

                if (!element.Fields.ContainsKey(temp))
                    return false;
            }
        }
        return true;
    }
    
    private async Task<CommandOperation> SetValuesNoResultAsync(Element element, Dictionary<string,object?> values)
    {
        const CommandOperation result = CommandOperation.None;
        var command = GetInsertOrReplaceCommand(element, values);
        await DataAccess.SetCommandAsync(command);

        return GetCommandFromValuesNoResult(element, command, result);
    }
    
    private static CommandOperation GetCommandFromValuesNoResult(Element element, DataAccessCommand command, CommandOperation ret)
    {
        var oret = command.Parameters.ToList().First(x => x.Name.Equals("@RET"));
        if (oret.Value != DBNull.Value)
        {
            if (!int.TryParse(oret.Value.ToString(), out var result))
            {
                string err = "Element";
                err += $" {element.Name}";
                err += ": " + "Invalid return of @RET variable in procedure";
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

        if (!ValidateOrderByClause(element, entityParameters.OrderBy.ToQueryParameter()))
            throw new ArgumentException("[order by] clause is not valid");

        var totalParameter = new DataAccessParameter($"{VariablePrefix}qtdtotal", recoverTotalOfRecords ? 0 : -1, DbType.Int32, 0, ParameterDirection.InputOutput);
        
        var command = GetReadCommand(element, entityParameters, totalParameter);
        
        var list =  DataAccess.GetDictionaryList(command);

        int totalRecords = 0;
        
        if (totalParameter is { Value: not null } && totalParameter.Value != DBNull.Value)
            totalRecords = (int)totalParameter.Value;

        return new DictionaryListResult(list, totalRecords);
    }
}
