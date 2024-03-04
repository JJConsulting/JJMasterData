using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Data.Entity.Providers;

public class PlainTextReader(EntityProviderBase provider, ILogger<PlainTextReader> logger)
{
    private ILogger<PlainTextReader> Logger { get; } = logger;
    public bool ShowLogInfo { get; init; }
    public string Delimiter { get; init; } = "|";


    public async Task<string> GetFieldsListAsTextAsync(Element element, EntityParameters entityParameters)
    {
        var sRet = new StringBuilder();
        var dStart = DateTime.Now;
        var culture = CultureInfo.CreateSpecificCulture("en-US");
        var dataAccess = provider.DataAccess;
        string currentField = null;
        DbConnection conn = null;

        var (filters, _, currentPage, recordsPerPage) = entityParameters;

        try
        {
            var totalParameter = new DataAccessParameter($"{provider.VariablePrefix}qtdtotal", 1, DbType.Int32, 0,
                ParameterDirection.InputOutput);
            var cmd = provider.GetReadCommand(element, entityParameters, totalParameter);
            var providerFactory = SqlClientFactory.Instance;
            conn = providerFactory.CreateConnection();
            if (conn == null)
                throw new JJMasterDataException("Error on create connection object");

            conn.ConnectionString = dataAccess.ConnectionString;
            await conn.OpenAsync();
            int qtd = 0;
            using (var dbCmd = providerFactory.CreateCommand())
            {
                if (dbCmd == null)
                    throw new JJMasterDataException("Error on create DbCommand");

                foreach (DataAccessParameter parm in cmd.Parameters)
                {
                    DbParameter oPar = providerFactory.CreateParameter();
                    if (oPar == null)
                        throw new JJMasterDataException("Error on create DbParameter");

                    oPar.DbType = parm.Type;
                    oPar.Value = parm.Value ?? DBNull.Value;
                    oPar.ParameterName = parm.Name;
                    dbCmd.Parameters.Add(oPar);
                }

                dbCmd.CommandType = cmd.Type;
                dbCmd.CommandText = cmd.Sql;
                dbCmd.Connection = conn;
                dbCmd.CommandTimeout = dataAccess.TimeOut;

                using (var dr = await dbCmd.ExecuteReaderAsync())
                {
                    int col = 0;
          
                    var columns = new Dictionary<string, int>();

                    if (dr.HasRows)
                    {
                        for (int i = 0; i < dr.FieldCount; i++)
                        {
                            columns.Add(dr.GetName(i), i);
                        }

                        while (await dr.ReadAsync())
                        {
                            qtd++;
                            foreach (ElementField field in element.Fields)
                            {
                                currentField = field.Name;
                                if (!columns.ContainsKey(field.Name))
                                    throw new JJMasterDataException($"{field.Name} field not found");

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
                                            sRet.Append(double.Parse(dr.GetValue(ordinal).ToString() ?? string.Empty)
                                                .ToString("G", culture));
                                            break;
                                        default:
                                            sRet.Append(dr.GetValue(ordinal).ToString()?.TrimEnd());
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
                }
            }

            if (ShowLogInfo)
            {
                var ts = DateTime.Now - dStart;
                var logMessage = new StringBuilder();
                logMessage.Append("Synchronizing");
                logMessage.Append(" ");
                logMessage.AppendLine(element.Name);

                if (filters.Any())
                {
                    logMessage.Append("- ");
                    logMessage.Append("Filters");
                    logMessage.Append(": ");

                    foreach (var filter in filters)
                    {
                        logMessage.Append("  ");
                        logMessage.Append(filter.Key);
                        logMessage.Append("=");
                        logMessage.Append(filter.Value);
                    }

                    logMessage.AppendLine("");
                }

                logMessage.Append("- ");
                logMessage.Append("TotPerPage");
                logMessage.Append(": ");
                logMessage.AppendLine(recordsPerPage.ToString());
                logMessage.Append("- ");
                logMessage.Append("CurrentPage");
                logMessage.Append(": ");
                logMessage.AppendLine(currentPage.ToString());

                logMessage.AppendLine($"{qtd.ToString()} records sync. Time {ts.TotalMilliseconds:N3}ms");
                Logger.LogInformation("Log message {LogMessage}", logMessage.ToString());
            }
        }
        catch (Exception ex)
        {
            var message = new StringBuilder();

            message.AppendLine("Error synchronizing.");
            message.AppendLine($"Object: {element.Name}");
            message.AppendLine($"Page: {currentPage}");
            message.AppendLine($"Field: {currentField}");
            message.AppendLine($"Exception: {ex.Message}");
            message.AppendLine($"Stacktrace: {ex.StackTrace}");


            var exception = new JJMasterDataException(message.ToString(), ex);

            throw exception;
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