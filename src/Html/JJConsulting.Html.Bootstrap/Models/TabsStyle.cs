namespace JJConsulting.Html.Bootstrap.Models;

public enum TabsStyle
{
    Default,
    Underline,
    Pills
}

public static class TabStyleExtensions
{
    extension(TabsStyle style)
    {
        public string CssClass
        {
            get
            {
                return style switch
                {
                    TabsStyle.Default => "nav-tabs",
                    TabsStyle.Underline => "nav-underline",
                    TabsStyle.Pills => "nav-pills",
                    _ => throw new ArgumentOutOfRangeException(nameof(style), style, null)
                };
            }
        }
    }
}