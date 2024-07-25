#nullable enable

using System;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.UI.Components;

internal class ComponentFactory(IServiceProvider serviceProvider) : IComponentFactory
{
    private RouteContext? _routeContext;
    private HtmlComponentFactory? _htmlComponentFactory;

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

    public IComponentFactory<JJUploadView> UploadView =>
        GetFactory<IComponentFactory<JJUploadView>>();

    public IComponentFactory<JJFileDownloader> Downloader =>
        GetFactory<IComponentFactory<JJFileDownloader>>();

    public IComponentFactory<JJUploadArea> UploadArea =>
        GetFactory<IComponentFactory<JJUploadArea>>();

    public ControlFactory Controls =>
        GetFactory<ControlFactory>();

    public HtmlComponentFactory Html  =>
        _htmlComponentFactory ??= serviceProvider.GetRequiredService<HtmlComponentFactory>();

    public ActionButtonFactory ActionButton => GetFactory<ActionButtonFactory>();
    public TextGroupFactory TextGroup  => GetFactory<TextGroupFactory>();

    public RouteContext RouteContext =>
        _routeContext ??= serviceProvider.GetRequiredService<RouteContextFactory>().Create();

    private T GetFactory<T>() where T : notnull
    {
        return serviceProvider.GetRequiredService<T>();
    }
}