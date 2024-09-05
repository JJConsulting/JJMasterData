using JJMasterData.Core.Configuration;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.WebApi.Models;
using JJMasterData.WebApi.Services;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace JJMasterData.WebApi.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJJMasterDataWebApi(this IServiceCollection services)
    {
        services.AddScoped<MasterApiService>();
        services.AddScoped<FileService>();
        services.AddScoped<DictionariesService>();
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        services.AddScoped<IValidationDictionary, ModelStateWrapper>();

        services.AddJJMasterDataCore();
        
        return services;
    }
}