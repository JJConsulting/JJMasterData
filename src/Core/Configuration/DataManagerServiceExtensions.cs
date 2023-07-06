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
        services.AddTransient<IExpressionsService, ExpressionsService>();
        services.AddTransient<IFieldFormattingService, FieldFormattingService>();
        services.AddTransient<IFieldVisibilityService, FieldVisibilityService>();
        services.AddTransient<IFieldValidationService, FieldValidationService>();
        services.AddTransient<IFormService, FormService>();
        services.AddTransient<IFieldValuesService, FieldValuesService>();
        services.AddTransient<IFieldsService, FieldsService>();
        services.AddScoped<IFormValuesService,FormValuesService>();
        
        return services;
    }
}