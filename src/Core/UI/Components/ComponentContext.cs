namespace JJMasterData.Core.UI.Components;

public enum ComponentContext
{
    /// <summary>
    /// Renders the component
    /// </summary>
    RenderComponent,
        
    FormViewReload,
    
    /// <summary>
    /// The component is requesting a JJSearchBox result from a GridView filter
    /// </summary>
    GridViewFilterSearchBox,
    GridViewSelectAllRows,
    GridViewRow,
    GridViewReload,
        
    /// <summary>
    /// Reloads the current JJDataPanel
    /// </summary>
    DataPanelReload,
    
    /// <summary>
    /// The component is inside a modal
    /// </summary>
    Modal,
        
    DataExportation,
    
    DataImportation,
    DataImportationFileUpload,
        
    TextFileUploadView,
    TextFileFileUpload,
    
    /// <summary>
    /// The component is requesting a url redirect
    /// </summary>
    UrlRedirect,
    
    /// <summary>
    /// The component is requesting a JJSearchBox result
    /// </summary>
    SearchBox,
    DownloadFile,
    
    AuditLogView,

    InsertSelection
}