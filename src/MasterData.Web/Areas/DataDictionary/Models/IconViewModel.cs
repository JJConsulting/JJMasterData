using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Web.Areas.DataDictionary.Models;

public sealed class IconViewModel
{
    public bool EnablePopUp { get; init; } = true;
    public IconType SelectedIcon { get; init; }
    public required string InputId { get; init; }
}