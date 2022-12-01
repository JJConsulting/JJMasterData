using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;

namespace JJMasterData.Commons.Dao.Providers;

internal abstract class BaseProvider
{
    internal DataAccess DataAccess { get; private set; }
    public BaseProvider(DataAccess dataAccess)
    {
        DataAccess = dataAccess;
    }
    public abstract string VariablePrefix { get; }
    public abstract string GetScriptCreateTable(Element element);
    public abstract string GetScriptWriteProcedure(Element element);
    public abstract string GetScriptReadProcedure(Element element);
    public abstract Element GetElementFromTable(string tableName);

    public abstract DataAccessCommand GetCommandInsert(Element element, Hashtable values);
    public abstract DataAccessCommand GetCommandUpdate(Element element, Hashtable values);
    public abstract DataAccessCommand GetCommandDelete(Element element, Hashtable filters);
    public abstract DataAccessCommand GetCommandRead(Element element, Hashtable filters, string orderby, int regporpag, int pag, ref DataAccessParameter pTot);
    public abstract DataAccessCommand GetCommandInsertOrReplace(Element element, Hashtable values);

    
    public void Insert(Element element, Hashtable values)
    {
        var command = GetCommandInsert(element, values);
        var newFields = DataAccess.GetFields(command);

        if (newFields == null)
            return;

        foreach (DictionaryEntry entry in newFields)
        {
            if (element.Fields.ContainsKey(entry.Key.ToString()))
            {
                if (values.ContainsKey(entry.Key))
                    values[entry.Key] = entry.Value;
                else
                    values.Add(entry.Key, entry.Value);
            }
        }
    }

    
    public int Update(Element element, Hashtable values)
    {
        var cmd = GetCommandUpdate(element, values);
        int numberRowsAffected = DataAccess.SetCommand(cmd);
        return numberRowsAffected;
    }

    
    public CommandOperation SetValues(Element element, Hashtable values)
    {
        var commandType = CommandOperation.None;
        var command = GetCommandInsertOrReplace(element, values);
        var newFields = DataAccess.GetFields(command);

        var ret = command.Parameters.ToList().First(x => x.Name.Equals("@RET"));

        if (ret.Value != DBNull.Value)
        {
            if (!int.TryParse(ret.Value.ToString(), out var nret))
            {
                string err = Translate.Key("Element");
                err += " " + element.Name;
                err += ": " + Translate.Key("Invalid return of @RET variable in procedure");
                throw new DataDictionaryException(err);
            }

            commandType = (CommandOperation)nret;
        }

        if (newFields == null) return commandType;
        foreach (DictionaryEntry entry in newFields)
        {
            if (!element.Fields.ContainsKey(entry.Key.ToString())) continue;

            if (values.ContainsKey(entry.Key))
                values[entry.Key] = entry.Value;

            else
                values.Add(entry.Key, entry.Value);
        }

        return commandType;
    }

    
    public CommandOperation SetValues(Element element, Hashtable values, bool ignoreResults)
    {
        if (ignoreResults)
            return SetValuesNoResult(element, values);

        return SetValues(element, values);
    }

    
    public int Delete(Element element, Hashtable filters)
    {
        var cmd = GetCommandDelete(element, filters);
        int numberRowsAffected = DataAccess.SetCommand(cmd);
        return numberRowsAffected;
    }

    
    public Hashtable GetFields(Element element, Hashtable filters)
    {
        DataAccessParameter pTot =
            new DataAccessParameter("@qtdtotal", 1, DbType.Int32, 0, ParameterDirection.InputOutput);
        var cmd = GetCommandRead(element, filters, "", 1, 1, ref pTot);
        return DataAccess.GetFields(cmd);
    }

    
    public DataTable GetDataTable(Element element, Hashtable filters, string orderBy, int recordsPerPage, int currentPage, ref int tot)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (!ValidateOrderByClause(element, orderBy))
            throw new ArgumentException(Translate.Key("[order by] clause is not valid"));

        DataAccessParameter pTot = new DataAccessParameter(VariablePrefix + "qtdtotal", tot, DbType.Int32, 0, ParameterDirection.InputOutput);
        var cmd = GetCommandRead(element, filters, orderBy, recordsPerPage, currentPage, ref pTot);
        DataTable dt = DataAccess.GetDataTable(cmd);
        tot = 0;
        if (pTot != null && pTot.Value != null && pTot.Value != DBNull.Value)
            tot = (int)pTot.Value;

        return dt;
    }

    
    public DataTable GetDataTable(Element element, Hashtable filters)
    {
        int tot = 1;
        return GetDataTable(element, filters, null, int.MaxValue, 1, ref tot);
    }

    
    public int GetCount(Element element, Hashtable filters)
    {
        int tot = 0;
        GetDataTable(element, filters, null, 1, 1, ref tot);
        return tot;
    }

    
    public void CreateDataModel(Element element)
    {
        var scriptSql = new StringBuilder();
        scriptSql.AppendLine(GetScriptCreateTable(element));
        scriptSql.AppendLine(GetScriptWriteProcedure(element));
        scriptSql.AppendLine(GetScriptReadProcedure(element));
        DataAccess.ExecuteBatch(scriptSql.ToString());
    }

    
    public string GetListFieldsAsText(Element element, Hashtable filters, string orderBy, int recordsPerPage, int currentPage,
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

    private CommandOperation SetValuesNoResult(Element element, Hashtable values)
    {
        var ret = CommandOperation.None;
        var command = GetCommandInsertOrReplace(element, values);
        DataAccess.SetCommand(command);

        var oret = command.Parameters.ToList().First(x => x.Name.Equals("@RET"));
        if (oret.Value != DBNull.Value)
        {
            int nret;
            if (!int.TryParse(oret.Value.ToString(), out nret))
            {
                string err = Translate.Key("Element");
                err += " " + element.Name;
                err += ": " + Translate.Key("Invalid return of @RET variable in procedure");
                throw new DataDictionaryException(err);
            }

            ret = (CommandOperation)nret;
        }

        return ret;
    }
}
