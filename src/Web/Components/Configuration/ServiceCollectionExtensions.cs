using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Web.Components.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddComponentServices(this IServiceCollection services)
    {
        services.AddScoped<HtmlRenderer>();
        services.AddTransient<ComponentRenderer>();
    }
}