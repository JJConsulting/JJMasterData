using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.DataManager.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class DataManagerServiceExtensions
{
    public static IServiceCollection AddDataManagerServices(this IServiceCollection services)
    {
        services.AddTransient<IAuditLogService, AuditLogService>();
        services.AddTransient<IDataItemService, DataItemService>();
        services.AddTransient<ILookupService, LookupService>();
        services.AddTransient<IFieldFormattingService, FieldFormattingService>();
        services.AddTransient<IFieldValidationService, FieldValidationService>();
        services.AddTransient<IFormService, FormService>();
        services.AddTransient<IUrlRedirectService, UrlRedirectService>();
        services.AddTransient<IFieldValuesService, FieldValuesService>();
        services.AddTransient<IFieldsService, FieldsService>();
        services.AddTransient<IUploadAreaService, UploadAreaService>();
        services.AddScoped<IFormValuesService,FormValuesService>();
        
        services.AddTransient<FormFileManagerFactory>();
        services.AddTransient<IFormFileService,FormFileService>();
        
        return services;
    }
}