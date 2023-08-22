using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

/// <summary>
/// Represents a plain text.
/// </summary>
public class JJText : HtmlComponent
{
    private string Text { get; set; }

    public JJText()
    {
        Visible = true;
    }

    public JJText(string text)
    {
        Visible = true;
        Text = text;
    }

    internal override HtmlBuilder BuildHtml() => new(Text);
}
