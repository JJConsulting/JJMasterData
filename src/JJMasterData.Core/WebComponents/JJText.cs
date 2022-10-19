using JJMasterData.Core.Html;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Represents a plain text.
/// </summary>
public class JJText : JJBaseView
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

    internal override HtmlElement RenderHtmlElement() => new(Text);
}
