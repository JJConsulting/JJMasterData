using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace JJMasterData.Commons.Protheus;

/// <summary>
/// Classe responsável por gerenciar as conexões e chamadas para o Protheus
/// </summary>
public static class ProtheusManager 
{
    private static List<Connection> listConnection = new List<Connection>();

    public static void ResetConnection()
    {
        lock (listConnection)
        {
            foreach (Connection cx in listConnection)
            {
                cx.ProtheusConn.APDestroyConnControl(cx.ObjectIdCon);
                cx.ProtheusConn.Dispose();
            }

            listConnection = new List<Connection>();
        }
    }
       
    /// <summary>
    /// Executa conexão com o Protheus através das DLLs APAPI.dll APCONN.dll
    /// </summary>
    /// <param name="config">Configurações de conexão com o Protheus</param>
    /// <param name="functionname">Nome da Função</param>
    /// <param name="param">Paramentros da Função</param>
    /// <returns>Retorno da função executada no Protheus</returns>
    public static string CallFunction(ProtheusConfig config, string functionname, string param)
    {
        Connection cx = GetConnection(config);
        if (cx == null)
            cx = CreateConnection(config);

        string sRet = null;
        lock (cx.ProtheusConn)
        {
            bool isConnect = true; //cx.ProtheusConn.APConnect(cx.ObjectIdCon);
            if (isConnect)
            {
                cx.ProtheusConn.AddStringParam(cx.ObjectIdCon, cx.Config.Empresa);
                cx.ProtheusConn.AddStringParam(cx.ObjectIdCon, cx.Config.Filial);
                cx.ProtheusConn.AddStringParam(cx.ObjectIdCon, param);
                cx.ProtheusConn.CallProc(cx.ObjectIdCon, functionname);
                sRet = cx.ProtheusConn.ResultAsString(cx.ObjectIdCon);
                //cx.ProtheusConn.APDisconnect(cx.ObjectIdCon); 
            }
            else
            {
                StringBuilder sErr = new StringBuilder();
                sErr.AppendLine("Não foi possível realizar a conexão com o servidor protheus.");
                sErr.AppendLine("Provavelmente o serviço esteja fora do ar ou inacessível.");
                sErr.AppendLine("Dados do RPC: ");
                sErr.Append("DLL Path: ");
                sErr.AppendLine(cx.Config.DllPath);
                sErr.Append("Servidor: ");
                sErr.AppendLine(cx.Config.Server);
                sErr.Append("Porta: ");
                sErr.AppendLine(cx.Config.Port.ToString());
                sErr.Append("Ambiente: ");
                sErr.AppendLine(cx.Config.Environment);
                sErr.Append("Usuário: ");
                sErr.AppendLine(cx.Config.User);
                throw new Exception(sErr.ToString());
            }
        }

        return sRet; 
    }

    /// <summary>
    /// Executa conexão com o Protheus por um serviço http
    /// </summary>
    /// <param name="urlProtheus">Url do Servidor Protheus</param>
    /// <param name="functionname">Nome da função</param>
    /// <param name="parms">Paramentros separados por ponto e virgula</param>
    /// <returns>Retorno da função executada no Protheus</returns>
    /// <example>
    /// <code lang="c#">
    /// <![CDATA[
    /// string urlProtheus = "http://10.0.0.6:8181/websales/jjmain.apw";
    /// string functionName = "u_vldpan";
    /// string parms = "1;2";
    /// string ret = ProtheusManager.CallOrcLib(urlProtheus, functionName, parms);
    /// ]]>
    /// </code> 
    /// </example>
    /// <remarks>Pré requisito aplicar o patch JJxFun e configurar a conexão http no Protheus</remarks>
    public static string CallOrcLib(string urlProtheus, string functionname, string parms)
    {
        string sRet = null;
        HttpWebRequest fr = null;
        try
        {
            StringBuilder url = new StringBuilder();
            url.Append(urlProtheus);
            url.Append("?request=");
            url.Append(functionname);
            url.Append("&parms=");
            url.Append(parms);
            Uri targetUri = new Uri(url.ToString());
            fr = (HttpWebRequest)HttpWebRequest.Create(targetUri);
            fr.Timeout = 600000;
            fr.ReadWriteTimeout = 600000;
            fr.AllowAutoRedirect = false;
            fr.KeepAlive = true;

            var postData = "token=JJWEBSALES";
            var data = Encoding.ASCII.GetBytes(postData);

            fr.Method = "POST";
            fr.ContentType = "application/x-www-form-urlencoded";
            fr.ContentLength = data.Length;

            using (var stream = fr.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            using (WebResponse response = fr.GetResponse())
            {
                if ((response.ContentLength > 0))
                {
                    StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.Default);
                    sRet = sr.ReadToEnd();
                }
            }
        }
        catch (Exception ex)
        {
            throw new ProtheusException(ex.Message, ex);
        }
        finally
        {
            fr = null;
        }

        return sRet;
    }


    private static Connection CreateConnection(ProtheusConfig config)
    {
        Connection cx = null; 
        lock (listConnection)
        {
            ProtheusConnect conn = new ProtheusConnect(config.DllPath);
            int objectId = conn.APCreateConnControl(config.Server, config.Port, config.Environment, config.User, config.PassWord);

            bool isConnect = conn.APConnect(objectId);
            if (!isConnect)
            {
                throw new Exception("Não foi possível realizar a conexão com o servidor protheus, provavelmente o serviço esteja fora do ar ou inacessível");
            }

            cx = new Connection();
            cx.DateStartConn = DateTime.Now;
            cx.Config = config;
            cx.ProtheusConn = conn;
            cx.ObjectIdCon = objectId;
            listConnection.Add(cx);
        }

        return cx;
    }
        
    private static Connection GetConnection(ProtheusConfig config)
    {
        Connection cxRet = null;
        lock (listConnection)
        {
            foreach (Connection cx in listConnection)
            {
                if (cx.Config.Empresa.Equals(config.Empresa) &&
                    cx.Config.Filial.Equals(config.Filial) &&
                    cx.Config.DllPath.Equals(config.DllPath))
                {
                    cxRet = cx;
                    break;
                }
            }
        }

        return cxRet;
    }

    private class Connection
    {
        public DateTime DateStartConn { get; set; }
        public ProtheusConfig Config { get; set; }
        public ProtheusConnect ProtheusConn { get; set; }
        public int ObjectIdCon { get; set; }
    }

}