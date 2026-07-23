using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class HttpServiceExtensions
{
    public static void AddHttpServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

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
