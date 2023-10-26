using System.Text;
using JJMasterData.Commons.Data.Entity.Models;

namespace JJMasterData.Commons.Data.Entity.Providers;

public abstract class SqlServerScriptsBase
{
    protected const char Tab = '\t';

    public static string GetFieldDataTypeScript(ElementField field)
    {
        var sql = new StringBuilder();
        sql.Append(field.DataType.ToString());

        if (field.DataType is FieldType.UniqueIdentifier && field.AutoNum)
        {
            sql.Append(" DEFAULT (newsequentialid())");
        }

        if (field.DataType is FieldType.Varchar or FieldType.NVarchar or FieldType.DateTime2)
        {
            sql.Append(" (");
            sql.Append(field.Size == -1 ? "MAX" : field.Size);
            sql.Append(")");
        }

        if (field.IsRequired)
            sql.Append(" NOT NULL");

        if (field.AutoNum && field.DataType is not FieldType.UniqueIdentifier)
            sql.Append(" IDENTITY ");

        return sql.ToString();
    }
    
    protected static string GetFieldDefinition(ElementField field)
    {
        var sql = new StringBuilder();
        sql.Append("[");
        sql.Append(field.Name);
        sql.Append("] ");

        sql.Append(GetFieldDataTypeScript(field));

        return sql.ToString();
    }
}