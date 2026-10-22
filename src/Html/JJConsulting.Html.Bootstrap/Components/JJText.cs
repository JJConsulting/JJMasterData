using JJConsulting.Html.Bootstrap.Abstractions;

namespace JJConsulting.Html.Bootstrap.Components;

/// <summary>
/// Represents a plain text.
/// </summary>
public sealed class JJText : HtmlComponent
{
    private string Text { get; }

    public JJText(string text)
    {
        Visible = true;
        Text = text;
    }

    protected override HtmlBuilder BuildHtml() => new(Text);
}
