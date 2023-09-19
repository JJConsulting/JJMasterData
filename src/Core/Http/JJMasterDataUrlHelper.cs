#nullable enable

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using JJMasterData.Core.Options;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JJMasterData.Core.Web;

public class JJMasterDataUrlHelper 
{
    private IHttpRequest HttpRequest { get; }
    private string? JJMasterDataUrl { get; }
    public JJMasterDataUrlHelper(IHttpRequest httpRequest,IOptions<JJMasterDataCoreOptions> options)
    {
        JJMasterDataUrl = options.Value.JJMasterDataUrl;
        HttpRequest = httpRequest;
    }

    public string GetUrl([AspMvcAction]string? action = null, [AspMvcController] string? controller = null,string? area = null, object? values = null)
    {

        string baseUrl;
        
        if (JJMasterDataUrl is null || string.IsNullOrEmpty(JJMasterDataUrl))
        {
            var appPath = HttpRequest.ApplicationPath;

            baseUrl = appPath.IsNullOrEmpty() ? "/" : appPath;
        }
        else
        {
            baseUrl = JJMasterDataUrl;
        }


        var valuesDictionary = new Dictionary<string, string?>();
        
        if (values != null)
        {
            valuesDictionary = values.GetType().GetProperties()
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(values)?.ToString());
        }
        
        if(valuesDictionary.TryGetValue("dictionaryName", out var dictionaryName))
        {
            valuesDictionary.Remove("dictionaryName");
        }
        
        var url = baseUrl;

        url += $"{CultureInfo.CurrentUICulture}/";
        
        if (!string.IsNullOrEmpty(area))
        {
            url += $"{area}/";
        }

        if (!string.IsNullOrEmpty(controller))
        {
            url += $"{controller}/";
        }

        if (!string.IsNullOrEmpty(action))
        {
            url += $"{action}/";
        }

        if (!string.IsNullOrEmpty(dictionaryName))
        {
            url += $"{dictionaryName}";
        }
        
        if (valuesDictionary.Count > 0)
        {
            url += $"?{string.Join("&", valuesDictionary.Select(kv => $"{kv.Key}={kv.Value}"))}";
        }
        

        return url;
    }
}