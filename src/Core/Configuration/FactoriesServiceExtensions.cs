using System;
using System.Linq;
using JJMasterData.Commons.Configuration;
using JJMasterData.Core.DataDictionary.Factories;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Structure;
using JJMasterData.Core.DataManager.Exports;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.GridView;
using JJMasterData.Core.UI.Components.Widgets;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class FactoriesServiceExtensions
{
    public static IServiceCollection AddFactories(this IServiceCollection services)
    {
        services.AddFormElementFactories();
        services.AddComponentsFactories();

        services.AddTransient<ExportationWriterFactory>();
        
        return services;
    }
    
    private static IServiceCollection AddFormElementFactories(this IServiceCollection services)
    {
        services.AddTransient<IFormElementFactory,DataDictionaryFormElementFactory>();
        services.AddTransient<IFormElementFactory,LocalizationFormElementFactory>();
        services.AddTransient<IFormElementFactory,LoggerFormElementFactory>();
        
        return services;
    }
    
    private static IServiceCollection AddComponentsFactories(this IServiceCollection services)
    {
        services.AddTransient<TextBoxFactory>();
        services.AddTransient<LinkButtonFactory>();
        
        services.AddTransient<IControlFactory<JJComboBox>,ComboBoxFactory>();
        services.AddTransient<IControlFactory<JJLookup>,LookupFactory>();
        services.AddTransient<IControlFactory<JJSearchBox>,SearchBoxFactory>();
        services.AddTransient<IControlFactory<JJTextArea>,TextAreaFactory>();
        services.AddTransient<IControlFactory<JJSlider>,SliderFactory>();
        services.AddTransient<IControlFactory<JJTextGroup>,TextBoxFactory>();
        services.AddTransient<IControlFactory<JJTextRange>,TextRangeFactory>();
        services.AddTransient<IControlFactory<JJTextFile>,TextFileFactory>();
        services.AddTransient<ControlFactory>();

        services.AddTransient<IFormElementComponentFactory<JJAuditLogView>, AuditLogViewFactory>();
        services.AddTransient<IFormElementComponentFactory<JJDataPanel>, DataPanelFactory>();
        services.AddTransient<IFormElementComponentFactory<JJFormView>, FormViewFactory>();
        services.AddTransient<IFormElementComponentFactory<JJGridView>, GridViewFactory>();
        services.AddTransient<IFormElementComponentFactory<JJDataExportation>, DataExportationFactory>();
        services.AddTransient<IFormElementComponentFactory<JJDataImportation>, DataImportationFactory>();

        services.AddTransient<IComponentFactory<JJUploadView>, UploadViewFactory>();
        services.AddTransient<IComponentFactory<JJFileDownloader>, FileDownloaderFactory>();
        services.AddTransient<IComponentFactory<JJUploadArea>, UploadAreaFactory>();
        services.AddTransient<ComponentFactory>();

        return services;
    }
}