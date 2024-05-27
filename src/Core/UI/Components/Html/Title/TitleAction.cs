#nullable enable

using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.UI.Components;

public sealed class TitleAction
{
    public IconType? Icon { get; set; }
    public string? Text { get; set; }
    public string? Tooltip { get; set; }
    public required string Url { get; set; }
}