namespace JJMasterData.Web.Areas.MasterData.Models;

public sealed class InternalRedirectViewModel(string title, string htmlContent, bool showToolBar)
{
    public string HtmlContent { get; set; } = htmlContent;
    public bool ShowToolBar { get; set; } = showToolBar;

    public string Title { get; set; } = title;
}