using System;
using JJMasterData.Commons.Configuration;
using JJMasterData.Core.Web.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class FactoriesServiceExtensions
{
    public static IServiceCollection AddFactories(this IServiceCollection services)
    {
        services.AddTransient<DataExportationFactory>();
        services.AddTransient<ComboBoxFactory>();
        services.AddTransient<DataImportationFactory>();
        services.AddTransient<AuditLogViewFactory>();
        services.AddTransient<CheckBoxFactory>();
        services.AddTransient<FieldControlFactory>();
        services.AddTransient<DataPanelFactory>();
        services.AddTransient<FormUploadFactory>();
        services.AddTransient<FileDownloaderFactory>();
        services.AddTransient<FormViewFactory>();
        
        services.AddTransient<GridViewFactory>().AllowLazyInicialization();
        
        services.AddTransient<JJMasterDataFactory>();
        services.AddTransient<LookupFactory>();
        services.AddTransient<SearchBoxFactory>();
        services.AddTransient<TextAreaFactory>();
        services.AddTransient<SliderFactory>();
        services.AddTransient<TextGroupFactory>();
        services.AddTransient<TextRangeFactory>();
        services.AddTransient<UploadAreaFactory>();
        services.AddTransient<TextFileFactory>();

        return services;
    }

}