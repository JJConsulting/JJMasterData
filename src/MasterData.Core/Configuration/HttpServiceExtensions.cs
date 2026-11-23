using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JJMasterData.Core.Configuration;

public static class HttpServiceExtensions
{
    public static void AddHttpServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        //todo: Deve ser removido pois esta obsoleto, o correto é usar LinkGenerator nas services.
#pragma warning disable ASPDEPR006
        services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
        
        services.TryAddScoped(serviceProvider =>
        {
            var actionContextAccessor = serviceProvider.GetRequiredService<IActionContextAccessor>();
#pragma warning restore ASPDEPR006
            var urlHelperFactory = serviceProvider.GetRequiredService<IUrlHelperFactory>();
            return urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext ?? new());
        });
    }
}
