namespace JJMasterData.Web.Areas.MasterData.Models;

public sealed class InternalRedirectViewModel
{
    public required string HtmlContent { get; init; }
    public required bool ShowToolbar { get; init; } 
    public required string Title { get; init; }
    public bool SubmitParentWindow { get; set; }
}