using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace JJMasterData.Commons.Dao.Entity;

class ProviderSQLite : IProvider
{
    private const string INSERT = "I";
    private const string UPDATE = "A";
    private const string DELETE = "E";
    private const string TAB = "\t";

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

        StringBuilder sSql = new StringBuilder();

        sSql.AppendLine("-- TABLE");
        sSql.Append("CREATE TABLE [");
        sSql.Append(element.TableName);
        sSql.AppendLine("] (");
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
            sSql.Append("[");
            sSql.Append(f.Name);
            sSql.Append("] ");

            switch (f.DataType)
            {
                case FieldType.Int:
                    sSql.Append("INTEGER");
                    break;
                case FieldType.Float:
                    sSql.Append("Real");
                    break;
                case FieldType.NText:
                case FieldType.NVarchar:
                case FieldType.Varchar:
                    sSql.Append("TEXT");
                    break;
                default:
                    sSql.Append(f.DataType.ToString());
                    break;
            }

            if (f.IsRequired)
                sSql.Append(" NOT NULL");

            if (f.AutoNum && f.IsPk)
                sSql.Append(" PRIMARY KEY AUTOINCREMENT ");
        }
        
        isFirst = true;
        foreach (var f in fields.ToList().FindAll(x => x.IsPk && !x.AutoNum))
        {
            if (isFirst)
            {
                isFirst = false;
                sSql.AppendLine(",");
                sSql.Append(TAB);
                sSql.Append("PRIMARY KEY (");
            }
            else
            {
                sSql.Append(",");
            }

            sSql.Append("[");
            sSql.Append(f.Name);
            sSql.Append("] ");
        }

        if (!isFirst)
            sSql.Append(")");


        sSql.AppendLine("");
        sSql.AppendLine(")");
        sSql.AppendLine("GO");
        sSql.AppendLine("");

        //sSql.AppendLine(DoSqlCreateRelation(element));
        sSql.AppendLine("");
        
        int nIndex = 1;
        if (element.Indexes.Count > 0)
        {
            foreach (var index in element.Indexes)
            {
                sSql.Append("CREATE");
                sSql.Append(index.IsUnique ? " UNIQUE" : "");
                sSql.Append(index.IsClustered ? " CLUSTERED" : "");
                sSql.Append(" INDEX [IX_");
                sSql.Append(element.TableName);
                sSql.Append("_");
                sSql.Append(nIndex);
                sSql.Append("] ON ");
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
                sSql.AppendLine("GO");
                nIndex++;
            }
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
                sSql.Append("ADD CONSTRAINT [");
                sSql.Append(contraintName);
                sSql.AppendLine("] ");
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

                    sSql.Append("[");
                    sSql.Append(r.Columns[rc].PkColumn);
                    sSql.Append("]");
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
                sSql.AppendLine("GO");
            }
        }

        return sSql.ToString();
    }

    public string GetWriteProcedureScript(Element element)
    {
        return null;
    }

    public string GetReadProcedureScript(Element element)
    {
        return null;
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

    public DataAccessCommand GetReadCommand(Element element, Hashtable filters, string orderby, int regporpag, int pag, ref DataAccessParameter pTot)
    {
        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

        var isFirst = true;
        var sSql = new StringBuilder();

        sSql.Append("SELECT * FROM ");
        sSql.Append(element.TableName);

        foreach (DictionaryEntry filter in filters)
        {
            if (isFirst)
            {
                sSql.Append(TAB).Append(TAB);
                sSql.Append(" WHERE ");
                isFirst = false;
            }
            else
            {
                sSql.Append(TAB).Append(TAB);
                sSql.Append("AND ");
            }

            sSql.Append(filter.Key);
            sSql.Append(" = ");
            sSql.AppendLine("?");

        }

        if (!string.IsNullOrEmpty(orderby))
        {
            sSql.Append(" ORDER BY ");
            sSql.Append(orderby);
        }

        if (pTot != null && (int)pTot.Value == 0 && regporpag > 0)
        {
            var offset = (pag - 1) * regporpag;
            sSql.Append("LIMIT ");
            sSql.Append(regporpag);
            sSql.Append(" OFFSET");
            sSql.Append(offset);
        }

        DataAccessCommand cmd = new DataAccessCommand();
        cmd.CmdType = System.Data.CommandType.Text;
        cmd.Sql = sSql.ToString();
        cmd.Parameters = new List<DataAccessParameter>();

        foreach (DictionaryEntry filter in filters)
        {
            ElementField f = element.Fields[filter.Key.ToString()];
            var param = new DataAccessParameter();
            //param.Name = string.Format(f.Name);
            //param.Size = f.Size;
            param.Direction = ParameterDirection.Input;
            param.Value = filter.Value;
            param.Type = GetDbType(f.DataType);
            cmd.Parameters.Add(param);
        }

        return cmd;
    }

    public DataAccessCommand GetWriteCommand(string action, Element element, Hashtable values)
    {
        DataAccessCommand cmd;
        switch (action)
        {
            case INSERT:
                cmd = GetScriptInsert(element, values, false);
                break;
            case UPDATE:
                cmd = GetScriptUpdate(element, values);
                break;
            case DELETE:
                cmd = GetScriptDelete(element, values);
                break;
            default:
                cmd = GetScriptInsert(element, values, true);//REPLACE
                break;
        }

        return cmd;
    }

    public DataTable GetDataTable(Element element, Hashtable filters, string orderby, int regporpag, int pag, ref int tot, ref IDataAccess dataAccess)
    {
        DataAccessParameter pTot = null;
        var cmd = GetReadCommand(element, filters, orderby, regporpag, pag, ref pTot);
        DataTable dt = dataAccess.GetDataTable(cmd);
        tot = 0;

        if (regporpag > 0 && tot == 0)
        {
            var obj = dataAccess.GetResult(GetScriptCount(element, filters));
            if (obj != null)
                tot = int.Parse(obj.ToString());
        }

        return dt;
    }


    private DataAccessCommand GetScriptInsert(Element element, Hashtable values, bool isReplace)
    {
        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real
                          && !x.AutoNum);

        var sSql = new StringBuilder();
        if (isReplace)
            sSql.Append("REPLACE INTO ");
        else
            sSql.Append("INSERT INTO ");

        sSql.Append(element.TableName);
        sSql.Append(" (");

        bool isFirst = true;
        foreach (var c in fields)
        {
            if (isFirst)
                isFirst = false;
            else
                sSql.AppendLine(",");

            sSql.Append(c.Name);
        }
        sSql.Append(")");
        sSql.Append(" VALUES (");
        isFirst = true;
        foreach (var f in fields)
        {
            if (isFirst)
                isFirst = false;
            else
                sSql.AppendLine(",");

            sSql.Append("?");

        }
        sSql.Append(")");

        DataAccessCommand cmd = new DataAccessCommand();
        cmd.CmdType = System.Data.CommandType.Text;
        cmd.Sql = sSql.ToString();
        cmd.Parameters = new List<DataAccessParameter>();

        foreach (var f in fields)
        {
            object value = GetElementValue(f, values);
            var param = new DataAccessParameter();
            //param.Name = string.Format(f.Name);
            //param.Size = f.Size;
            param.Direction = ParameterDirection.Input;
            param.Value = value;
            param.Type = GetDbType(f.DataType);
            cmd.Parameters.Add(param);
        }

        return cmd;
    }

    private DataAccessCommand GetScriptUpdate(Element element, Hashtable values)
    {
        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

        var sSql = new StringBuilder();
        sSql.Append("UPDATE ");
        sSql.Append(element.TableName);
        sSql.Append(" SET ");

        bool isFirst = true;
        foreach (var c in fields)
        {
            if (!c.IsPk)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sSql.AppendLine(",");

                sSql.Append(c.Name);
                sSql.Append(" = ");
                sSql.Append(VariablePrefix + c.Name);
            }
        }

        isFirst = true;
        foreach (var f in fields)
        {
            if (f.IsPk)
            {
                if (isFirst)
                {
                    sSql.Append(TAB).Append(TAB);
                    sSql.Append(" WHERE ");
                    isFirst = false;
                }
                else
                {
                    sSql.Append(TAB).Append(TAB);
                    sSql.Append("AND ");
                }

                sSql.Append(f.Name);
                sSql.Append(" = ");
                sSql.AppendLine(VariablePrefix + f.Name);
            }
        }


        DataAccessCommand cmd = new DataAccessCommand();
        cmd.CmdType = System.Data.CommandType.Text;
        cmd.Sql = sSql.ToString();
        cmd.Parameters = new List<DataAccessParameter>();

        foreach (var f in fields)
        {
            object value = GetElementValue(f, values);
            var param = new DataAccessParameter();
            param.Name = string.Format(VariablePrefix + f.Name);
            //param.Size = f.Size;
            param.Direction = ParameterDirection.Input;
            param.Value = value;
            param.Type = GetDbType(f.DataType);
            cmd.Parameters.Add(param);
        }

        return cmd;

    }

    private DataAccessCommand GetScriptDelete(Element element, Hashtable values)
    {
        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

        bool isFirst = true;
        var sSql = new StringBuilder();

        sSql.Append("DELETE FROM ");
        sSql.Append(element.TableName);

        isFirst = true;
        foreach (var f in fields)
        {
            if (f.IsPk)
            {
                if (isFirst)
                {
                    sSql.Append(TAB).Append(TAB);
                    sSql.Append(" WHERE ");
                    isFirst = false;
                }
                else
                {
                    sSql.Append(TAB).Append(TAB);
                    sSql.Append("AND ");
                }

                sSql.Append(f.Name);
                sSql.Append(" = ");
                sSql.AppendLine(VariablePrefix + f.Name);
            }
        }

        DataAccessCommand cmd = new DataAccessCommand();
        cmd.CmdType = System.Data.CommandType.Text;
        cmd.Sql = sSql.ToString();
        cmd.Parameters = new List<DataAccessParameter>();

        foreach (var f in fields)
        {
            object value = GetElementValue(f, values);
            var param = new DataAccessParameter();
            param.Name = string.Format(VariablePrefix + f.Name);
            //param.Size = f.Size;
            param.Direction = ParameterDirection.Input;
            param.Value = value;
            param.Type = GetDbType(f.DataType);
            cmd.Parameters.Add(param);
        }

        return cmd;
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
                //if (f.DataType == TField.FLOAT)
                //{
                //    double nValue;
                //    if (double.TryParse(values[f.Name].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out nValue))
                //        value = nValue;
                //}
                //else
                //{
                    
                //}
                
            }
        }

        return value;
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
                t = DbType.Single;
                break;
            case FieldType.Int:
                t = DbType.Int32;
                break;
        }
        return t;
    }

    private DataAccessCommand GetScriptCount(Element element, Hashtable filters)
    {
        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

        var isFirst = true;
        var sSql = new StringBuilder();

        sSql.Append("SELECT Count(*) FROM ");
        sSql.Append(element.TableName);

        foreach (var f in fields)
        {
            if (f.IsPk)
            {
                if (isFirst)
                {
                    sSql.Append(TAB).Append(TAB);
                    sSql.Append(" WHERE ");
                    isFirst = false;
                }
                else
                {
                    sSql.Append(TAB).Append(TAB);
                    sSql.Append("AND ");
                }

                sSql.Append(f.Name);
                sSql.Append(" = ");
                sSql.AppendLine("?");
            }
        }

        DataAccessCommand cmd = new DataAccessCommand();
        cmd.CmdType = System.Data.CommandType.Text;
        cmd.Sql = sSql.ToString();
        cmd.Parameters = new List<DataAccessParameter>();

        foreach (var f in fields)
        {
            object value = GetElementValue(f, filters);
            var param = new DataAccessParameter();
            //param.Name = string.Format(f.Name);
            //param.Size = f.Size;
            param.Direction = ParameterDirection.Input;
            param.Value = value;
            param.Type = GetDbType(f.DataType);
            cmd.Parameters.Add(param);
        }

        return cmd;
    }


}