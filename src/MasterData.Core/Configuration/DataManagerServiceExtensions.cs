using JJMasterData.Commons.Storage;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.DataManager.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JJMasterData.Core.Configuration;

public static class DataManagerServiceExtensions
{
    public static IServiceCollection AddDataManagerServices(this IServiceCollection services)
    {
        services.TryAddScoped<IMasterDataUser, MasterDataUser>();
        
        services.TryAddTransient<AuditLogService>();
        services.TryAddTransient<DataItemService>();
        services.TryAddTransient<LookupService>();
        services.TryAddTransient<FieldFormattingService>();
        services.TryAddTransient<FieldValidationService>();
        
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IRuleExecutor, SqlRuleExecutor>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IRuleExecutor, JavaScriptRuleScriptExecutor>());
        
        services.TryAddTransient<FormService>();
        services.TryAddTransient<FieldValuesService>();
        services.TryAddTransient<UploadAreaService>();
        services.TryAddTransient<FormValuesService>();
        
        services.TryAddScoped<IFileStorage, DiskFileStorage>();
        
        services.TryAddTransient<FormFileService>();
        services.TryAddTransient<ElementFileService>();
        services.TryAddTransient<ElementMapService>();
        services.TryAddTransient<UrlRedirectService>();

        return services;
    }
}
