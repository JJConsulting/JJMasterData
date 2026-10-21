using System.Collections.Generic;

namespace JJMasterData.Core.UI.Components;

/// <summary>All saved preferences for one grid.</summary>
public sealed class GridPreferences
{
    public GridSettings? Settings { get; set; }
    public Dictionary<string, object?>? Filter { get; set; }
    public string? Order { get; set; }
    public int? CurrentPage { get; set; }
}
