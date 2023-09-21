using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class DataManagerServiceExtensions
{
    public static IServiceCollection AddDataManagerServices(this IServiceCollection services)
    {
        services.AddTransient<AuditLogService>();
        services.AddTransient<DataItemService>();
        services.AddTransient<LookupService>();
        services.AddTransient<FieldFormattingService>();
        services.AddTransient<FieldValidationService>();
        services.AddTransient<FormService>();
        services.AddTransient<UrlRedirectService>();
        services.AddTransient<FieldValuesService>();
        services.AddTransient<FieldsService>();
        services.AddTransient<UploadAreaService>();
        services.AddScoped<FormValuesService>();

        
        services.AddTransient<FormFileManagerFactory>();
        services.AddTransient<FormFileService>();

        services.AddTransient<ElementMapService>();
        return services;
    }
}