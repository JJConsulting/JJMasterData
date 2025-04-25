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
    
    GridViewInfiniteScroll,
    
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
    
    /// <summary>
    /// The component is requesting a JJSearchBox result from a PageState.Filter
    /// </summary>
    SearchBoxFilter,
    
    DownloadFile,
    
    AuditLogView,

    InsertSelection,
    LookupDescription
}