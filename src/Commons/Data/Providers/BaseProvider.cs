#nullable enable

using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Data.Providers;

public abstract class BaseProvider
{
    internal DataAccess DataAccess { get; set; }
    protected JJMasterDataCommonsOptions Options { get; }
    private ILoggerFactory LoggerFactory { get; }
    
    public abstract DataAccessProvider DataAccessProvider { get; }
    
    protected BaseProvider(DataAccess dataAccess, JJMasterDataCommonsOptions options, ILoggerFactory loggerFactory)
    {
        DataAccess = dataAccess;
        Options = options;
        LoggerFactory = loggerFactory;
    }
    
    public abstract string VariablePrefix { get; }
    public abstract string GetCreateTableScript(Element element);
    public abstract string? GetWriteProcedureScript(Element element);
    public abstract string? GetReadProcedureScript(Element element);
    public abstract string GetAlterTableScript(Element element, IEnumerable<ElementField> addedFields);
    public abstract Task<Element> GetElementFromTableAsync(string tableName);
    public abstract DataAccessCommand GetInsertCommand(Element element, IDictionary<string,object?> values);
    public abstract DataAccessCommand GetUpdateCommand(Element element, IDictionary<string,object?> values);
    public abstract DataAccessCommand GetDeleteCommand(Element element, IDictionary<string,object> primaryKeys);
    public abstract DataAccessCommand GetReadCommand(Element element, EntityParameters parameters, DataAccessParameter totalOfRecordsParameter);
    protected abstract DataAccessCommand GetInsertOrReplaceCommand(Element element, IDictionary<string,object?> values);
   
    
    public async Task InsertAsync(Element element, IDictionary<string,object?> values)
    {
        var command = GetInsertCommand(element, values);
        var newFields = await DataAccess.GetDictionaryAsync(command);

        foreach (var entry in newFields.Where(entry => element.Fields.ContainsKey(entry.Key)))
        {
            values[entry.Key] = entry.Value;
        }
    }
    
    public async Task<int> UpdateAsync(Element element, IDictionary<string,object?> values)
    {
        var cmd = GetUpdateCommand(element, values);
        int numberRowsAffected = await DataAccess.SetCommandAsync(cmd);
        return numberRowsAffected;
    }
    
    
    public async Task<CommandOperation> SetValuesAsync(Element element, IDictionary<string,object?> values)
    {
        const CommandOperation commandType = CommandOperation.None;
        var command = GetInsertOrReplaceCommand(element, values);
        var newFields = await DataAccess.GetDictionaryAsync(command);

        return GetCommandOperation(element, values, command, commandType, newFields);
    }


    private static CommandOperation GetCommandOperation(Element element, IDictionary<string,object?> values, DataAccessCommand command,
        CommandOperation commandType, IDictionary<string, object?>? newFields)
    {
        var ret = command.Parameters.ToList().First(x => x.Name.Equals("@RET"));

        if (ret.Value != DBNull.Value)
        {
            if (!int.TryParse(ret.Value.ToString(), out var nret))
            {
                string err = "Element";
                err += " " + element.Name;
                err += ": " + "Invalid return of @RET variable in procedure";
                throw new JJMasterDataException(err);
            }

            commandType = (CommandOperation)nret;
        }

        if (newFields == null)
            return commandType;
        
        foreach (var entry in newFields.Where(entry => element.Fields.ContainsKey(entry.Key.ToString())))
        {
            values[entry.Key] = entry.Value;
        }

        return commandType;
    }
    
    
    public async Task<CommandOperation> SetValuesAsync(Element element, IDictionary<string,object?> values, bool ignoreResults)
    {
        if (ignoreResults)
            return await SetValuesNoResultAsync(element, values);

        return await SetValuesAsync(element, values);
    }
    
    public async Task<int> DeleteAsync(Element element, IDictionary<string,object> primaryKeys)
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

        var totalParameter = new DataAccessParameter(VariablePrefix + "qtdtotal", recoverTotalOfRecords ? 0 : -1, DbType.Int32, 0, ParameterDirection.InputOutput);
        
        var cmd = GetReadCommand(element, entityParameters, totalParameter);
        
        var list = await DataAccess.GetDictionaryListAsync(cmd);

        int totalRecords = 0;
        
        if (totalParameter is { Value: not null } && totalParameter.Value != DBNull.Value)
            totalRecords = (int)totalParameter.Value;

        return new DictionaryListResult(list, totalRecords);
    }
    public async Task CreateDataModelAsync(Element element)
    {
        var sqlScripts = GetDataModelScripts(element);
        await DataAccess.ExecuteBatchAsync(sqlScripts);
    }

    private string GetDataModelScripts(Element element)
    {
        var sqlScripts = new StringBuilder();
        sqlScripts.AppendLine(GetCreateTableScript(element));
        sqlScripts.AppendLine(GetWriteProcedureScript(element));
        sqlScripts.AppendLine(GetReadProcedureScript(element));
        return sqlScripts.ToString();
    }
    
    public async Task<string> GetFieldsListAsTextAsync(Element element, EntityParameters entityParameters,
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

        return await plainTextWriter.GetFieldsListAsTextAsync(element, entityParameters);
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
    
    
    private async Task<CommandOperation> SetValuesNoResultAsync(Element element, IDictionary<string,object?> values)
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
                err += " " + element.Name;
                err += ": " + "Invalid return of @RET variable in procedure";
                throw new JJMasterDataException(err);
            }

            ret = (CommandOperation)result;
        }

        return ret;
    }
    
}
