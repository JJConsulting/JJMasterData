namespace JJMasterData.Core.UI.Components;

public enum ComponentContext
{
    /// <summary>
    /// Renders the component
    /// </summary>
    RenderComponent,
        
    FormViewReload,
        
    GridViewSelectAllRows,
    GridViewRow,
    GridViewReload,

    GridViewFilterReload,
    
    /// <summary>
    /// The component is requesting a JJSearchBox result from a GridView filter
    /// </summary>
    GridViewFilterSearchBox,
    
    /// <summary>
    /// Reloads the current JJDataPanel
    /// </summary>
    DataPanelReload,
        
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

    InsertSelection,
    LookupDescription
}