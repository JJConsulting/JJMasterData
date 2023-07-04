#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

#if NET6_0_OR_GREATER
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
#endif

namespace JJMasterData.Core.Web;

public class JJMasterDataUrlHelper
{
#if NET6_0_OR_GREATER
    private IUrlHelper UrlHelper { get; }
#endif
    private string? OptionsUrl { get; }
#if NET48 || NETSTANDARD
    public JJMasterDataUrlHelper(IOptions<JJMasterDataCoreOptions> options)
    {
        OptionsUrl = options.Value.JJMasterDataUrl;
    }
#else
    public JJMasterDataUrlHelper(
        IUrlHelperFactory urlHelperFactory,
        IActionContextAccessor actionContextAccessor, 
        IOptions<JJMasterDataCoreOptions> options)
    {
        UrlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext!);
        OptionsUrl = options.Value.JJMasterDataUrl;
    }
#endif

    [Obsolete("Development time workaround, use constructor injection.")]
    public static JJMasterDataUrlHelper GetInstance()
    {
        var scope = JJService.Provider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<JJMasterDataUrlHelper>();
    }

    public string GetUrl([AspMvcAction]string? action = null, [AspMvcController]string? controller = null,object? values = null)
    {
        if (OptionsUrl is not null)
        {
            var url = Path.Combine(OptionsUrl, "MasterData");
            if (controller != null)
            {
                url += Path.Combine(url, controller);
            }

            if (action != null)
            {
                url += Path.Combine(url, action);
            }

            if (values != null)
            {
                var dictionary = (IDictionary<string, dynamic>)values;
                var queryString = string.Join("&", dictionary.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                url += "?" + queryString;
            }

            return url;
        }
#if NET48 || NETSTANDARD
        throw new JJMasterDataException("JJMasterDataCoreOptions.JJMasterDataUrl cannot be null at your target framework.");
#elif NET6_0_OR_GREATER
        values ??= new  {Area = "MasterData"};
        return UrlHelper.Action(action, controller, values) ?? throw new JJMasterDataException("Invalid action and/or controller.");
#endif
    }
}