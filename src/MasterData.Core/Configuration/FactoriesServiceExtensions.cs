using JJConsulting.Html.Bootstrap.Components;
using JJMasterData.Core.DataDictionary.Structure;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Importation;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.ColorPicker;
using JJMasterData.Core.UI.Components.Factories;
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
        return services;
    }

    private static IServiceCollection AddComponentsFactories(this IServiceCollection services)
    {
        services.AddControlsFactories();
        services.AddFormElementComponentsFactories();

        services.AddScoped<UploadViewFactory>();
        services.AddScoped<FileDownloaderFactory>();
        services.AddScoped<UploadAreaFactory>();
        services.AddScoped<TitleFactory>();
        services.AddScoped<LabelFactory>();
        services.AddScoped<MessageBoxFactory>();
        services.AddScoped<RouteContextFactory>();
        services.AddScoped<TextGroupFactory>();
        services.AddScoped<IComponentFactory, ComponentFactory>();
        services.AddScoped<ActionButtonFactory>();
        
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
        services.AddScoped<IControlFactory<JJCheckBox>, CheckboxFactory>();
        services.AddScoped<IControlFactory<JJColorPicker>, ColorPickerFactory>();
        services.AddScoped<IControlFactory<JJRadioButtonGroup>, RadioButtonGroupFactory>();
        services.AddScoped<IControlFactory<JJIconPicker>, IconPickerFactory>();
        services.AddScoped<IControlFactory<JJCodeEditor>, CodeEditorFactory>();
        services.AddScoped<ControlFactory>();
    }
}