

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Represents a string.
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
    
    protected override string RenderHtml()
    {
        return Text;
    }

    
}
