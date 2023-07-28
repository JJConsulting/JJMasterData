using System;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.GridView;
using JJMasterData.Core.Web.Components;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Web.Factories;

public class ComponentFactory
{
    private IServiceProvider ServiceProvider { get; }

    public IFormElementComponentFactory<JJAuditLogView> AuditLog =>
        GetFactory<IFormElementComponentFactory<JJAuditLogView>>();

    public IFormElementComponentFactory<JJDataExp> DataExportation =>
        GetFactory<IFormElementComponentFactory<JJDataExp>>();

    public IFormElementComponentFactory<JJDataImp> DataImportation =>
        GetFactory<IFormElementComponentFactory<JJDataImp>>();

    public IFormElementComponentFactory<JJDataPanel> DataPanel => GetFactory<IFormElementComponentFactory<JJDataPanel>>();
    public IFormElementComponentFactory<JJFormView> FormView => GetFactory< IFormElementComponentFactory<JJFormView> >();
    public IFormElementComponentFactory<JJGridView> GridView => GetFactory<IFormElementComponentFactory<JJGridView>>();
    public IComponentFactory<JJFormUpload> FormUpload => GetFactory<IComponentFactory<JJFormUpload> >();
    public ControlFactory Controls => GetFactory<ControlFactory>();

    public ComponentFactory(
        IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
    
    private T GetFactory<T>()
    {
        return ServiceProvider.GetRequiredService<T>();
    }
}