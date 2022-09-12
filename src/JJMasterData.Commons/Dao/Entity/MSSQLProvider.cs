using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Settings;
using JJMasterData.Commons.Util;

namespace JJMasterData.Commons.Dao.Entity;

public class MSSQLProvider : IProvider
{
    private const string INSERT = "I";
    private const string UPDATE = "A";
    private const string DELETE = "E";
    private const char TAB = '\t';

    public string VariablePrefix
    {
        get
        {
            return "@";
        }
    }

    public string GetCreateTableScript(Element element)
    {
        if (element == null)
            throw new Exception("Invalid element");

        if (element.Fields == null || element.Fields.Count == 0)
            throw new Exception("Invalid fields");

        var sql = new StringBuilder();
        var sKeys = new StringBuilder();

        sql.Append("CREATE TABLE [");
        sql.Append(element.TableName);
        sql.AppendLine("] (");
        bool isFirst = true;
        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

        foreach (var f in fields)
        {
            if (isFirst)
                isFirst = false;
            else
                sql.AppendLine(",");

            sql.Append(TAB);
            sql.Append("[");
            sql.Append(f.Name);
            sql.Append("] ");
            sql.Append(f.DataType.ToString());
            if (f.DataType == FieldType.Varchar ||
                f.DataType == FieldType.NVarchar)
            {
                sql.Append(" (");
                sql.Append(f.Size);
                sql.Append(")");
            }

            if (f.IsRequired)
                sql.Append(" NOT NULL");

            if (f.AutoNum)
                sql.Append(" IDENTITY ");

            if (f.IsPk)
            {
                if (sKeys.Length > 0)
                    sKeys.Append(",");

                sKeys.Append("[");
                sKeys.Append(f.Name);
                sKeys.Append("] ASC ");
            }
        }

        if (sKeys.Length > 0)
        {
            sql.AppendLine(", ");
            sql.Append(TAB);
            sql.Append("CONSTRAINT [PK_");
            sql.Append(element.TableName);
            sql.Append("] PRIMARY KEY NONCLUSTERED (");
            sql.Append(sKeys);
            sql.Append(")");
        }

        sql.AppendLine("");
        sql.AppendLine(")");
        //sSql.AppendLine("GO");
        sql.AppendLine("");

        sql.AppendLine(GetRelationScript(element));
        sql.AppendLine("");

        int nIndex = 1;
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
                sql.Append(nIndex);
                sql.Append("] ON ");
                sql.AppendLine(element.TableName);

                sql.Append(TAB);
                sql.AppendLine("(");
                for (int i = 0; i < index.Columns.Count; i++)
                {
                    if (i > 0)
                        sql.AppendLine(", ");

                    sql.Append(TAB);
                    sql.Append(index.Columns[i]);
                }
                sql.AppendLine("");
                sql.Append(TAB);
                sql.AppendLine(")");
                sql.AppendLine("GO");
                nIndex++;
            }
        }

        return sql.ToString();
    }

    private string GetRelationScript(Element element)
    {
        StringBuilder sql = new StringBuilder();

        if (element.Relations.Count > 0)
        {
            sql.AppendLine("-- RELATIONS");
            var listContraint = new List<string>();
            foreach (var r in element.Relations)
            {
                string contraintName = string.Format("FK_{0}_{1}", r.ChildElement, element.TableName);

                //Tratamento para nome repedido de contraint
                if (!listContraint.Contains(contraintName))
                {
                    listContraint.Add(contraintName);
                }
                else
                {
                    bool hasContraint = true;
                    int nCount = 1;
                    while (hasContraint)
                    {
                        if (!listContraint.Contains(contraintName + nCount))
                        {
                            contraintName = contraintName + nCount;
                            listContraint.Add(contraintName);
                            hasContraint = false;
                        }
                        nCount++;
                    }
                }

                sql.Append("ALTER TABLE ");
                sql.AppendLine(r.ChildElement);
                sql.Append("ADD CONSTRAINT [");
                sql.Append(contraintName);
                sql.AppendLine("] ");
                sql.Append(TAB);
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
                sql.Append(TAB);
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
                    sql.Append(TAB).Append(TAB);
                    sql.Append("ON UPDATE CASCADE ");
                }

                if (r.DeleteOnCascade)
                {
                    sql.AppendLine("");
                    sql.Append(TAB).Append(TAB);
                    sql.Append("ON DELETE CASCADE ");
                }

                sql.AppendLine("");
                sql.AppendLine("GO");
            }
        }

        return sql.ToString();
    }
           
    public string GetWriteProcedureScript(Element element)
    {
        if (element == null)
            throw new Exception("Invalid element");

        if (element.Fields == null || element.Fields.Count == 0)
            throw new Exception("Invalid fields");

        StringBuilder sSql = new StringBuilder();
        bool isFirst = true;
        bool hasUpd = HasUpdateFields(element);
        string procedureFinalName = JJMasterDataSettings.GetProcNameSet(element);
        var pks = element.Fields.ToList().FindAll(x => x.IsPk);

        //Se exisitir apaga
        sSql.AppendLine(GetSqlDropIfExist(procedureFinalName));

        //Criando proc
        sSql.Append("CREATE PROCEDURE [");
        sSql.Append(procedureFinalName);
        sSql.AppendLine("] ");
        sSql.AppendLine("@action varchar(1), ");

        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

        foreach (var f in fields)
        {
            sSql.Append("@");
            sSql.Append(f.Name);
            sSql.Append(" ");
            sSql.Append(f.DataType);
            if (f.DataType == FieldType.Varchar ||
                f.DataType == FieldType.NVarchar)
            {
                sSql.Append("(");
                sSql.Append(f.Size);
                sSql.Append(")");
            }
            sSql.AppendLine(", ");

        }
        sSql.AppendLine("@RET INT OUTPUT ");
        sSql.AppendLine("AS ");
        sSql.AppendLine("BEGIN ");
        sSql.Append(TAB);
        sSql.AppendLine("DECLARE @TYPEACTION VARCHAR(1) ");
        sSql.Append(TAB);
        sSql.AppendLine("SET @TYPEACTION = @action ");
        sSql.Append(TAB);
        sSql.AppendLine("IF @TYPEACTION = ' ' ");
        sSql.Append(TAB);
        sSql.AppendLine("BEGIN ");
        sSql.Append(TAB).Append(TAB);
        sSql.AppendLine("SET @TYPEACTION = '" + INSERT + "' ");

        if (pks.Count > 0)
        {
            sSql.Append(TAB).Append(TAB);
            sSql.AppendLine("DECLARE @NCOUNT INT ");
            sSql.AppendLine(" ");

            //CHECK
            sSql.Append(TAB).Append(TAB);
            sSql.AppendLine("SELECT @NCOUNT = COUNT(*) ");
            sSql.Append(TAB).Append(TAB);
            sSql.Append("FROM ");
            sSql.Append(element.TableName);
            sSql.AppendLine(" WITH (NOLOCK) ");
            isFirst = true;
            foreach (var f in pks)
            {
                if (isFirst)
                {
                    sSql.Append(TAB).Append(TAB);
                    sSql.Append("WHERE ");
                    isFirst = false;
                }
                else
                {
                    sSql.Append(TAB).Append(TAB);
                    sSql.Append("AND ");
                }

                sSql.Append(f.Name);
                sSql.Append(" = @");
                sSql.AppendLine(f.Name);
            }
            sSql.AppendLine(" ");
            sSql.Append(TAB).Append(TAB);
            sSql.AppendLine("IF @NCOUNT > 0 ");
            sSql.Append(TAB).Append(TAB);
            sSql.AppendLine("BEGIN ");
            sSql.Append(TAB).Append(TAB).Append(TAB);
            sSql.AppendLine("SET @TYPEACTION = '" + UPDATE + "'");
            sSql.Append(TAB).Append(TAB);
            sSql.AppendLine("END ");
        }
        sSql.Append(TAB);
        sSql.AppendLine("END ");
        sSql.AppendLine(" ");


        //SCRIPT INSERT
        sSql.Append(TAB);
        sSql.AppendLine("IF @TYPEACTION = '" + INSERT + "' ");
        sSql.Append(TAB);
        sSql.AppendLine("BEGIN ");
        sSql.Append(TAB).Append(TAB);
        sSql.Append("INSERT INTO [");
        sSql.Append(element.TableName);
        sSql.AppendLine("] (");
        isFirst = true;
        foreach (var f in fields)
        {
            if (!f.AutoNum)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sSql.AppendLine(",");

                sSql.Append("			");
                sSql.Append(f.Name);
            }
        }
        sSql.AppendLine(")");
        sSql.Append(TAB).Append(TAB);
        sSql.AppendLine("VALUES (");
        isFirst = true;
        foreach (var f in fields)
        {
            if (!f.AutoNum)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sSql.AppendLine(",");

                sSql.Append(TAB).Append(TAB).Append(TAB);
                sSql.Append("@");
                sSql.Append(f.Name);
            }
        }
        sSql.AppendLine(")");
        sSql.Append(TAB).Append(TAB);
        sSql.AppendLine("SET @RET = 0; ");

        var autonum = pks.FindAll(x => x.AutoNum);
        foreach (var f in autonum)
        {
            sSql.Append(TAB);
            sSql.Append("SELECT @@IDENTITY AS ");
            sSql.Append(f.Name);
            sSql.AppendLine(";");
            break;
        }

        sSql.Append(TAB);
        sSql.AppendLine("END ");

        //SCRIPT UPDATE
        isFirst = true;
        if (hasUpd)
        {
            sSql.Append(TAB);
            sSql.AppendLine("ELSE IF @TYPEACTION = '" + UPDATE + "' ");
            sSql.Append(TAB);
            sSql.AppendLine("BEGIN ");
            sSql.Append(TAB).Append(TAB);
            sSql.Append("UPDATE [");
            sSql.Append(element.TableName);
            sSql.AppendLine("] SET ");
            isFirst = true;
            foreach (var f in fields)
            {
                if (!f.IsPk)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        sSql.AppendLine(", ");

                    sSql.Append(TAB).Append(TAB).Append(TAB);
                    sSql.Append(f.Name);
                    sSql.Append(" = @");
                    sSql.Append(f.Name);
                }
            }
            sSql.AppendLine("");
            isFirst = true;
            foreach (var f in fields)
            {
                if (f.IsPk)
                {
                    if (isFirst)
                    {
                        sSql.Append(TAB).Append(TAB);
                        sSql.Append("WHERE ");
                        isFirst = false;
                    }
                    else
                    {
                        sSql.Append(TAB).Append(TAB);
                        sSql.Append("AND ");
                    }

                    sSql.Append(f.Name);
                    sSql.Append(" = @");
                    sSql.AppendLine(f.Name);
                }
            }

            sSql.Append(TAB).Append(TAB);
            sSql.AppendLine("SET @RET = 1; ");
            sSql.Append(TAB);
            sSql.AppendLine("END ");
        }
        else
        {
            sSql.Append(TAB);
            sSql.AppendLine("ELSE IF @TYPEACTION = '" + UPDATE + "' ");
            sSql.Append(TAB);
            sSql.AppendLine("BEGIN ");
            sSql.Append(TAB).Append(TAB);
            sSql.AppendLine("--NO UPDATABLED");
            sSql.Append(TAB).Append(TAB);
            sSql.AppendLine("SET @RET = 1; ");
            sSql.Append(TAB);
            sSql.AppendLine("END ");
        }

        //SCRIPT DELETE
        sSql.Append(TAB);
        sSql.AppendLine("ELSE IF @TYPEACTION = '" + DELETE + "' ");
        sSql.Append(TAB);
        sSql.AppendLine("BEGIN ");
        sSql.Append(TAB).Append(TAB);
        sSql.Append("DELETE FROM [");
        sSql.Append(element.TableName);
        sSql.AppendLine("] ");
        isFirst = true;
        foreach (var f in fields)
        {
            if (f.IsPk)
            {
                if (isFirst)
                {
                    sSql.Append(TAB).Append(TAB);
                    sSql.Append("WHERE ");
                    isFirst = false;
                }
                else
                {
                    sSql.Append(TAB).Append(TAB);
                    sSql.Append("AND ");
                }

                sSql.Append(f.Name);
                sSql.Append(" = @");
                sSql.AppendLine(f.Name);
            }
        }

        sSql.Append(TAB).Append(TAB);
        sSql.AppendLine("SET @RET = 2; ");
        sSql.Append(TAB);
        sSql.AppendLine("END ");
        sSql.AppendLine(" ");
        sSql.AppendLine("END ");
        sSql.AppendLine("GO ");

        return sSql.ToString();
    }

    public string GetReadProcedureScript(Element element)
    {
        if (element == null)
            throw new Exception("Invalid element");

        if (element.Fields == null || element.Fields.Count == 0)
            throw new Exception("Invalid fields");

        //Verificamos se existe chave primaria
        bool hasPk = HasPK(element);

        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior != FieldBehavior.Virtual);

        StringBuilder sSql = new StringBuilder();
        string procedureFinalName = JJMasterDataSettings.GetProcNameGet(element);

        //Se exisitir apaga
        sSql.AppendLine(GetSqlDropIfExist(procedureFinalName));

        //Criando proc
        sSql.Append("CREATE PROCEDURE [");
        sSql.Append(procedureFinalName);
        sSql.AppendLine("] ");
        sSql.AppendLine("@orderby NVARCHAR(MAX), ");

        foreach (var f in fields)
        {
            if (f.Filter.Type == FilterMode.Range)
            {
                sSql.Append("@");
                sSql.Append(f.Name);
                sSql.Append("_from ");
                sSql.Append(f.DataType.ToString());
                if (f.DataType == FieldType.Varchar ||
                    f.DataType == FieldType.NVarchar)
                {
                    sSql.Append("(");
                    sSql.Append(f.Size);
                    sSql.Append(")");
                }
                sSql.AppendLine(",");
                sSql.Append("@");
                sSql.Append(f.Name);
                sSql.Append("_to ");
                sSql.Append(f.DataType.ToString());
                if (f.DataType == FieldType.Varchar ||
                    f.DataType == FieldType.NVarchar)
                {
                    sSql.Append("(");
                    sSql.Append(f.Size);
                    sSql.Append(") ");
                }
                sSql.AppendLine(",");
            }
            else if (f.Filter.Type == FilterMode.MultValuesContain ||
                     f.Filter.Type == FilterMode.MultValuesEqual)
            {
                sSql.Append("@");
                sSql.Append(f.Name);
                sSql.Append(" ");
                sSql.Append(f.DataType.ToString());
                sSql.AppendLine("(MAX),");
            }
            else if (f.Filter.Type != FilterMode.None || f.IsPk)
            {
                sSql.Append("@");
                sSql.Append(f.Name);
                sSql.Append(" ");
                sSql.Append(f.DataType.ToString());
                if (f.DataType == FieldType.Varchar ||
                    f.DataType == FieldType.NVarchar)
                {
                    sSql.Append("(");
                    sSql.Append(f.Size);
                    sSql.AppendLine("), ");
                }
                else
                {
                    sSql.AppendLine(", ");
                }
            }
        }

        sSql.AppendLine("@regporpag INT, ");
        sSql.AppendLine("@pag INT, ");
        sSql.AppendLine("@qtdtotal INT OUTPUT ");
        sSql.AppendLine("AS ");
        sSql.AppendLine("BEGIN ");
        sSql.Append(TAB);
        sSql.AppendLine("DECLARE @sqlcolumn   NVARCHAR(MAX)");
        sSql.Append(TAB);
        sSql.AppendLine("DECLARE @sqltable    NVARCHAR(MAX)");
        sSql.Append(TAB);
        sSql.AppendLine("DECLARE @sqlcond     NVARCHAR(MAX)");
        sSql.Append(TAB);
        sSql.AppendLine("DECLARE @sqlorder    NVARCHAR(MAX)");
        sSql.Append(TAB);
        sSql.AppendLine("DECLARE @sqloffset   NVARCHAR(MAX)");
        sSql.Append(TAB);
        sSql.AppendLine("DECLARE @query       NVARCHAR(MAX)");
        sSql.Append(TAB);

        if (fields.Exists(x => x.Filter.Type == FilterMode.MultValuesContain || 
                               x.Filter.Type == FilterMode.MultValuesEqual))
        {
            sSql.AppendLine("DECLARE @likein      NVARCHAR(MAX)");
            sSql.Append(TAB);
        }

        sSql.AppendLine("DECLARE @count       INT");
        sSql.AppendLine("");

        sSql.Append(TAB);
        sSql.AppendLine("--COLUMNS");
        sSql.Append(TAB);
        sSql.AppendLine("SET @sqlcolumn = '");

        int nAux = 1;
        foreach (var f in fields)
        {
            sSql.Append(TAB).Append(TAB);
            sSql.Append("");
            if (f.DataBehavior == FieldBehavior.ViewOnly)
            {
                sSql.Append("NULL AS ");
                sSql.Append(f.Name);
                if (nAux != fields.Count)
                    sSql.Append(", ");

                sSql.AppendLine(" --TODO");
            }
            else
            {
                sSql.Append(f.Name);
                if (nAux != fields.Count)
                    sSql.AppendLine(", ");
                else
                    sSql.AppendLine("");
            }

            nAux++;
        }

        sSql.Append(TAB);
        sSql.AppendLine(" '");

        sSql.Append(TAB);
        sSql.AppendLine("--TABLES");
        sSql.Append(TAB);
        sSql.Append("SET @sqltable = 'FROM ");
        sSql.Append(element.TableName);
        sSql.AppendLine(" WITH (NOLOCK)'");
        sSql.AppendLine("");

        sSql.Append(TAB);
        sSql.AppendLine("--CONDITIONALS");
        sSql.Append(TAB);
        sSql.AppendLine("SET @sqlcond = ' WHERE 1=1 '");

        foreach (var f in fields)
        {
            if (f.Filter.Type != FilterMode.None || f.IsPk)
            {
                if (f.DataBehavior == FieldBehavior.ViewOnly)
                {
                    sSql.AppendLine("");
                    sSql.Append(TAB);
                    sSql.AppendLine("/*");
                    sSql.Append("TODO: FILTER ");
                    sSql.AppendLine(f.Name);
                }
            }

            if (f.Filter.Type == FilterMode.Range)
            {
                sSql.AppendLine("");

                if (f.DataType == FieldType.Date || f.DataType == FieldType.DateTime)
                {
                    sSql.Append(TAB);
                    sSql.Append("IF @");
                    sSql.Append(f.Name);
                    sSql.AppendLine("_from IS NOT NULL");
                    sSql.Append(TAB).Append(TAB);
                    sSql.Append("SET @sqlcond = @sqlcond + ' AND CONVERT(DATE, ");
                    sSql.Append(f.Name);
                    sSql.Append(") BETWEEN ' + CHAR(39) + CONVERT(VARCHAR(10), @");
                    sSql.Append(f.Name);
                    sSql.Append("_from, 112) + CHAR(39) + ' AND ' + CHAR(39) + CONVERT(VARCHAR(10), @");
                    sSql.Append(f.Name);
                    sSql.AppendLine("_to, 112) + CHAR(39)");
                }
                else
                {
                    sSql.Append(TAB);
                    sSql.Append("IF @");
                    sSql.Append(f.Name);
                    sSql.AppendLine("_from IS NOT NULL");
                    sSql.Append(TAB).Append(TAB);
                    sSql.Append("SET @sqlcond = @sqlcond + ' AND ");
                    sSql.Append(f.Name);
                    sSql.Append(" BETWEEN ' + CHAR(39) + @");
                    sSql.Append(f.Name);
                    sSql.Append("_from + CHAR(39) + ' AND ' + CHAR(39) + @");
                    sSql.Append(f.Name);
                    sSql.AppendLine(" + CHAR(39)");
                }
            }
            else if (f.Filter.Type == FilterMode.Contain)
            {
                sSql.AppendLine("");
                sSql.Append(TAB);
                sSql.Append("IF @");
                sSql.Append(f.Name);
                sSql.AppendLine(" IS NOT NULL");
                sSql.Append(TAB).Append(TAB);
                sSql.Append("SET @sqlcond = @sqlcond + ' AND ");
                sSql.Append(f.Name);
                sSql.Append(" LIKE ' + CHAR(39) + '%' + @");
                sSql.Append(f.Name);
                sSql.AppendLine(" +  '%' + CHAR(39)");
            }
            else if (f.Filter.Type == FilterMode.MultValuesContain)
            {
                sSql.AppendLine("");
                sSql.Append(TAB);
                sSql.Append("IF @");
                sSql.Append(f.Name);
                sSql.AppendLine(" IS NOT NULL");
                sSql.Append(TAB);
                sSql.AppendLine("BEGIN");
                sSql.Append(TAB, 2);
                sSql.AppendLine("SET @likein = ' AND ( '");
                sSql.Append(TAB, 2);
                sSql.AppendFormat("WHILE CHARINDEX(',', @{0}) <> 0", f.Name);
                sSql.AppendLine("");
                sSql.Append(TAB, 2);
                sSql.AppendLine("BEGIN");
                sSql.Append(TAB, 3);
                sSql.AppendFormat("SET @likein = @likein + '{0} LIKE ' + CHAR(39) + '%' + SUBSTRING(@{0}, 1, CHARINDEX(',', @{0}) -1) + '%' + CHAR(39);", f.Name);
                sSql.AppendLine("");
                sSql.Append(TAB, 3);
                sSql.AppendFormat("SET @{0} = RIGHT(@{0} , LEN(@{0}) - CHARINDEX(',', @{0}));", f.Name);
                sSql.AppendLine("");
                sSql.Append(TAB, 3);
                sSql.AppendLine("SET @likein = @likein + ' OR ';");
                sSql.Append(TAB, 2);
                sSql.AppendLine("END");
                sSql.Append(TAB, 2);
                sSql.AppendFormat("SET @likein = @likein  + '{0} LIKE ' + CHAR(39) + '%' + @{0} + '%' + CHAR(39) + ' ) '", f.Name);
                sSql.AppendLine("");
                sSql.Append(TAB, 2);
                sSql.AppendLine("SET @sqlcond = @sqlcond + @likein");
                sSql.Append(TAB);
                sSql.AppendLine("END");
            }
            else if (f.Filter.Type == FilterMode.MultValuesEqual)
            {
                sSql.AppendLine("");
                sSql.Append(TAB);
                sSql.Append("IF @");
                sSql.Append(f.Name);
                sSql.AppendLine(" IS NOT NULL");
                sSql.Append(TAB);
                sSql.AppendLine("BEGIN");
                sSql.Append(TAB, 2);
                sSql.AppendFormat("SET @likein = ' AND {0} IN ('", f.Name);
                sSql.AppendLine("");
                sSql.Append(TAB, 2);
                sSql.AppendFormat("WHILE CHARINDEX(',', @{0}) <> 0", f.Name);
                sSql.AppendLine("");
                sSql.Append(TAB, 2);
                sSql.AppendLine("BEGIN");
                sSql.Append(TAB, 3);
                sSql.AppendFormat("SET @likein = @likein + CHAR(39) + SUBSTRING(@{0},1,CHARINDEX(',',@{0}) -1) + CHAR(39);", f.Name);
                sSql.AppendLine("");
                sSql.Append(TAB, 3);
                sSql.AppendFormat("SET @{0} = RIGHT(@{0} , LEN(@{0}) - CHARINDEX(',', @{0}));", f.Name);
                sSql.AppendLine("");
                sSql.Append(TAB, 3);
                sSql.AppendLine("SET @likein = @likein + ', ';");
                sSql.Append(TAB, 2);
                sSql.AppendLine("END");
                sSql.Append(TAB, 2);
                sSql.AppendFormat("SET @likein = @likein + CHAR(39) + @{0} + CHAR(39) + ') '", f.Name);
                sSql.AppendLine("");
                sSql.Append(TAB, 2);
                sSql.AppendLine("SET @sqlcond = @sqlcond + @likein");
                sSql.Append(TAB);
                sSql.AppendLine("END");
            }
            else if (f.Filter.Type == FilterMode.Equal || f.IsPk)
            {
                sSql.AppendLine("");
                sSql.Append(TAB);
                sSql.Append("IF @");
                sSql.Append(f.Name);
                sSql.AppendLine(" IS NOT NULL");
                sSql.Append(TAB).Append(TAB);
                sSql.Append("SET @sqlcond = @sqlcond + ' AND ");
                sSql.Append(f.Name);

                if (f.DataType == FieldType.Int || f.DataType == FieldType.Float)
                {
                    sSql.Append(" = ' + CONVERT(VARCHAR, @");
                    sSql.Append(f.Name);
                    sSql.AppendLine(")");
                }
                else if (f.DataType == FieldType.Date || f.DataType == FieldType.DateTime)
                {
                    sSql.Append(" = ' + CHAR(39) + CAST(@");
                    sSql.Append(f.Name);
                    sSql.AppendLine(" AS VARCHAR) +  CHAR(39)");
                }
                else
                {
                    sSql.Append(" = ' + CHAR(39) + @");
                    sSql.Append(f.Name);
                    sSql.AppendLine(" +  CHAR(39)");
                }

            }

            if (f.Filter.Type != FilterMode.None || f.IsPk)
            {
                if (f.DataBehavior == FieldBehavior.ViewOnly)
                {
                    sSql.Append(TAB);
                    sSql.AppendLine("*/");
                }
            }
        }

        sSql.AppendLine("");
        sSql.Append(TAB);
        sSql.AppendLine("--ORDER");
        sSql.Append(TAB);
        var listPk = fields.FindAll(x => x.IsPk);
        if (listPk == null || listPk.Count == 0)
        {
            sSql.Append("SET @sqlorder  = ' ORDER BY ");
            sSql.Append(fields[0].Name);
            sSql.AppendLine("'");
        }
        else
        {
            sSql.Append("SET @sqlorder  = ' ORDER BY ");
            sSql.Append(listPk[0].Name);
            sSql.AppendLine("'");
        }
        sSql.Append(TAB);
        sSql.AppendLine("IF @orderby IS NOT NULL AND @orderby <> ''");
        sSql.Append(TAB);
        sSql.AppendLine("BEGIN");
        sSql.Append(TAB);
        sSql.Append(TAB);
        sSql.AppendLine("SET @sqlorder  = ' ORDER BY ' + @orderby");
        sSql.Append(TAB);
        sSql.AppendLine("END");
        sSql.AppendLine("");

        sSql.Append(TAB);
        sSql.AppendLine("--PAGING");
        sSql.Append(TAB);
        sSql.AppendLine("SET @sqloffset = ' '");
        sSql.Append(TAB);
        sSql.AppendLine("SET @sqloffset = @sqloffset + 'OFFSET ('");
        sSql.Append(TAB);
        sSql.AppendLine("SET @sqloffset = @sqloffset + RTRIM(CONVERT(VARCHAR(10), @pag - 1))");
        sSql.Append(TAB);
        sSql.AppendLine("SET @sqloffset = @sqloffset + ' * '");
        sSql.Append(TAB);
        sSql.AppendLine("SET @sqloffset = @sqloffset + RTRIM(CONVERT(VARCHAR(10), @regporpag))");
        sSql.Append(TAB);
        sSql.AppendLine("SET @sqloffset = @sqloffset + ') ROWS FETCH NEXT '");
        sSql.Append(TAB);
        sSql.AppendLine("SET @sqloffset = @sqloffset + RTRIM(CONVERT(VARCHAR(10), @regporpag))");
        sSql.Append(TAB);
        sSql.AppendLine("SET @sqloffset = @sqloffset + ' ROWS ONLY '");
        sSql.AppendLine("");


        sSql.Append(TAB);
        sSql.AppendLine("--TOTAL OF RECORDS");
        sSql.Append(TAB);
        sSql.AppendLine("IF @qtdtotal is null or @qtdtotal = 0");
        sSql.Append(TAB);
        sSql.AppendLine("BEGIN");
        sSql.Append(TAB).Append(TAB);
        sSql.AppendLine("SET @qtdtotal = 0;");
        sSql.Append(TAB).Append(TAB);
        sSql.AppendLine("SET @query = N'SELECT @count = COUNT(*) ' + @sqltable + @sqlcond");
        sSql.Append(TAB).Append(TAB);
        sSql.AppendLine("EXECUTE sp_executesql @query, N'@count int output', @count = @qtdtotal output");
        sSql.Append(TAB);
        sSql.AppendLine("END");
        sSql.AppendLine("");

        sSql.Append(TAB);
        sSql.AppendLine("--DATASET RESULT");
        sSql.Append(TAB);

        sSql.AppendLine("SET @query = N'SELECT ' + @sqlcolumn + @sqltable + @sqlcond + @sqlorder + @sqloffset");
        sSql.Append(TAB);
        sSql.AppendLine("EXECUTE sp_executesql @query ");
        sSql.Append(TAB);
        sSql.AppendLine("--PRINT(@query)");

        sSql.AppendLine("");
        sSql.AppendLine("END");

        return sSql.ToString();
    }

    public DataAccessCommand GetInsertScript(Element element, Hashtable values)
    {
        return GetWriteCommand(INSERT, element, values);
    }

    public DataAccessCommand GetUpdateScript(Element element, Hashtable values)
    {
        return GetWriteCommand(UPDATE, element, values);
    }

    public DataAccessCommand GetDeleteScript(Element element, Hashtable filters)
    {
        return GetWriteCommand(DELETE, element, filters);
    }

    public DataAccessCommand GetReadCommand(Element element, Hashtable filters, string orderby, int regperpage, int pag, ref DataAccessParameter pTot)
    {
        DataAccessCommand cmd = new DataAccessCommand();
        cmd.CmdType = System.Data.CommandType.StoredProcedure;
        cmd.Sql = JJMasterDataSettings.GetProcNameGet(element);
        cmd.Parameters = new List<DataAccessParameter>();
        cmd.Parameters.Add(new DataAccessParameter("@orderby", orderby));
        cmd.Parameters.Add(new DataAccessParameter("@regporpag", regperpage));
        cmd.Parameters.Add(new DataAccessParameter("@pag", pag));

        foreach (var field in element.Fields)
        {
            if (field.Filter.Type == FilterMode.Range)
            {
                object valueFrom = DBNull.Value;
                if (filters != null &&
                    filters.ContainsKey(field.Name + "_from") &&
                    filters[field.Name + "_from"] != null)
                {
                    valueFrom = filters[field.Name + "_from"];
                    if (valueFrom != null)
                        valueFrom = StringManager.ClearText(valueFrom.ToString());
                }
                var pFrom = new DataAccessParameter();
                pFrom.Direction = ParameterDirection.Input;
                pFrom.Type = GetDbType(field.DataType);
                pFrom.Size = field.Size;
                pFrom.Name = field.Name + "_from";
                pFrom.Value = valueFrom;
                cmd.Parameters.Add(pFrom);

                object valueTo = DBNull.Value;
                if (filters != null &&
                    filters.ContainsKey(field.Name + "_to") &&
                    filters[field.Name + "_to"] != null)
                {
                    valueTo = filters[field.Name + "_to"];
                    if (valueTo != null)
                        valueTo = StringManager.ClearText(valueTo.ToString());
                }
                var pTo = new DataAccessParameter();
                pTo.Direction = ParameterDirection.Input;
                pTo.Type = GetDbType(field.DataType);
                pTo.Size = field.Size;
                pTo.Name = field.Name + "_to";
                pTo.Value = valueTo;
                cmd.Parameters.Add(pTo);
            }
            else if (field.Filter.Type != FilterMode.None || field.IsPk)
            {
                object value = GetElementValue(field, filters);
                if (value != null && value != DBNull.Value)
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
                cmd.Parameters.Add(parameter);
            }
        }

        cmd.Parameters.Add(pTot);

        return cmd;
    }

    public DataAccessCommand GetWriteCommand(string action, Element element, Hashtable values)
    {
        DataAccessCommand cmd = new DataAccessCommand();
        cmd.CmdType = System.Data.CommandType.StoredProcedure;
        cmd.Sql = JJMasterDataSettings.GetProcNameSet(element);
        cmd.Parameters = new List<DataAccessParameter>();
        cmd.Parameters.Add(new DataAccessParameter("@action", action, DbType.String, 1));

        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

        foreach (var f in fields)
        {
            object value = GetElementValue(f, values);
            var param = new DataAccessParameter();
            param.Name = string.Format("@{0}", f.Name);
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

    public DataTable GetDataTable(Element element, Hashtable filters, string orderby, int regporpag, int pag, ref int tot, ref IDataAccess dataAccess)
    {
        DataAccessParameter pTot = new DataAccessParameter(VariablePrefix + "qtdtotal", tot, DbType.Int32, 0, ParameterDirection.InputOutput);
        var cmd = GetReadCommand(element, filters, orderby, regporpag, pag, ref pTot);
        DataTable dt = dataAccess.GetDataTable(cmd);
        tot = 0;
        if (pTot != null && pTot.Value != null && pTot.Value != DBNull.Value)
            tot = (int)pTot.Value;

        return dt;
    }

    private object GetElementValue(ElementField f, Hashtable values)
    {
        object value = DBNull.Value;
        if (values != null &&
            values.ContainsKey(f.Name) &&
            values[f.Name] != null)
        {
            if ((f.DataType == FieldType.Date ||
                 f.DataType == FieldType.DateTime ||
                 f.DataType == FieldType.Float ||
                 f.DataType == FieldType.Int) &&
                values[f.Name].ToString().Trim().Length == 0)
            {
                value = DBNull.Value;
            }
            else
            {
                value = values[f.Name];
            }
        }

        return value;
    }

    private bool HasUpdateFields(Element element)
    {
        bool ret = false;
        foreach (var f in element.Fields)
        {
            if (!f.IsPk && f.DataBehavior == FieldBehavior.Real)
            {
                ret = true;
                break;
            }
        }
        return ret;
    }

    private bool HasPK(Element element)
    {
        bool ret = false;
        foreach (var f in element.Fields)
        {
            if (f.IsPk)
            {
                ret = true;
                break;
            }
        }
        return ret;
    }

    private string GetSqlDropIfExist(string objname)
    {
        StringBuilder sSql = new StringBuilder();
        sSql.AppendLine("IF  EXISTS (SELECT * ");
        sSql.Append(TAB).Append(TAB).Append(TAB);
        sSql.AppendLine("FROM sys.objects ");
        sSql.Append(TAB).Append(TAB).Append(TAB);
        sSql.Append("WHERE object_id = OBJECT_ID(N'[");
        sSql.Append(objname);
        sSql.AppendLine("]') ");
        sSql.Append(TAB).Append(TAB).Append(TAB);
        sSql.AppendLine("AND type in (N'P', N'PC'))");
        sSql.AppendLine("BEGIN");
        sSql.Append(TAB);
        sSql.Append("DROP PROCEDURE [");
        sSql.Append(objname);
        sSql.AppendLine("]");
        sSql.AppendLine("END");
        sSql.AppendLine("GO");

        return sSql.ToString();
    }

    private DbType GetDbType(FieldType dataType)
    {
        DbType t = DbType.String;
        switch (dataType)
        {
            case FieldType.Date:
                t = DbType.Date;
                break;
            case FieldType.DateTime:
                t = DbType.DateTime;
                break;
            case FieldType.Float:
                t = DbType.Double;
                break;
            case FieldType.Int:
                t = DbType.Int32;
                break;
        }
        return t;
    }

    private FieldType GetDataType(string databaseType)
    {
        if (string.IsNullOrEmpty(databaseType))
            return FieldType.NVarchar;

        databaseType = databaseType.ToLower().Trim();

        if (databaseType.Equals("varchar") |
            databaseType.Equals("char") |
            databaseType.Equals("bit"))
            return FieldType.Varchar;

        if (databaseType.Equals("nvarchar") |
            databaseType.Equals("nchar"))
            return FieldType.NVarchar;

        if (databaseType.Equals("int") |
            databaseType.Equals("bigint") |
            databaseType.Equals("tinyint"))
            return FieldType.Int;

        if (databaseType.Equals("float") |
            databaseType.Equals("numeric") |
            databaseType.Equals("money") |
            databaseType.Equals("smallmoney") |
            databaseType.Equals("real"))
            return FieldType.Float;

        if (databaseType.Equals("date"))
            return FieldType.Date;

        if (databaseType.Equals("datetime"))
            return FieldType.DateTime;

        if (databaseType.Equals("text"))
            return FieldType.Text;

        if (databaseType.Equals("ntext"))
            return FieldType.NText;

        return FieldType.NVarchar;

    }

    public Element GetElementFromTable(string tableName, ref IDataAccess dataAccess)
    {
        var element = new Element
        {
            Name = tableName
        };

        var cmdFields = new DataAccessCommand
        {
            CmdType = System.Data.CommandType.StoredProcedure,
            Sql = "sp_columns"
        };
        cmdFields.Parameters.Add(new DataAccessParameter("@table_name", tableName));

        var dtFields = dataAccess.GetDataTable(cmdFields);
        if (dtFields == null || dtFields.Rows.Count == 0)
            return null;

        foreach (DataRow row in dtFields.Rows)
        {
            var field = new ElementField
            {
                Name = row["COLUMN_NAME"].ToString(),
                Label = row["COLUMN_NAME"].ToString(),
                Size = int.Parse(row["LENGTH"].ToString()),
                AutoNum = row["TYPE_NAME"].ToString().ToUpper().Contains("IDENTITY"),
                IsRequired = row["NULLABLE"].ToString().Equals("0"),
                DataType = GetDataType(row["TYPE_NAME"].ToString())
            };
            
            element.Fields.Add(field);
        }

        //chave primária
        var cmdPks = new DataAccessCommand
        {
            CmdType = System.Data.CommandType.StoredProcedure,
            Sql = "sp_pkeys"
        };
        cmdPks.Parameters.Add(new DataAccessParameter("@table_name", tableName));
        var dtPks = dataAccess.GetDataTable(cmdPks);
        foreach (DataRow row in dtPks.Rows)
        {
            element.Fields[row["COLUMN_NAME"].ToString()].IsPk = true;
        }


        return element;
    }

}