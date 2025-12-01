using JJConsulting.Html.Bootstrap.Components;

namespace JJMasterData.Core.UI.Components;

public class InputAddons
{
    public JJIcon Icon { get; set; }

    public string Text { get; set; }

    public string Tooltip { get; set; }

    public InputAddons()
    {

    }

    public InputAddons(string text)
    {
        Text = text;
    }
}