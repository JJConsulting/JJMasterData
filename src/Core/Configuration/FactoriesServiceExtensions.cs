using JJMasterData.Core.DataDictionary.Structure;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Importation;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.ColorPicker;
using JJMasterData.Core.UI.Components.TextRange;
using JJMasterData.Core.UI.Routing;
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
        services.AddControlsFactories();
        services.AddFormElementComponentsFactories();

        services.AddScoped<IComponentFactory<JJUploadView>, UploadViewFactory>();
        services.AddScoped<IComponentFactory<JJFileDownloader>, FileDownloaderFactory>();
        services.AddScoped<IComponentFactory<JJUploadArea>, UploadAreaFactory>();
        services.AddScoped<IComponentFactory<JJCollapsePanel>, CollapsePanelFactory>();
        services.AddScoped<IComponentFactory<JJLinkButton>,LinkButtonFactory>();
        services.AddScoped<IComponentFactory<JJLinkButtonGroup>, LinkButtonGroupFactory>();
        services.AddScoped<IComponentFactory<JJCard>, CardFactory>();
        services.AddScoped<RouteContextFactory>();
        services.AddScoped<ValidationSummaryFactory>();
        services.AddScoped<HtmlComponentFactory>();
        services.AddScoped<TextGroupFactory>();
        services.AddScoped<IComponentFactory, ComponentFactory>();
        services.AddScoped<LinkButtonFactory>();
        services.AddScoped<ActionButtonFactory>();
        services.AddScoped<TextGroupFactory>();
        
        return services;
    }

    private static void AddFormElementComponentsFactories(this IServiceCollection services)
    {
        services.AddScoped<IFormElementComponentFactory<JJAuditLogView>, AuditLogViewFactory>();
        services.AddScoped<IFormElementComponentFactory<JJDataPanel>, DataPanelFactory>();
        services.AddScoped<IFormElementComponentFactory<JJFormView>, FormViewFactory>();
        services.AddScoped<IFormElementComponentFactory<JJGridView>, GridViewFactory>();
        services.AddScoped<IFormElementComponentFactory<JJDataExportation>, DataExportationFactory>();
        services.AddScoped<IFormElementComponentFactory<JJDataImportation>, DataImportationFactory>();
    }

    private static void AddControlsFactories(this IServiceCollection services)
    {
        services.AddScoped<IControlFactory<JJComboBox>, ComboBoxFactory>();
        services.AddScoped<IControlFactory<JJLookup>, LookupFactory>();
        services.AddScoped<IControlFactory<JJSearchBox>, SearchBoxFactory>();
        services.AddScoped<IControlFactory<JJTextArea>, TextAreaFactory>();
        services.AddScoped<IControlFactory<JJSlider>, SliderFactory>();
        services.AddScoped<IControlFactory<JJTextGroup>, TextGroupFactory>();
        services.AddScoped<IControlFactory<JJTextRange>, TextRangeFactory>();
        services.AddScoped<IControlFactory<JJTextFile>, TextFileFactory>();
        services.AddScoped<IControlFactory<JJTextBox>, TextBoxFactory>();
        services.AddScoped<IControlFactory<JJCheckBox>, CheckBoxFactory>();
        services.AddScoped<IControlFactory<JJColorPicker>, ColorPickerFactory>();
        services.AddScoped<IControlFactory<JJRadioButtonGroup>, RadioButtonGroupFactory>();
        services.AddScoped<ControlFactory>();
    }
}