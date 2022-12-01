using System;
using System.IO;
using System.Net;
using System.Text;

namespace JJMasterData.Commons.Protheus;

/// <summary>
/// Classe responsável por gerenciar as conexões e chamadas para o Protheus
/// </summary>
public static class ProtheusManager 
{

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
        try
        {
            var url = new StringBuilder();
            url.Append(urlProtheus);
            url.Append("?request=");
            url.Append(functionname);
            url.Append("&parms=");
            url.Append(parms);
            var targetUri = new Uri(url.ToString());
            var fr = (HttpWebRequest)WebRequest.Create(targetUri);
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

            using (var response = fr.GetResponse())
            {
                if (response.ContentLength > 0)
                {
                    var sr = new StreamReader(response.GetResponseStream()!, Encoding.Default);
                    sRet = sr.ReadToEnd();
                }
            }
        }
        catch (Exception ex)
        {
            throw new ProtheusException(ex.Message, ex);
        }

        return sRet;
    }
}