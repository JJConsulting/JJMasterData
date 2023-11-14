namespace JJMasterData.Core.UI.Html;

/// <summary>
/// Implementation of HTML tag.
/// </summary>
public class HtmlBuilderTag(HtmlTag tag, bool hasClosingTag)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlBuilderTag"/> class.
    /// </summary>
    public HtmlBuilderTag(HtmlTag tag) : this(tag, tag is not (HtmlTag.Br or HtmlTag.Input or HtmlTag.Hr))
    {
    }

    /// <summary>
    /// Name of the tag.
    /// </summary>
    public HtmlTag TagName { get; set; } = tag;

    /// <summary>
    /// Flag that indicates whether the tag is self closing (false) or not (true).
    /// </summary>
    public bool HasClosingTag { get; set; } = hasClosingTag;
}
