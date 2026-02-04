using Microsoft.AspNetCore.Html;

namespace JJMasterData.Web.Areas.MasterData.Models;

public sealed class InternalRedirectViewModel
{
    public required IHtmlContent HtmlContent { get; init; }
    public required bool IsModal { get; set; }
    public required string ParentElementName { get; set; }
    public required bool ShowToolbar { get; init; } 
    public required string Title { get; init; }
    public bool SubmitParentWindow { get; set; }
}