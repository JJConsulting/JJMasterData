#nullable enable

using System.Text;
using JJMasterData.Commons.Data.Entity.Models;

namespace JJMasterData.Commons.Data.Entity.Providers;

public abstract class SqlServerScriptsBase
{
    protected const char Tab = '\t';
    
    protected static string GetTableName(Element element)
    {
        return FormatWithSchema(element.Name, element.Schema);
    }
    
    protected static string FormatWithSchema(string name, string? schema)
    {
        var sql = new StringBuilder();

        if (!string.IsNullOrEmpty(schema))
        {
            sql.AppendFormat("[{0}]", schema);
            sql.Append('.');
        }
        sql.AppendFormat("[{0}]", name);

        return sql.ToString();
    }

    private static string GetFieldDataTypeScript(ElementField field)
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
            sql.Append(')');
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
        sql.Append('[');
        sql.Append(field.Name);
        sql.Append("] ");

        sql.Append(GetFieldDataTypeScript(field));

        return sql.ToString();
    }

    public static string GetSqlDropIfExists(string objname)
    {
        var sql = new StringBuilder();
        sql.AppendLine("IF EXISTS (SELECT * ");
        sql.Append(Tab);
        sql.AppendLine("FROM sys.objects ");
        sql.Append(Tab);
        sql.Append("WHERE object_id = OBJECT_ID(N'[");
        sql.Append(objname);
        sql.AppendLine("]') ");
        sql.Append(Tab);
        sql.AppendLine("AND type in (N'P', N'PC'))");
        sql.AppendLine("BEGIN");
        sql.Append(Tab);
        sql.Append("DROP PROCEDURE [");
        sql.Append(objname);
        sql.AppendLine("]");
        sql.AppendLine("END");
        sql.AppendLine("GO");

        return sql.ToString();
    }
}