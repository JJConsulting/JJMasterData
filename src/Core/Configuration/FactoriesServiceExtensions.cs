using System;
using System.Linq;
using JJMasterData.Commons.Configuration;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.GridView;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class FactoriesServiceExtensions
{
    public static IServiceCollection AddFactories(this IServiceCollection services)
    {
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
        services.AddTransient<IFormElementComponentFactory<JJDataExp>, DataExportationFactory>();
        services.AddTransient<IFormElementComponentFactory<JJDataImp>, DataImportationFactory>();

        services.AddTransient<IComponentFactory<JJFormUpload>, FormUploadFactory>();
        services.AddTransient<IComponentFactory<JJFileDownloader>, FileDownloaderFactory>();
        services.AddTransient<IComponentFactory<JJUploadArea>, UploadAreaFactory>();
        services.AddTransient<ComponentFactory>();

        return services;
    }
}