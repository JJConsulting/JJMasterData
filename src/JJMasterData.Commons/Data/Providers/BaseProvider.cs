using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;

namespace JJMasterData.Commons.Data.Providers;

public abstract class BaseProvider
{
    internal DataAccess DataAccess { get; private set; }
    public BaseProvider(DataAccess dataAccess)
    {
        DataAccess = dataAccess;
    }
    public abstract string VariablePrefix { get; }
    public abstract string GetCreateTableScript(Element element);
    public abstract string GetWriteProcedureScript(Element element);
    public abstract string GetReadProcedureScript(Element element);
    public abstract Element GetElementFromTable(string tableName);

    public abstract DataAccessCommand GetInsertCommand(Element element, IDictionary values);
    public abstract DataAccessCommand GetUpdateCommand(Element element, IDictionary values);
    public abstract DataAccessCommand GetDeleteCommand(Element element, IDictionary filters);
    public abstract DataAccessCommand GetReadCommand(Element element, IDictionary filters, string orderBy, int recordsPerPage, int currentPage, ref DataAccessParameter pTot);
    public abstract DataAccessCommand GetInsertOrReplaceCommand(Element element, IDictionary values);

    ///<inheritdoc cref="IEntityRepository.Insert(Element, IDictionary)"/>
    public void Insert(Element element, IDictionary values)
    {
        var command = GetInsertCommand(element, values);
        var newFields = DataAccess.GetFields(command);

        if (newFields == null)
            return;

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

    ///<inheritdoc cref="IEntityRepository.SetValues(Element, IDictionary)"/>
    public CommandOperation SetValues(Element element, IDictionary values)
    {
        var commandType = CommandOperation.None;
        var command = GetInsertOrReplaceCommand(element, values);
        var newFields = DataAccess.GetFields(command);

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

        if (newFields == null) return commandType;
        foreach (DictionaryEntry entry in newFields)
        {
            if (!element.Fields.ContainsKey(entry.Key.ToString())) continue;

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

    ///<inheritdoc cref="IEntityRepository.Delete(Element, Hashtable)"/>
    public int Delete(Element element, IDictionary filters)
    {
        var cmd = GetDeleteCommand(element, filters);
        int numberRowsAffected = DataAccess.SetCommand(cmd);
        return numberRowsAffected;
    }

    ///<inheritdoc cref="IEntityRepository.GetFields(Element, Hashtable)"/>
    public Hashtable GetFields(Element element, IDictionary filters)
    {
        DataAccessParameter pTot =
            new DataAccessParameter("@qtdtotal", 1, DbType.Int32, 0, ParameterDirection.InputOutput);
        var cmd = GetReadCommand(element, filters, "", 1, 1, ref pTot);
        return DataAccess.GetFields(cmd);
    }

    ///<inheritdoc cref="IEntityRepository.GetDataTable(JJMasterData.Commons.Data.Entity.Element,System.Collections.IDictionary,string,int,int,ref int)"/>
    public DataTable GetDataTable(Element element, IDictionary filters, string orderBy, int recordsPerPage, int currentPage, ref int tot)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (!ValidateOrderByClause(element, orderBy))
            throw new ArgumentException(Translate.Key("[order by] clause is not valid"));

        DataAccessParameter pTot = new DataAccessParameter(VariablePrefix + "qtdtotal", tot, DbType.Int32, 0, ParameterDirection.InputOutput);
        var cmd = GetReadCommand(element, filters, orderBy, recordsPerPage, currentPage, ref pTot);
        DataTable dt = DataAccess.GetDataTable(cmd);
        tot = 0;
        if (pTot != null && pTot.Value != null && pTot.Value != DBNull.Value)
            tot = (int)pTot.Value;

        return dt;
    }

    ///<inheritdoc cref="IEntityRepository.GetDataTable(Element, Hashtable)"/>
    public DataTable GetDataTable(Element element, IDictionary filters)
    {
        int tot = 1;
        return GetDataTable(element, filters, null, int.MaxValue, 1, ref tot);
    }

    ///<inheritdoc cref="IEntityRepository.GetCount(Element, Hashtable)"/>
    public int GetCount(Element element, IDictionary filters)
    {
        int tot = 0;
        GetDataTable(element, filters, null, 1, 1, ref tot);
        return tot;
    }

    ///<inheritdoc cref="IEntityRepository.CreateDataModel(Element)"/>
    public void CreateDataModel(Element element)
    {
        var scriptSql = new StringBuilder();
        scriptSql.AppendLine(GetCreateTableScript(element));
        scriptSql.AppendLine(GetWriteProcedureScript(element));
        scriptSql.AppendLine(GetReadProcedureScript(element));
        DataAccess.ExecuteBatch(scriptSql.ToString());
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

    private bool ValidateOrderByClause(Element element, string orderBy)
    {
        if (string.IsNullOrEmpty(orderBy))
            return true;

        var clauses = orderBy.Split(',');
        foreach (string clause in clauses)
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
        var ret = CommandOperation.None;
        var command = GetInsertOrReplaceCommand(element, values);
        DataAccess.SetCommand(command);

        var oret = command.Parameters.ToList().First(x => x.Name.Equals("@RET"));
        if (oret.Value != DBNull.Value)
        {
            if (!int.TryParse(oret.Value.ToString(), out var nret))
            {
                string err = Translate.Key("Element");
                err += " " + element.Name;
                err += ": " + Translate.Key("Invalid return of @RET variable in procedure");
                throw new JJMasterDataException(err);
            }

            ret = (CommandOperation)nret;
        }

        return ret;
    }
}
