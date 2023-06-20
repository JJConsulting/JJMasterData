using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration.Options;

namespace JJMasterData.Commons.Data.Providers;

public abstract class BaseProvider
{
    public abstract DataAccessProvider DataAccessProvider { get; }
    internal DataAccess DataAccess { get; set; }
    protected JJMasterDataCommonsOptions Options { get; }


    public BaseProvider(DataAccess dataAccess, JJMasterDataCommonsOptions options)
    {
        DataAccess = dataAccess;
        Options = options;
    }
    public abstract string VariablePrefix { get; }
    public abstract string GetCreateTableScript(Element element);
    public abstract string GetWriteProcedureScript(Element element);
    public abstract string GetReadProcedureScript(Element element);
    public abstract Element GetElementFromTable(string tableName);
    public abstract Task<Element> GetElementFromTableAsync(string tableName);
    public abstract DataAccessCommand GetInsertCommand(Element element, IDictionary values);
    public abstract DataAccessCommand GetUpdateCommand(Element element, IDictionary values);
    public abstract DataAccessCommand GetDeleteCommand(Element element, IDictionary filters);
    public abstract DataAccessCommand GetReadCommand(Element element, IDictionary filters, string orderBy, int recordsPerPage, int currentPage, ref DataAccessParameter pTot);
    public abstract DataAccessCommand GetInsertOrReplaceCommand(Element element, IDictionary values);
    public abstract string GetAlterTableScript(Element element, IEnumerable<ElementField> addedFields);
    ///<inheritdoc cref="IEntityRepository.Insert(Element, IDictionary)"/>
    public void Insert(Element element, IDictionary values)
    {
        var command = GetInsertCommand(element, values);
        var newFields = DataAccess.GetFields(command);

        if (newFields == null)
            return;

        SetInsertValues(element, values, newFields);
    }
    
    ///<inheritdoc cref="IEntityRepository.Insert(Element, IDictionary)"/>
    public async Task InsertAsync(Element element, IDictionary values)
    {
        var command = GetInsertCommand(element, values);
        var newFields = await DataAccess.GetFieldsAsync(command);

        if (newFields == null)
            return;

        SetInsertValues(element, values, newFields);
    }

    private static void SetInsertValues(Element element, IDictionary values, Hashtable newFields)
    {
        foreach (DictionaryEntry entry in newFields)
        {
            if (element.Fields.ContainsKey(entry.Key.ToString()))
            {
                if (values.Contains(entry.Key))
                    values[entry.Key] = entry.Value;
                else
                    values.Add(entry.Key, entry.Value);
            }
        }
    }

    ///<inheritdoc cref="IEntityRepository.Update(Element, IDictionary)"/>
    public int Update(Element element, IDictionary values)
    {
        var cmd = GetUpdateCommand(element, values);
        int numberRowsAffected = DataAccess.SetCommand(cmd);
        return numberRowsAffected;
    }
    
    
    ///<inheritdoc cref="IEntityRepository.Update(Element, IDictionary)"/>
    public async Task<int> UpdateAsync(Element element, IDictionary values)
    {
        var cmd = GetUpdateCommand(element, values);
        int numberRowsAffected = await DataAccess.SetCommandAsync(cmd);
        return numberRowsAffected;
    }

    ///<inheritdoc cref="IEntityRepository.SetValues(Element, IDictionary)"/>
    public CommandOperation SetValues(Element element, IDictionary values)
    {
        const CommandOperation commandType = CommandOperation.None;
        var command = GetInsertOrReplaceCommand(element, values);
        var newFields = DataAccess.GetFields(command);

        return GetCommandOperation(element, values, command, commandType, newFields);
    }
    
    ///<inheritdoc cref="IEntityRepository.SetValues(Element, IDictionary)"/>
    public async Task<CommandOperation> SetValuesAsync(Element element, IDictionary values)
    {
        const CommandOperation commandType = CommandOperation.None;
        var command = GetInsertOrReplaceCommand(element, values);
        var newFields = await DataAccess.GetFieldsAsync(command);

        return GetCommandOperation(element, values, command, commandType, newFields);
    }


    private static CommandOperation GetCommandOperation(Element element, IDictionary values, DataAccessCommand command,
        CommandOperation commandType, Hashtable newFields)
    {
        var ret = command.Parameters.ToList().First(x => x.Name.Equals("@RET"));

        if (ret.Value != DBNull.Value)
        {
            if (!int.TryParse(ret.Value.ToString(), out var nret))
            {
                string err = Translate.Key("Element");
                err += " " + element.Name;
                err += ": " + Translate.Key("Invalid return of @RET variable in procedure");
                throw new JJMasterDataException(err);
            }

            commandType = (CommandOperation)nret;
        }

        if (newFields == null)
            return commandType;
        foreach (var entry in newFields.Cast<DictionaryEntry>().Where(entry => element.Fields.ContainsKey(entry.Key.ToString())))
        {
            if (values.Contains(entry.Key))
                values[entry.Key] = entry.Value;

            else
                values.Add(entry.Key, entry.Value);
        }

        return commandType;
    }

    ///<inheritdoc cref="IEntityRepository.SetValues(Element, IDictionary, bool)"/>
    public CommandOperation SetValues(Element element, IDictionary values, bool ignoreResults)
    {
        if (ignoreResults)
            return SetValuesNoResult(element, values);

        return SetValues(element, values);
    }
    
    ///<inheritdoc cref="IEntityRepository.SetValues(Element, IDictionary, bool)"/>
    public async Task<CommandOperation> SetValuesAsync(Element element, IDictionary values, bool ignoreResults)
    {
        if (ignoreResults)
            return await SetValuesNoResultAsync(element, values);

        return await SetValuesAsync(element, values);
    }

    ///<inheritdoc cref="IEntityRepository.Delete(Element, Hashtable)"/>
    public int Delete(Element element, IDictionary filters)
    {
        var cmd = GetDeleteCommand(element, filters);
        int numberRowsAffected = DataAccess.SetCommand(cmd);
        return numberRowsAffected;
    }
    
    ///<inheritdoc cref="IEntityRepository.Delete(Element, Hashtable)"/>
    public async Task<int> DeleteAsync(Element element, IDictionary filters)
    {
        var cmd = GetDeleteCommand(element, filters);
        int numberRowsAffected = await DataAccess.SetCommandAsync(cmd);
        return numberRowsAffected;
    }

    ///<inheritdoc cref="IEntityRepository.GetFields(Element, Hashtable)"/>
    public Hashtable GetFields(Element element, IDictionary filters)
    {
        var total =
            new DataAccessParameter("@qtdtotal", 1, DbType.Int32, 0, ParameterDirection.InputOutput);
        var cmd = GetReadCommand(element, filters, "", 1, 1, ref total);
        return DataAccess.GetFields(cmd);
    }
    
    ///<inheritdoc cref="IEntityRepository.GetFields(Element, Hashtable)"/>
    public async Task<Hashtable> GetFieldsAsync(Element element, IDictionary filters)
    {
        var total =
            new DataAccessParameter("@qtdtotal", 1, DbType.Int32, 0, ParameterDirection.InputOutput);
        var cmd = GetReadCommand(element, filters, "", 1, 1, ref total);
        return await DataAccess.GetFieldsAsync(cmd);
    }

    ///<inheritdoc cref="IEntityRepository.GetDataTable(Element, IDictionary,string,int,int,ref int)"/>
    public DataTable GetDataTable(Element element, IDictionary filters, string orderBy, int recordsPerPage, int currentPage, ref int tot)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (!ValidateOrderByClause(element, orderBy))
            throw new ArgumentException(Translate.Key("[order by] clause is not valid"));

        var pTot = new DataAccessParameter(VariablePrefix + "qtdtotal", tot, DbType.Int32, 0, ParameterDirection.InputOutput);
        var cmd = GetReadCommand(element, filters, orderBy, recordsPerPage, currentPage, ref pTot);
        var dt = DataAccess.GetDataTable(cmd);
        tot = 0;
        if (pTot is { Value: not null } && pTot.Value != DBNull.Value)
            tot = (int)pTot.Value;

        return dt;
    }
    
    
    ///<inheritdoc cref="IEntityRepository.GetDataTable(Element, IDictionary,string,int,int,ref int)"/>
    public async Task<(DataTable, int)> GetDataTableAsync(Element element, IDictionary filters, string orderBy, int recordsPerPage, int currentPage, int total)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (!ValidateOrderByClause(element, orderBy))
            throw new ArgumentException(Translate.Key("[order by] clause is not valid"));

        var pTot = new DataAccessParameter(VariablePrefix + "qtdtotal", total, DbType.Int32, 0, ParameterDirection.InputOutput);
        var cmd = GetReadCommand(element, filters, orderBy, recordsPerPage, currentPage, ref pTot);
        var dt = await DataAccess.GetDataTableAsync(cmd);
        total = 0;
        if (pTot is { Value: not null } && pTot.Value != DBNull.Value)
            total = (int)pTot.Value;

        return (dt, total);
    }
    
    ///<inheritdoc cref="IEntityRepository.GetDataTable(Element, IDictionary,string,int,int,ref int)"/>
    public async Task<(List<Dictionary<string,dynamic>>,int)> GetDictionaryListAsync(Element element, IDictionary filters, string orderBy, int recordsPerPage, int currentPage, int total)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (!ValidateOrderByClause(element, orderBy))
            throw new ArgumentException(Translate.Key("[order by] clause is not valid"));

        var pTot = new DataAccessParameter(VariablePrefix + "qtdtotal", total, DbType.Int32, 0, ParameterDirection.InputOutput);
        var cmd = GetReadCommand(element, filters, orderBy, recordsPerPage, currentPage, ref pTot);
        var dt = await DataAccess.GetDictionaryListAsync(cmd);
        total = 0;
        if (pTot is { Value: not null } && pTot.Value != DBNull.Value)
            total = (int)pTot.Value;

        return (dt, total);
    }

    ///<inheritdoc cref="IEntityRepository.GetDataTable(Element, Hashtable)"/>
    public DataTable GetDataTable(Element element, IDictionary filters)
    {
        var tot = 1;
        return GetDataTable(element, filters, null, int.MaxValue, 1, ref tot);
    }
    
    ///<inheritdoc cref="IEntityRepository.GetDataTable(Element, Hashtable)"/>
    public async Task<DataTable> GetDataTableAsync(Element element, IDictionary filters)
    {
        const int total = 1;
        return (await GetDataTableAsync(element, filters, null, int.MaxValue, 1, total)).Item1;
    }

    ///<inheritdoc cref="IEntityRepository.GetCount(Element, Hashtable)"/>
    public int GetCount(Element element, IDictionary filters)
    {
        var tot = 0;
        GetDataTable(element, filters, null, 1, 1, ref tot);
        return tot;
    }
    
    ///<inheritdoc cref="IEntityRepository.GetCount(Element, Hashtable)"/>
    public async Task<int> GetCountAsync(Element element, IDictionary filters)
    {
        var tot = 0;
        var tuple = await GetDataTableAsync(element, filters, null, 1, 1, tot);
        return tuple.Item2;
    }

    ///<inheritdoc cref="IEntityRepository.CreateDataModel(Element)"/>
    public void CreateDataModel(Element element)
    {
        var sqlScripts = GetDataModelScripts(element);
        DataAccess.ExecuteBatch(sqlScripts);
    }
    
    ///<inheritdoc cref="IEntityRepository.CreateDataModel(Element)"/>
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

    ///<inheritdoc cref="IEntityRepository.GetListFieldsAsText(Element, Hashtable, string, int, int, bool, string)"/>
    public string GetListFieldsAsText(Element element, IDictionary filters, string orderBy, int recordsPerPage, int currentPage,
        bool showLogInfo, string delimiter = "|")
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (!ValidateOrderByClause(element, orderBy))
            throw new ArgumentException(Translate.Key("[order by] clause is not valid"));

        var plainTextWriter = new PlainTextReader(this);
        plainTextWriter.ShowLogInfo = showLogInfo;
        plainTextWriter.Delimiter = delimiter;

        return plainTextWriter.GetListFieldsAsText(element, filters, orderBy, recordsPerPage, currentPage);
    }

    private static bool ValidateOrderByClause(Element element, string orderBy)
    {
        if (string.IsNullOrEmpty(orderBy))
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

    private CommandOperation SetValuesNoResult(Element element, IDictionary values)
    {
        const CommandOperation result = CommandOperation.None;
        var command = GetInsertOrReplaceCommand(element, values);
        DataAccess.SetCommand(command);

        return GetCommandFromValuesNoResult(element, command, result);
    }
    
    private async Task<CommandOperation> SetValuesNoResultAsync(Element element, IDictionary values)
    {
        const CommandOperation result = CommandOperation.None;
        var command = GetInsertOrReplaceCommand(element, values);
        await DataAccess.SetCommandAsync(command);

        return GetCommandFromValuesNoResult(element, command, result);
    }
    
    private static CommandOperation GetCommandFromValuesNoResult(Element element, DataAccessCommand command,
        CommandOperation ret)
    {
        var oret = command.Parameters.ToList().First(x => x.Name.Equals("@RET"));
        if (oret.Value != DBNull.Value)
        {
            if (!int.TryParse(oret.Value.ToString(), out var result))
            {
                string err = Translate.Key("Element");
                err += " " + element.Name;
                err += ": " + Translate.Key("Invalid return of @RET variable in procedure");
                throw new JJMasterDataException(err);
            }

            ret = (CommandOperation)result;
        }

        return ret;
    }


}
