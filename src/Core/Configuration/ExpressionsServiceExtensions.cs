using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.DataManager.Expressions.Providers;
using Microsoft.Extensions.DependencyInjection;
using NCalc.Cache.Configuration;
using NCalc.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class ExpressionsServiceExtensions
{
    public static IServiceCollection AddExpressionServices(this IServiceCollection services)
    {
        services.AddNCalc().WithMemoryCache();

        services.AddTransient<ExpressionsService>();
        services.AddTransient<ExpressionParser>();

        services.AddScoped<IExpressionProvider, ValueExpressionProvider>();
        services.AddScoped<IExpressionProvider, DefaultExpressionProvider>();
        services.AddScoped<IExpressionProvider, SqlExpressionProvider>();

        return services;
    }
}