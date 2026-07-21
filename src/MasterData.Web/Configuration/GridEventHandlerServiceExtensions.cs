using JJMasterData.Web.Events.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Web.Configuration;

public static class GridEventHandlerServiceExtensions
{
    public static IServiceCollection AddGridEventHandler<TEventHandler>(this IServiceCollection services,
        string elementName, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TEventHandler : class, IGridEventHandler
    {
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                services.AddKeyedSingleton<IGridEventHandler, TEventHandler>(elementName);
                break;
            case ServiceLifetime.Scoped:
                services.AddKeyedScoped<IGridEventHandler, TEventHandler>(elementName);
                break;
            case ServiceLifetime.Transient:
                services.AddKeyedTransient<IGridEventHandler, TEventHandler>(elementName);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }

        return services;
    }
}
