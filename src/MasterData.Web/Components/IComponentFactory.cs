using JJMasterData.Web.Components.Factories;
using JJMasterData.Web.Routing;

namespace JJMasterData.Web.Components;

public interface IComponentFactory
{
    IFormElementComponentFactory<JJAuditLogView> AuditLog { get; }
    IFormElementComponentFactory<JJDataExportation> DataExportation { get; }
    IFormElementComponentFactory<JJDataImportation> DataImportation { get; }
    IFormElementComponentFactory<JJDataPanel> DataPanel { get; }
    IFormElementComponentFactory<JJFormView> FormView { get; }
    IFormElementComponentFactory<JJGridView> GridView { get; }
    UploadViewFactory UploadView { get; }
    FileDownloaderFactory Downloader { get; }
    UploadAreaFactory UploadArea { get; }
    TitleFactory Title { get; }
    MessageBoxFactory MessageBox { get; }
    LabelFactory Label { get; }
    ControlFactory Controls { get; }
    ActionButtonFactory ActionButton { get; }
    TextGroupFactory TextGroup { get; }
    RouteContext RouteContext { get; }
}