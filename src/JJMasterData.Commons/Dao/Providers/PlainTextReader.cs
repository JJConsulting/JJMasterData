using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;

namespace JJMasterData.Commons.Dao.Providers;

internal class PlainTextReader
{
    private BaseProvider _provider;
    public bool ShowLogInfo { get; set; }
    public string Delimiter { get; set; }


    public PlainTextReader(BaseProvider provider)
    {
        _provider = provider;
        Delimiter = "|";
    }


    public string GetListFieldsAsText(Element element, Hashtable filters, string orderby, int regporpag, int pag)
    {
        var sRet = new StringBuilder();
        var dStart = DateTime.Now;
        var culture = CultureInfo.CreateSpecificCulture("en-US");
        var dataAccess = _provider.DataAccess;
        string currentField = null;
        DbConnection conn = null;

        try
        {
            var pTot = new DataAccessParameter(_provider.VariablePrefix + "qtdtotal", 1, DbType.Int32, 0,
                ParameterDirection.InputOutput);
            var cmd = _provider.GetCommandRead(element, filters, orderby, regporpag, pag, ref pTot);
            var providerFactory = SqlClientFactory.Instance;
            conn = providerFactory.CreateConnection();
            if (conn == null)
                throw new Exception("Error on create connection object");

            conn.ConnectionString = dataAccess.ConnectionString;
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
            dbCmd.CommandTimeout = dataAccess.TimeOut;
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
                            sRet.Append(Delimiter);

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
                                        .Replace(Delimiter, "&#124;");
                                    sRet.Append(val);
                                    break;
                                case FieldType.Int:
                                    sRet.Append(dr.GetInt32(ordinal));
                                    break;
                                case FieldType.Float:
                                    sRet.Append(double.Parse(dr.GetValue(ordinal).ToString()).ToString("G", culture));
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
            if (ShowLogInfo)
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

}

