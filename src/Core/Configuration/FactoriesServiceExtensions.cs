using JJMasterData.Core.DataDictionary.Factories;
using JJMasterData.Core.DataDictionary.Structure;
using JJMasterData.Core.DataManager.Exports;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.Controls;
using JJMasterData.Core.UI.Components.Controls.TextBox;
using JJMasterData.Core.UI.Components.GridView;
using JJMasterData.Core.UI.Components.Importation;
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
        return services.AddFormElementFactories()
            .AddComponentsFactories()
            .AddDataFactories();
    }

    private static IServiceCollection AddDataFactories(this IServiceCollection services)
    {
        services.AddTransient<DataExportationWriterFactory>();
        services.AddTransient<DataImportationWorkerFactory>();

        return services;
    }

    private static IServiceCollection AddFormElementFactories(this IServiceCollection services)
    {
        services.AddTransient<DataDictionaryFormElementFactory>();
        services.AddTransient<LocalizationFormElementFactory>();
        services.AddTransient<LoggerFormElementFactory>();
        return services;
    }

    private static IServiceCollection AddComponentsFactories(this IServiceCollection services)
    {
        services.AddTransient<LinkButtonFactory>();

        services.AddControlsFactories();

        services.AddFormElementComponentsFactories();

        services.AddTransient<IComponentFactory<JJUploadView>, UploadViewFactory>();
        services.AddTransient<IComponentFactory<JJFileDownloader>, FileDownloaderFactory>();
        services.AddTransient<IComponentFactory<JJUploadArea>, UploadAreaFactory>();
        services.AddTransient<IComponentFactory<JJTextBox>, TextBoxFactory>();
        services.AddScoped<RouteContextFactory>();
        services.AddTransient<ValidationSummaryFactory>();
        services.AddTransient<HtmlComponentFactory>();
        services.AddTransient<IComponentFactory, ComponentFactory>();
        
        return services;
    }

    private static void AddFormElementComponentsFactories(this IServiceCollection services)
    {
        services.AddTransient<IFormElementComponentFactory<JJAuditLogView>, AuditLogViewFactory>();
        services.AddTransient<IFormElementComponentFactory<JJDataPanel>, DataPanelFactory>();
        services.AddTransient<IFormElementComponentFactory<JJFormView>, FormViewFactory>();
        services.AddTransient<IFormElementComponentFactory<JJGridView>, GridViewFactory>();
        services.AddTransient<IFormElementComponentFactory<JJDataExportation>, DataExportationFactory>();
        services.AddTransient<IFormElementComponentFactory<JJDataImportation>, DataImportationFactory>();
    }

    private static void AddControlsFactories(this IServiceCollection services)
    {
        services.AddTransient<IControlFactory<JJComboBox>, ComboBoxFactory>();
        services.AddTransient<IControlFactory<JJLookup>, LookupFactory>();
        services.AddTransient<IControlFactory<JJSearchBox>, SearchBoxFactory>();
        services.AddTransient<IControlFactory<JJTextArea>, TextAreaFactory>();
        services.AddTransient<IControlFactory<JJSlider>, SliderFactory>();
        services.AddTransient<IControlFactory<JJTextGroup>, TextGroupFactory>();
        services.AddTransient<IControlFactory<JJTextRange>, TextRangeFactory>();
        services.AddTransient<IControlFactory<JJTextFile>, TextFileFactory>();
        services.AddTransient<IControlFactory<JJCheckBox>, CheckBoxFactory>();
        services.AddTransient<IControlFactory, ControlFactory>();
    }
}