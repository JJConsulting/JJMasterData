using JJMasterData.Core.UI.Components.GridView;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.Web.Factories;

public class ComponentFactory
{
    private IEnumerable<IComponentFactory> ComponentFactories { get; }
    private IEnumerable<IFormElementComponentFactory> FormElementComponentFactories { get; }
    private ControlFactory Control { get; }

    public ComponentFactory(
        IEnumerable<IComponentFactory> componentFactories,
        IEnumerable<IFormElementComponentFactory> formElementComponentFactories,
        ControlFactory controlFactory)
    {
        ComponentFactories = componentFactories;
        FormElementComponentFactories = formElementComponentFactories;
        Control = controlFactory;
    }

    private T GetFormElementComponentFactory<T>() where T : IComponentFactory
    {
        return (T)FormElementComponentFactories.First(f => f is T);
    }

    private T GetFactory<T>() where T : IComponentFactory
    {
        return (T)ComponentFactories.First(f => f is T);
    }

    public IFormElementComponentFactory<JJAuditLogView> AuditLog =>
        GetFormElementComponentFactory<AuditLogViewFactory>();

    public IFormElementComponentFactory<JJDataExp> DataExportation =>
        GetFormElementComponentFactory<DataExportationFactory>();

    public IFormElementComponentFactory<JJDataImp> DataImportation =>
        GetFormElementComponentFactory<DataImportationFactory>();

    public IFormElementComponentFactory<JJDataPanel> DataPanel => GetFormElementComponentFactory<DataPanelFactory>();
    public IFormElementComponentFactory<JJFormView> FormView => GetFormElementComponentFactory<FormViewFactory>();
    public IFormElementComponentFactory<JJGridView> GridView => GetFormElementComponentFactory<GridViewFactory>();
    public IComponentFactory<JJFormUpload> FormUpload => GetFactory<FormUploadFactory>();
}

public class ComponentFactory<T> : IComponentFactory<T> where T : JJBaseView
{
    private IServiceProvider ServiceProvider { get; }

    public ComponentFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
    
    public T Create()
    {
        var factories = ServiceProvider.GetRequiredService<IEnumerable<IComponentFactory>>();
        var factory = (IComponentFactory<T>)factories.First(f => f is IComponentFactory<T>);
        return factory.Create();
    }
}