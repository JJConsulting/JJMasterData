using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Dao.Entity;

public class Factory : IEntityRepository
{
    private DataAccess _dataAccess;
    private IProvider _provider;

    internal DataAccess DataAccess
    {
        get
        {
            if (_dataAccess == null)
                _dataAccess = new DataAccess();

            return _dataAccess;
        }
    }

    internal IProvider Provider
    {
        get
        {
            if (_provider != null) return _provider;

            _provider = DataAccess.ConnectionProvider switch
            {
                DataAccessProvider.MSSQL => new MSSQLProvider(),
                DataAccessProvider.Oracle => new OracleProvider(),
                DataAccessProvider.SQLite => new ProviderSQLite(),
                _ => throw new InvalidOperationException(Translate.Key("Invalid data provider.") + " [" +
                                                         DataAccess.ConnectionProvider + "]")
            };

            return _provider;
        }
    }

    public Factory()
    {
        
    }


    ///<inheritdoc cref="IEntityRepository.Insert(Element, Hashtable)"/>
    public void Insert(Element element, Hashtable values)
    {
        var command = Provider.GetCommandInsert(element, values);
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


    ///<inheritdoc cref="IEntityRepository.Update(Element, Hashtable)"/>
    public int Update(Element element, Hashtable values)
    {
        var cmd = Provider.GetCommandUpdate(element, values);
        int numberRowsAffected = DataAccess.SetCommand(cmd);
        return numberRowsAffected;
    }

    ///<inheritdoc cref="IEntityRepository.SetValues(Element, Hashtable)"/>
    public CommandType SetValues(Element element, Hashtable values)
    {
        var commandType = CommandType.None;
        var command = Provider.GetCommandInsertOrReplace(element, values);
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

            commandType = (CommandType)nret;
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


    ///<inheritdoc cref="IEntityRepository.SetValues(Element, Hashtable, bool)"/>
    public CommandType SetValues(Element element, Hashtable values, bool ignoreResults)
    {
        if (ignoreResults)
            return SetValuesNoResult(element, values);
        return SetValues(element, values);
    }

    private CommandType SetValuesNoResult(Element element, Hashtable values)
    {
        var ret = CommandType.None;
        var command = Provider.GetCommandInsertOrReplace(element, values);
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

            ret = (CommandType)nret;
        }

        return ret;
    }


    ///<inheritdoc cref="IEntityRepository.Delete(Element, Hashtable)"/>
    public int Delete(Element element, Hashtable filters)
    {
        var cmd = Provider.GetCommandDelete(element, filters);
        int numberRowsAffected = DataAccess.SetCommand(cmd);
        return numberRowsAffected;
    }


    ///<inheritdoc cref="IEntityRepository.GetFields(Element, Hashtable)"/>
    public Hashtable GetFields(Element element, Hashtable filters)
    {
        DataAccessParameter pTot =
            new DataAccessParameter("@qtdtotal", 1, DbType.Int32, 0, ParameterDirection.InputOutput);
        var cmd = Provider.GetCommandRead(element, filters, "", 1, 1, ref pTot);
        return DataAccess.GetFields(cmd);
    }


    ///<inheritdoc cref="IEntityRepository.GetDataTable(Element, Hashtable, string, int, int, ref int)"/>
    public DataTable GetDataTable(Element element, Hashtable filters, string orderby, int regperpage, int pag,
        ref int tot)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (!ValidateOrderByClause(element, orderby))
            throw new ArgumentException(Translate.Key("[order by] clause is not valid"));

        var da = DataAccess;
        return Provider.GetDataTable(element,
            filters, orderby, regperpage, pag, ref tot, ref da);
    }


    ///<inheritdoc cref="IEntityRepository.GetDataTable(Element, Hashtable)"/>
    public DataTable GetDataTable(Element element, Hashtable filters)
    {
        int tot = 1;
        return GetDataTable(element, filters, null, int.MaxValue, 1, ref tot);
    }


    ///<inheritdoc cref="IEntityRepository.GetDataTable(string)"/>
    public DataTable GetDataTable(string sql)
    {
        return DataAccess.GetDataTable(sql);
    }

    ///<inheritdoc cref="IEntityRepository.GetResult(string)"/>
    public object GetResult(string sql)
    {
        return DataAccess.GetResult(sql);
    }

    ///<inheritdoc cref="IEntityRepository.SetCommand(string)"/>
    public void SetCommand(string sql)
    {
        DataAccess.SetCommand(sql);
    }

    ///<inheritdoc cref="IEntityRepository.SetCommand(ArrayList)"/>
    public int SetCommand(ArrayList sqlList)
    {
        return DataAccess.SetCommand(sqlList);
    }

    ///<inheritdoc cref="IEntityRepository.TableExists(string)"/>
    public bool TableExists(string tableName)
    {
        return DataAccess.TableExists(tableName);
    }

    ///<inheritdoc cref="IEntityRepository.ExecuteBatch(string)"/>
    public bool ExecuteBatch(string script)
    {
        return DataAccess.ExecuteBatch(script);
    }

    ///<inheritdoc cref="IEntityRepository.GetCount(Element, Hashtable)"/>
    public int GetCount(Element element, Hashtable filters)
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
        ExecuteBatch(scriptSql.ToString());
    }


    public string GetCreateTableScript(Element element) => Provider.GetScriptCreateTable(element);

    public string GetWriteProcedureScript(Element element) => Provider.GetScriptWriteProcedure(element);

    public string GetReadProcedureScript(Element element) => Provider.GetScriptReadProcedure(element);

    public Element GetElementFromTable(string tableName)
    {
        if (string.IsNullOrEmpty(tableName))
            throw new ArgumentNullException(nameof(tableName));

        if (!DataAccess.TableExists(tableName))
            throw new Exception(Translate.Key("Table {0} not found", tableName));

        Element element = null;
        try
        {
            var da = DataAccess;
            if (Provider is MSSQLProvider sqlProvider)
                element = sqlProvider.GetElementFromTable(tableName, ref da);
        }
        catch (Exception)
        {
            // ignored
        }

        return element;
    }


    ///<inheritdoc cref="IEntityRepository.GetListFieldsAsText(Element, Hashtable, string, int, int, bool, string)"/>
    public string GetListFieldsAsText(Element element, Hashtable filters, string orderby, int regporpag, int pag,
        bool showLogInfo, string delimiter = "|")
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (!ValidateOrderByClause(element, orderby))
            throw new ArgumentException(Translate.Key("[order by] clause is not valid"));

        var sRet = new StringBuilder();
        var dStart = DateTime.Now;
        var culture = CultureInfo.CreateSpecificCulture("en-US");
        string currentField = null;
        DbConnection conn = null;
        try
        {
            var pTot = new DataAccessParameter(Provider.VariablePrefix + "qtdtotal", 1, DbType.Int32, 0,
                ParameterDirection.InputOutput);
            var cmd = Provider.GetCommandRead(element, filters, orderby, regporpag, pag, ref pTot);
            var providerFactory = SqlClientFactory.Instance;
            conn = providerFactory.CreateConnection();
            if (conn == null)
                throw new Exception("Error on create connection object");

            conn.ConnectionString = DataAccess.ConnectionString;
            conn.Open();

            DbCommand dbCmd = providerFactory.CreateCommand();
            if (dbCmd == null)
                throw new Exception("Error on create DbCommand");

            foreach (DataAccessParameter parm in cmd.Parameters)
            {
                DbParameter oPar = providerFactory.CreateParameter();
                if (oPar == null)
                    throw new Exception("Error on create DbParameter");

                oPar.DbType = parm.Type;
                oPar.Value = parm.Value == null ? DBNull.Value : parm.Value;
                oPar.ParameterName = parm.Name;
                dbCmd.Parameters.Add(oPar);
            }

            dbCmd.CommandType = cmd.CmdType;
            dbCmd.CommandText = cmd.Sql;
            dbCmd.Connection = conn;
            dbCmd.CommandTimeout = DataAccess.TimeOut;
            DbDataReader dr = dbCmd.ExecuteReader();

            int col = 0;
            int qtd = 0;
            var columns = new Dictionary<string, int>();

            if (dr.HasRows)
            {
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    columns.Add(dr.GetName(i), i);
                }

                while (dr.Read())
                {
                    qtd++;
                    foreach (ElementField field in element.Fields)
                    {
                        currentField = field.Name;
                        if (!columns.ContainsKey(field.Name))
                            throw new Exception(Translate.Key("{0} field not found", field.Name));

                        if (col > 0)
                            sRet.Append(delimiter);

                        int ordinal = columns[field.Name];
                        if (!dr.IsDBNull(ordinal))
                        {
                            switch (field.DataType)
                            {
                                case FieldType.Date:
                                case FieldType.DateTime:
                                    sRet.Append(dr.GetDateTime(ordinal).ToString("yyyy-MM-dd HH:mm:ss"));
                                    break;
                                case FieldType.NVarchar:
                                case FieldType.Varchar:
                                case FieldType.Text:
                                    string val = dr.GetString(ordinal)
                                        .TrimEnd()
                                        .Replace("\r\n", "&#182;")
                                        .Replace("\r", "&#182;")
                                        .Replace("\n", "&#182;")
                                        .Replace(delimiter, "&#124;");
                                    sRet.Append(val);
                                    break;
                                case FieldType.Int:
                                    sRet.Append(dr.GetInt32(ordinal));
                                    break;
                                case FieldType.Float:
                                    sRet.Append(Double.Parse(dr.GetValue(ordinal).ToString()).ToString("G", culture));
                                    break;
                                default:
                                    sRet.Append(dr.GetValue(ordinal).ToString().TrimEnd());
                                    break;
                            }
                        }

                        col++;
                    }

                    sRet.AppendLine("");
                    col = 0;
                }
            }

            dr.Close();
            dr.Dispose();
            dbCmd.Dispose();

            //Log
            if (showLogInfo)
            {
                TimeSpan ts = DateTime.Now - dStart;
                StringBuilder sLog = new StringBuilder();
                sLog.Append(Translate.Key("Synchronizing"));
                sLog.Append(" ");
                sLog.AppendLine(element.Name);

                if (filters != null)
                {
                    sLog.Append("- ");
                    sLog.Append(Translate.Key("Filters"));
                    sLog.Append(": ");

                    foreach (DictionaryEntry val in filters)
                    {
                        sLog.Append("  ");
                        sLog.Append(val.Key);
                        sLog.Append("=");
                        sLog.Append(val.Value);
                    }

                    sLog.AppendLine("");
                }

                sLog.Append("- ");
                sLog.Append(Translate.Key("TotPerPage"));
                sLog.Append(": ");
                sLog.AppendLine(regporpag.ToString());
                sLog.Append("- ");
                sLog.Append(Translate.Key("CurrentPage"));
                sLog.Append(": ");
                sLog.AppendLine(pag.ToString());

                sLog.AppendLine(Translate.Key("{0} records sync. Time {1}ms", qtd.ToString(),
                    ts.TotalMilliseconds.ToString("N3")));
                Log.AddInfo(sLog.ToString());
            }
        }
        catch (Exception ex)
        {
            var message = new StringBuilder();

            message.AppendLine("Error synchronizing.");
            message.AppendLine($"Object: {element.Name}");
            message.AppendLine($"Page: {pag}");
            message.AppendLine($"Field: {currentField}");
            message.AppendLine($"Exception: {ex.Message}");
            message.AppendLine($"Stacktrace: {ex.StackTrace}");

            Log.AddError(message.ToString());

            throw new Exception(message.ToString(), ex);
        }
        finally
        {
            if (conn != null)
            {
                conn.Close();
                conn.Dispose();
            }
        }

        return sRet.ToString();
    }


    private bool ValidateOrderByClause(Element element, string orderby)
    {
        if (string.IsNullOrEmpty(orderby))
            return true;

        var clauses = orderby.Split(',');
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


}