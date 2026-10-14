namespace JJConsulting.Html.Bootstrap.Abstractions;

/// <summary>
/// Base class of every component that renders to HTML.
/// </summary>
public abstract class ComponentBase
{
    #region "Properties"

    public bool Visible { get; set; } = true;

    /// <summary>
    /// Represents the component unique identifier.
    /// The name will be sent to the client, do not expose table names and/or sensitive data.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// HTML attributes represented by key/value pairs
    /// </summary>
    public Dictionary<string, string> Attributes { get; set; } = new(StringComparer.InvariantCultureIgnoreCase);

    public string? CssClass { get; set; }

    #endregion

    public string GetAttribute(string key)
    {
        return Attributes.TryGetValue(key, out var attribute) ? attribute : string.Empty;
    }

    public void SetAttribute(string key, string? value)
    {
        if (value == null || string.IsNullOrEmpty(value))
        {
            Attributes.Remove(key);
        }
        else
        {
            Attributes[key] = value;
        }
    }

    public void SetAttributes(Dictionary<string, object?>? values)
    {
        if (values == null)
            return;

        foreach (var v in values)
        {
            SetAttribute(v.Key, v.Value?.ToString());
        }
    }
}
