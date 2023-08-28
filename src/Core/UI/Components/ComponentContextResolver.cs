#nullable enable

using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

internal class ComponentContextResolver
{
    private IHttpContext HttpContext { get; }

    private string? DataPanelName { get; }
    private string? GridViewName { get; }
    private string CurrentComponentName { get; }
    private string? CurrentFieldName { get; }
    private string? ElementName { get; }
    
    public ComponentContextResolver(JJFormView formView)
    {
        HttpContext = formView.CurrentContext;
        DataPanelName = formView.DataPanel.Name;
        CurrentComponentName = formView.Name;
        ElementName = formView.FormElement.Name;
        GridViewName = formView.GridView.Name;
    }
    
    public ComponentContextResolver(JJGridView gridView)
    {
        HttpContext = gridView.CurrentContext;
        CurrentComponentName = gridView.Name;
        ElementName = gridView.FormElement.Name;
        GridViewName = gridView.Name;
    }
    
    public ComponentContextResolver(JJDataPanel dataPanel)
    {
        HttpContext = dataPanel.CurrentContext;
        DataPanelName = dataPanel.Name;
        CurrentComponentName = dataPanel.Name;
        ElementName = dataPanel.FormElement.Name;
    }
    
    public ComponentContextResolver(JJSearchBox searchBox)
    {
        HttpContext = searchBox.CurrentContext;
        CurrentComponentName = searchBox.Name;
        CurrentFieldName = searchBox.FieldName;
        ElementName = searchBox.ElementName;
    }
    
    public ComponentContext GetContext()
    {
        var context = HttpContext.Request.QueryString("context");
        var panelName = HttpContext.Request.QueryString("panelName");
        var gridName = HttpContext.Request.QueryString("gridViewName");
        var elementName = HttpContext.Request.QueryString("elementName");
     
        
        if (context == "htmlContent")
            return ComponentContext.HtmlContent;
        if (context == "modal")
            return ComponentContext.Modal;
        if (context == "dataExportation" && GridViewName == gridName)
            return ComponentContext.DataExportation;
        if (context == "dataImportation" && GridViewName == gridName)
            return ComponentContext.DataImportation ;
        if (context == "fileUpload")
            return ComponentContext.FileUpload;
        if (context == "panelReload" && DataPanelName == panelName)
            return ComponentContext.PanelReload;
        if (context == "urlRedirect")
            return ComponentContext.UrlRedirect;

        if (context == "gridViewRow")
        {
            if (GridViewName == gridName)
            {
                return ComponentContext.GridViewRow;
            }
        }
        
        if (IsUploadViewRoute())
            return ComponentContext.FileUpload;
        
        if (IsLookupRoute())
            return ComponentContext.Lookup;
        
        if (IsDownloadRoute())
            return ComponentContext.DownloadFile;
        
        if (context == "searchBox" && (elementName == ElementName || elementName is null))
        {
            return ComponentContext.SearchBox;
        }
         
        return ComponentContext.RenderComponent;
    }

    private bool IsUploadViewRoute()
    {
        return HttpContext.Request.QueryString(JJTextFile.UploadViewParameterName + DataPanelName) != null;
    }
    
    private bool IsLookupRoute()
    {
        string lookupRoute = HttpContext.Request.QueryString("lookup-" + DataPanelName);
        return !string.IsNullOrEmpty(lookupRoute);
    }
    
    
    private bool IsDownloadRoute()
    {
        if (HttpContext.Request.QueryString(JJFileDownloader.DirectDownloadParameter) != null)
            return true;
        
        return HttpContext.Request.QueryString(JJFileDownloader.DownloadParameter) != null;
    }

}