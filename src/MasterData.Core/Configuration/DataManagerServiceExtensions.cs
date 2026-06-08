using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.IO;
using JJMasterData.Core.DataManager.IO.Storage;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.DataManager.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JJMasterData.Core.Configuration;

public static class DataManagerServiceExtensions
{
    public static IServiceCollection AddDataManagerServices(this IServiceCollection services)
    {
        services.AddScoped<IMasterDataUser, MasterDataUser>();
        services.AddScoped<AuditLogService>();
        services.AddScoped<DataItemService>();
        services.AddScoped<LookupService>();
        services.AddScoped<FieldFormattingService>();
        services.AddScoped<FieldValidationService>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IRuleExecutor, SqlRuleExecutor>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IRuleExecutor, JavaScriptRuleScriptExecutor>());
        services.AddScoped<FormService>();
        services.AddScoped<FieldValuesService>();
        services.AddScoped<UploadAreaService>();
        services.AddScoped<FormValuesService>();
        services.AddScoped<IFileStorage, DiskFileStorage>();
        services.AddScoped<ITemporaryUploadStore, TemporaryDiskUploadStore>();
        services.AddScoped<FormFileManagerFactory>();
        services.AddScoped<FormFileService>();
        services.AddScoped<ElementFileService>();
        services.AddScoped<ElementMapService>();
        services.AddScoped<UrlRedirectService>();

        return services;
    }
}
