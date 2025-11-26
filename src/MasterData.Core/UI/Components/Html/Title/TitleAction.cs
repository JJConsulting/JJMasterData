#nullable enable

using JJConsulting.FontAwesome;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.UI.Components;

public sealed class TitleAction
{
    public FontAwesomeIcon? Icon { get; set; }
    public string? Text { get; set; }
    public string? Tooltip { get; set; }
    public required string Url { get; set; }
}