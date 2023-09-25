namespace JJMasterData.Web.Areas.MasterData.Models;

public class InternalRedirectViewModel
{
    public string HtmlContent { get; set; }
    public bool ShowToolBar { get; set; }
    
    public string Title { get; set; }
    
    public InternalRedirectViewModel(string title, string htmlContent, bool showToolBar)
    {
        Title = title;
        HtmlContent = htmlContent;
        ShowToolBar = showToolBar;
    }
}