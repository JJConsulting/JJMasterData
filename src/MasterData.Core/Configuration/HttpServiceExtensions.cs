using JJMasterData.Core.Http;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class HttpServiceExtensions
{
    public static void AddHttpServices(this IServiceCollection services)
    {
        services.AddScoped<IHttpContext, HttpContextWrapper>();
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
    }
}
