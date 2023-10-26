using System.Collections.Generic;
using JJMasterData.Commons.Data.Entity.Models;

namespace JJMasterData.Commons.Data.Entity.Providers;

public class SqlServerScripts
{
    private SqlServerReadProcedureScripts ReadProcedureScripts { get; }

    private SqlServerWriteProcedureScripts WriteProcedureScripts { get; }

    public SqlServerScripts(SqlServerReadProcedureScripts sqlServerReadProcedureScripts,
        SqlServerWriteProcedureScripts writeProcedureScripts)
    {
        ReadProcedureScripts = sqlServerReadProcedureScripts;
        WriteProcedureScripts = writeProcedureScripts;
    }

    public string GetReadProcedureScript(Element element)
    {
        return ReadProcedureScripts.GetReadProcedureScript(element);
    }

    public string GetWriteProcedureScript(Element element)
    {
        return WriteProcedureScripts.GetWriteProcedureScript(element);
    }

    public static string GetCreateTableScript(Element element)
    {
        return SqlServerCreateTableScripts.GetCreateTableScript(element);
    }

    public string GetAlterTableScript(Element element, IEnumerable<ElementField> fields)
    {
        return SqlServerAlterTableScripts.GetAlterTableScript(element, fields);
    }
}