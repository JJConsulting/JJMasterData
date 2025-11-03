using JJMasterData.Commons.Serialization;
using JJMasterData.Core.Configuration;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.WebApi.Models;
using JJMasterData.WebApi.Services;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace JJMasterData.WebApi.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJJMasterDataWebApi(this IServiceCollection services)
    {
        services.AddScoped<MasterApiService>();
        services.AddScoped<DictionariesService>();
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        services.AddScoped<IValidationDictionary, ModelStateWrapper>();

        services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
            options.SerializerOptions.Converters.Add(new DictionaryStringObjectJsonConverter());
        });
        
        services.AddJJMasterDataCore();
        
        return services;
    }
}