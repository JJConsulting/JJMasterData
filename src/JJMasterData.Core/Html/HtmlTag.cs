namespace JJMasterData.Core.Html;

/// <summary>
/// Implementation of HTML tag.
/// </summary>
public class HtmlTag
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlTag"/> class.
    /// </summary>
    public HtmlTag()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlTag"/> class.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="hasClosingTag"></param>
    public HtmlTag(string name, bool hasClosingTag = true)
    {
        this.Name = name;
        this.HasClosingTag = hasClosingTag;
    }

    /// <summary>
    /// Name of the tag.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Flag that indicates whether the tag is self closing (false) or not (true).
    /// </summary>
    public bool HasClosingTag { get; set; }

    public static implicit operator string(HtmlTag tag)
    {
        return tag.Name;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return this.Name;
    }
}
