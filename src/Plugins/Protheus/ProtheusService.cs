using System.Text;
using JJMasterData.Protheus.Abstractions;

namespace JJMasterData.Protheus;

/// <summary>
/// Execute connection with Protheus through an http service
/// </summary>
/// <param name="urlProtheus">Protheus Server Url</param>
/// <param name="functionname">Function name</param>
/// <param name="parms">Parameters separated by semicolons</param>
/// <returns>Return of the function executed in Protheus</returns>
/// <example>
/// <code lang="c#">
/// <![CDATA[
/// string urlProtheus = "http://10.0.0.6:8181/websales/jjmain.apw";
/// string functionName = "u_vldpan";
/// string parms = "1;2";
/// string result = ProtheusManager.CallOrcLib(urlProtheus, functionName, parms);
/// ]]>
/// </code>
/// </example>
/// <remarks>Pre-requisite to apply the JJxFun patch and configure the http connection in Protheus</remarks>
public class ProtheusService : IProtheusService
{
    private readonly HttpClient _httpClient;

    public ProtheusService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> CallFunctionAsync(string urlProtheus, string functionName, string parms)
    {
        try
        {
            var url = new StringBuilder();
            url.Append(urlProtheus);
            url.Append("?request=");
            url.Append(functionName);
            url.Append("&parms=");
            url.Append(parms);

            const string postData = "token=JJWEBSALES";
            var data = Encoding.ASCII.GetBytes(postData);

            var content = new ByteArrayContent(data);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

            using var response = await _httpClient.PostAsync(url.ToString(), content);

            var responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
        catch (Exception ex)
        {
            throw new ProtheusException(ex.Message, ex);
        }
    }
} 