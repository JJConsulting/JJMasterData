using JJMasterData.Core.DataManager.IO;
using JJMasterData.Core.DataManager.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class DataManagerServiceExtensions
{
    public static IServiceCollection AddDataManagerServices(this IServiceCollection services)
    {
        services.AddScoped<AuditLogService>();
        services.AddScoped<DataItemService>();
        services.AddScoped<LookupService>();
        services.AddScoped<FieldFormattingService>();
        services.AddScoped<FieldValidationService>();
        services.AddScoped<FormService>();
        services.AddScoped<FieldValuesService>();
        services.AddScoped<FieldsService>();
        services.AddScoped<UploadAreaService>();
        services.AddScoped<FormValuesService>();
        services.AddScoped<FormFileManagerFactory>();
        services.AddScoped<FormFileService>();
        services.AddScoped<ElementMapService>();
        services.AddScoped<UrlRedirectService>();
        
        return services;
    }
}