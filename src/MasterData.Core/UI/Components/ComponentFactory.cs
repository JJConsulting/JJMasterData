#nullable enable

using System;
using JJConsulting.Html.Bootstrap.Components;
using JJMasterData.Core.UI.Components.Factories;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.UI.Components;

internal sealed class ComponentFactory(IServiceProvider serviceProvider) : IComponentFactory
{
    private RouteContext? _routeContext;

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

    public UploadViewFactory UploadView =>
        GetFactory<UploadViewFactory>();

    public FileDownloaderFactory Downloader =>
        GetFactory<FileDownloaderFactory>();

    public UploadAreaFactory UploadArea =>
        GetFactory<UploadAreaFactory>();

    public TitleFactory Title => GetFactory<TitleFactory>();
    public MessageBoxFactory MessageBox => GetFactory<MessageBoxFactory>();
    public LabelFactory Label => GetFactory<LabelFactory>();

    public ControlFactory Controls =>
        GetFactory<ControlFactory>();
    
    public ActionButtonFactory ActionButton => GetFactory<ActionButtonFactory>();
    public TextGroupFactory TextGroup  => GetFactory<TextGroupFactory>();

    public RouteContext RouteContext =>
        _routeContext ??= serviceProvider.GetRequiredService<RouteContextFactory>().Create();

    private T GetFactory<T>() where T : notnull => serviceProvider.GetRequiredService<T>();
}