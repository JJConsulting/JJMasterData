namespace JJMasterData.Core.UI.Components;

internal enum ComponentContext
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
    SearchBox
}

internal static class ComponentContextParser
{
    public static ComponentContext FromString(string context)
    {
        return context switch
        {
            "htmlContent"=> ComponentContext.HtmlContent,
            "modal" => ComponentContext.Modal,
            "dataExportation"=> ComponentContext.DataExportation,
            "dataImportation" => ComponentContext.DataImportation,
            "fileUpload" => ComponentContext.FileUpload,
            "panelReload" => ComponentContext.PanelReload,
            "urlRedirect" => ComponentContext.UrlRedirect,
            "searchBox" => ComponentContext.SearchBox,
            _ => ComponentContext.RenderComponent
        };
    }
}