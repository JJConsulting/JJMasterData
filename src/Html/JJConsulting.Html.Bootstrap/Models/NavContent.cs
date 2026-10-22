using JJConsulting.FontAwesome;
using Microsoft.AspNetCore.Html;

namespace JJConsulting.Html.Bootstrap.Models;

public class NavContent
{
    public required string Title { get; set; }
    public required FontAwesomeIcon? Icon { get; set; }
    public IHtmlContent? Content { get; set; }
}
