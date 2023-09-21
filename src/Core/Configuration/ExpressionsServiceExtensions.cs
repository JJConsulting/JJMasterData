using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.DataManager.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;


public static class ExpressionsServiceExtensions
{
    public static IServiceCollection AddExpressionServices(this IServiceCollection services)
    {
        services.AddTransient<ExpressionsService,ExpressionsService>();
        services.AddTransient<IExpressionParser, ExpressionParser>();

        services.AddScoped<IExpressionProvider, ValueExpressionProvider>();
        services.AddScoped<IExpressionProvider, InMemoryExpressionProvider>();
        services.AddScoped<IExpressionProvider, SqlExpressionProvider>();

        return services;
    }
}
