using JJConsulting.FontAwesome;
using JJConsulting.Html;
using JJMasterData.Core.DataDictionary;


namespace JJMasterData.Core.UI.Components;

public class NavContent
{
    public required string Title { get; set; }
    public required FontAwesomeIcon? Icon { get; set; }
    public HtmlBuilder HtmlContent { get; set; }
    
}
