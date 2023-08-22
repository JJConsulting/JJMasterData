using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.DataManager.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class ExpressionsServiceExtensions
{
    public static IServiceCollection AddExpressionServices(this IServiceCollection services)
    {
        services.AddTransient<IExpressionsService,ExpressionsService>();
        services.AddTransient<IExpressionParser, ExpressionParser>();
        
        services.AddScoped<IExpressionProvider, ValueExpressionProvider>();
        services.AddScoped<IExpressionProvider, InMemoryExpressionProvider>();
        services.AddScoped<IExpressionProvider, SqlExpressionProvider>();

        return services;
    }
}