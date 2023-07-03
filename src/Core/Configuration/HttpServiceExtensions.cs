using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Http;
using JJMasterData.Core.Web.Http.Abstractions;
#if NET
using Microsoft.AspNetCore.Mvc.Infrastructure;
#endif
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class HttpServiceExtensions
{
    public static void AddHttpServices(this IServiceCollection services)
    {
#if NET
        services.AddHttpContextAccessor();
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
#endif
        services.AddScoped<JJMasterDataUrlHelper>();

        services.AddScoped<IHttpSession, JJSession>();
        services.AddScoped<IHttpRequest, JJRequest>();
        services.AddScoped<IHttpResponse, JJResponse>();
        services.AddScoped<IHttpContext, JJHttpContext>();
    }
}