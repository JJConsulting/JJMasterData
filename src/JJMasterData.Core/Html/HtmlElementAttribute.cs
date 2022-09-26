namespace JJMasterData.Core.Html;

/// <summary>
/// Implementation of HTML element attribute.
/// </summary>
public class HtmlElementAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlElementAttribute"/> class.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    internal HtmlElementAttribute(string name, string value)
    {
        this.Name = name;
        this.Value = value;
    }

    /// <summary>
    /// Name of the attribute.
    /// </summary>
    internal string Name { get; private set; }

    /// <summary>
    /// Value of the attribute.
    /// </summary>
    internal string Value { get; private set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return string.Format(Layouts.AttributeLayout, this.Name, this.Value);
    }
}
