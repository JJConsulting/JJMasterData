using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity.Models;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Data.Entity.Providers;

public class SqlServerReadProcedureScripts
{
    private MasterDataCommonsOptions Options { get; }
    private const char Tab = '\t';

    public SqlServerReadProcedureScripts(IOptions<MasterDataCommonsOptions> options)
    {
        Options = options.Value;
    }
    
    public string GetReadProcedureScript(Element element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (element.Fields == null || element.Fields.Count == 0)
            throw new ArgumentNullException(nameof(Element.Fields));

        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior != FieldBehavior.Virtual);

        var sql = new StringBuilder();
        string procedureFinalName = Options.GetReadProcedureName(element);

        sql.Append("CREATE OR ALTER PROCEDURE [");
        sql.Append(procedureFinalName);
        sql.AppendLine("] ");
        GetReadProcedureFieldsDefinition(sql, fields);
        sql.AppendLine("AS ");
        sql.AppendLine("BEGIN ");
        sql.Append(Tab);
        sql.AppendLine("DECLARE @sqlcolumn   NVARCHAR(MAX)");
        sql.Append(Tab);
        sql.AppendLine("DECLARE @sqltable    NVARCHAR(MAX)");
        sql.Append(Tab);
        sql.AppendLine("DECLARE @sqlcond     NVARCHAR(MAX)");
        sql.Append(Tab);
        sql.AppendLine("DECLARE @sqlorder    NVARCHAR(MAX)");
        sql.Append(Tab);
        sql.AppendLine("DECLARE @sqloffset   NVARCHAR(MAX)");
        sql.Append(Tab);
        sql.AppendLine("DECLARE @query       NVARCHAR(MAX)");
        sql.Append(Tab);

        if (fields.Exists(x => x.Filter.Type == FilterMode.MultValuesContain ||
                               x.Filter.Type == FilterMode.MultValuesEqual))
        {
            sql.AppendLine("DECLARE @likein      NVARCHAR(MAX)");
            sql.Append(Tab);
        }

        sql.AppendLine("DECLARE @count       INT");
        sql.AppendLine("");

        sql.Append(Tab);
        sql.AppendLine("--COLUMNS");
        sql.Append(Tab);
        sql.AppendLine("SET @sqlcolumn = '");

        int index = 1;
        foreach (var f in fields)
        {
            sql.Append(Tab).Append(Tab);
            sql.Append("");
            if (f.DataBehavior == FieldBehavior.ViewOnly)
            {
                sql.Append("NULL AS ");
                sql.Append(f.Name);
                if (index != fields.Count)
                    sql.Append(", ");

                sql.AppendLine(" --TODO");
            }
            else
            {
                sql.Append(f.Name);

                sql.AppendLine(index != fields.Count ? ", " : "");
            }

            index++;
        }

        sql.Append(Tab);
        sql.AppendLine(" '");

        sql.Append(Tab);
        sql.AppendLine("--TABLES");
        sql.Append(Tab);
        sql.Append("SET @sqltable = 'FROM ");
        sql.Append(element.TableName);
        sql.AppendLine(" WITH (NOLOCK)'");
        sql.AppendLine("");

        sql.Append(Tab);
        sql.AppendLine("--CONDITIONALS");
        sql.Append(Tab);
        sql.AppendLine("SET @sqlcond = ' WHERE 1=1 '");

        foreach (var f in fields)
        {
            if (f.Filter.Type != FilterMode.None || f.IsPk)
            {
                if (f.DataBehavior == FieldBehavior.ViewOnly)
                {
                    sql.AppendLine("");
                    sql.Append(Tab);
                    sql.AppendLine("/*");
                    sql.Append("TODO: FILTER ");
                    sql.AppendLine(f.Name);
                }
            }

            switch (f.Filter.Type)
            {
                case FilterMode.Range:
                {
                    sql.AppendLine("");

                    if (f.DataType is FieldType.Date or FieldType.DateTime or FieldType.DateTime2)
                    {
                        sql.Append(Tab);
                        sql.Append("IF @");
                        sql.Append(f.Name);
                        sql.AppendLine("_from IS NOT NULL");
                        sql.Append(Tab).Append(Tab);
                        sql.Append("SET @sqlcond = @sqlcond + ' AND CONVERT(DATE, ");
                        sql.Append(f.Name);
                        sql.Append(") BETWEEN ' + CHAR(39) + CONVERT(VARCHAR(10), @");
                        sql.Append(f.Name);
                        sql.Append("_from, 112) + CHAR(39) + ' AND ' + CHAR(39) + CONVERT(VARCHAR(10), @");
                        sql.Append(f.Name);
                        sql.AppendLine("_to, 112) + CHAR(39)");
                    }
                    else
                    {
                        sql.Append(Tab);
                        sql.Append("IF @");
                        sql.Append(f.Name);
                        sql.AppendLine("_from IS NOT NULL");
                        sql.Append(Tab).Append(Tab);
                        sql.Append("SET @sqlcond = @sqlcond + ' AND ");
                        sql.Append(f.Name);
                        sql.Append(" BETWEEN ' + CHAR(39) + @");
                        sql.Append(f.Name);
                        sql.Append("_from + CHAR(39) + ' AND ' + CHAR(39) + @");
                        sql.Append(f.Name);
                        sql.AppendLine(" + CHAR(39)");
                    }

                    break;
                }
                case FilterMode.Contain:
                    sql.AppendLine("");
                    sql.Append(Tab);
                    sql.Append("IF @");
                    sql.Append(f.Name);
                    sql.AppendLine(" IS NOT NULL");
                    sql.Append(Tab).Append(Tab);
                    sql.Append("SET @sqlcond = @sqlcond + ' AND ");
                    sql.Append(f.Name);
                    sql.Append(" LIKE ' + CHAR(39) + '%' + @");
                    sql.Append(f.Name);
                    sql.AppendLine(" +  '%' + CHAR(39)");
                    break;
                case FilterMode.MultValuesContain:
                    sql.AppendLine("");
                    sql.Append(Tab);
                    sql.Append("IF @");
                    sql.Append(f.Name);
                    sql.AppendLine(" IS NOT NULL");
                    sql.Append(Tab);
                    sql.AppendLine("BEGIN");
                    sql.Append(Tab, 2);
                    sql.AppendLine("SET @likein = ' AND ( '");
                    sql.Append(Tab, 2);
                    sql.AppendFormat("WHILE CHARINDEX(',', @{0}) <> 0", f.Name);
                    sql.AppendLine("");
                    sql.Append(Tab, 2);
                    sql.AppendLine("BEGIN");
                    sql.Append(Tab, 3);
                    sql.AppendFormat(
                        "SET @likein = @likein + '{0} LIKE ' + CHAR(39) + '%' + SUBSTRING(@{0}, 1, CHARINDEX(',', @{0}) -1) + '%' + CHAR(39);",
                        f.Name);
                    sql.AppendLine("");
                    sql.Append(Tab, 3);
                    sql.AppendFormat("SET @{0} = RIGHT(@{0} , LEN(@{0}) - CHARINDEX(',', @{0}));", f.Name);
                    sql.AppendLine("");
                    sql.Append(Tab, 3);
                    sql.AppendLine("SET @likein = @likein + ' OR ';");
                    sql.Append(Tab, 2);
                    sql.AppendLine("END");
                    sql.Append(Tab, 2);
                    sql.AppendFormat(
                        "SET @likein = @likein  + '{0} LIKE ' + CHAR(39) + '%' + @{0} + '%' + CHAR(39) + ' ) '", f.Name);
                    sql.AppendLine("");
                    sql.Append(Tab, 2);
                    sql.AppendLine("SET @sqlcond = @sqlcond + @likein");
                    sql.Append(Tab);
                    sql.AppendLine("END");
                    break;
                case FilterMode.MultValuesEqual:
                    sql.AppendLine("");
                    sql.Append(Tab);
                    sql.Append("IF @");
                    sql.Append(f.Name);
                    sql.AppendLine(" IS NOT NULL");
                    sql.Append(Tab);
                    sql.AppendLine("BEGIN");
                    sql.Append(Tab, 2);
                    sql.AppendFormat("SET @likein = ' AND {0} IN ('", f.Name);
                    sql.AppendLine("");
                    sql.Append(Tab, 2);
                    sql.AppendFormat("WHILE CHARINDEX(',', @{0}) <> 0", f.Name);
                    sql.AppendLine("");
                    sql.Append(Tab, 2);
                    sql.AppendLine("BEGIN");
                    sql.Append(Tab, 3);
                    sql.AppendFormat(
                        "SET @likein = @likein + CHAR(39) + SUBSTRING(@{0},1,CHARINDEX(',',@{0}) -1) + CHAR(39);", f.Name);
                    sql.AppendLine("");
                    sql.Append(Tab, 3);
                    sql.AppendFormat("SET @{0} = RIGHT(@{0} , LEN(@{0}) - CHARINDEX(',', @{0}));", f.Name);
                    sql.AppendLine("");
                    sql.Append(Tab, 3);
                    sql.AppendLine("SET @likein = @likein + ', ';");
                    sql.Append(Tab, 2);
                    sql.AppendLine("END");
                    sql.Append(Tab, 2);
                    sql.AppendFormat("SET @likein = @likein + CHAR(39) + @{0} + CHAR(39) + ') '", f.Name);
                    sql.AppendLine("");
                    sql.Append(Tab, 2);
                    sql.AppendLine("SET @sqlcond = @sqlcond + @likein");
                    sql.Append(Tab);
                    sql.AppendLine("END");
                    break;
                default:
                {
                    if (f.Filter.Type == FilterMode.Equal || f.IsPk)
                    {
                        sql.AppendLine("");
                        sql.Append(Tab);
                        sql.Append("IF @");
                        sql.Append(f.Name);
                        sql.AppendLine(" IS NOT NULL");
                        sql.Append(Tab).Append(Tab);
                        sql.Append("SET @sqlcond = @sqlcond + ' AND ");
                        sql.Append(f.Name);

                        if (f.DataType is FieldType.Int or FieldType.Float or FieldType.Bit)
                        {
                            sql.Append(" = ' + CONVERT(VARCHAR, @");
                            sql.Append(f.Name);
                            sql.AppendLine(")");
                        }
                        else if (f.DataType is FieldType.Date or FieldType.DateTime or FieldType.DateTime2
                                 or FieldType.UniqueIdentifier)
                        {
                            sql.Append(" = ' + CHAR(39) + CAST(@");
                            sql.Append(f.Name);
                            sql.AppendLine(" AS VARCHAR(MAX)) +  CHAR(39)");
                        }
                        else
                        {
                            sql.Append(" = ' + CHAR(39) + @");
                            sql.Append(f.Name);
                            sql.AppendLine(" +  CHAR(39)");
                        }
                    }

                    break;
                }
            }

            if (f.Filter.Type == FilterMode.None && !f.IsPk) continue;
            if (f.DataBehavior != FieldBehavior.ViewOnly) continue;

            sql.Append(Tab);
            sql.AppendLine("*/");
        }

        sql.AppendLine("");
        sql.Append(Tab);
        sql.AppendLine("--ORDER");
        sql.Append(Tab);
        var listPk = fields.FindAll(x => x.IsPk);
        if (listPk.Count == 0)
        {
            sql.Append("SET @sqlorder  = ' ORDER BY ");
            sql.Append(fields[0].Name);
            sql.AppendLine("'");
        }
        else
        {
            sql.Append("SET @sqlorder  = ' ORDER BY ");
            sql.Append(listPk[0].Name);
            sql.AppendLine("'");
        }

        sql.Append(Tab);
        sql.AppendLine("IF @orderby IS NOT NULL AND @orderby <> ''");
        sql.Append(Tab);
        sql.AppendLine("BEGIN");
        sql.Append(Tab);
        sql.Append(Tab);
        sql.AppendLine("SET @sqlorder  = ' ORDER BY ' + @orderby");
        sql.Append(Tab);
        sql.AppendLine("END");
        sql.AppendLine("");

        sql.Append(Tab);
        sql.AppendLine("--PAGING");
        sql.Append(Tab);
        sql.AppendLine("SET @sqloffset = ' '");
        sql.Append(Tab);
        sql.AppendLine("SET @sqloffset = @sqloffset + 'OFFSET ('");
        sql.Append(Tab);
        sql.AppendLine("SET @sqloffset = @sqloffset + RTRIM(CONVERT(VARCHAR(10), @pag - 1))");
        sql.Append(Tab);
        sql.AppendLine("SET @sqloffset = @sqloffset + ' * '");
        sql.Append(Tab);
        sql.AppendLine("SET @sqloffset = @sqloffset + RTRIM(CONVERT(VARCHAR(10), @regporpag))");
        sql.Append(Tab);
        sql.AppendLine("SET @sqloffset = @sqloffset + ') ROWS FETCH NEXT '");
        sql.Append(Tab);
        sql.AppendLine("SET @sqloffset = @sqloffset + RTRIM(CONVERT(VARCHAR(10), @regporpag))");
        sql.Append(Tab);
        sql.AppendLine("SET @sqloffset = @sqloffset + ' ROWS ONLY '");
        sql.AppendLine("");


        sql.Append(Tab);
        sql.AppendLine("--TOTAL OF RECORDS");
        sql.Append(Tab);
        sql.AppendLine("IF @qtdtotal is null or @qtdtotal = 0");
        sql.Append(Tab);
        sql.AppendLine("BEGIN");
        sql.Append(Tab).Append(Tab);
        sql.AppendLine("SET @qtdtotal = 0;");
        sql.Append(Tab).Append(Tab);
        sql.AppendLine("SET @query = N'SELECT @count = COUNT(*) ' + @sqltable + @sqlcond");
        sql.Append(Tab).Append(Tab);
        sql.AppendLine("EXECUTE sp_executesql @query, N'@count int output', @count = @qtdtotal output");
        sql.Append(Tab);
        sql.AppendLine("END");
        sql.AppendLine("");

        sql.Append(Tab);
        sql.AppendLine("--DATASET RESULT");
        sql.Append(Tab);

        sql.AppendLine("SET @query = N'SELECT ' + @sqlcolumn + @sqltable + @sqlcond + @sqlorder + @sqloffset");
        sql.Append(Tab);
        sql.AppendLine("EXECUTE sp_executesql @query ");
        sql.Append(Tab);
        sql.AppendLine("--PRINT(@query)");

        sql.AppendLine("");
        sql.AppendLine("END");

        return sql.ToString();
    }
        private static void GetReadProcedureFieldsDefinition(StringBuilder sql, List<ElementField> fields)
    {
        sql.AppendLine("@orderby NVARCHAR(MAX), ");

        foreach (var f in fields)
        {
            if (f.Filter.Type == FilterMode.Range)
            {
                sql.Append("@");
                sql.Append(f.Name);
                sql.Append("_from ");
                sql.Append(f.DataType.ToString());
                if (f.DataType is FieldType.Varchar or FieldType.NVarchar or FieldType.DateTime2)
                {
                    sql.Append("(");
                    sql.Append(f.Size == -1 ? "MAX" : f.Size);
                    sql.Append(")");
                }

                sql.AppendLine(",");
                sql.Append("@");
                sql.Append(f.Name);
                sql.Append("_to ");
                sql.Append(f.DataType.ToString());
                if (f.DataType is FieldType.Varchar or FieldType.NVarchar or FieldType.DateTime2)
                {
                    sql.Append("(");
                    sql.Append(f.Size == -1 ? "MAX" : f.Size);
                    sql.Append(") ");
                }

                sql.AppendLine(",");
            }
            else if (f.Filter.Type is FilterMode.MultValuesContain or FilterMode.MultValuesEqual)
            {
                sql.Append("@");
                sql.Append(f.Name);
                sql.Append(" ");
                sql.Append(f.DataType.ToString());
                sql.AppendLine("(MAX),");
            }
            else if (f.Filter.Type != FilterMode.None || f.IsPk)
            {
                sql.Append("@");
                sql.Append(f.Name);
                sql.Append(" ");
                sql.Append(f.DataType.ToString());
                if (f.DataType is FieldType.Varchar or FieldType.NVarchar or FieldType.DateTime2)
                {
                    sql.Append("(");
                    sql.Append(f.Size == -1 ? "MAX" : f.Size);
                    sql.AppendLine("), ");
                }
                else
                {
                    sql.AppendLine(", ");
                }
            }
        }

        sql.AppendLine("@regporpag INT, ");
        sql.AppendLine("@pag INT, ");
        sql.AppendLine("@qtdtotal INT OUTPUT ");
    }

}