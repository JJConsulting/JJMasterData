#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Util;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Data.Providers;

public class SqlServerProvider : BaseProvider
{
    private const string InsertInitial = "I";
    private const string UpdateInitial = "A";
    private const string DeleteInitial = "E";
    private const char Tab = '\t';
    public override string VariablePrefix => "@";
    public override DataAccessProvider DataAccessProvider => DataAccessProvider.SqlServer;


    public SqlServerProvider(DataAccess dataAccess, JJMasterDataCommonsOptions options, ILoggerFactory loggerFactory) : base(dataAccess, options,loggerFactory)
    {
    }

    public override string GetCreateTableScript(Element element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (element.Fields == null || element.Fields.Count == 0)
            throw new ArgumentNullException(nameof(Element.Fields));

        var sql = new StringBuilder();
        var keys = new StringBuilder();

        sql.Append("CREATE TABLE [");
        sql.Append(element.TableName);
        sql.AppendLine("] (");

        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

        bool isFirst = true;
        foreach (var f in fields)
        {
            if (isFirst)
                isFirst = false;
            else
                sql.AppendLine(",");

            sql.Append(Tab);
            sql.Append(GetFieldDefinition(f));

            if (!f.IsPk) continue;
            if (keys.Length > 0)
                keys.Append(",");

            keys.Append("[");
            keys.Append(f.Name);
            keys.Append("] ASC ");
        }

        if (keys.Length > 0)
        {
            sql.AppendLine(", ");
            sql.Append(Tab);
            sql.Append("CONSTRAINT [PK_");
            sql.Append(element.TableName);
            sql.Append("] PRIMARY KEY NONCLUSTERED (");
            sql.Append(keys);
            sql.Append(")");
        }

        sql.AppendLine("");
        sql.AppendLine(")");

        sql.AppendLine("");

        sql.AppendLine(GetRelationshipsScript(element));
        sql.AppendLine("");

        int counter = 1;
        if (element.Indexes.Count > 0)
        {
            foreach (var index in element.Indexes)
            {
                sql.Append("CREATE");
                sql.Append(index.IsUnique ? " UNIQUE" : "");
                sql.Append(index.IsClustered ? " CLUSTERED" : " NONCLUSTERED");
                sql.Append(" INDEX [IX_");
                sql.Append(element.TableName);
                sql.Append("_");
                sql.Append(counter);
                sql.Append("] ON ");
                sql.AppendLine(element.TableName);

                sql.Append(Tab);
                sql.AppendLine("(");
                for (int i = 0; i < index.Columns.Count; i++)
                {
                    if (i > 0)
                        sql.AppendLine(", ");

                    sql.Append(Tab);
                    sql.Append(index.Columns[i]);
                }
                sql.AppendLine("");
                sql.Append(Tab);
                sql.AppendLine(")");
                sql.AppendLine("GO");
                counter++;
            }
        }

        return sql.ToString();
    }

    private static string GetFieldDefinition(ElementField field)
    {
        var sql = new StringBuilder();
        sql.Append("[");
        sql.Append(field.Name);
        sql.Append("] ");

        sql.Append(GetFieldDataTypeScript(field));

        return sql.ToString();
    }

    public static string GetFieldDataTypeScript(ElementField field)
    {
        var sql = new StringBuilder();
        sql.Append(field.DataType.ToString());
        if (field.DataType is FieldType.Varchar or FieldType.NVarchar or FieldType.DateTime2)
        {
            sql.Append(" (");
            sql.Append(field.Size == -1 ? "MAX" : field.Size);
            sql.Append(")");
        }

        if (field.IsRequired)
            sql.Append(" NOT NULL");

        if (field.AutoNum)
            sql.Append(" IDENTITY ");

        return sql.ToString();
    }

    private static string GetRelationshipsScript(Element element)
    {
        var sql = new StringBuilder();

        if (element.Relationships.Count > 0)
        {
            sql.AppendLine("-- RELATIONSHIPS");
            var listContraint = new List<string>();
            foreach (var r in element.Relationships)
            {
                string contraintName = $"FK_{r.ChildElement}_{element.TableName}";

                //Prevents repeated name.
                if (!listContraint.Contains(contraintName))
                {
                    listContraint.Add(contraintName);
                }
                else
                {
                    bool hasContraint = true;

                    int counter = 1;
                    while (hasContraint)
                    {
                        if (!listContraint.Contains(contraintName + counter))
                        {
                            contraintName = contraintName + counter;
                            listContraint.Add(contraintName);
                            hasContraint = false;
                        }
                        counter++;
                    }
                }

                sql.Append("ALTER TABLE ");
                sql.AppendLine(r.ChildElement);
                sql.Append("ADD CONSTRAINT [");
                sql.Append(contraintName);
                sql.AppendLine("] ");
                sql.Append(Tab);
                sql.Append("FOREIGN KEY (");

                for (int rc = 0; rc < r.Columns.Count; rc++)
                {
                    if (rc > 0)
                        sql.Append(", ");

                    sql.Append("[");
                    sql.Append(r.Columns[rc].FkColumn);
                    sql.Append("]");
                }
                sql.AppendLine(")");
                sql.Append(Tab);
                sql.Append("REFERENCES ");
                sql.Append(element.TableName);
                sql.Append(" (");
                for (int rc = 0; rc < r.Columns.Count; rc++)
                {
                    if (rc > 0)
                        sql.Append(", ");

                    sql.Append("[");
                    sql.Append(r.Columns[rc].PkColumn);
                    sql.Append("]");
                }
                sql.Append(")");

                if (r.UpdateOnCascade)
                {
                    sql.AppendLine("");
                    sql.Append(Tab).Append(Tab);
                    sql.Append("ON UPDATE CASCADE ");
                }

                if (r.DeleteOnCascade)
                {
                    sql.AppendLine("");
                    sql.Append(Tab).Append(Tab);
                    sql.Append("ON DELETE CASCADE ");
                }

                sql.AppendLine("");
                sql.AppendLine("GO");
            }
        }

        return sql.ToString();
    }

    public override string GetWriteProcedureScript(Element element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (element.Fields == null || element.Fields.Count == 0)
            throw new ArgumentNullException(nameof(Element.Fields));

        StringBuilder sql = new StringBuilder();

        bool updateScript = HasUpdateFields(element);
        string procedureFinalName = Options.GetWriteProcedureName(element);
        var pks = element.Fields.ToList().FindAll(x => x.IsPk);

        sql.AppendLine(GetSqlDropIfExists(procedureFinalName));

        sql.Append("CREATE PROCEDURE [");
        sql.Append(procedureFinalName);
        sql.AppendLine("] ");
        sql.AppendLine("@action varchar(1), ");

        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

        foreach (var f in fields)
        {
            sql.Append("@");
            sql.Append(f.Name);
            sql.Append(" ");
            sql.Append(f.DataType);
            if (f.DataType is FieldType.Varchar or FieldType.NVarchar or FieldType.DateTime2)
            {
                sql.Append("(");
                sql.Append(f.Size == -1 ? "MAX" : f.Size);
                sql.Append(")");
            }
            sql.AppendLine(", ");

        }
        sql.AppendLine("@RET INT OUTPUT ");
        sql.AppendLine("AS ");
        sql.AppendLine("BEGIN ");
        sql.Append(Tab);
        sql.AppendLine("DECLARE @TYPEACTION VARCHAR(1) ");
        sql.Append(Tab);
        sql.AppendLine("SET @TYPEACTION = @action ");
        sql.Append(Tab);
        sql.AppendLine("IF @TYPEACTION = ' ' ");
        sql.Append(Tab);
        sql.AppendLine("BEGIN ");
        sql.Append(Tab).Append(Tab);
        sql.AppendLine($"SET @TYPEACTION = '{InsertInitial}' ");

        bool isFirst = true;
        if (pks.Count > 0)
        {
            sql.Append(Tab).Append(Tab);
            sql.AppendLine("DECLARE @NCOUNT INT ");
            sql.AppendLine(" ");

            //Check
            sql.Append(Tab).Append(Tab);
            sql.AppendLine("SELECT @NCOUNT = COUNT(*) ");
            sql.Append(Tab).Append(Tab);
            sql.Append("FROM ");
            sql.Append(element.TableName);
            sql.AppendLine(" WITH (NOLOCK) ");

            foreach (var f in pks)
            {
                if (isFirst)
                {
                    sql.Append(Tab).Append(Tab);
                    sql.Append("WHERE ");
                    isFirst = false;
                }
                else
                {
                    sql.Append(Tab).Append(Tab);
                    sql.Append("AND ");
                }

                sql.Append(f.Name);
                sql.Append(" = @");
                sql.AppendLine(f.Name);
            }
            sql.AppendLine(" ");
            sql.Append(Tab).Append(Tab);
            sql.AppendLine("IF @NCOUNT > 0 ");
            sql.Append(Tab).Append(Tab);
            sql.AppendLine("BEGIN ");
            sql.Append(Tab).Append(Tab).Append(Tab);
            sql.AppendLine($"SET @TYPEACTION = '{UpdateInitial}'");
            sql.Append(Tab).Append(Tab);
            sql.AppendLine("END ");
        }
        sql.Append(Tab);
        sql.AppendLine("END ");
        sql.AppendLine(" ");


        //Insert Script
        sql.Append(Tab);
        sql.AppendLine($"IF @TYPEACTION = '{InsertInitial}' ");
        sql.Append(Tab);
        sql.AppendLine("BEGIN ");
        sql.Append(Tab).Append(Tab);
        sql.Append("INSERT INTO [");
        sql.Append(element.TableName);
        sql.AppendLine("] (");

        isFirst = true;

        foreach (var f in fields.Where(f => !f.AutoNum))
        {
            if (isFirst)
                isFirst = false;
            else
                sql.AppendLine(",");

            sql.Append("			");
            sql.Append(f.Name);
        }
        sql.AppendLine(")");
        sql.Append(Tab).Append(Tab);
        sql.AppendLine("VALUES (");

        isFirst = true;
        foreach (var f in fields.Where(f => !f.AutoNum))
        {
            if (isFirst)
                isFirst = false;
            else
                sql.AppendLine(",");

            sql.Append(Tab).Append(Tab).Append(Tab);
            sql.Append("@");
            sql.Append(f.Name);
        }
        sql.AppendLine(")");
        sql.Append(Tab).Append(Tab);
        sql.AppendLine("SET @RET = 0; ");

        var autonum = pks.FindAll(x => x.AutoNum);
        foreach (var f in autonum)
        {
            sql.Append(Tab);
            sql.Append("SELECT @@IDENTITY AS ");
            sql.Append(f.Name);
            sql.AppendLine(";");
            break;
        }

        sql.Append(Tab);
        sql.AppendLine("END ");

        //Update Script
        if (updateScript)
        {
            sql.Append(Tab);
            sql.AppendLine($"ELSE IF @TYPEACTION = '{UpdateInitial}' ");
            sql.Append(Tab);
            sql.AppendLine("BEGIN ");
            sql.Append(Tab).Append(Tab);
            sql.Append("UPDATE [");
            sql.Append(element.TableName);
            sql.AppendLine("] SET ");

            isFirst = true;
            foreach (var f in fields.Where(f => !f.IsPk))
            {
                if (isFirst)
                    isFirst = false;
                else
                    sql.AppendLine(", ");

                sql.Append(Tab).Append(Tab).Append(Tab);
                sql.Append(f.Name);
                sql.Append(" = @");
                sql.Append(f.Name);
            }
            sql.AppendLine("");

            isFirst = true;
            foreach (var f in fields.Where(f => f.IsPk))
            {
                if (isFirst)
                {
                    sql.Append(Tab).Append(Tab);
                    sql.Append("WHERE ");
                    isFirst = false;
                }
                else
                {
                    sql.Append(Tab).Append(Tab);
                    sql.Append("AND ");
                }

                sql.Append(f.Name);
                sql.Append(" = @");
                sql.AppendLine(f.Name);
            }

            sql.Append(Tab).Append(Tab);
            sql.AppendLine("SET @RET = 1; ");
            sql.Append(Tab);
            sql.AppendLine("END ");
        }
        else
        {
            sql.Append(Tab);
            sql.AppendLine($"ELSE IF @TYPEACTION = '{UpdateInitial}' ");
            sql.Append(Tab);
            sql.AppendLine("BEGIN ");
            sql.Append(Tab).Append(Tab);
            sql.AppendLine("--NO UPDATABLED");
            sql.Append(Tab).Append(Tab);
            sql.AppendLine("SET @RET = 1; ");
            sql.Append(Tab);
            sql.AppendLine("END ");
        }

        //Delete Script
        sql.Append(Tab);
        sql.AppendLine($"ELSE IF @TYPEACTION = '{DeleteInitial}' ");
        sql.Append(Tab);
        sql.AppendLine("BEGIN ");
        sql.Append(Tab).Append(Tab);
        sql.Append("DELETE FROM [");
        sql.Append(element.TableName);
        sql.AppendLine("] ");

        isFirst = true;
        foreach (var f in fields.Where(f => f.IsPk && f.EnableOnDelete))
        {
            if (isFirst)
            {
                sql.Append(Tab).Append(Tab);
                sql.Append("WHERE ");
                isFirst = false;
            }
            else
            {
                sql.Append(Tab).Append(Tab);
                sql.Append("AND ");
            }

            sql.Append(f.Name);
            sql.Append(" = @");
            sql.AppendLine(f.Name);
        }

        sql.Append(Tab).Append(Tab);
        sql.AppendLine("SET @RET = 2; ");
        sql.Append(Tab);
        sql.AppendLine("END ");
        sql.AppendLine(" ");
        sql.AppendLine("END ");
        sql.AppendLine("GO ");

        return sql.ToString();
    }

    public override string GetReadProcedureScript(Element element)
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

        //Se exisitir apaga
        sql.AppendLine(GetSqlDropIfExists(procedureFinalName));

        //Criando proc
        sql.Append("CREATE PROCEDURE [");
        sql.Append(procedureFinalName);
        sql.AppendLine("] ");
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
            else if (f.Filter.Type == FilterMode.MultValuesContain ||
                     f.Filter.Type == FilterMode.MultValuesEqual)
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
                    sql.AppendFormat("SET @likein = @likein + '{0} LIKE ' + CHAR(39) + '%' + SUBSTRING(@{0}, 1, CHARINDEX(',', @{0}) -1) + '%' + CHAR(39);", f.Name);
                    sql.AppendLine("");
                    sql.Append(Tab, 3);
                    sql.AppendFormat("SET @{0} = RIGHT(@{0} , LEN(@{0}) - CHARINDEX(',', @{0}));", f.Name);
                    sql.AppendLine("");
                    sql.Append(Tab, 3);
                    sql.AppendLine("SET @likein = @likein + ' OR ';");
                    sql.Append(Tab, 2);
                    sql.AppendLine("END");
                    sql.Append(Tab, 2);
                    sql.AppendFormat("SET @likein = @likein  + '{0} LIKE ' + CHAR(39) + '%' + @{0} + '%' + CHAR(39) + ' ) '", f.Name);
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
                    sql.AppendFormat("SET @likein = @likein + CHAR(39) + SUBSTRING(@{0},1,CHARINDEX(',',@{0}) -1) + CHAR(39);", f.Name);
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
                            else if (f.DataType is FieldType.Date or FieldType.DateTime or FieldType.DateTime2)
                            {
                                sql.Append(" = ' + CHAR(39) + CAST(@");
                                sql.Append(f.Name);
                                sql.AppendLine(" AS VARCHAR) +  CHAR(39)");
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

    public override DataAccessCommand GetInsertCommand(Element element, IDictionary<string,object?> values)
    {
        return GetWriteCommand(InsertInitial, element, values);
    }

    public override DataAccessCommand GetUpdateCommand(Element element, IDictionary<string,object?> values)
    {
        return GetWriteCommand(UpdateInitial, element, values);
    }

    public override DataAccessCommand GetDeleteCommand(Element element, IDictionary<string,object> filters)
    {
        return GetWriteCommand(DeleteInitial, element, filters!);
    }

    protected override DataAccessCommand GetInsertOrReplaceCommand(Element element, IDictionary<string,object?> values)
    {
        return GetWriteCommand(string.Empty, element, values);
    }

    public override string GetAlterTableScript(Element element, IEnumerable<ElementField> fields)
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

    public override DataAccessCommand GetReadCommand(Element element, EntityParameters parameters, DataAccessParameter totalOfRecordsParameter)
    {
       var command = new DataAccessCommand
        {
            CmdType = CommandType.StoredProcedure,
            Sql = Options.GetReadProcedureName(element),
            Parameters = new List<DataAccessParameter>
            {
                new("@orderby", parameters.OrderBy.ToQueryParameter()),
                new("@regporpag", parameters.RecordsPerPage),
                new("@pag", parameters.CurrentPage)
            }
        };

        foreach (var field in element.Fields)
        {
            if (field.Filter.Type == FilterMode.Range)
            {
                object? valueFrom = DBNull.Value;
                if (parameters.Filters.ContainsKey($"{field.Name}_from") &&
                    parameters.Filters[$"{field.Name}_from"] != null)
                {
                    valueFrom = parameters.Filters[$"{field.Name}_from"];
                    if (valueFrom != null)
                        valueFrom = StringManager.ClearText(valueFrom.ToString());
                }
                var fromParameter = new DataAccessParameter
                {
                    Direction = ParameterDirection.Input,
                    Type = GetDbType(field.DataType),
                    Size = field.Size,
                    Name = $"{field.Name}_from",
                    Value = valueFrom
                };
                command.Parameters.Add(fromParameter);

                object? valueTo = DBNull.Value;
                if (parameters.Filters.ContainsKey($"{field.Name}_to") &&
                    parameters.Filters[$"{field.Name}_to"] != null)
                {
                    valueTo = parameters.Filters[$"{field.Name}_to"];
                    if (valueTo != null)
                        valueTo = StringManager.ClearText(valueTo.ToString());
                }
                var pTo = new DataAccessParameter();
                pTo.Direction = ParameterDirection.Input;
                pTo.Type = GetDbType(field.DataType);
                pTo.Size = field.Size;
                pTo.Name = $"{field.Name}_to";
                pTo.Value = valueTo;
                command.Parameters.Add(pTo);
            }
            else if (field.Filter.Type != FilterMode.None || field.IsPk)
            {
                object value = GetElementValue(field, parameters.Filters);
                if (value != DBNull.Value)
                    value = StringManager.ClearText(value.ToString());

                var dbType = GetDbType(field.DataType);
                var parameter = new DataAccessParameter
                {
                    Direction = ParameterDirection.Input,
                    Type = dbType,
                    Size = field.Size,
                    Name = field.Name,
                    Value = value
                };
                command.Parameters.Add(parameter);
            }
        }

        command.Parameters.Add(totalOfRecordsParameter);

        return command;
    }


    private DataAccessCommand GetWriteCommand(string action, Element element, IDictionary<string,object?> values)
    {
        DataAccessCommand cmd = new DataAccessCommand
        {
            CmdType = CommandType.StoredProcedure,
            Sql = Options.GetWriteProcedureName(element)
        };
        cmd.Parameters.Add(new DataAccessParameter("@action", action, DbType.String, 1));

        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

        foreach (var f in fields)
        {
            object value = GetElementValue(f, values);
            var param = new DataAccessParameter();
            param.Name = $"@{f.Name}";
            param.Size = f.Size;
            param.Value = value;
            param.Type = GetDbType(f.DataType);
            cmd.Parameters.Add(param);
        }

        var pRet = new DataAccessParameter();
        pRet.Direction = ParameterDirection.Output;
        pRet.Name = "@RET";
        pRet.Value = 0;
        pRet.Type = DbType.Int32;
        cmd.Parameters.Add(pRet);

        return cmd;
    }


    private static object GetElementValue(ElementField f, IDictionary<string,object?> values)
    {
        if (!values.ContainsKey(f.Name)) 
            return DBNull.Value;
        
        object? value = values[f.Name];
        if (value == null)
            return DBNull.Value;
        
        if (f.DataType is FieldType.Date or FieldType.DateTime or FieldType.Float or FieldType.Int &&
            string.IsNullOrEmpty(value.ToString()))
        {
            return DBNull.Value;
        }

        if (f.DataType is FieldType.Bit)
            return StringManager.ParseBool(values[f.Name]);
        
        return value;
    }

    private static bool HasUpdateFields(Element element)
    {
        return element.Fields.Any(f => !f.IsPk && f.DataBehavior == FieldBehavior.Real);
    }

    private static string GetSqlDropIfExists(string objname)
    {
        StringBuilder sSql = new StringBuilder();
        sSql.AppendLine("IF  EXISTS (SELECT * ");
        sSql.Append(Tab).Append(Tab).Append(Tab);
        sSql.AppendLine("FROM sys.objects ");
        sSql.Append(Tab).Append(Tab).Append(Tab);
        sSql.Append("WHERE object_id = OBJECT_ID(N'[");
        sSql.Append(objname);
        sSql.AppendLine("]') ");
        sSql.Append(Tab).Append(Tab).Append(Tab);
        sSql.AppendLine("AND type in (N'P', N'PC'))");
        sSql.AppendLine("BEGIN");
        sSql.Append(Tab);
        sSql.Append("DROP PROCEDURE [");
        sSql.Append(objname);
        sSql.AppendLine("]");
        sSql.AppendLine("END");
        sSql.AppendLine("GO");

        return sSql.ToString();
    }

    private static DbType GetDbType(FieldType dataType)
    {
        return dataType switch
        {
            FieldType.Date => DbType.Date,
            FieldType.DateTime => DbType.DateTime,
            FieldType.DateTime2 => DbType.DateTime,
            FieldType.Float => DbType.Double,
            FieldType.Int => DbType.Int32,
            FieldType.Bit => DbType.Boolean,
            _ => DbType.String
        };
    }

    private static FieldType GetDataType(string databaseType)
    {
        if (string.IsNullOrEmpty(databaseType))
            return FieldType.NVarchar;

        databaseType = databaseType.ToLower().Trim();

        if (databaseType.Equals("varchar") ||
            databaseType.Equals("char"))
            return FieldType.Varchar;

        if (databaseType.Equals("bit"))
            return FieldType.Bit;
        
        if (databaseType.Equals("nvarchar") ||
            databaseType.Equals("nchar"))
            return FieldType.NVarchar;

        if (databaseType.Equals("int") ||
            databaseType.Equals("bigint") ||
            databaseType.Equals("tinyint"))
            return FieldType.Int;

        if (databaseType.Equals("float") ||
            databaseType.Equals("numeric") ||
            databaseType.Equals("money") ||
            databaseType.Equals("smallmoney") ||
            databaseType.Equals("real"))
            return FieldType.Float;

        if (databaseType.Equals("date"))
            return FieldType.Date;

        if (databaseType.Equals("datetime"))
            return FieldType.DateTime;
        
        if (databaseType.Equals("datetime2"))
            return FieldType.DateTime2;

        if (databaseType.Equals("text"))
            return FieldType.Text;

        if (databaseType.Equals("ntext"))
            return FieldType.NText;

        return FieldType.NVarchar;
    }
    
    public override async Task<Element> GetElementFromTableAsync(string tableName)
    {
        if (string.IsNullOrEmpty(tableName))
            throw new ArgumentNullException(nameof(tableName));

        if (!await DataAccess.TableExistsAsync(tableName))
            throw new JJMasterDataException($"Table {tableName} not found");

        var element = new Element
        {
            Name = tableName
        };

        var cmdFields = new DataAccessCommand
        {
            CmdType = CommandType.StoredProcedure,
            Sql = "sp_columns"
        };
        cmdFields.Parameters.Add(new DataAccessParameter("@table_name", tableName));

        var dtFields = await DataAccess.GetDataTableAsync(cmdFields);
        if (dtFields.Rows.Count == 0)
            throw new JJMasterDataException($"Table {tableName} has invalid structure");

        foreach (DataRow row in dtFields.Rows)
        {
            var field = new ElementField
            {
                Name = row["COLUMN_NAME"].ToString()!.Replace(" ","_"),
                Label = (string)row["COLUMN_NAME"],
                Size = (int)row["LENGTH"],
                AutoNum = ((string)row["TYPE_NAME"]).ToUpper().Contains("IDENTITY"),
                IsRequired = row["NULLABLE"].ToString()?.Equals("0") ?? false,
                DataType = GetDataType((string)row["TYPE_NAME"])
            };

            element.Fields.Add(field);
        }

        //Primary Keys
        var cmdPks = new DataAccessCommand
        {
            CmdType = CommandType.StoredProcedure,
            Sql = "sp_pkeys"
        };

        cmdPks.Parameters.Add(new DataAccessParameter("@table_name", tableName));
        var dtPks = await DataAccess.GetDataTableAsync(cmdPks);
        foreach (DataRow row in dtPks.Rows)
        {
            element.Fields[row["COLUMN_NAME"].ToString()].IsPk = true;
        }

        return element;
    }
    
}