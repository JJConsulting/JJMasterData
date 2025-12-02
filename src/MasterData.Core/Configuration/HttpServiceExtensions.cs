using JJMasterData.Core.Http;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.DependencyInjection;
#if NET
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
#endif

namespace JJMasterData.Core.Configuration;

public static class HttpServiceExtensions
{
    public static void AddHttpServices(this IServiceCollection services)
    {
        services.AddScoped<IHttpContext, HttpContextWrapper>();
#if NET
        services.AddHttpContextAccessor();
        services.AddScoped<IRequestLengthService, Http.AspNetCore.RequestLengthService>();  

        services.AddScoped<IHttpSession, Http.AspNetCore.HttpSessionWrapper>();
        services.AddScoped<IHttpRequest, Http.AspNetCore.HttpRequestWrapper>();
        services.AddScoped<IQueryString, Http.AspNetCore.QueryStringWrapper>();  
        services.AddScoped<IFormValues, Http.AspNetCore.FormValuesWrapper>();  
        services.AddScoped<IClaimsPrincipalAccessor, Http.AspNetCore.ClaimsPrincipalWrapper>();

#pragma warning disable ASPDEPR006
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        
        services.AddScoped(serviceProvider =>
        {
            var actionContextAccessor = serviceProvider.GetRequiredService<IActionContextAccessor>();
#pragma warning restore ASPDEPR006
            var urlHelperFactory = serviceProvider.GetRequiredService<IUrlHelperFactory>();
            return urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext!);
        });
#endif
   

#if NETFRAMEWORK
        services.AddScoped<IRequestLengthService, Http.SystemWeb.SystemWebRequestLengthService>();
        services.AddScoped<IHttpSession, Http.SystemWeb.SystemWebHttpSessionWrapper>();
        services.AddScoped<IHttpRequest, Http.SystemWeb.SystemWebHttpRequestWrapper>();
        services.AddScoped<IQueryString, Http.SystemWeb.SystemWebQueryStringWrapper>();
        services.AddScoped<IFormValues, Http.SystemWeb.SystemWebFormValuesWrapper>();
        services.AddScoped<IClaimsPrincipalAccessor, Http.SystemWeb.SystemWebClaimsPrincipalWrapper>();  
        services.AddScoped<IUrlHelper, Http.SystemWeb.SystemWebUrlHelperWrapper>();  
#endif


    }
}