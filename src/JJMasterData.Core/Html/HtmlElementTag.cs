namespace JJMasterData.Core.Html;

/// <summary>
/// Implementation of HTML tag.
/// </summary>
public class HtmlElementTag
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlElementTag"/> class.
    /// </summary>
    public HtmlElementTag(HtmlTag tag)
    {
        TagName = tag;

        if (tag is HtmlTag.Br or HtmlTag.Input or HtmlTag.Hr)
            HasClosingTag = false;
        else
            HasClosingTag = true;
    }

    public HtmlElementTag(HtmlTag tag, bool hasClosingTag)
    {
        TagName = tag;
        HasClosingTag = hasClosingTag;
    }
    
    /// <summary>
    /// Name of the tag.
    /// </summary>
    public HtmlTag TagName { get; set; }

    /// <summary>
    /// Flag that indicates whether the tag is self closing (false) or not (true).
    /// </summary>
    public bool HasClosingTag { get; set; }

}
