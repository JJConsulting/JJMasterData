﻿#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Data.Entity.Providers;

public class OracleProvider(IOptionsSnapshot<MasterDataCommonsOptions> options)
    : IEntityProvider
{
    private readonly MasterDataCommonsOptions _options = options.Value;
    
    private const string InsertKeyword = "I";
    private const string UpdateKeyword = "A";
    private const string DeleteKeyword = "E";
    private const char Tab = '\t';
    public string VariablePrefix => "p_";

    public string GetCreateTableScript(Element element, List<RelationshipReference>? relationships = null)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (element.Fields == null || element.Fields.Count == 0)
            throw new ArgumentNullException(nameof(element.Fields));

        var sqlScript = new StringBuilder();
        var sKeys = new StringBuilder();

        sqlScript.AppendLine("-- TABLE");
        sqlScript.Append("CREATE TABLE ");
        sqlScript.Append(element.TableName);
        sqlScript.AppendLine(" (");
        var isFirst = true;
        var fields = element.Fields.FindAll(x => x.DataBehavior == FieldBehavior.Real);

        foreach (var f in fields)
        {
            if (isFirst)
                isFirst = false;
            else
                sqlScript.AppendLine(",");

            sqlScript.Append(Tab);
            sqlScript.Append(f.Name);
            sqlScript.Append(' ');
            sqlScript.Append(GetStrType(f.DataType));

            if (f.DataType is FieldType.Varchar or FieldType.NVarchar)
            {
                sqlScript.Append(" (");
                sqlScript.Append(f.Size);
                sqlScript.Append(')');
            }

            if (f.IsRequired)
                sqlScript.Append(" NOT NULL");

            if (f.IsPk)
            {
                if (sKeys.Length > 0)
                    sKeys.Append(',');

                sKeys.Append(f.Name);
                sKeys.Append(' ');
            }
        }

        if (sKeys.Length > 0)
        {
            sqlScript.AppendLine(", ");
            sqlScript.Append(Tab);
            sqlScript.Append("CONSTRAINT PK_");
            sqlScript.Append(element.TableName);
            sqlScript.Append(" PRIMARY KEY (");
            sqlScript.Append(sKeys);
            sqlScript.Append(')');
        }


        sqlScript.AppendLine("");
        sqlScript.AppendLine(")");
        sqlScript.AppendLine("/");
        sqlScript.AppendLine("");
        sqlScript.AppendLine(GetRelationshipsScript(element));
        sqlScript.AppendLine("");

        var nIndex = 1;
        if (element.Indexes.Count > 0)
        {
            foreach (var index in element.Indexes)
            {
                sqlScript.Append("CREATE");
                sqlScript.Append(index.IsUnique ? " UNIQUE" : "");
                sqlScript.Append(index.IsClustered ? " CLUSTERED" : "");
                sqlScript.Append(" INDEX IX_");
                sqlScript.Append(element.TableName);
                sqlScript.Append('_');
                sqlScript.Append(nIndex);
                sqlScript.Append(" ON ");
                sqlScript.AppendLine(element.TableName);

                sqlScript.Append(Tab);
                sqlScript.AppendLine("(");
                for (var i = 0; i < index.Columns.Count; i++)
                {
                    if (i > 0)
                        sqlScript.AppendLine(", ");

                    sqlScript.Append(Tab);
                    sqlScript.Append(index.Columns[i]);
                }
                sqlScript.AppendLine("");
                sqlScript.Append(Tab);
                sqlScript.AppendLine(")");
                sqlScript.AppendLine("/");
                nIndex++;
            }
        }
        sqlScript.AppendLine("");

        //Criando sequencias (IDENTITY)
        var listSeq = element.Fields.FindAll(x => x.AutoNum);
        for (var i = 0; i < listSeq.Count; i++)
        {
            sqlScript.Append("CREATE SEQUENCE ");
            sqlScript.Append(element.TableName);
            sqlScript.Append("_seq");
            if (i > 0)
                sqlScript.Append(i.ToString());

            sqlScript.AppendLine(" START WITH 1 INCREMENT BY 1;");
            sqlScript.AppendLine("/");
        }
        sqlScript.AppendLine("");

        return sqlScript.ToString();
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
                var contraintName = $"FK_{r.ChildElement}_{element.TableName}";

                //Tratamento para nome repedido de contraint
                if (!listContraint.Contains(contraintName))
                {
                    listContraint.Add(contraintName);
                }
                else
                {
                    var hasContraint = true;
                    for (var nCount = 1; hasContraint; nCount++)
                    {
                        if (!listContraint.Contains(contraintName + nCount))
                        {
                            contraintName += nCount;
                            listContraint.Add(contraintName);
                            hasContraint = false;
                        }
                    }
                }

                sql.Append("ALTER TABLE ");
                sql.AppendLine(r.ChildElement);
                sql.Append("ADD CONSTRAINT ");
                sql.Append(contraintName);
                sql.AppendLine(" ");
                sql.Append(Tab);
                sql.Append("FOREIGN KEY (");

                for (var rc = 0; rc < r.Columns.Count; rc++)
                {
                    if (rc > 0)
                        sql.Append(", ");

                    sql.Append('[');
                    sql.Append(r.Columns[rc].FkColumn);
                    sql.Append(']');
                }
                sql.AppendLine(")");
                sql.Append(Tab);
                sql.Append("REFERENCES ");
                sql.Append(element.TableName);
                sql.Append(" (");
                for (var rc = 0; rc < r.Columns.Count; rc++)
                {
                    if (rc > 0)
                        sql.Append(", ");

                    sql.Append(r.Columns[rc].PkColumn);
                }
                sql.Append(')');

                if (r.UpdateOnCascade)
                {
                    sql.AppendLine();
                    sql.Append(Tab).Append(Tab);
                    sql.Append("ON UPDATE CASCADE ");
                }

                if (r.DeleteOnCascade)
                {
                    sql.AppendLine();
                    sql.Append(Tab).Append(Tab);
                    sql.Append("ON DELETE CASCADE ");
                }

                sql.AppendLine();
                sql.AppendLine("/");
            }
        }

        return sql.ToString();
    }

    public string GetWriteProcedureScript(Element element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (element.Fields == null || element.Fields.Count == 0)
            throw new ArgumentNullException(nameof(Element.Fields));

        var sql = new StringBuilder();
        var isFirst = true;
        var hasPk = HasPk(element);
        var hasUpd = HasUpdateFields(element);
        var procedureFinalName = _options.GetWriteProcedureName(element);

        sql.AppendLine("-- PROC SET");

        //Criando proc
        sql.Append("CREATE OR REPLACE PROCEDURE ");
        sql.Append(procedureFinalName);
        sql.AppendLine("( ");
        sql.Append(VariablePrefix);
        sql.AppendLine("action IN VARCHAR2, ");

        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

        foreach (var f in fields)
        {
            sql.Append(VariablePrefix);
            sql.Append(f.Name);
            sql.Append(" IN ");
            sql.Append(GetStrType(f.DataType));
            sql.AppendLine(", ");
        }
        sql.Append(VariablePrefix);
        sql.AppendLine("RET OUT NUMBER)");
        sql.AppendLine("IS ");
        sql.Append(Tab);
        sql.AppendLine(" v_TYPEACTION VARCHAR2(1); ");
        sql.Append(Tab);
        sql.AppendLine(" v_NCOUNT INTEGER; ");
        sql.AppendLine("BEGIN ");
        sql.Append(Tab);
        sql.Append("v_TYPEACTION := ");
        sql.Append(VariablePrefix);
        sql.AppendLine("action; ");
        sql.Append(Tab);
        sql.AppendLine("IF v_TYPEACTION = ' ' THEN");
        sql.Append(Tab).Append(Tab);
        sql.AppendLine($"v_TYPEACTION := '{InsertKeyword}'; ");

        if (hasPk)
        {
            sql.Append(Tab).Append(Tab);
            sql.AppendLine("SELECT COUNT(*) AS QTD INTO v_NCOUNT ");
            sql.Append(Tab).Append(Tab);
            sql.Append("FROM ");
            sql.Append(element.TableName);
            foreach (var f in fields)
            {
                if (f.IsPk)
                {
                    sql.AppendLine();
                    sql.Append(Tab).Append(Tab);
                    if (isFirst)
                    {
                        sql.Append("WHERE ");
                        isFirst = false;
                    }
                    else
                    {
                        sql.Append("AND ");
                    }

                    sql.Append(f.Name);
                    sql.Append(" = ");
                    sql.Append(VariablePrefix);
                    sql.Append(f.Name);
                }
            }
            sql.AppendLine(";");
            sql.AppendLine(" ");
            sql.Append(Tab).Append(Tab);
            sql.AppendLine("IF v_NCOUNT > 0 THEN ");
            sql.Append(Tab).Append(Tab).Append(Tab);
            sql.AppendLine($"v_TYPEACTION := '{UpdateKeyword}';");
            sql.Append(Tab).Append(Tab);
            sql.AppendLine("END IF;");
        }
        sql.Append(Tab);
        sql.AppendLine("END IF;");
        sql.AppendLine(" ");


        //SCRIPT INSERT
        sql.Append(Tab);
        sql.AppendLine($"IF v_TYPEACTION = '{InsertKeyword}' THEN");
        sql.Append(Tab).Append(Tab);
        sql.Append("INSERT INTO ");
        sql.Append(element.TableName);
        sql.AppendLine(" (");
        isFirst = true;
        foreach (var f in fields)
        {
            if (!f.AutoNum)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sql.AppendLine(",");

                sql.Append("			");
                sql.Append(f.Name);
            }
        }
        sql.AppendLine(")");
        sql.Append(Tab).Append(Tab);
        sql.AppendLine("VALUES (");
        isFirst = true;
        foreach (var f in fields)
        {
            if (!f.AutoNum)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sql.AppendLine(",");

                sql.Append(Tab).Append(Tab).Append(Tab);
                sql.Append(VariablePrefix);
                sql.Append(f.Name);
            }
        }
        sql.AppendLine(");");
        sql.Append(Tab).Append(Tab);
        sql.Append(VariablePrefix);
        sql.AppendLine("RET := 0; ");

        //SCRIPT UPDATE
        isFirst = true;
        sql.Append(Tab);
        sql.AppendLine($"ELSIF v_TYPEACTION = '{UpdateKeyword}' THEN ");
        sql.Append(Tab).Append(Tab);
        if (hasUpd)
        {
            sql.Append("UPDATE ");
            sql.Append(element.TableName);
            sql.AppendLine(" SET ");
            foreach (var f in fields)
            {
                if (!f.IsPk)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        sql.AppendLine(", ");

                    sql.Append(Tab).Append(Tab).Append(Tab);
                    sql.Append(f.Name);
                    sql.Append(" = ");
                    sql.Append(VariablePrefix);
                    sql.Append(f.Name);
                }
            }

            isFirst = true;
            foreach (var f in fields)
            {
                if (f.IsPk)
                {
                    sql.AppendLine();
                    sql.Append(Tab).Append(Tab);
                    if (isFirst)
                    {
                        sql.Append("WHERE ");
                        isFirst = false;
                    }
                    else
                    {
                        sql.Append("AND ");
                    }

                    sql.Append(f.Name);
                    sql.Append(" = ");
                    sql.Append(VariablePrefix);
                    sql.Append(f.Name);
                }
            }
            sql.AppendLine(";");
        }
        else
        {
            sql.AppendLine("--NO UPDATABLED");
        }

        sql.Append(Tab).Append(Tab);
        sql.Append(VariablePrefix);
        sql.AppendLine("RET := 1; ");

        //SCRIPT DELETE
        sql.Append(Tab);
        sql.AppendLine($"ELSIF v_TYPEACTION = '{DeleteKeyword}' THEN ");
        sql.Append(Tab).Append(Tab);
        sql.Append("DELETE FROM ");
        sql.Append(element.TableName);

        isFirst = true;
        foreach (var f in fields)
        {
            if (f.IsPk)
            {
                sql.AppendLine();
                sql.Append(Tab).Append(Tab);
                if (isFirst)
                {
                    sql.Append("WHERE ");
                    isFirst = false;
                }
                else
                {
                    sql.Append("AND ");
                }

                sql.Append(f.Name);
                sql.Append(" = ");
                sql.Append(VariablePrefix);
                sql.Append(f.Name);
            }
        }
        sql.AppendLine(";");
        sql.Append(Tab).Append(Tab);
        sql.Append(VariablePrefix);
        sql.AppendLine("RET := 2; ");
        sql.Append(Tab);
        sql.AppendLine("END IF;");
        sql.AppendLine(" ");
        sql.AppendLine("END; ");
        sql.AppendLine("/");

        return sql.ToString();
    }

    public string GetReadProcedureScript(Element element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (element.Fields == null || element.Fields.Count == 0)
            throw new ArgumentNullException(nameof(Element.Fields));

        //Verificamos se existe chave primaria
        var unused = HasPk(element);

        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior != FieldBehavior.Virtual);

        var sql = new StringBuilder();
        var procedureFinalName = _options.GetReadProcedureName(element);

        sql.AppendLine("-- PROC GET");

        //Criando proc
        sql.Append("CREATE OR REPLACE PROCEDURE ");
        sql.Append(procedureFinalName);
        sql.AppendLine(" (");
        sql.Append(VariablePrefix);
        sql.AppendLine("orderby NVARCHAR2, ");

        foreach (var f in fields)
        {
            if (f.Filter.Type == FilterMode.Range)
            {
                sql.Append(VariablePrefix);
                sql.Append(f.Name);
                sql.Append("_from ");
                sql.Append(GetStrType(f.DataType));
                sql.AppendLine(",");
                sql.Append(VariablePrefix);
                sql.Append(f.Name);
                sql.Append("_to ");
                sql.Append(GetStrType(f.DataType));
                sql.AppendLine(",");
            }
            else if (f.Filter.Type != FilterMode.None || f.IsPk)
            {
                sql.Append(VariablePrefix);
                sql.Append(f.Name);
                sql.Append(' ');
                sql.Append(GetStrType(f.DataType));
                sql.AppendLine(", ");
            }
        }

        sql.Append(VariablePrefix);
        sql.AppendLine("regporpag INTEGER, ");
        sql.Append(VariablePrefix);
        sql.AppendLine("pag INTEGER, ");
        sql.Append(VariablePrefix);
        sql.AppendLine("qtdtotal IN OUT INTEGER, ");
        sql.Append(VariablePrefix);
        sql.AppendLine("cur_OUT OUT SYS_REFCURSOR) ");
        sql.AppendLine("IS ");
        sql.Append(Tab);
        sql.AppendLine("v_sqlcolumn    CLOB;");
        sql.Append(Tab);
        sql.AppendLine("v_sqltable     CLOB;");
        sql.Append(Tab);
        sql.AppendLine("v_sqlcond      CLOB;");
        sql.Append(Tab);
        sql.AppendLine("v_sqlorder     CLOB;");
        sql.Append(Tab);
        sql.AppendLine("v_query        CLOB;");
        sql.Append(Tab);
        sql.AppendLine("v_Cursor       SYS_REFCURSOR;");
        sql.AppendLine("BEGIN");
        sql.AppendLine();

        sql.Append(Tab);
        sql.AppendLine("--COLUMNS");
        sql.Append(Tab);
        sql.AppendLine("v_sqlcolumn := '';");
        var nAux = 1;
        foreach (var f in fields)
        {
            sql.Append(Tab);


            sql.Append("v_sqlcolumn := v_sqlcolumn || '");

            if (f.DataBehavior == FieldBehavior.ViewOnly)
                sql.Append("'''' AS ");

            sql.Append(f.Name);

            if (nAux != element.Fields.Count)
                sql.Append(", ");

            sql.Append("'; ");

            if (f.DataBehavior == FieldBehavior.ViewOnly)
                sql.Append(" --TODO: ");

            sql.AppendLine();

            nAux++;
        }

        sql.AppendLine();
        sql.AppendLine();

        sql.Append(Tab);
        sql.AppendLine("--TABLES");
        sql.Append(Tab);

        sql.Append("v_sqltable := 'FROM ");
        sql.Append(element.TableName);
        sql.AppendLine("';");
        sql.AppendLine();
        sql.AppendLine();

        sql.Append(Tab);
        sql.AppendLine("--CONDITIONALS");
        sql.Append(Tab);
        sql.AppendLine("v_sqlcond := ' WHERE 1=1 ';");

        foreach (var f in fields)
        {
            if (f.Filter.Type != FilterMode.None || f.IsPk)
            {
                if (f.DataBehavior == FieldBehavior.ViewOnly)
                {
                    sql.AppendLine();
                    sql.Append(Tab);
                    sql.AppendLine("/*");
                    sql.Append(Tab);
                    sql.Append("TODO: FILTER ");
                    sql.AppendLine(f.Name);
                }
            }

            if (f.Filter.Type == FilterMode.Range)
            {
                sql.AppendLine();
                sql.Append(Tab);
                sql.Append("IF ");
                sql.Append(VariablePrefix);
                sql.Append(f.Name);
                sql.AppendLine("_from IS NOT NULL THEN");
                sql.Append(Tab).Append(Tab);
                sql.Append("v_sqlcond := v_sqlcond || ' AND ");
                sql.Append(f.Name);
                sql.AppendLine(" BETWEEN ';");
                sql.Append(Tab).Append(Tab);
                sql.Append("v_sqlcond := v_sqlcond || 'TO_DATE (''' || TO_CHAR (");
                sql.Append(VariablePrefix);
                sql.Append(f.Name);
                sql.AppendLine("_from, 'YYYY-MM-DD') || ''', ''YYYY-MM-DD'') '; ");
                sql.Append(Tab).Append(Tab);
                sql.AppendLine("v_sqlcond := v_sqlcond || ' AND ';");
                sql.Append(Tab).Append(Tab);
                sql.Append("v_sqlcond := v_sqlcond || 'TO_DATE (''' || TO_CHAR (");
                sql.Append(VariablePrefix);
                sql.Append(f.Name);
                sql.AppendLine("_to, 'YYYY-MM-DD') || ''', ''YYYY-MM-DD'') '; ");
                sql.Append(Tab);
                sql.AppendLine("END IF;");
            }
            else if (f.Filter.Type == FilterMode.Contain)
            {
                sql.AppendLine();
                sql.Append(Tab);
                sql.Append("IF ");
                sql.Append(VariablePrefix);
                sql.Append(f.Name);
                sql.AppendLine(" IS NOT NULL THEN");
                sql.Append(Tab).Append(Tab);
                sql.Append("v_sqlcond := v_sqlcond || ' AND ");
                sql.Append(VariablePrefix);
                sql.Append(" LIKE ''%' || ");
                sql.Append(VariablePrefix);
                sql.Append(f.Name);
                sql.AppendLine(" ||  '%''';");
                sql.Append(Tab);
                sql.AppendLine("END IF;");
            }
            else if (f.Filter.Type == FilterMode.Equal || f.IsPk)
            {
                sql.AppendLine();
                sql.Append(Tab);
                sql.Append("IF ");
                sql.Append(VariablePrefix);
                sql.Append(f.Name);
                sql.AppendLine(" IS NOT NULL THEN");
                sql.Append(Tab).Append(Tab);
                sql.Append("v_sqlcond := v_sqlcond || ' AND ");
                sql.Append(VariablePrefix);
                sql.Append(" = ''' || ");
                sql.Append(VariablePrefix);
                sql.Append(f.Name);
                sql.AppendLine(" ||  '''';");
                sql.Append(Tab);
                sql.AppendLine("END IF;");
            }

            if (f.Filter.Type != FilterMode.None || f.IsPk)
            {
                if (f.DataBehavior == FieldBehavior.ViewOnly)
                {
                    sql.Append(Tab);
                    sql.AppendLine("*/");
                }
            }
        }

        sql.AppendLine();
        sql.AppendLine();
        sql.Append(Tab);
        sql.AppendLine("--ORDER");
        sql.Append(Tab);
        sql.AppendLine("v_sqlorder := ''; ");
        sql.Append(Tab);
        sql.Append("IF ");
        sql.Append(VariablePrefix);
        sql.AppendLine("orderby IS NOT NULL THEN ");
        sql.Append(Tab).Append(Tab);
        sql.Append("v_sqlorder  := ' ORDER BY ' || ");
        sql.Append(VariablePrefix);
        sql.AppendLine("orderby;");
        sql.Append(Tab);
        sql.Append("END IF; ");
        sql.AppendLine();
        sql.AppendLine();


        sql.Append(Tab);
        sql.AppendLine("--TOTAL OF RECORDS");
        sql.Append(Tab);
        sql.Append("IF ");
        sql.Append(VariablePrefix);
        sql.Append("qtdtotal IS NULL OR ");
        sql.Append(VariablePrefix);
        sql.AppendLine("qtdtotal = 0 THEN");
        sql.Append(Tab).Append(Tab);
        sql.Append(VariablePrefix);
        sql.AppendLine("qtdtotal := 0;");
        sql.Append(Tab).Append(Tab);
        sql.AppendLine("v_query := N'SELECT COUNT(*) ' || v_sqltable || v_sqlcond;");
        sql.AppendLine();
        sql.Append(Tab).Append(Tab);
        sql.AppendLine("EXECUTE IMMEDIATE v_query");
        sql.Append(Tab).Append(Tab);
        sql.Append("INTO ");
        sql.Append(VariablePrefix);
        sql.AppendLine("qtdtotal; ");
        sql.Append(Tab);
        sql.AppendLine("END IF;");
        sql.AppendLine();
        sql.AppendLine();


        sql.Append(Tab);
        sql.AppendLine("--DATASET RESULT");
        sql.Append(Tab);
        sql.AppendLine("v_query := 'SELECT ' || v_sqlcolumn || ' ' ||");
        sql.Append(Tab);
        sql.AppendLine("'FROM (");
        sql.Append(Tab).Append(Tab);
        sql.AppendLine("SELECT ROWNUM R_NUM, TMP.* ");
        sql.Append(Tab).Append(Tab);
        sql.AppendLine("FROM (");
        sql.Append(Tab).Append(Tab).Append(Tab);
        sql.AppendLine("SELECT ' || v_sqlcolumn || v_sqltable || v_sqlcond || v_sqlorder || ");
        sql.Append(Tab).Append(Tab);
        sql.AppendLine("') TMP' ||");
        sql.Append(Tab);
        sql.AppendLine("')");
        sql.Append(Tab);
        sql.AppendLine("WHERE R_NUM BETWEEN ' || ");
        sql.Append(Tab);
        sql.Append("to_char((");
        sql.Append(VariablePrefix);
        sql.Append("pag - 1) * ");
        sql.Append(VariablePrefix);
        sql.AppendLine("regporpag + 1) || ");
        sql.Append(Tab);
        sql.Append("' AND ' ||");
        sql.Append(Tab);
        sql.Append("to_char(");
        sql.Append(VariablePrefix);
        sql.Append("pag * ");
        sql.Append(VariablePrefix);
        sql.AppendLine("regporpag); ");
        sql.AppendLine();
        sql.Append(Tab);
        sql.AppendLine("OPEN v_Cursor FOR v_query;");
        sql.Append(Tab);
        sql.Append(VariablePrefix);
        sql.AppendLine("cur_OUT := v_Cursor;");

        sql.AppendLine();
        sql.AppendLine();
        sql.Append(Tab);
        sql.AppendLine("--DBMS_OUTPUT.PUT_LINE(v_query);");

        sql.AppendLine();
        sql.AppendLine("END;");

        return sql.ToString();
    }


    public Task<Element> GetElementFromTableAsync(string schemaName, string connectionId, Guid? guid)
    {
        throw new NotImplementedException();
    }

    public DataAccessCommand GetInsertCommand(Element element, Dictionary<string,object?> values)
    {
        return GetCommandWrite(InsertKeyword, element, values);
    }

    public DataAccessCommand GetUpdateCommand(Element element, Dictionary<string,object?> values)
    {
        return GetCommandWrite(UpdateKeyword, element, values);
    }

    public DataAccessCommand GetDeleteCommand(Element element, Dictionary<string,object> filters)
    {
        return GetCommandWrite(DeleteKeyword, element, filters!);
    }

    public DataAccessCommand GetInsertOrReplaceCommand(Element element, Dictionary<string,object?> values)
    {
        return GetCommandWrite(string.Empty, element, values);
    }

    public bool TableExists(string tableName, Guid? connectionId = null)
    {
        throw new NotImplementedException();
    }

    public Task<bool> TableExistsAsync(string schema, string tableName, Guid? connectionId = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> TableExistsAsync(string tableName, Guid? connectionId = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ColumnExistsAsync(string tableName, string columnName, Guid? connectionId = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<string?> GetStoredProcedureDefinitionAsync(string procedureName, Guid? connectionId = null)
    {
        throw new NotImplementedException();
    }

    public Task DropStoredProcedureAsync(string procedureName, Guid? connectionId = null)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> GetStoredProcedureListAsync(Guid? connectionId = null)
    {
        throw new NotImplementedException();
    }
    
    public string GetAlterTableScript(Element element, IEnumerable<ElementField> fields)
    {
        return "Not implemented";
    }

    private DataAccessCommand GetCommandWrite(string action, Element element, Dictionary<string,object?> values)
    {
        var cmd = new DataAccessCommand
        {
            Type = CommandType.StoredProcedure
        };
        cmd.Sql = _options.GetWriteProcedureName(element);
        cmd.Parameters.Add(new DataAccessParameter($"{VariablePrefix}action", action, DbType.AnsiString, 1));

        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

        foreach (var f in fields)
        {
            var param = new DataAccessParameter
            {
                Name = $"{VariablePrefix}{f.Name}",
                Size = f.Size,
                Value = values.TryGetValue(f.Name, out var value) ? value : DBNull.Value,
                Type = GetDbType(f.DataType)
            };
            cmd.Parameters.Add(param);
        }

        var pRet = new DataAccessParameter
        {
            Direction = ParameterDirection.Output,
            Name = $"{VariablePrefix}RET",
            Type = DbType.Int32
        };
        cmd.Parameters.Add(pRet);

        return cmd;
    }

    public DataAccessCommand GetReadCommand(Element element, EntityParameters entityParameters, DataAccessParameter totalOfRecordsParameter)
    {
        
        var (filters, orderBy, currentPage, recordsPerPage) = entityParameters;
        
        var cmd = new DataAccessCommand
        {
            Type = CommandType.StoredProcedure,
            Sql = _options.GetReadProcedureName(element),
            Parameters =
            {
                new($"{VariablePrefix}orderby", orderBy.ToQueryParameter()),
                new($"{VariablePrefix}regporpag", recordsPerPage),
                new($"{VariablePrefix}pag", currentPage)
            }
        };

        foreach (var field in element.Fields)
        {
            if (field.Filter.Type == FilterMode.Range)
            {
                object? valueFrom = DBNull.Value;
                if (filters != null &&
                    filters.ContainsKey($"{field.Name}_from") &&
                    filters[$"{field.Name}_from"] != null)
                {
                    valueFrom = filters[$"{field.Name}_from"];
                }
                var fromParameter = new DataAccessParameter
                {
                    Direction = ParameterDirection.Input,
                    Type = GetDbType(field.DataType),
                    Size = field.Size,
                    Name = $"{VariablePrefix}{field.Name}_from",
                    Value = valueFrom
                };
                cmd.Parameters.Add(fromParameter);

                object? valueTo = DBNull.Value;
                if (filters != null &&
                    filters.ContainsKey($"{field.Name}_to") &&
                    filters[$"{field.Name}_to"] != null)
                {
                    valueTo = filters[$"{field.Name}_to"];
                }
                var toParameter = new DataAccessParameter
                {
                    Direction = ParameterDirection.Input,
                    Type = GetDbType(field.DataType),
                    Size = field.Size,
                    Name = $"{VariablePrefix}{field.Name}_to",
                    Value = valueTo
                };
                cmd.Parameters.Add(toParameter);
            }
            else if (field.Filter.Type != FilterMode.None || field.IsPk)
            {
                object? value = DBNull.Value;
                if (filters?.ContainsKey(field.Name) == true &&
                    filters[field.Name] != null)
                {
                    value = filters[field.Name];
                }

                var parameter = new DataAccessParameter
                {
                    Direction = ParameterDirection.Input,
                    Type = GetDbType(field.DataType),
                    Size = field.Size,
                    Name = VariablePrefix + field.Name,
                    Value = value
                };
                cmd.Parameters.Add(parameter);
            }
        }

        cmd.Parameters.Add(totalOfRecordsParameter);

        var pCur = new DataAccessParameter
        {
            Name = $"{VariablePrefix}cur_OUT",
            Direction = ParameterDirection.Output,
            Type = DbType.Object
        };

        cmd.Parameters.Add(pCur);


        return cmd;
    }
    
    private static string GetStrType(FieldType dataType)
    {
        var sType = dataType.ToString();
        switch (dataType)
        {
            case FieldType.Int:
                sType = "INTEGER";
                break;
            case FieldType.Varchar:
            case FieldType.NVarchar:
                sType = $"{dataType}2";
                break;
            case FieldType.DateTime:
                sType = "DATE";
                break;
            case FieldType.Text:
                sType = "CLOB";
                break;
            case FieldType.NText:
                sType = "NCLOB";
                break;
        }

        return sType;

    }

    private static DbType GetDbType(FieldType dataType)
    {
        var t = DbType.AnsiString;
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

    private static bool HasPk(Element element)
    {
        var ret = false;
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

    private static bool HasUpdateFields(Element element)
    {
        var ret = false;
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
    
    public Task<Element> GetElementFromTableAsync(string tableName, Guid? connectionId = null)
    {
        throw new NotImplementedException();
    }

}