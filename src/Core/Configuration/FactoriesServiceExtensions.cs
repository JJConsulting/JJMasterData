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
        services.AddTransient(typeof(Lazy<>),typeof(LazyService<>));
        services.AddTransient(typeof(IControlFactory<>),typeof(ControlFactory<>));
        services.AddTransient(typeof(IComponentFactory<>), typeof(ComponentFactory<>));
        services.AddTransient(typeof(IFormElementComponentFactory<>),typeof(FormElementComponentFactory<>));

        services.AddTransient<IControlFactory,ComboBoxFactory>();
        services.AddTransient<IControlFactory,LookupFactory>();
        services.AddTransient<IControlFactory,SearchBoxFactory>();
        services.AddTransient<IControlFactory,TextAreaFactory>();
        services.AddTransient<IControlFactory,SliderFactory>();
        services.AddTransient<IControlFactory,TextBoxFactory>();
        services.AddTransient<IControlFactory,TextRangeFactory>();
        services.AddTransient<IControlFactory,TextFileFactory>();
        services.AddTransient<ControlFactory>();

        services.AddTransient<IFormElementComponentFactory, AuditLogViewFactory>();
        services.AddTransient<IFormElementComponentFactory, DataPanelFactory>();
        services.AddTransient<IFormElementComponentFactory, FormViewFactory>();
        services.AddTransient<IFormElementComponentFactory, GridViewFactory>();
        services.AddTransient<IFormElementComponentFactory, DataExportationFactory>();
        services.AddTransient<IFormElementComponentFactory, DataImportationFactory>();
        
        services.AddTransient<IComponentFactory, FormUploadFactory>();
        services.AddTransient<IComponentFactory, FileDownloaderFactory>();
        services.AddTransient<IComponentFactory, UploadAreaFactory>();

        services.AddTransient<ComponentFactory>();

        return services;
    }
}