using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using JJMasterData.Commons.Data;

namespace JJMasterData.Commons.Sys;

#if NET48
/// <summary>
/// Parametros e configurações armazenadas no banco de dados
/// </summary>
public class Param
{
    public string TableName { get; set; }
    private DataAccess Dao { get; set; }

    public Param()
    {
        Dao = new DataAccess();
        TableName = "tb_sysparam";
    }

    /// <summary>
    /// Parametros do sistema armazenados no banco de dados
    /// </summary>
    /// <param name="strConn">Nome da String de conexão</param>
    /// <param name="strProvider">Provider da Conexão</param>
    /// <param name="tablename">Nome da Tabela</param>
    public Param(string strConn, string strProvider, string tablename)
    {
        Dao = new DataAccess(strConn, strProvider);
        TableName = tablename;
    }


    /// <summary>
    /// Verifica se existe o parametro cadastrado
    /// </summary>
    /// <param name="key">Chave do parametro</param>
    /// <returns></returns>
    public bool HasParam(string key)
    {
        bool bRet = false;
        try
        {
            StringBuilder sSql = new StringBuilder();
            sSql.Append("SELECT COUNT(*) AS QTD FROM [");
            sSql.Append(TableName);
            sSql.Append("] WHERE cfg_txt_key = @cfg_txt_key");

            var parms = new List<DataAccessParameter> { new DataAccessParameter("@cfg_txt_key", key, DbType.String) };
            bRet = int.Parse(Dao.GetResult(new DataAccessCommand(sSql.ToString(), parms)).ToString()) > 0;
        }
        catch (Exception)
        {
            if (TryCreateStructure())
                bRet = HasParam(key);
            else
                throw;
        }

        return bRet;
    }

    /// <summary>
    /// Recupera o parametro cadastrado
    /// </summary>
    /// <param name="key">Chave do parametro</param>
    /// <returns></returns>
    public string GetParam(string key)
    {
        string sRet = null;
        try
        {
            StringBuilder sSql = new StringBuilder();
            sSql.Append("SELECT cfg_txt_value AS QTD FROM [");
            sSql.Append(TableName);
            sSql.Append("] WHERE cfg_txt_key = @cfg_txt_key");

            List<DataAccessParameter> parms = new List<DataAccessParameter>();
            parms.Add(new DataAccessParameter("@cfg_txt_key", key, DbType.String));
            object oRet = Dao.GetResult(new DataAccessCommand(sSql.ToString(), parms));
            if (oRet != null)
                sRet = oRet.ToString();
        }
        catch (Exception)
        {
            if (TryCreateStructure())
                sRet = GetParam(key);
            else
                throw;
        }

        return sRet;
    }

    /// <summary>
    /// Grava o parametro no banco
    /// </summary>
    /// <param name="key">Chave do parametro</param>
    /// <param name="value">Valor do parametro</param>
    /// <param name="description">Descrição do parametro</param>
    /// <returns>
    /// Se existir atualiza o parametro se não existir inclui
    /// </returns>
    public int SetParam(string key, string value, string description)
    {
        int nQtd;
        try
        {
            nQtd = Dao.SetCommand(HasParam(key)
                ? GetCmdUpdate(key, value, description)
                : GetCmdInsert(key, value, description));
        }
        catch (Exception)
        {
            if (TryCreateStructure())
                nQtd = SetParam(key, value, description);
            else
                throw;
        }

        return nQtd;
    }

    /// <summary>
    /// Exclui um parametro armazenado
    /// </summary>
    /// <param name="key">Chave do parametro</param>
    public void DelParam(string key)
    {
        try
        {
            Dao.SetCommand(GetCmdDelete(key));
        }
        catch (Exception)
        {
            if (TryCreateStructure())
                DelParam(key);
            else
                throw;
        }
    }

    /// <summary>
    /// Cria estrutura no banco para armazenar os parametro
    /// Tabela padrão (tb_sysparam)
    /// </summary>
    /// <returns></returns>
    public bool TryCreateStructure()
    {
        bool bCreate = false;
        try
        {
            if (!Dao.TableExists(TableName))
            {
                CreateDbStructureParam();
                bCreate = true;
            }
        }
        catch
        {
            // ignored
        }

        return bCreate;
    }


    private DataAccessCommand GetCmdInsert(string key, string value, string description)
    {
        StringBuilder sSql = new StringBuilder();
        sSql.Append("INSERT INTO [");
        sSql.Append(TableName);
        sSql.Append("] ");
        sSql.Append("(cfg_txt_key, cfg_txt_value, cfg_txt_desc) ");
        sSql.Append("VALUES ");
        sSql.Append("(@cfg_txt_key, @cfg_txt_value, @cfg_txt_desc) ");

        List<DataAccessParameter> parms = new List<DataAccessParameter>();
        parms.Add(new DataAccessParameter("@cfg_txt_key", key, DbType.String));
        parms.Add(new DataAccessParameter("@cfg_txt_value", value, DbType.String));
        parms.Add(new DataAccessParameter("@cfg_txt_desc", description, DbType.String));

        DataAccessCommand cmd = new DataAccessCommand(sSql.ToString(), parms);
        return cmd;
    }


    private DataAccessCommand GetCmdUpdate(string key, string value, string description)
    {
        StringBuilder sSql = new StringBuilder();
        sSql.Append("UPDATE [");
        sSql.Append(TableName);
        sSql.Append("] SET ");
        sSql.Append("cfg_txt_value = @cfg_txt_value, ");
        sSql.Append("cfg_txt_desc = @cfg_txt_desc ");
        sSql.Append("WHERE cfg_txt_key = @cfg_txt_key ");

        List<DataAccessParameter> parms = new List<DataAccessParameter>();
        parms.Add(new DataAccessParameter("@cfg_txt_key", key, DbType.String));
        parms.Add(new DataAccessParameter("@cfg_txt_value", value, DbType.String));
        parms.Add(new DataAccessParameter("@cfg_txt_desc", description, DbType.String));

        DataAccessCommand cmd = new DataAccessCommand(sSql.ToString(), parms);
        return cmd;
    }


    private DataAccessCommand GetCmdDelete(string key)
    {
        StringBuilder script = new StringBuilder();
        script.Append("DELETE [");
        script.Append(TableName);
        script.Append("] WHERE cfg_txt_key = @cfg_txt_key ");

        List<DataAccessParameter> parms = new List<DataAccessParameter>();
        parms.Add(new DataAccessParameter("@cfg_txt_key", key, DbType.String));

        DataAccessCommand cmd = new DataAccessCommand(script.ToString(), parms);
        return cmd;
    }


    private void CreateDbStructureParam()
    {
        StringBuilder script = new StringBuilder();
        script.AppendLine("CREATE TABLE [" + TableName + "]( ");
        script.AppendLine("	[cfg_txt_key] [varchar](30) NOT NULL, ");
        script.AppendLine("	[cfg_txt_value] [nvarchar](200) NOT NULL, ");
        script.AppendLine("	[cfg_txt_desc] [varchar](400) NULL, ");
        script.AppendLine(" CONSTRAINT [PK_" + TableName + "] PRIMARY KEY NONCLUSTERED  ");
        script.AppendLine("	( ");
        script.AppendLine("		[cfg_txt_key] ASC ");
        script.AppendLine("	) ON [PRIMARY] ");
        script.AppendLine(") ");
        Dao.SetCommand(script.ToString());
    }
}
#endif