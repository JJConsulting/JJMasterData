using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Dao.Entity;

public class Factory
{
    private IDataAccess _dataAccess;

    private IProvider _provider;

    public IProvider Provider
    {
        get
        {
            if (_provider != null) return _provider;

            _provider = _dataAccess.ConnectionProvider switch
            {
                DataAccessProvider.MSSQL => new MSSQLProvider(),
                DataAccessProvider.Oracle => new OracleProvider(),
                DataAccessProvider.SQLite => new ProviderSQLite(),
                _ => throw new InvalidOperationException(Translate.Key("Invalid data provider.") + " [" +
                                                         _dataAccess.ConnectionProvider + "]")
            };

            return _provider;
        }
    }

    public Factory()
    {
        _dataAccess = JJService.DataAccess;
    }

    public Factory(IDataAccess dataAccess)
    {
        _dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));
    }

    /// <summary>
    /// Add a record to the database
    /// </summary>
    /// <param name="elementName">Dictionary name</param>
    /// <param name="values">List of values ​​to be stored in the database</param>
    /// <remarks>
    /// How to do:
    /// [key(database field name), value(value to be stored in the database)].
    /// </remarks>
    public void Insert(string elementName, Hashtable values)
    {
        var element = GetElement(elementName);
        Insert(element, values);
    }

    /// <summary>
    /// Add a record to the database.
    /// Retorno o id no campo values como referencia
    /// Return the id in the values ​​field as a reference
    /// </summary>
    /// <param name="element">Base element with the basic structure of the table</param>
    /// <param name="values">List of values ​​to be stored in the database</param>
    /// <remarks>
    /// How to do:
    /// [key(database field name), value(value to be stored in the database)].
    /// </remarks>
    public void Insert(Element element, Hashtable values)
    {
        var command = Provider.GetInsertScript(element, values);
        var newFields = _dataAccess.GetFields(command);

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

    /// <summary>
    /// Update a record in the database
    /// </summary>
    /// <param name="elementName">Dictionary name</param>
    /// <param name="values">List of values ​​to be stored in the database</param>
    /// <remarks>
    /// How to do:
    /// [key(database field name), value(value to be stored in the database)].
    /// </remarks>
    public void Update(string elementName, Hashtable values)
    {
        var element = GetElement(elementName);
        Update(element, values);
    }

    /// <summary>
    /// Update a record in the database
    /// </summary>
    /// <param name="element">Base element with a basic table structure</param>
    /// <param name="values">List of values ​​to be stored in the database</param>
    /// <remarks>
    /// How to do:
    /// [key(database field name), value(value to be stored in the database)].
    /// </remarks>
    public void Update(Element element, Hashtable values)
    {
        var cmd = Provider.GetUpdateScript(element, values);
        _dataAccess.SetCommand(cmd);
    }

    /// <summary>
    /// Set a record in the database.
    /// If it exists then update it, otherwise add.
    /// Include PK in Hashtable in case of indentity
    /// </summary>
    /// <returns>NONE=-1, INSERT=0, UPDATE=1, DELETE=2</returns>
    /// <param name="element">Base element with the basic structure of the table</param>
    /// <param name="values">List of values ​​to be stored in the database</param>
    /// <remarks>
    /// How to do:
    /// [key(database field name), value(value to be stored in the database)].
    /// </remarks>
    public CommandType SetValues(Element element, Hashtable values)
    {
        var commandType = CommandType.None;
        var command = Provider.GetWriteCommand("", element, values);
        var newFields = _dataAccess.GetFields(command);

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


    /// <summary>
    /// Set a record in the database.
    /// If it exists then update it, otherwise add.
    /// </summary>
    /// <returns>NONE=-1, INSERT=0, UPDATE=1, DELETE=2</returns>
    /// <param name="element">Base element with the basic structure of the table</param>
    /// <param name="values">List of values ​​to be stored in the database</param>
    /// <param name="ignoreResults">By default the values ​​returned in the set procedures are returned by reference in the hashtable values ​​object, 
    /// if this ignoreResults parameter is true this action is ignored, improving performance</param>
    /// <remarks>
    /// How to do:
    /// [key(database field name), value(value to be stored in the database)].
    /// </remarks>
    public CommandType SetValues(Element element, Hashtable values, bool ignoreResults)
    {
        if (ignoreResults)
            return SetValuesNoResult(element, values);
        return SetValues(element, values);
    }

    private CommandType SetValuesNoResult(Element element, Hashtable values)
    {
        var ret = CommandType.None;
        var command = Provider.GetWriteCommand("", element, values);
        _dataAccess.SetCommand(command);

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

    /// <summary>
    /// Delete records based on filter.  
    /// [key(database field), valor(value stored in database)].
    /// </summary>
    /// <param name="element">Base element with the basic structure of the table</param>
    /// <param name="filters">List of filters to be used</param>
    /// <returns>Returns the count of deleted records</returns>
    public int Delete(Element element, Hashtable filters)
    {
        var cmd = Provider.GetDeleteScript(element, filters);
        int ret = _dataAccess.SetCommand(cmd);

        return ret;
    }

    /// <summary>
    /// Returns first record based on filter.  
    /// </summary>
    /// <param name="elementName">Dictionary name</param>
    /// <param name="filters">List of filters to be used. [key(database field), valor(value stored in database)]</param>
    /// <returns>
    /// Return a Hashtable Object. 
    /// If no record is found then returns null.
    /// </returns>
    public Hashtable GetFields(string elementName, Hashtable filters)
    {
        var element = GetElement(elementName);
        return GetFields(element, filters);
    }

    /// <summary>
    /// Returns first record based on filter.  
    /// </summary>
    /// <param name="element">Base element with the basic structure of the table.</param>
    /// <param name="filters">List of filters to be used. [key(database field), valor(value stored in database)]</param>
    /// <returns>
    /// Return a Hashtable Object. 
    /// If no record is found then returns null.
    /// </returns>
    public Hashtable GetFields(Element element, Hashtable filters)
    {
        DataAccessParameter pTot =
            new DataAccessParameter("@qtdtotal", 1, DbType.Int32, 0, ParameterDirection.InputOutput);
        var cmd = Provider.GetReadCommand(element, filters, "", 1, 1, ref pTot);
        return _dataAccess.GetFields(cmd);
    }

    /// <summary>
    /// Returns records from the database based on the filter.  
    /// </summary>
    /// <param name="elementName">Dictionary element name</param>
    /// <param name="filters">List of filters to be used. [key(database field), valor(value stored in database)]</param>
    /// <param name="orderby">Record Order, field followed by ASC or DESC</param>
    /// <param name="regperpage">Number of records to be displayed per page</param>
    /// <param name="pag">Current page</param>
    /// <param name="tot">
    /// If the value is zero, it returns as a reference the number of records based on the filter, 
    /// running an extra processing in the database, if filled, do not execute the count</param>
    /// <returns>
    /// Returns a DataTable with the records found. 
    /// If no record is found it returns null.
    /// </returns>
    public DataTable GetDataTable(string elementName, Hashtable filters, string orderby, int regperpage, int pag,
        ref int tot)
    {
        var element = GetElement(elementName);
        return GetDataTable(element, filters, orderby, regperpage, pag, ref tot);
    }

    /// <summary>
    /// Returns records from the database based on the filter.    
    /// </summary>
    /// <param name="element">Base element with the basic structure of the table.</param>
    /// <param name="filters">List of filters to be used. [key(database field), valor(value stored in database)]</param>
    /// <param name="orderby">Record Order, field followed by ASC or DESC</param>
    /// <param name="regperpage">Number of records to be displayed per page</param>
    /// <param name="pag">Current page</param>
    /// <param name="tot">If the value is zero, it returns as a reference the number of records based on the filter.</param>
    /// <returns>
    /// Returns a DataTable with the records found.
    /// If no record is found it returns null.
    /// </returns>
    public DataTable GetDataTable(Element element, Hashtable filters, string orderby, int regperpage, int pag,
        ref int tot)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (!ValidateOrderByClause(element, orderby))
            throw new ArgumentException(Translate.Key("[order by] clause is not valid"));

        return Provider.GetDataTable(element,
            filters, orderby, regperpage, pag, ref tot, ref _dataAccess);
    }


    /// <summary>
    /// Returns records from the database based on the filter.  
    /// </summary>
    /// <param name="element">Base element with the basic structure of the table.</param>
    /// <param name="filters">List of filters to be used. [key(database field), valor(value stored in database)]</param>
    /// <returns>
    /// Returns a DataTable with the records found. 
    /// If no record is found it returns null.
    /// </returns>
    public DataTable GetDataTable(Element element, Hashtable filters)
    {
        int tot = 1;
        return GetDataTable(element, filters, null, int.MaxValue, 1, ref tot);
    }


    /// <summary>
    /// Returns the number of records in the database
    /// </summary>
    /// <param name="element">Base element with the basic structure of the table.</param>
    /// <param name="filters">List of filters to be used. [key(database field), valor(value stored in database)]</param>
    /// <returns>
    /// Returns an integer.
    /// </returns>
    public int GetCount(Element element, Hashtable filters)
    {
        int tot = 0;
        GetDataTable(element, filters, null, 1, 1, ref tot);
        return tot;
    }

    /// <summary>
    /// Returns an element with the basic structure of the table
    /// </summary>
    /// <param name="elementName">Dictionary name</param>
    /// <returns></returns>
    public Element GetElement(string elementName)
    {
        if (elementName == null)
            throw new ArgumentNullException(nameof(elementName), Translate.Key("Invalid dictionary name"));

        var filter = new Hashtable();
        filter.Add("name", elementName);
        filter.Add("type", "T");

        var resultElement = GetFields(GetStructure(), filter);

        if (resultElement == null)
            throw new ArgumentException(Translate.Key("Dictionary {0} not found", elementName));

        var element = JsonConvert.DeserializeObject<Element>(resultElement["json"].ToString());
        element.Info = resultElement["info"].ToString();

        return element;
    }

    /// <summary>
    /// Returns a list of base elements
    /// </summary>
    /// <returns></returns>
    public List<Element> GetListElement()
    {
        var filter = new Hashtable();
        filter.Add("type", "T");
        return GetListElement(filter, null, 1000, 1);
    }

    /// <summary>
    /// Returns a list of base elements
    /// </summary>
    /// <param name="filters">List of filters to be used. [key(database field), valor(value stored in database)]</param>
    /// <param name="orderby">Record Order, field followed by ASC or DESC</param>
    /// <param name="regporpag">Number of records to be displayed per page</param>
    /// <param name="pag">Current page</param>
    /// <returns></returns>
    public List<Element> GetListElement(Hashtable filters, string orderby, int regporpag, int pag)
    {
        var element = GetStructure();
        if (!ValidateOrderByClause(element, orderby))
            throw new ArgumentException(Translate.Key("[order by] clause is not valid"));

        var listElement = new List<Element>();
        int tot = 1000;
        var dt = GetDataTable(element, filters, orderby, regporpag, pag, ref tot);
        foreach (DataRow row in dt.Rows)
        {
            if (row["type"].ToString().Equals("T"))
            {
                Element e = JsonConvert.DeserializeObject<Element>(row["json"].ToString());
                listElement.Add(e);
            }
        }

        return listElement;
    }

    /// <summary>
    /// Create an element's tables and procedures
    /// </summary>
    /// <param name="element">Element with table data</param>
    public void CreateDataModel(Element element)
    {
        string script = GetScriptCreateDataModel(element);
        _dataAccess.ExecuteBatch(script);
    }

    /// <summary>
    /// Returns the script to create an element's tables and procedures
    /// </summary>
    /// <param name="element"></param>
    /// <returns>Script to be run on the database</returns>
    public string GetScriptCreateDataModel(Element element)
    {
        StringBuilder sSql = new StringBuilder();
        //create table
        sSql.AppendLine(Provider.GetCreateTableScript(element));
        //procedure set
        sSql.AppendLine(Provider.GetWriteProcedureScript(element));
        //procedure get
        sSql.AppendLine(Provider.GetReadProcedureScript(element));

        return sSql.ToString();
    }

    /// <summary>
    /// Returns database records based on filter.  
    /// </summary>
    /// <param name="element">Elemento base com uma estrutura básica da tabela.</param>
    /// <param name="filters">List of filters to be used. [key(database field), valor(value stored in database)]</param>
    /// <param name="orderby">Record Order, field followed by ASC or DESC</param>
    /// <param name="regporpag">Number of records to be displayed per page</param>
    /// <param name="pag">Current page</param>
    /// <param name="showLogInfo">Records detailed log of each operation</param>
    /// <param name="delimiter">Field delimiter in text file (default is pipe)</param>
    /// <returns>
    /// Returns a string with one record per line separated by the delimiter.<para/>
    /// If no record is found it returns null. <para/>
    /// *Warning: <para/>
    /// - Some special characters will be replaced:<para/>
    ///   enter =  #182;<para/>
    ///   {delimiter} = #124;<para/>
    /// - Submitted formats:<para/>
    ///   Numbers = en-US<para/>
    ///   Date = yyyy-MM-dd HH:mm:ss
    /// </returns>
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
            var cmd = Provider.GetReadCommand(element, filters, orderby, regporpag, pag, ref pTot);
            var providerFactory = SqlClientFactory.Instance;
            conn = providerFactory.CreateConnection();
            if (conn == null)
                throw new Exception("Error on create connection object");

            conn.ConnectionString = _dataAccess.ConnectionString;
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
            dbCmd.CommandTimeout = _dataAccess.TimeOut;
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

    public Element GetStructure()
    {
        var element = new Element(JJService.Settings.TableName, "Data Dictionaries");

        element.Fields.AddPK("type", "Type", FieldType.Varchar, 1, false, FilterMode.Equal);
        element.Fields["type"].EnableOnDelete = false;

        element.Fields.AddPK("name", "Dictionary Name", FieldType.NVarchar, 64, false, FilterMode.Equal);
        element.Fields.Add("namefilter", "Dictionary Name", FieldType.NVarchar, 30, false, FilterMode.Contain,
            FieldBehavior.ViewOnly);
        element.Fields.Add("tablename", "Table Name", FieldType.NVarchar, 64, false, FilterMode.MultValuesContain);
        element.Fields.Add("info", "Info", FieldType.NVarchar, 150, false, FilterMode.None);
        element.Fields.Add("owner", "Owner", FieldType.NVarchar, 64, false, FilterMode.None);
        element.Fields.Add("sync", "Sync", FieldType.Varchar, 1, false, FilterMode.Equal);
        element.Fields.Add("modified", "Last Modified", FieldType.DateTime, 15, true, FilterMode.Range);
        element.Fields.Add("json", "Object", FieldType.Text, 0, false, FilterMode.None);

        return element;
    }
}