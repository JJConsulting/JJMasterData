using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JJMasterData.Commons.Data.Entity.Models;

namespace JJMasterData.Commons.Data.Entity.Providers;

public class SqlServerAlterTableScripts : SqlServerScriptsBase
{
    
    [CanBeNull]
    public static string GetAlterTableScript(Element element, IEnumerable<ElementField> fields)
    {
        var elementFields = fields.ToList();
    
        if (elementFields.Count == 0)
        {
            return null;
        }

        var fieldDefinitions =
            from field in elementFields
            let fieldName = field.Name
            let dataType = GetFieldDefinition(field)
            select $"{dataType}";

        var tableName = GetTableName(element);
        
        var fieldDefinitionsString = string.Join(",\n", fieldDefinitions);
        var alterTableScript = $"ALTER TABLE {tableName}\nADD {fieldDefinitionsString};";
    
        return alterTableScript;
    }
}