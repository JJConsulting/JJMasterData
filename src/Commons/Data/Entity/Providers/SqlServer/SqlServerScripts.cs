using System.Collections.Generic;
using JJMasterData.Commons.Data.Entity.Models;

namespace JJMasterData.Commons.Data.Entity.Providers;

public class SqlServerScripts(
    SqlServerReadProcedureScripts sqlServerReadProcedureScripts,
    SqlServerWriteProcedureScripts writeProcedureScripts)
{
    private SqlServerReadProcedureScripts ReadProcedureScripts { get; } = sqlServerReadProcedureScripts;

    private SqlServerWriteProcedureScripts WriteProcedureScripts { get; } = writeProcedureScripts;

    public string GetReadProcedureScript(Element element)
    {
        return ReadProcedureScripts.GetReadProcedureScript(element);
    }

    public string GetWriteProcedureScript(Element element)
    {
        return WriteProcedureScripts.GetWriteProcedureScript(element);
    }
    
    public static string GetWriteScript(Element element)
    {
        var fields = element.Fields.FindAll(x => x.DataBehavior is FieldBehavior.Real);
        return SqlServerWriteProcedureScripts.GetWriteScript(element, fields);
    }

    public string GetReadScript(Element element)
    {
        var fields = element.Fields.FindAll(f => f.DataBehavior is FieldBehavior.Real);
        
        return ReadProcedureScripts.GetReadScript(element, fields);
    }
    
    public static string GetCreateTableScript(Element element, List<RelationshipReference> relationships)
    {
        return SqlServerCreateTableScripts.GetCreateTableScript(element, relationships);
    }

    public static string GetAlterTableScript(Element element, IEnumerable<ElementField> fields)
    {
        return SqlServerAlterTableScripts.GetAlterTableScript(element, fields);
    }
}