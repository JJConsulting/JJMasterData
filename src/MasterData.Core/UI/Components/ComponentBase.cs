#nullable enable
using System;
using System.Collections.Generic;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// Base class of every component that renders to HTML.
/// Every public component inherits from this class.
/// </summary>
public abstract class ComponentBase
{
    #region "Properties"

    private Dictionary<string, object?>? _userValues;
    private Dictionary<string, string>? _attributes;
    
    /// <summary>
    /// Values specified by the user.
    /// Used to replace values who support expression during runtime .
    /// </summary>
    public Dictionary<string, object?> UserValues
    {
        get => _userValues ??= new Dictionary<string, object?>();
        set => _userValues = value;
    }

    public bool Visible { get; set; } = true;

    /// <summary>
    /// Represents the component unique identifier.
    /// The name will be sent to the client, do not expose table names and/or sensitive data. 
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// HTML attributes represented by key/value pairs
    /// </summary>
    public Dictionary<string, string> Attributes
    {
        get => _attributes ??= new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        set => _attributes = value;
    }

    public string? CssClass { get; set; }

    #endregion

    public string GetAttr(string key)
    {
        return Attributes.TryGetValue(key, out var attribute) ? attribute : string.Empty;
    }

    public void SetAttr(string key, object? value)
    {
        SetAttr(key, value?.ToString());
    }
    
    public void SetAttr(string key, string? value)
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
    
    public void SetAttr(Dictionary<string, object?>? values)
    {
        if (values == null)
            return;

        foreach (var v in values)
        {
            SetAttr(v.Key, v.Value);
        }
    }

    /// <summary>
    /// Add or update a value in UserValues.<br></br>
    /// If exists, insert it, else, update it.
    /// </summary>
    /// <param name="field">Name of the field</param>
    /// <param name="value">Name of the field</param>
    public void SetUserValues(string field, object? value)
    {
        UserValues[field] = value;
    }
}
