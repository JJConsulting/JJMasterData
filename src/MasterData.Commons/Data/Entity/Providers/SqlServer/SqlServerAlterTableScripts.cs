using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using JJMasterData.Commons.Data.Entity.Models;

namespace JJMasterData.Commons.Data.Entity.Providers;

public static class SqlServerAlterTableScripts
{
    [CanBeNull]
    public static string GetAlterTableScript(Element element, IEnumerable<ElementField> fields)
    {
        var elementFields = fields.ToList();
    
        if (elementFields.Count == 0)
            return null;
        
        var tableName = SqlServerScriptsHelper.GetTableName(element);

        var sb = new StringBuilder();

        var first = true;
        foreach (var field in elementFields)
        {
            if (!first)
            {
                sb.Append(",\n");
            }

            sb.Append(SqlServerScriptsHelper.GetFieldDefinition(field));
            first = false;
        }

        var alterTableScript = $"ALTER TABLE {tableName}\nADD {sb};";
        
        return alterTableScript;
    }
}