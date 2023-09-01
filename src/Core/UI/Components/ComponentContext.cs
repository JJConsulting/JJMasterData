namespace JJMasterData.Core.UI.Components;

public enum ComponentContext
{
    /// <summary>
    /// Renders the component
    /// </summary>
    RenderComponent,
    
    /// <summary>
    /// Returns only part of the component
    /// </summary>
    HtmlContent,
        
    /// <summary>
    /// Reloads the current JJDataPanel
    /// </summary>
    PanelReload,
    
    /// <summary>
    /// The component is inside a modal
    /// </summary>
    Modal,
        
    DataExportation,
    DataImportation,
    
    /// <summary>
    /// The component is uploading a file
    /// </summary>
    FileUpload,
    
    /// <summary>
    /// The component is requesting a url redirect
    /// </summary>
    UrlRedirect,
    
    /// <summary>
    /// The component is requesting a JJSearchBox result
    /// </summary>
    SearchBox,
    
    /// <summary>
    /// The component is requesting a JJSearchBox result from a GridView filter
    /// </summary>
    GridViewFilterSearchBox,
    
    DownloadFile,
    
    SelectAllGridRows,
    
    GridViewRow
}