#nullable enable
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.Widgets;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.Web.Factories;

public interface IComponentFactory
{
    IFormElementComponentFactory<JJAuditLogView> AuditLog { get; }
    IFormElementComponentFactory<JJDataExportation> DataExportation { get; }
    IFormElementComponentFactory<JJDataImportation> DataImportation { get; }
    IFormElementComponentFactory<JJDataPanel> DataPanel { get; }
    IFormElementComponentFactory<JJFormView> FormView { get; }
    IFormElementComponentFactory<JJGridView> GridView { get; }
    IComponentFactory<JJUploadView> FormUpload { get; }
    IComponentFactory<JJFileDownloader> Downloader { get; }
    IComponentFactory<JJUploadArea> UploadArea { get; }
    LinkButtonFactory LinkButton { get; }
    IControlFactory Controls { get; }
    RouteContext RouteContext { get; }
}