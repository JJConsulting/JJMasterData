namespace JJMasterData.Core.UI.Html;

/// <summary>
/// Implementation of HTML tag.
/// </summary>
public class HtmlBuilderTag(HtmlTag tag, bool hasClosingTag)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlBuilderTag"/> class.
    /// </summary>
    public HtmlBuilderTag(HtmlTag tag) : this(tag, CheckClosingTagPresence(tag))
    {
    }
    
    /// <summary>
    /// Name of the tag.
    /// </summary>
    public HtmlTag TagName { get;} = tag;

    /// <summary>
    /// Flag that indicates whether the tag is self closing (false) or not (true).
    /// </summary>
    public bool HasClosingTag { get; } = hasClosingTag;

    private static bool CheckClosingTagPresence(HtmlTag htmlTag)
    {
        return htmlTag switch
        {
            HtmlTag.Area => false,
            HtmlTag.Br => false,
            HtmlTag.Hr => false,
            HtmlTag.Img => false,
            HtmlTag.Input => false,
            _ => true
        };
    }
}
