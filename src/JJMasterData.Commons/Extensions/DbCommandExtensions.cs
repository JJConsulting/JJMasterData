using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace JJMasterData.Commons.Extensions;

public static class SqlCommandExtensions
{
    public static string ParameterValueAsSql(this SqlParameter sp)
    {
        var retval = "";

        switch (sp.SqlDbType)
        {
            case SqlDbType.Char:
            case SqlDbType.NChar:
            case SqlDbType.NText:
            case SqlDbType.NVarChar:
            case SqlDbType.Text:
            case SqlDbType.Time:
            case SqlDbType.VarChar:
            case SqlDbType.Xml:
            case SqlDbType.Date:
            case SqlDbType.DateTime:
            case SqlDbType.DateTime2:
            case SqlDbType.DateTimeOffset:
                retval = "'" + sp.Value.ToString().Replace("'", "''") + "'";
                break;
            default:
                retval = sp.Value.ToString().Replace("'", "''");
                break;
        }

        return retval;
    }
    
    /// <summary>
    /// Convert a DbCommand object to a SQL string.
    /// </summary>
    /// <param name="dbCommand"></param>
    /// <returns></returns>
    public static string CommandAsSql(this DbCommand dbCommand)
    {
        var sql = new StringBuilder();
        var firstParam = true;

        sql.AppendLine("use " + dbCommand.Connection.Database + ";");
        switch (dbCommand.CommandType)
        {
            case CommandType.StoredProcedure:
                sql.AppendLine("declare @return_value int;");

                foreach (SqlParameter sp in dbCommand.Parameters)
                {
                    if (sp.Direction is not (ParameterDirection.InputOutput or ParameterDirection.Output)) continue;
                    
                    sql.Append("declare " + sp.ParameterName + "\t" + sp.SqlDbType.ToString() + "\t= ");

                    sql.AppendLine(((sp.Direction == ParameterDirection.Output) ? "null" : sp.ParameterValueAsSql()) + ";");
                }

                sql.AppendLine("exec [" + dbCommand.CommandText + "]");

                foreach (SqlParameter sp in dbCommand.Parameters)
                {
                    if (sp.Direction == ParameterDirection.ReturnValue) continue;
                    
                    sql.Append((firstParam) ? "\t" : "\t, ");

                    if (firstParam) firstParam = false;

                    if (sp.Direction == ParameterDirection.Input)
                        sql.AppendLine(sp.ParameterName + " = " + sp.ParameterValueAsSql());
                    else

                        sql.AppendLine(sp.ParameterName + " = " + sp.ParameterName + " output");
                }
                sql.AppendLine(";");

                sql.AppendLine("select 'Return Value' = convert(varchar, @return_value);");

                foreach (SqlParameter sp in dbCommand.Parameters)
                {
                    if ((sp.Direction == ParameterDirection.InputOutput) || (sp.Direction == ParameterDirection.Output))
                    {
                        sql.AppendLine("select '" + sp.ParameterName + "' = convert(varchar, " + sp.ParameterName + ");");
                    }
                }
                break;
            case CommandType.Text:
                sql.AppendLine(dbCommand.CommandText);
                break;
        }

        return sql.ToString();
    }

}