using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JJMasterData.Commons.Data.Entity.Providers;

public class SqlServerReadProcedureScripts(
    IOptions<MasterDataCommonsOptions> options,
    SqlServerInfo sqlServerInfo)
    : SqlServerScriptsBase
{
    private MasterDataCommonsOptions Options { get; } = options.Value;
    private SqlServerInfo SqlServerInfo { get; } = sqlServerInfo;

    public string GetReadProcedureScript(Element element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (element.Fields == null || element.Fields.Count == 0)
            throw new ArgumentNullException(nameof(Element.Fields));

        var fields = element.Fields
            .ToList()
            .FindAll(f => f.DataBehavior is not FieldBehavior.Virtual);

        var sql = new StringBuilder();
        string procedureFinalName = Options.GetReadProcedureName(element);

        if (SqlServerInfo.GetCompatibilityLevel() >= 130)
        {
            sql.Append("CREATE OR ALTER PROCEDURE [");
        }
        else
        {
            sql.AppendLine(GetSqlDropIfExists(procedureFinalName));
            sql.Append("CREATE PROCEDURE [");
        }

        sql.Append(procedureFinalName);
        sql.AppendLine("] ");
        sql.AppendLine("@orderby VARCHAR(MAX), ");
        sql.AppendLine(GetParameters(fields, addMasterDataParameters: true));
        sql.AppendLine("AS ");
        sql.AppendLine("BEGIN ");
        sql.Append(GetReadScript(element, fields));
        sql.AppendLine("END");

        return sql.ToString();
    }

    internal string GetReadScript(Element element, List<ElementField> fields)
    {
        var sql = new StringBuilder();
        sql.Append(Tab);
        sql.AppendLine("DECLARE @sqlColumn   NVARCHAR(MAX)");
        sql.Append(Tab);
        sql.AppendLine("DECLARE @sqlTable    NVARCHAR(MAX)");
        sql.Append(Tab);
        sql.AppendLine("DECLARE @sqlWhere     NVARCHAR(MAX)");
        sql.Append(Tab);
        sql.AppendLine("DECLARE @sqlOrderBy    NVARCHAR(MAX)");
        sql.Append(Tab);
        sql.AppendLine("DECLARE @sqlOffset   NVARCHAR(MAX)");
        sql.Append(Tab);
        sql.AppendLine("DECLARE @query       NVARCHAR(MAX)");
        sql.Append(Tab);

        if (fields.Exists(f => f.Filter.Type is FilterMode.MultValuesContain or FilterMode.MultValuesEqual))
        {
            sql.AppendLine("DECLARE @sqlLikeIn      NVARCHAR(MAX)");
            sql.Append(Tab);
        }

        sql.AppendLine("DECLARE @count       INT");
        sql.AppendLine("");

        sql.Append(Tab);
        sql.AppendLine("--COLUMNS");
        sql.Append(Tab);
        sql.AppendLine("SET @sqlColumn = '");

        int index = 1;
        foreach (var field in fields)
        {
            sql.Append(Tab).Append(Tab);
            sql.Append("");
            if (field.DataBehavior == FieldBehavior.ViewOnly)
            {
                sql.Append("NULL AS ");
                sql.Append($"[{field.Name}]");
                if (index != fields.Count)
                    sql.Append(", ");

                sql.AppendLine(" --TODO");
            }
            else
            {
                sql.Append($"[{field.Name}]");

                sql.AppendLine(index != fields.Count ? ", " : "");
            }

            index++;
        }

        sql.Append(Tab);
        sql.AppendLine(" '");

        sql.Append(Tab);
        sql.AppendLine("--TABLES");
        sql.Append(Tab);
        sql.Append("SET @sqlTable = 'FROM ");
        sql.Append(element.TableName);
        sql.AppendLine(" WITH (NOLOCK)'");
        sql.AppendLine("");

        sql.Append(Tab);
        sql.AppendLine("--CONDITIONALS");
        sql.Append(Tab);
        sql.AppendLine("SET @sqlWhere = ' WHERE 1=1 '");

        foreach (var field in fields)
        {
            if (field.Filter.Type != FilterMode.None || field.IsPk)
            {
                if (field.DataBehavior == FieldBehavior.ViewOnly)
                {
                    sql.AppendLine("");
                    sql.Append(Tab);
                    sql.AppendLine("/*");
                    sql.Append("TODO: FILTER ");
                    sql.AppendLine($"[{field.Name}]");
                }
            }

            switch (field.Filter.Type)
            {
                case FilterMode.Range:
                {
                    sql.AppendLine("");

                    if (field.DataType is FieldType.Date or FieldType.DateTime or FieldType.DateTime2)
                    {
                        sql.Append(Tab);
                        sql.Append("IF @");
                        sql.Append(field.Name);
                        sql.AppendLine("_from IS NOT NULL");
                        sql.Append(Tab, 2);
                        sql.Append("SET @sqlWhere = @sqlWhere + ' AND CONVERT(DATE, ");
                        sql.Append($"[{field.Name}]");
                        sql.Append(") BETWEEN CONVERT(VARCHAR(10), @");
                        sql.Append(field.Name);
                        sql.Append("_from, 112) AND CONVERT(VARCHAR(10), @");
                        sql.Append(field.Name);
                        sql.AppendLine("_to, 112) '");
                    }
                    else
                    {
                        sql.Append(Tab);
                        sql.Append("IF @");
                        sql.Append(field.Name);
                        sql.AppendLine("_from IS NOT NULL");
                        sql.Append(Tab, 2);
                        sql.Append("SET @sqlWhere = @sqlWhere + ' AND ");
                        sql.Append($"[{field.Name}]");
                        sql.Append(" BETWEEN @");
                        sql.Append(field.Name);
                        sql.Append("_from AND  @");
                        sql.Append(field.Name);
                        sql.AppendLine("_TO");
                        sql.AppendLine("'");
                    }

                    break;
                }
                case FilterMode.Contain:
                    sql.AppendLine("");
                    sql.Append(Tab);
                    sql.Append("IF @");
                    sql.Append(field.Name);
                    sql.AppendLine(" IS NOT NULL");
                    sql.Append(Tab, 2);
                    sql.Append("SET @sqlWhere = @sqlWhere + ' AND ");
                    sql.Append($"[{field.Name}]");
                    sql.Append($" LIKE  ''%'' + @{field.Name} + ''%'' '");
                    break;
                case FilterMode.MultValuesContain:
                    sql.Append(GetFilterMultValuesContains(field.Name));
                    break;
                case FilterMode.MultValuesEqual:
                    sql.AppendLine();
                    sql.Append(Tab);
                    sql.Append("IF @");
                    sql.Append(field.Name);
                    sql.AppendLine(" IS NOT NULL");
                    sql.Append(Tab,2);
                    sql.Append("SET @sqlWhere = @sqlWhere + ' AND ");
                    sql.Append($"CHARINDEX([{field.Name}], @{field.Name}) > 0'");
                    break;
                default:
                {
                    if (field.Filter.Type == FilterMode.Equal || field.IsPk)
                    {
                        sql.AppendLine("");
                        sql.Append(Tab);
                        sql.Append("IF @");
                        sql.Append(field.Name);
                        sql.AppendLine(" IS NOT NULL");
                        sql.Append(Tab,2);
                        sql.Append("SET @sqlWhere = @sqlWhere + ' AND ");
                        sql.Append($"[{field.Name}] = @{field.Name}'");
                    }

                    break;
                }
            }

            if (field.Filter.Type == FilterMode.None && !field.IsPk)
                continue;
            if (field.DataBehavior != FieldBehavior.ViewOnly)
                continue;

            sql.Append(Tab);
            sql.AppendLine("*/");
        }

        sql.AppendLine("");
        sql.Append(Tab);
        sql.AppendLine("--ORDER BY");
        sql.Append(Tab);
        var listPk = fields.FindAll(x => x.IsPk);
        if (listPk.Count == 0)
        {
            sql.Append("SET @sqlOrderBy  = ' ORDER BY ");
            sql.Append($"[{fields[0].Name}]");
            sql.AppendLine("'");
        }
        else
        {
            sql.Append("SET @sqlOrderBy  = ' ORDER BY ");
            sql.Append($"[{listPk[0].Name}]");
            sql.AppendLine("'");
        }

        sql.Append(Tab);
        sql.AppendLine("IF @orderby IS NOT NULL AND @orderby <> ''");
        sql.Append(Tab);
        sql.AppendLine("BEGIN");
        sql.Append(Tab);
        sql.Append(Tab);
        sql.AppendLine("SET @sqlOrderBy  = ' ORDER BY ' + @orderby");
        sql.Append(Tab);
        sql.AppendLine("END");
        sql.AppendLine();

        sql.Append(Tab);
        sql.AppendLine("--PAGINATION");
        sql.Append(Tab);

        sql.AppendLine("IF @pag < 1");
        sql.Append(Tab, 2);
        sql.Append("SET @pag = 1");
        sql.AppendLine();
        sql.Append(Tab);
        sql.AppendLine("SET @sqlOffset = ' '");
        sql.Append(Tab);
        sql.AppendLine("SET @sqlOffset = @sqlOffset + ' OFFSET ('");
        sql.Append(Tab);
        sql.AppendLine("SET @sqlOffset = @sqlOffset + '(@pag - 1)'");
        sql.Append(Tab);
        sql.AppendLine("SET @sqlOffset = @sqlOffset + ' * '");
        sql.Append(Tab);
        sql.AppendLine("SET @sqlOffset = @sqlOffset + '@regporpag'");
        sql.Append(Tab);
        sql.AppendLine("SET @sqlOffset = @sqlOffset + ') ROWS FETCH NEXT '");
        sql.Append(Tab);
        sql.AppendLine("SET @sqlOffset = @sqlOffset + '@regporpag'");
        sql.Append(Tab);
        sql.AppendLine("SET @sqlOffset = @sqlOffset + ' ROWS ONLY '");
        sql.AppendLine("");


        sql.Append(Tab);
        sql.AppendLine("--TOTAL OF RECORDS");
        sql.Append(Tab);
        sql.AppendLine("IF @qtdtotal is null or @qtdtotal = 0");
        sql.Append(Tab);
        sql.AppendLine("BEGIN");
        sql.Append(Tab, 2);
        sql.AppendLine("SET @qtdtotal = 0;");
        sql.Append(Tab, 2);
        sql.AppendLine("SET @query = N'SELECT @count = COUNT(*) ' + @sqlTable + @sqlWhere");
        sql.Append(Tab, 2);
        sql.AppendLine("EXECUTE sp_executesql @query,");
        sql.Append(Tab, 2);
        sql.Append("N'");
        sql.Append(GetParameters(fields, addMasterDataParameters: false, tabLevel: 2));
        sql.Append("@count int output',");
        sql.AppendLine();
        sql.Append(Tab, 2);
        sql.Append(GetFilterParametersScript(fields, tabCount: 2));
        sql.Append("@count = @qtdtotal output");
        sql.AppendLine();
        sql.Append(Tab);
        sql.AppendLine("END");
        sql.AppendLine();

        sql.Append(Tab);
        sql.AppendLine("--DATASET RESULT");
        sql.Append(Tab);

        sql.AppendLine("SET @query = N'SELECT ' + @sqlColumn + @sqlTable + @sqlWhere + @sqlOrderBy + @sqlOffset");
        sql.Append(Tab);
        sql.AppendLine("EXECUTE sp_executesql @query,");
        sql.Append(Tab);
        sql.Append("N'");
        sql.Append(GetParameters(fields, addMasterDataParameters: true, tabLevel: 1));
        sql.Append("'");
        sql.Append(",");
        sql.AppendLine();
        sql.Append(Tab);
        sql.AppendLine();
        sql.Append(Tab);
        sql.Append(GetFilterParametersScript(fields));
        sql.AppendLine("@regporpag,");
        sql.Append(Tab);
        sql.AppendLine("@pag,");
        sql.Append(Tab);
        sql.AppendLine("@qtdtotal");
        sql.Append(Tab);
        sql.AppendLine();

        return sql.ToString();
    }

    private static string GetFilterParametersScript(IEnumerable<ElementField> fields, int tabCount = 1)
    {
        StringBuilder sql = new();
        foreach (var field in fields.Where(IsFilter))
        {
            if (field.Filter.Type is FilterMode.Range)
            {
                sql.AppendLine($"@{field.Name}_from,");
                sql.Append(Tab, tabCount);
                sql.AppendLine($"@{field.Name}_to,");
                sql.Append(Tab, tabCount);
            }
            else
            {
                sql.AppendLine($"@{field.Name},");
                sql.Append(Tab, tabCount);
            }
        }

        return sql.ToString();
    }

    private static bool IsFilter(ElementField field)
    {
        return field.Filter.Type != FilterMode.None || field.IsPk;
    }

    private static string GetParameters(List<ElementField> fields, bool addMasterDataParameters, int tabLevel = 0)
    {
        var sql = new StringBuilder();

        foreach (var field in fields)
        {
            switch (field.Filter.Type)
            {
                case FilterMode.Range:
                {
                    sql.Append("@");
                    sql.Append(field.Name);
                    sql.Append("_from ");
                    sql.Append(field.DataType.ToString());
                    if (field.DataType is FieldType.Varchar or FieldType.NVarchar or FieldType.DateTime2)
                    {
                        sql.Append("(");
                        sql.Append(field.Size == -1 ? "MAX" : field.Size);
                        sql.Append(")");
                    }

                    sql.AppendLine(",");
                    sql.Append(Tab, tabLevel);
                    sql.Append("@");
                    sql.Append(field.Name);
                    sql.Append("_to ");
                    sql.Append(field.DataType.ToString());
                    if (field.DataType is FieldType.Varchar or FieldType.NVarchar or FieldType.DateTime2)
                    {
                        sql.Append("(");
                        sql.Append(field.Size == -1 ? "MAX" : field.Size);
                        sql.Append(") ");
                    }
                    sql.AppendLine(",");
                    sql.Append(Tab, tabLevel);
                    break;
                }
                case FilterMode.MultValuesContain or FilterMode.MultValuesEqual:
                    sql.Append("@");
                    sql.Append(field.Name);
                    sql.Append(" ");
                    sql.Append("VARCHAR");
                    sql.AppendLine("(MAX),");
                    sql.Append(Tab, tabLevel);
                    break;
                default:
                {
                    if (IsFilter(field))
                    {
                        sql.Append("@");
                        sql.Append(field.Name);
                        sql.Append(" ");
                        sql.Append(field.DataType.ToString());
                        if (field.DataType is FieldType.Varchar or FieldType.NVarchar or FieldType.DateTime2)
                        {
                            sql.Append("(");
                            sql.Append(field.Size == -1 ? "MAX" : field.Size);
                            sql.AppendLine("), ");
                            sql.Append(Tab, tabLevel);
                        }
                        else
                        {
                            sql.AppendLine(", ");
                            sql.Append(Tab, tabLevel);
                        }
                    }

                    break;
                }
            }
        }

        if (addMasterDataParameters)
        {
            sql.AppendLine("@regporpag INT, ");
            sql.Append(Tab, tabLevel);
            sql.AppendLine("@pag INT, ");
            sql.Append(Tab, tabLevel);
            sql.Append("@qtdtotal INT OUTPUT ");
        }

        return sql.ToString();
    }

    private string GetFilterMultValuesContains(string fieldName)
    {
        var sql = new StringBuilder();
        sql.AppendLine();
        sql.Append(Tab);
        sql.Append("IF @");
        sql.Append(fieldName);
        sql.AppendLine(" IS NOT NULL");
        sql.Append(Tab);
        sql.AppendLine("BEGIN");
        

        if (SqlServerInfo.GetCompatibilityLevel() >= 130)
        {
            sql.Append(Tab, 2);
            sql.Append("SET @sqlWhere = @sqlWhere + ' AND ");
            sql.Append($"""
                                       EXISTS (
                                           SELECT 1
                                           FROM STRING_SPLIT(@{fieldName}, '','') AS s
                                           WHERE [{fieldName}] LIKE ''%'' + s.value + ''%''
                                        )'
                               """);
        }
        else
        {
            sql.Append(Tab, 2);
            sql.AppendLine("DECLARE @likein      NVARCHAR(MAX)");
            sql.Append(Tab, 2);
            sql.AppendLine("SET @likein = ' AND ( '");
            sql.Append(Tab, 2);
            sql.AppendFormat("WHILE CHARINDEX(',', @{0}) <> 0", fieldName);
            sql.AppendLine("");
            sql.Append(Tab, 2);
            sql.AppendLine("BEGIN");
            sql.Append(Tab, 3);
            sql.AppendFormat("SET @likein = @likein + '{0} LIKE ' + CHAR(39) + '%' + SUBSTRING(@{0}, 1, CHARINDEX(',', @{0}) -1) + '%' + CHAR(39);", fieldName);
            sql.AppendLine("");
            sql.Append(Tab, 3);
            sql.AppendFormat("SET @{0} = RIGHT(@{0} , LEN(@{0}) - CHARINDEX(',', @{0}));", fieldName);
            sql.AppendLine("");
            sql.Append(Tab, 3);
            sql.AppendLine("SET @likein = @likein + ' OR ';");
            sql.Append(Tab, 2);
            sql.AppendLine("END");
            sql.Append(Tab, 2);
            sql.AppendFormat("SET @likein = @likein  + '{0} LIKE ' + CHAR(39) + '%' + @{0} + '%' + CHAR(39) + ' ) '", fieldName);
            sql.AppendLine("");
            sql.Append(Tab, 2);
            sql.AppendLine("SET @sqlWhere = @sqlWhere + @likein");
        }

        sql.Append(Tab);
        sql.AppendLine("END");

        return sql.ToString();

    }

}