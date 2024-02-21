#nullable enable
#if !NET
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JJMasterData.Core.Http.SystemWeb;

public class SystemWebUrlHelperWrapper(IHttpRequest httpRequest, IOptionsSnapshot<MasterDataCoreOptions> options)
    : IMasterDataUrlHelper
{
    private IHttpRequest HttpRequest { get; } = httpRequest;
    private string? MasterDataUrl { get; } = options.Value.MasterDataUrl;
    private bool EnableCultureProvider { get; } = options.Value.EnableCultureProviderAtUrl;

    public string Action([AspMvcAction]string? action = null, [AspMvcController] string? controller =
 null, object? values = null)
    {

        string baseUrl;
        
        if (MasterDataUrl is null || string.IsNullOrEmpty(MasterDataUrl))
        {
            var appPath = HttpRequest.ApplicationPath;

            baseUrl = appPath.IsNullOrEmpty() ? "/" : appPath;
        }
        else
        {
            baseUrl = MasterDataUrl;
        }

        if (!baseUrl.EndsWith("/"))
        {
            baseUrl += "/";
        }

        var valuesDictionary = new Dictionary<string, string?>();
        
        if (values != null)
        {
            valuesDictionary = values.GetType().GetProperties()
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(values)?.ToString());
        }
        
        if(valuesDictionary.TryGetValue("elementName", out var elementName))
        {
            valuesDictionary.Remove("elementName");
        }
        
        if(valuesDictionary.TryGetValue("Area", out var areaValue))
        {
            valuesDictionary.Remove("Area");
        }
        
        var url = baseUrl;

        if(EnableCultureProvider)
            url += $"{CultureInfo.CurrentUICulture}/";
        
        if (!string.IsNullOrEmpty(areaValue))
        {
            url += $"{areaValue}/";
        }

        if (!string.IsNullOrEmpty(controller))
        {
            url += $"{controller}/";
        }

        if (!string.IsNullOrEmpty(action))
        {
            url += $"{action}/";
        }

        if (!string.IsNullOrEmpty(elementName))
        {
            url += $"{elementName}";
        }
        
        if (valuesDictionary.Count > 0)
        {
            url += $"?{string.Join("&", valuesDictionary.Select(kv => $"{kv.Key}={kv.Value}"))}";
        }
        

        return url;
    }

}
#endif