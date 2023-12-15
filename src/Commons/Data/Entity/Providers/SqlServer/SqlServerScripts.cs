using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Data.Entity.Models;

namespace JJMasterData.Commons.Data.Entity.Providers;

public class SqlServerScripts
{
    private SqlServerReadProcedureScripts ReadProcedureScripts { get; }

    private SqlServerWriteProcedureScripts WriteProcedureScripts { get; }

    public SqlServerScripts(
        SqlServerReadProcedureScripts sqlServerReadProcedureScripts,
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
    
    public string GetWriteScript(Element element)
    {
        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior is FieldBehavior.Real);
        return WriteProcedureScripts.GetWriteScript(element, fields);
    }

    public string GetReadScript(Element element)
    {
        var fields = element.Fields
            .ToList()
            .FindAll(f => f.DataBehavior is FieldBehavior.Real);
        
        return ReadProcedureScripts.GetReadScript(element, fields);
    }
    
    public static string GetCreateTableScript(Element element)
    {
        return SqlServerCreateTableScripts.GetCreateTableScript(element);
    }

    public static string GetAlterTableScript(Element element, IEnumerable<ElementField> fields)
    {
        return SqlServerAlterTableScripts.GetAlterTableScript(element, fields);
    }
}