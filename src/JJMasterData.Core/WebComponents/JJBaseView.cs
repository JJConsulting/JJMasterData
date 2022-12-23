using JJMasterData.Core.Html;
using System;
using System.Collections;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Base class of every component that renders to HTML.
/// Every public component inherits from this class.
/// </summary>
public abstract class JJBaseView
{
    #region "Properties"

    private Hashtable _userValues;
    private Hashtable _attributes;
 
    /// <summary>
    /// Values specified by the user.
    /// Used to replace values who support expression during runtime .
    /// </summary>
    public Hashtable UserValues
    {
        get => _userValues ??= new Hashtable();
        set => _userValues = value;
    }

    public bool Visible { get; set; } = true;

    /// <summary>
    /// Represents the component unique identifier
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// HTML attributes represented by key/value pairs
    /// </summary>
    public Hashtable Attributes
    {
        get => _attributes ??= new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        set => _attributes = value;
    }

    public string CssClass { get; set; }
    

    #endregion

    #region "Constructors"

    #endregion

    /// <summary>
    /// Returns the object representation of the HTML
    /// </summary>
    internal abstract HtmlBuilder RenderHtml();

    public HtmlBuilder GetHtmlBuilder()
    {
        return Visible ? RenderHtml() : null;
    }

    /// <summary>
    /// Renders the content in HTML.
    /// </summary>
    /// <returns>
    /// The HTML string.
    /// </returns>
    public string GetHtml()
    {
        return Visible ? RenderHtml()?.ToString(true) : string.Empty;
    }


    public string GetAttr(string key)
    {
        return Attributes.ContainsKey(key) ? Attributes[key].ToString() : string.Empty;
    }

    public void SetAttr(string key, object value)
    {
        if (Attributes.ContainsKey(key))
            Attributes[key] = value;
        else
            Attributes.Add(key, value);

        if (value == null || string.IsNullOrEmpty(value.ToString()))
            Attributes.Remove(key);
    }

    public void SetAttr(Hashtable values)
    {
        if (values == null)
            return;

        foreach (DictionaryEntry v in values)
        {
            SetAttr(v.Key.ToString(), v.Value);
        }
    }


    /// <summary>
    /// Add or update a value in UserValues.<br></br>
    /// If exists, insert it, else, update it.
    /// </summary>
    /// <param name="field">Name of the field</param>
    /// <param name="value">Name of the field</param>
    public void SetUserValues(string field, object value)
    {
        if (UserValues.ContainsKey(field))
            UserValues[field] = value;
        else
            UserValues.Add(field, value);
    }
}
