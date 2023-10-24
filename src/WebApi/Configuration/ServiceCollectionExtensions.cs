using JJMasterData.WebApi.Services;

namespace JJMasterData.WebApi.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJJMasterDataApi(this IServiceCollection services)
    {
        services.AddTransient<MasterApiService>();
        services.AddTransient<AccountService>();
        services.AddTransient<FileService>();
        services.AddTransient<DictionariesService>();

        return services;
    }
}