using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Settings;

namespace JJMasterData.Commons.Dao.Entity;

class OracleProvider : IProvider
{
    private const string INSERT = "I";
    private const string UPDATE = "A";
    private const string DELETE = "E";
    private const string TAB = "\t";

    public string VariablePrefix
    {
        get
        {
            return "p_";
        }
    }

    public string GetCreateTableScript(Element element)
    {
        if (element == null)
            throw new Exception("Invalid element");

        if (element.Fields == null || element.Fields.Count == 0)
            throw new Exception("Invalid fields");

        StringBuilder sSql = new StringBuilder();
        StringBuilder sKeys = new StringBuilder();

        sSql.AppendLine("-- TABLE");
        sSql.Append("CREATE TABLE ");
        sSql.Append(element.TableName);
        sSql.AppendLine(" (");
        bool isFirst = true;
        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

        foreach (var f in fields)
        {
            if (isFirst)
                isFirst = false;
            else
                sSql.AppendLine(",");

            sSql.Append(TAB);
            sSql.Append(f.Name);
            sSql.Append(" ");
            sSql.Append(GetStrType(f.DataType));

            if (f.DataType == FieldType.Varchar ||
                f.DataType == FieldType.NVarchar)
            {
                sSql.Append(" (");
                sSql.Append(f.Size);
                sSql.Append(")");
            }

            if (f.IsRequired)
                sSql.Append(" NOT NULL");
                
            if (f.IsPk)
            {
                if (sKeys.Length > 0)
                    sKeys.Append(",");

                sKeys.Append(f.Name);
                sKeys.Append(" ");
            }
        }

        if (sKeys.Length > 0)
        {
            sSql.AppendLine(", ");
            sSql.Append(TAB);
            sSql.Append("CONSTRAINT PK_");
            sSql.Append(element.TableName);
            sSql.Append(" PRIMARY KEY (");
            sSql.Append(sKeys);
            sSql.Append(")");
        }


        sSql.AppendLine("");
        sSql.AppendLine(")");
        sSql.AppendLine("/");
        sSql.AppendLine("");
        sSql.AppendLine(GetRelationScript(element));
        sSql.AppendLine("");
            
        int nIndex = 1;
        if (element.Indexes.Count > 0)
        {
            foreach (var index in element.Indexes)
            {
                sSql.Append("CREATE");
                sSql.Append(index.IsUnique ? " UNIQUE" : "");
                sSql.Append(index.IsClustered ? " CLUSTERED" : "");
                sSql.Append(" INDEX IX_");
                sSql.Append(element.TableName);
                sSql.Append("_");
                sSql.Append(nIndex);
                sSql.Append(" ON ");
                sSql.AppendLine(element.TableName);

                sSql.Append(TAB);
                sSql.AppendLine("(");
                for (int i = 0; i < index.Columns.Count; i++)
                {
                    if (i > 0)
                        sSql.AppendLine(", ");

                    sSql.Append(TAB);
                    sSql.Append(index.Columns[i]);
                }
                sSql.AppendLine("");
                sSql.Append(TAB);
                sSql.AppendLine(")");
                sSql.AppendLine("/");
                nIndex++;
            }
        }
        sSql.AppendLine("");

        //Criando sequencias (IDENTITY)
        var listSeq = element.Fields.ToList().FindAll(x => x.AutoNum);
        for (int i = 0; i < listSeq.Count; i++)
        {
            sSql.Append("CREATE SEQUENCE ");
            sSql.Append(element.TableName);
            sSql.Append("_seq");
            if (i > 0)
                sSql.Append(i.ToString());

            sSql.AppendLine(" START WITH 1 INCREMENT BY 1;");
            sSql.AppendLine("/");
        }
        sSql.AppendLine("");
            
        return sSql.ToString();
    }

    private string GetRelationScript(Element element)
    {
        StringBuilder sSql = new StringBuilder();

        if (element.Relations.Count > 0)
        {
            sSql.AppendLine("-- RELATIONS");
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

                sSql.Append("ALTER TABLE ");
                sSql.AppendLine(r.ChildElement);
                sSql.Append("ADD CONSTRAINT ");
                sSql.Append(contraintName);
                sSql.AppendLine(" ");
                sSql.Append(TAB);
                sSql.Append("FOREIGN KEY (");

                for (int rc = 0; rc < r.Columns.Count; rc++)
                {
                    if (rc > 0)
                        sSql.Append(", ");

                    sSql.Append("[");
                    sSql.Append(r.Columns[rc].FkColumn);
                    sSql.Append("]");
                }
                sSql.AppendLine(")");
                sSql.Append(TAB);
                sSql.Append("REFERENCES ");
                sSql.Append(element.TableName);
                sSql.Append(" (");
                for (int rc = 0; rc < r.Columns.Count; rc++)
                {
                    if (rc > 0)
                        sSql.Append(", ");

                    sSql.Append(r.Columns[rc].PkColumn);
                }
                sSql.Append(")");

                if (r.UpdateOnCascade)
                {
                    sSql.AppendLine("");
                    sSql.Append(TAB).Append(TAB);
                    sSql.Append("ON UPDATE CASCADE ");
                }

                if (r.DeleteOnCascade)
                {
                    sSql.AppendLine("");
                    sSql.Append(TAB).Append(TAB);
                    sSql.Append("ON DELETE CASCADE ");
                }

                sSql.AppendLine("");
                sSql.AppendLine("/");
            }
        }

        return sSql.ToString();
    }

    public string GetWriteProcedureScript(Element element)
    {
        if (element == null)
            throw new Exception("Invalid element");

        if (element.Fields == null || element.Fields.Count == 0)
            throw new Exception("Invalid fields");

        StringBuilder sql = new StringBuilder();
        bool isFirst = true;
        bool hasPk = HasPK(element);
        bool hasUpd = HasUpdateFields(element);
        string procedureFinalName = JJMasterDataSettings.GetProcNameSet(element);

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
        sql.Append(TAB);
        sql.AppendLine(" v_TYPEACTION VARCHAR2(1); ");
        sql.Append(TAB);
        sql.AppendLine(" v_NCOUNT INTEGER; ");
        sql.AppendLine("BEGIN ");
        sql.Append(TAB);
        sql.Append("v_TYPEACTION := ");
        sql.Append(VariablePrefix);
        sql.AppendLine("action; ");
        sql.Append(TAB);
        sql.AppendLine("IF v_TYPEACTION = ' ' THEN");
        sql.Append(TAB).Append(TAB);
        sql.AppendLine("v_TYPEACTION := '" + INSERT + "'; ");

        if (hasPk)
        {
            sql.Append(TAB).Append(TAB);
            sql.AppendLine("SELECT COUNT(*) AS QTD INTO v_NCOUNT ");
            sql.Append(TAB).Append(TAB);
            sql.Append("FROM ");
            sql.Append(element.TableName);
            isFirst = true;
            foreach (var f in fields)
            {
                if (f.IsPk)
                {
                    sql.AppendLine("");
                    if (isFirst)
                    {
                        sql.Append(TAB).Append(TAB);
                        sql.Append("WHERE ");
                        isFirst = false;
                    }
                    else
                    {
                        sql.Append(TAB).Append(TAB);
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
            sql.Append(TAB).Append(TAB);
            sql.AppendLine("IF v_NCOUNT > 0 THEN ");
            sql.Append(TAB).Append(TAB).Append(TAB);
            sql.AppendLine("v_TYPEACTION := '" + UPDATE + "';");
            sql.Append(TAB).Append(TAB);
            sql.AppendLine("END IF;");
        }
        sql.Append(TAB);
        sql.AppendLine("END IF;");
        sql.AppendLine(" ");


        //SCRIPT INSERT
        sql.Append(TAB);
        sql.AppendLine("IF v_TYPEACTION = '" + INSERT + "' THEN");
        sql.Append(TAB).Append(TAB);
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
        sql.Append(TAB).Append(TAB);
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

                sql.Append(TAB).Append(TAB).Append(TAB);
                sql.Append(VariablePrefix);
                sql.Append(f.Name);
            }
        }
        sql.AppendLine(");");
        sql.Append(TAB).Append(TAB);
        sql.Append(VariablePrefix);
        sql.AppendLine("RET := 0; ");

        //SCRIPT UPDATE
        isFirst = true;
        if (hasUpd)
        {
            sql.Append(TAB);
            sql.AppendLine("ELSIF v_TYPEACTION = '" + UPDATE + "' THEN ");
            sql.Append(TAB).Append(TAB);
            sql.Append("UPDATE ");
            sql.Append(element.TableName);
            sql.AppendLine(" SET ");
            isFirst = true;
            foreach (var f in fields)
            {
                if (!f.IsPk)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        sql.AppendLine(", ");

                    sql.Append(TAB).Append(TAB).Append(TAB);
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
                    sql.AppendLine("");
                    if (isFirst)
                    {
                        sql.Append(TAB).Append(TAB);
                        sql.Append("WHERE ");
                        isFirst = false;
                    }
                    else
                    {
                        sql.Append(TAB).Append(TAB);
                        sql.Append("AND ");
                    }

                    sql.Append(f.Name);
                    sql.Append(" = ");
                    sql.Append(VariablePrefix);
                    sql.Append(f.Name);
                }
            }
            sql.AppendLine(";");
            sql.Append(TAB).Append(TAB);
            sql.Append(VariablePrefix);
            sql.AppendLine("RET := 1; ");
        }
        else
        {
            sql.Append(TAB);
            sql.AppendLine("ELSIF v_TYPEACTION = '" + UPDATE + "' THEN ");
            sql.Append(TAB).Append(TAB);
            sql.AppendLine("--NO UPDATABLED");
            sql.Append(TAB).Append(TAB);
            sql.Append(VariablePrefix);
            sql.AppendLine("RET := 1; ");
        }

        //SCRIPT DELETE
        sql.Append(TAB);
        sql.AppendLine("ELSIF v_TYPEACTION = '" + DELETE + "' THEN ");
        sql.Append(TAB).Append(TAB);
        sql.Append("DELETE FROM ");
        sql.Append(element.TableName);
            
        isFirst = true;
        foreach (var f in fields)
        {
            if (f.IsPk)
            {
                sql.AppendLine("");
                if (isFirst)
                {
                    sql.Append(TAB).Append(TAB);
                    sql.Append("WHERE ");
                    isFirst = false;
                }
                else
                {
                    sql.Append(TAB).Append(TAB);
                    sql.Append("AND ");
                }

                sql.Append(f.Name);
                sql.Append(" = ");
                sql.Append(VariablePrefix);
                sql.Append(f.Name);
            }
        }
        sql.AppendLine(";");
        sql.Append(TAB).Append(TAB);
        sql.Append(VariablePrefix);
        sql.AppendLine("RET := 2; ");
        sql.Append(TAB);
        sql.AppendLine("END IF;");
        sql.AppendLine(" ");
        sql.AppendLine("END; ");
        sql.AppendLine("/");

        return sql.ToString();
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

        StringBuilder sql = new StringBuilder();
        string procedureFinalName = JJMasterDataSettings.GetProcNameGet(element);

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
                sql.Append(" ");
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
        sql.Append(TAB);
        sql.AppendLine("v_sqlcolumn    CLOB;");
        sql.Append(TAB);
        sql.AppendLine("v_sqltable     CLOB;");
        sql.Append(TAB);
        sql.AppendLine("v_sqlcond      CLOB;");
        sql.Append(TAB);
        sql.AppendLine("v_sqlorder     CLOB;");
        sql.Append(TAB);
        sql.AppendLine("v_query        CLOB;");
        sql.Append(TAB);
        sql.AppendLine("v_Cursor       SYS_REFCURSOR;");
        sql.AppendLine("BEGIN");
        sql.AppendLine("");

        sql.Append(TAB);
        sql.AppendLine("--COLUMNS");
        sql.Append(TAB);
        sql.AppendLine("v_sqlcolumn := '';");
        int nAux = 1;
        foreach (var f in fields)
        {
            sql.Append(TAB);

                
            sql.Append("v_sqlcolumn := v_sqlcolumn || '");

            if (f.DataBehavior == FieldBehavior.ViewOnly)
                sql.Append("'''' AS ");

            sql.Append(f.Name);

            if (nAux != element.Fields.Count)
                sql.Append(", ");

            sql.Append("'; ");

            if (f.DataBehavior == FieldBehavior.ViewOnly)
                sql.Append(" --TODO: ");

            sql.AppendLine("");

            nAux++;
        }

        sql.AppendLine("");
        sql.AppendLine("");

        sql.Append(TAB);
        sql.AppendLine("--TABLES");
        sql.Append(TAB);

        sql.Append("v_sqltable := 'FROM ");
        sql.Append(element.TableName);
        sql.AppendLine("';");
        sql.AppendLine("");
        sql.AppendLine("");

        sql.Append(TAB);
        sql.AppendLine("--CONDITIONALS");
        sql.Append(TAB);
        sql.AppendLine("v_sqlcond := ' WHERE 1=1 ';");

        foreach (var f in fields)
        {
            if (f.Filter.Type != FilterMode.None || f.IsPk)
            {
                if (f.DataBehavior == FieldBehavior.ViewOnly)
                {
                    sql.AppendLine("");
                    sql.Append(TAB);
                    sql.AppendLine("/*");
                    sql.Append(TAB);
                    sql.Append("TODO: FILTER ");
                    sql.AppendLine(f.Name);
                }
            }

            if (f.Filter.Type == FilterMode.Range)
            {
                sql.AppendLine("");
                sql.Append(TAB);
                sql.Append("IF ");
                sql.Append(VariablePrefix);
                sql.Append(f.Name);
                sql.AppendLine("_from IS NOT NULL THEN");
                sql.Append(TAB).Append(TAB);
                sql.Append("v_sqlcond := v_sqlcond || ' AND ");
                sql.Append(f.Name);
                sql.AppendLine(" BETWEEN ';");
                sql.Append(TAB).Append(TAB);
                sql.Append("v_sqlcond := v_sqlcond || 'TO_DATE (''' || TO_CHAR (");
                sql.Append(VariablePrefix);
                sql.Append(f.Name);
                sql.AppendLine("_from, 'YYYY-MM-DD') || ''', ''YYYY-MM-DD'') '; ");
                sql.Append(TAB).Append(TAB);
                sql.AppendLine("v_sqlcond := v_sqlcond || ' AND ';");
                sql.Append(TAB).Append(TAB);
                sql.Append("v_sqlcond := v_sqlcond || 'TO_DATE (''' || TO_CHAR (");
                sql.Append(VariablePrefix);
                sql.Append(f.Name);
                sql.AppendLine("_to, 'YYYY-MM-DD') || ''', ''YYYY-MM-DD'') '; ");
                sql.Append(TAB);
                sql.AppendLine("END IF;");
            }
            else if (f.Filter.Type == FilterMode.Contain)
            {
                sql.AppendLine("");
                sql.Append(TAB);
                sql.Append("IF ");
                sql.Append(VariablePrefix);
                sql.Append(f.Name);
                sql.AppendLine(" IS NOT NULL THEN");
                sql.Append(TAB).Append(TAB);
                sql.Append("v_sqlcond := v_sqlcond || ' AND ");
                sql.Append(VariablePrefix);
                sql.Append(" LIKE ''%' || ");
                sql.Append(VariablePrefix);
                sql.Append(f.Name);
                sql.AppendLine(" ||  '%''';");
                sql.Append(TAB);
                sql.AppendLine("END IF;");
            }
            else if (f.Filter.Type == FilterMode.Equal || f.IsPk)
            {
                sql.AppendLine("");
                sql.Append(TAB);
                sql.Append("IF ");
                sql.Append(VariablePrefix);
                sql.Append(f.Name);
                sql.AppendLine(" IS NOT NULL THEN");
                sql.Append(TAB).Append(TAB);
                sql.Append("v_sqlcond := v_sqlcond || ' AND ");
                sql.Append(VariablePrefix);
                sql.Append(" = ''' || ");
                sql.Append(VariablePrefix);
                sql.Append(f.Name);
                sql.AppendLine(" ||  '''';");
                sql.Append(TAB);
                sql.AppendLine("END IF;");
            }

            if (f.Filter.Type != FilterMode.None || f.IsPk)
            {
                if (f.DataBehavior == FieldBehavior.ViewOnly)
                {
                    sql.Append(TAB);
                    sql.AppendLine("*/");
                }
            }
        }

        sql.AppendLine("");
        sql.AppendLine("");
        sql.Append(TAB);
        sql.AppendLine("--ORDER");
        sql.Append(TAB);
        sql.AppendLine("v_sqlorder := ''; ");
        sql.Append(TAB);
        sql.Append("IF ");
        sql.Append(VariablePrefix);
        sql.AppendLine("orderby IS NOT NULL THEN ");
        sql.Append(TAB).Append(TAB);
        sql.Append("v_sqlorder  := ' ORDER BY ' || ");
        sql.Append(VariablePrefix);
        sql.AppendLine("orderby;");
        sql.Append(TAB);
        sql.Append("END IF; ");
        sql.AppendLine("");
        sql.AppendLine("");


        sql.Append(TAB);
        sql.AppendLine("--TOTAL OF RECORDS");
        sql.Append(TAB);
        sql.Append("IF ");
        sql.Append(VariablePrefix);
        sql.Append("qtdtotal IS NULL OR ");
        sql.Append(VariablePrefix);
        sql.AppendLine("qtdtotal = 0 THEN");
        sql.Append(TAB).Append(TAB);
        sql.Append(VariablePrefix);
        sql.AppendLine("qtdtotal := 0;");
        sql.Append(TAB).Append(TAB);
        sql.AppendLine("v_query := N'SELECT COUNT(*) ' || v_sqltable || v_sqlcond;");
        sql.AppendLine("");
        sql.Append(TAB).Append(TAB);
        sql.AppendLine("EXECUTE IMMEDIATE v_query");
        sql.Append(TAB).Append(TAB);
        sql.Append("INTO ");
        sql.Append(VariablePrefix);
        sql.AppendLine("qtdtotal; ");
        sql.Append(TAB);
        sql.AppendLine("END IF;");
        sql.AppendLine("");
        sql.AppendLine("");


        sql.Append(TAB);
        sql.AppendLine("--DATASET RESULT");
        sql.Append(TAB);
        sql.AppendLine("v_query := 'SELECT ' || v_sqlcolumn || ' ' ||");
        sql.Append(TAB);
        sql.AppendLine("'FROM (");
        sql.Append(TAB).Append(TAB);
        sql.AppendLine("SELECT ROWNUM R_NUM, TMP.* ");
        sql.Append(TAB).Append(TAB);
        sql.AppendLine("FROM (");
        sql.Append(TAB).Append(TAB).Append(TAB);
        sql.AppendLine("SELECT ' || v_sqlcolumn || v_sqltable || v_sqlcond || v_sqlorder || ");
        sql.Append(TAB).Append(TAB);
        sql.AppendLine("') TMP' ||");
        sql.Append(TAB);
        sql.AppendLine("')");
        sql.Append(TAB);
        sql.AppendLine("WHERE R_NUM BETWEEN ' || ");
        sql.Append(TAB);
        sql.Append("to_char((");
        sql.Append(VariablePrefix);
        sql.Append("pag - 1) * ");
        sql.Append(VariablePrefix);
        sql.AppendLine("regporpag + 1) || ");
        sql.Append(TAB);
        sql.Append("' AND ' ||");
        sql.Append(TAB);
        sql.Append("to_char(");
        sql.Append(VariablePrefix);
        sql.Append("pag * ");
        sql.Append(VariablePrefix);
        sql.AppendLine("regporpag); ");
        sql.AppendLine("");
        sql.Append(TAB);
        sql.AppendLine("OPEN v_Cursor FOR v_query;");
        sql.Append(TAB);
        sql.Append(VariablePrefix);
        sql.AppendLine("cur_OUT := v_Cursor;");

        sql.AppendLine("");
        sql.AppendLine("");
        sql.Append(TAB);
        sql.AppendLine("--DBMS_OUTPUT.PUT_LINE(v_query);");
            
        sql.AppendLine("");
        sql.AppendLine("END;");

        return sql.ToString();
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

    public DataAccessCommand GetWriteCommand(string action, Element element, Hashtable values)
    {
        DataAccessCommand cmd = new DataAccessCommand();
        cmd.CmdType = System.Data.CommandType.StoredProcedure;
        cmd.Sql = JJMasterDataSettings.GetProcNameSet(element);
        cmd.Parameters = new List<DataAccessParameter>();
        cmd.Parameters.Add(new DataAccessParameter(VariablePrefix + "action", action, DbType.String, 1));

        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

        foreach (var f in fields)
        {
            var param = new DataAccessParameter();
            param.Name = string.Format("{0}{1}", VariablePrefix, f.Name);
            param.Size = f.Size;
            param.Value = values.ContainsKey(f.Name) ? values[f.Name] : DBNull.Value;
            param.Type = GetDbType(f.DataType);
            cmd.Parameters.Add(param);
        }

        var pRet = new DataAccessParameter();
        pRet.Direction = ParameterDirection.Output;
        pRet.Name = VariablePrefix + "RET";
        pRet.Type = DbType.Int32;
        cmd.Parameters.Add(pRet);

        return cmd;
    }

    public DataAccessCommand GetReadCommand(Element element, Hashtable filters, string orderby, int regperpage, int pag, ref DataAccessParameter pTot)
    {
        DataAccessCommand cmd = new DataAccessCommand();
        cmd.CmdType = System.Data.CommandType.StoredProcedure;
        cmd.Sql = JJMasterDataSettings.GetProcNameGet(element);
        cmd.Parameters = new List<DataAccessParameter>();
        cmd.Parameters.Add(new DataAccessParameter(VariablePrefix + "orderby", orderby));
        cmd.Parameters.Add(new DataAccessParameter(VariablePrefix + "regporpag", regperpage));
        cmd.Parameters.Add(new DataAccessParameter(VariablePrefix + "pag", pag));

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
                }
                var pFrom = new DataAccessParameter();
                pFrom.Direction = ParameterDirection.Input;
                pFrom.Type = GetDbType(field.DataType);
                pFrom.Size = field.Size;
                pFrom.Name = VariablePrefix + field.Name + "_from";
                pFrom.Value = valueFrom;
                cmd.Parameters.Add(pFrom);

                object valueTo = DBNull.Value;
                if (filters != null &&
                    filters.ContainsKey(field.Name + "_to") &&
                    filters[field.Name + "_to"] != null)
                {
                    valueTo = filters[field.Name + "_to"];
                }
                var pTo = new DataAccessParameter();
                pTo.Direction = ParameterDirection.Input;
                pTo.Type = GetDbType(field.DataType);
                pTo.Size = field.Size;
                pTo.Name = VariablePrefix + field.Name + "_to";
                pTo.Value = valueTo;
                cmd.Parameters.Add(pTo);
            }
            else if (field.Filter.Type != FilterMode.None || field.IsPk)
            {
                object value = DBNull.Value;
                if (filters != null &&
                    filters.ContainsKey(field.Name) &&
                    filters[field.Name] != null)
                {
                    value = filters[field.Name];
                }

                var p = new DataAccessParameter();
                p.Direction = ParameterDirection.Input;
                p.Type = GetDbType(field.DataType);
                p.Size = field.Size;
                p.Name = VariablePrefix + field.Name;
                p.Value = value;
                cmd.Parameters.Add(p);
            }
        }

        cmd.Parameters.Add(pTot);

        var pCur = new DataAccessParameter();
        pCur.Name = VariablePrefix + "cur_OUT";
        pCur.Direction = ParameterDirection.Output;
        pCur.Type = DbType.Object;
            
        cmd.Parameters.Add(pCur);

            
        return cmd;
    }

    public DataTable GetDataTable(Element element, Hashtable filters, string orderby, int regporpag, int pag, ref int tot, ref DataAccess dataAccess)
    {
        DataAccessParameter pTot = new DataAccessParameter(VariablePrefix + "qtdtotal", tot, DbType.Int32, 0, ParameterDirection.InputOutput);
        var cmd = GetReadCommand(element, filters, orderby, regporpag, pag, ref pTot);
        DataTable dt = dataAccess.GetDataTable(cmd);
        tot = 0;
        if (pTot != null && pTot.Value != null && pTot.Value != DBNull.Value)
            tot = (int)pTot.Value;

        return dt;
    }

    private string GetStrType(FieldType dataType)
    {
        string sType = dataType.ToString();
        switch (dataType)
        {
            case FieldType.Int:
                sType = "INTEGER";
                break;
            case FieldType.Varchar:
            case FieldType.NVarchar:
                sType = dataType + "2";
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

}