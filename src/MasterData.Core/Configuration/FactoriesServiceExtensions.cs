using JJMasterData.Core.DataDictionary.Structure;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.ColorPicker;
using JJMasterData.Core.UI.Components.Factories;
using JJMasterData.Core.UI.Components.Phone;
using JJMasterData.Core.UI.Components.TextRange;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JJMasterData.Core.Configuration;

public static class FactoriesServiceExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFactories()
        {
            return services.AddFormElementFactories()
                .AddComponentsFactories();
        }

        private IServiceCollection AddFormElementFactories()
        {
            services.TryAddScoped<DataDictionaryFormElementFactory>();
            return services;
        }

        private IServiceCollection AddComponentsFactories()
        {
            services.AddControlsFactories();
            services.AddFormElementComponentsFactories();

            services.TryAddScoped<UploadViewFactory>();
            services.TryAddScoped<FileDownloaderFactory>();
            services.TryAddScoped<UploadAreaFactory>();
            services.TryAddScoped<TitleFactory>();
            services.TryAddScoped<LabelFactory>();
            services.TryAddScoped<MessageBoxFactory>();
            services.TryAddScoped<RouteContextFactory>();
            services.TryAddScoped<TextGroupFactory>();
            services.TryAddScoped<IComponentFactory, ComponentFactory>();
            services.TryAddScoped<ActionButtonFactory>();
        
            return services;
        }

        private void AddFormElementComponentsFactories()
        {
            services.TryAddScoped<IFormElementComponentFactory<JJAuditLogView>, AuditLogViewFactory>();
            services.TryAddScoped<IFormElementComponentFactory<JJDataPanel>, DataPanelFactory>();
            services.TryAddScoped<IFormElementComponentFactory<JJFormView>, FormViewFactory>();
            services.TryAddScoped<IFormElementComponentFactory<JJGridView>, GridViewFactory>();
            services.TryAddScoped<IFormElementComponentFactory<JJDataExportation>, DataExportationFactory>();
            services.TryAddScoped<IFormElementComponentFactory<JJDataImportation>, DataImportationFactory>();
        }

        private void AddControlsFactories()
        {
            services.TryAddScoped<IControlFactory<JJComboBox>, ComboBoxFactory>();
            services.TryAddScoped<IControlFactory<JJLookup>, LookupFactory>();
            services.TryAddScoped<IControlFactory<JJSearchBox>, SearchBoxFactory>();
            services.TryAddScoped<IControlFactory<JJTextArea>, TextAreaFactory>();
            services.TryAddScoped<IControlFactory<JJSlider>, SliderFactory>();
            services.TryAddScoped<IControlFactory<JJPhoneGroup>, PhoneGroupFactory>();
            services.TryAddScoped<IControlFactory<JJTextGroup>, TextGroupFactory>();
            services.TryAddScoped<IControlFactory<JJTextRange>, TextRangeFactory>();
            services.TryAddScoped<IControlFactory<JJTextFile>, TextFileFactory>();
            services.TryAddScoped<IControlFactory<JJTextBox>, TextBoxFactory>();
            services.TryAddScoped<IControlFactory<JJCheckBox>, CheckboxFactory>();
            services.TryAddScoped<IControlFactory<JJColorPicker>, ColorPickerFactory>();
            services.TryAddScoped<IControlFactory<JJRadioButtonGroup>, RadioButtonGroupFactory>();
            services.TryAddScoped<IControlFactory<JJIconPicker>, IconPickerFactory>();
            services.TryAddScoped<IControlFactory<JJCodeEditor>, CodeEditorFactory>();
            services.TryAddScoped<IControlFactory<JJHierarchy>, HierarchyFactory>();
            services.TryAddScoped<ControlFactory>();
        }
    }
}
