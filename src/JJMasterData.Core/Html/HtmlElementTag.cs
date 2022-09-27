namespace JJMasterData.Core.Html;

/// <summary>
/// Implementation of HTML tag.
/// </summary>
public class HtmlElementTag
{
    
    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlElementTag"/> class.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="hasClosingTag"></param>
    public HtmlElementTag(HtmlTag tag)
    {
        this.TagName = tag;

        if (tag == HtmlTag.Br |
            tag == HtmlTag.Input)
            this.HasClosingTag = false;
        else
            this.HasClosingTag = true;
    }

    public HtmlElementTag(HtmlTag tag, bool hasClosingTag)
    {
        this.TagName = tag;
        this.HasClosingTag = hasClosingTag;
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
