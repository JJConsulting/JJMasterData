using JJConsulting.FontAwesome;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Web.Areas.DataDictionary.Models;

public sealed class IconViewModel
{
    public bool EnablePopUp { get; init; } = true;
    public FontAwesomeIcon SelectedIcon { get; init; }
    public required string InputId { get; init; }
}