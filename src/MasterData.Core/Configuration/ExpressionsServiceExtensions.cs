using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.DataManager.Expressions.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NCalc.Cache.Configuration;
using NCalc.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class ExpressionsServiceExtensions
{
    public static IServiceCollection AddExpressionServices(this IServiceCollection services)
    {
        services.AddNCalc().WithMemoryCache();

        services.TryAddScoped<ExpressionsService>();
        services.TryAddScoped<ExpressionParser>();

        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IExpressionProvider, ValueExpressionProvider>());

        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IExpressionProvider, DefaultExpressionProvider>());

        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IExpressionProvider, SqlExpressionProvider>());

        return services;
    }
}