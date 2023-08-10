using JJMasterData.Core.DataDictionary.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.Web.FormEvents.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class EventHandlerServiceExtensions
{
    public static IServiceCollection AddEventHandlers(this IServiceCollection services)
    {
        services.AddTransient<IFormEventHandlerFactory,FormEventHandlerFactory>();
        services.AddTransient<IGridEventHandlerFactory,GridEventHandlerFactory>();

        return services;
    }
}