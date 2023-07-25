

namespace JJMasterData.Core.Web.Components;

/// <summary>
/// Class responsible to render the UI on JJGridView
/// </summary>
public class GridSettings
{
    /// <remarks>
    /// (Default = 5)
    /// </remarks>
    public int TotalPerPage { get; set; }

    public int TotalPaginationButtons { get; set; }

    public bool ShowBorder { get; set; }

    public bool ShowRowStriped { get; set; }

    public bool ShowRowHover { get; set; }

    public bool IsResponsive { get; set; }

    public bool IsHeaderFixed { get; set; }
    
    public GridSettings()
    {
        TotalPerPage = 5;
        TotalPaginationButtons = 5;
        ShowBorder = false;
        ShowRowStriped = true;
        ShowRowHover = true;
        IsResponsive = true;
    }

    
}