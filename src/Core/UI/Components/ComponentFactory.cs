#nullable enable

using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.Widgets;
using JJMasterData.Core.Web.Components;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace JJMasterData.Core.Web.Factories;

internal class ComponentFactory : IComponentFactory
{
    private RouteContext? _routeContext;
    private HtmlComponentFactory? _htmlComponentFactory;
    private IServiceProvider ServiceProvider { get; }

    public IFormElementComponentFactory<JJAuditLogView> AuditLog =>
        GetFactory<IFormElementComponentFactory<JJAuditLogView>>();

    public IFormElementComponentFactory<JJDataExportation> DataExportation =>
        GetFactory<IFormElementComponentFactory<JJDataExportation>>();

    public IFormElementComponentFactory<JJDataImportation> DataImportation =>
        GetFactory<IFormElementComponentFactory<JJDataImportation>>();

    public IFormElementComponentFactory<JJDataPanel> DataPanel =>
        GetFactory<IFormElementComponentFactory<JJDataPanel>>();

    public IFormElementComponentFactory<JJFormView> FormView =>
        GetFactory<IFormElementComponentFactory<JJFormView>>();

    public IFormElementComponentFactory<JJGridView> GridView =>
        GetFactory<IFormElementComponentFactory<JJGridView>>();

    public IComponentFactory<JJUploadView> FormUpload =>
        GetFactory<IComponentFactory<JJUploadView>>();

    public IComponentFactory<JJFileDownloader> Downloader =>
        GetFactory<IComponentFactory<JJFileDownloader>>();

    public IComponentFactory<JJUploadArea> UploadArea =>
        GetFactory<IComponentFactory<JJUploadArea>>();

    public LinkButtonFactory LinkButton =>
        GetFactory<LinkButtonFactory>();

    public IControlFactory Controls =>
        GetFactory<IControlFactory>();

    public HtmlComponentFactory Html  =>
        _htmlComponentFactory ??= ServiceProvider.GetRequiredService<HtmlComponentFactory>();

    public RouteContext RouteContext =>
        _routeContext ??= ServiceProvider.GetRequiredService<RouteContextFactory>().Create();

    public ComponentFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    private T GetFactory<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }
}