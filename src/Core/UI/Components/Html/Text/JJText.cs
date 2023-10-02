using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// Represents a plain text.
/// </summary>
public class JJText : HtmlComponent
{
    private string Text { get; set; }

    public JJText(string text)
    {
        Visible = true;
        Text = text;
    }

    internal override HtmlBuilder BuildHtml() => new(Text);
}
