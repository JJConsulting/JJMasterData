using JJMasterData.Core.Http;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class HttpServiceExtensions
{
    public static void AddHttpServices(this IServiceCollection services)
    {
        services.AddScoped<IHttpContext, HttpContextWrapper>();
#if NET
        services.AddHttpContextAccessor();
        services.AddScoped<IHttpSession, Http.AspNetCore.HttpSessionWrapper>();
        services.AddScoped<IHttpRequest, Http.AspNetCore.HttpRequestWrapper>();
        services.AddScoped<IQueryString, Http.AspNetCore.QueryStringWrapper>();  
        services.AddScoped<IFormValues, Http.AspNetCore.FormValuesWrapper>();  
        services.AddScoped<IClaimsPrincipalAccessor, Http.AspNetCore.ClaimsPrincipalWrapper>();  
#endif
        services.AddScoped<JJMasterDataUrlHelper>();

#if NETFRAMEWORK
        services.AddScoped<IHttpSession, Http.SystemWeb.SystemWebHttpSessionWrapper>();
        services.AddScoped<IHttpRequest, Http.SystemWeb.SystemWebHttpRequestWrapper>();
        services.AddScoped<IQueryString, Http.SystemWeb.SystemWebQueryStringWrapper>();
        services.AddScoped<IFormValues, Http.SystemWeb.SystemWebFormValuesWrapper>();
        services.AddScoped<IClaimsPrincipalAccessor, Http.SystemWeb.SystemWebClaimsPrincipalWrapper>();  
#endif


    }
}