using JJMasterData.Core.Http;

using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Http.Abstractions;
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
#endif
        services.AddScoped<JJMasterDataUrlHelper>();

#if NETFRAMEWORK
        services.AddScoped<IHttpSession, Http.SystemWeb.SystemWebHttpSessionWrapper>();
        services.AddScoped<IHttpRequest, Http.SystemWeb.SystemWebHttpRequestWrapper>();
#endif


    }
}