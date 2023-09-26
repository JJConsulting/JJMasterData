

namespace JJMasterData.Core.Web.Components;

/// <summary>
/// Class responsible to render the UI on JJGridView
/// </summary>
public class GridSettings
{
    /// <remarks>
    /// (Default = 5)
    /// </remarks>
    public int RecordsPerPage { get; set; } = 5;

    public int TotalPaginationButtons { get; set; } = 5;

    public bool ShowBorder { get; set; } = false;

    public bool ShowRowStriped { get; set; } = true;

    public bool ShowRowHover { get; set; } = true;

    public bool IsResponsive { get; set; } = true;

    public bool IsHeaderFixed { get; set; }
}