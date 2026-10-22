using JJConsulting.FontAwesome;

namespace JJConsulting.Html.Bootstrap.Models;

public sealed class TitleAction
{
    public FontAwesomeIcon? Icon { get; set; }
    public string? Text { get; set; }
    public string? Tooltip { get; set; }
    public required string Url { get; set; }
}