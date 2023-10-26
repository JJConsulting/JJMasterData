using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Data.Entity.Models;

namespace JJMasterData.Commons.Data.Entity.Providers;

public class SqlServerAlterTableScripts : SqlServerScriptsBase
{
    
    public static string GetAlterTableScript(Element element, IEnumerable<ElementField> fields)
    {
        var elementFields = fields.ToList();
    
        if (!elementFields.Any())
        {
            return string.Empty; 
        }

        var fieldDefinitions =
            from field in elementFields
            let fieldName = field.Name
            let dataType = GetFieldDefinition(field)
            select $"{dataType}";

        var tableName = element.TableName;
        
        var fieldDefinitionsString = string.Join(",\n", fieldDefinitions);
        var alterTableScript = $"ALTER TABLE {tableName}\nADD {fieldDefinitionsString};";
    
        return alterTableScript;
    }
}