using System;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.GridView;
using JJMasterData.Core.UI.Components.Widgets;
using JJMasterData.Core.Web.Components;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Web.Factories;

public class ComponentFactory
{
    private IServiceProvider ServiceProvider { get; }

    public IFormElementComponentFactory<JJAuditLogView> AuditLog =>
        GetFactory<IFormElementComponentFactory<JJAuditLogView>>();

    public IFormElementComponentFactory<JJDataExportation> DataExportation =>
        GetFactory<IFormElementComponentFactory<JJDataExportation>>();

    public IFormElementComponentFactory<JJDataImportation> DataImportation =>
        GetFactory<IFormElementComponentFactory<JJDataImportation>>();

    public IFormElementComponentFactory<JJDataPanel> DataPanel => GetFactory<IFormElementComponentFactory<JJDataPanel>>();
    public IFormElementComponentFactory<JJFormView> JJView => GetFactory< IFormElementComponentFactory<JJFormView> >();
    public IFormElementComponentFactory<JJGridView> GridView => GetFactory<IFormElementComponentFactory<JJGridView>>();
    public IComponentFactory<JJUploadView> FormUpload => GetFactory<IComponentFactory<JJUploadView> >();
    public IComponentFactory<JJFileDownloader> Downloader => GetFactory<IComponentFactory<JJFileDownloader>>();
    public IComponentFactory<JJUploadArea> UploadArea => GetFactory<IComponentFactory<JJUploadArea>>();
    public LinkButtonFactory LinkButtonFactory => GetFactory<LinkButtonFactory>();
    public ControlFactory Controls => GetFactory<ControlFactory>();

    public ComponentFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
    
    private T GetFactory<T>()
    {
        return ServiceProvider.GetRequiredService<T>();
    }
}