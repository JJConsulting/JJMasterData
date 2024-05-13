using System;

namespace JJMasterData.Core.UI.Components;

public enum OffcanvasPosition
{
    Top,
    Bottom,
    Start,
    End
}

public static class OffcanvasPositionExtensions
{
    public static string GetCssClass(this OffcanvasPosition offcanvasPosition)
    {
        return offcanvasPosition switch
        {
            OffcanvasPosition.Top => "offcanvas-top",
            OffcanvasPosition.Bottom =>"offcanvas-bottom",
            OffcanvasPosition.Start => "offcanvas-start",
            OffcanvasPosition.End => "offcanvas-end",
            _ => throw new ArgumentOutOfRangeException(nameof(offcanvasPosition), offcanvasPosition, null)
        };
    }
}