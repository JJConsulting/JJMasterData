using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.GridView;
using JJMasterData.Core.Web.Components;
using System.Collections.Generic;
using System.Linq;

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
