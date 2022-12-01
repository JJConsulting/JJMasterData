namespace JJMasterData.Web.Areas.MasterData.Models.ViewModel;

public class InternalRedirectViewModel
{
    public string HtmlContent { get; set; }
    public bool ShowToolBar { get; set; }

    public InternalRedirectViewModel(string htmlContent, bool showToolBar)
    {
        HtmlContent = htmlContent;
        ShowToolBar = showToolBar;
    }
}