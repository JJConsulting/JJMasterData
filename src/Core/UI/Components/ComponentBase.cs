using System;
using System.Collections;
using System.Collections.Generic;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Components;

/// <summary>
/// Base class of every component that renders to HTML.
/// Every public component inherits from this class.
/// </summary>
public abstract class ComponentBase
{
    #region "Properties"

    private IDictionary<string, object>_userValues;
    private IDictionary<string, string> _attributes;

    
    /// <summary>
    /// Values specified by the user.
    /// Used to replace values who support expression during runtime .
    /// </summary>
    public IDictionary<string, object>UserValues
    {
        get => _userValues ??= new Dictionary<string, object>();
        set => _userValues = value;
    }
    
    internal bool IsExternalRoute
    {
        get => false;
        set
        {
            value = false;
        } 
    }

    public bool Visible { get; set; } = true;

    /// <summary>
    /// Represents the component unique identifier
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// HTML attributes represented by key/value pairs
    /// </summary>
    public IDictionary<string, string> Attributes
    {
        get => _attributes ??= new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        set => _attributes = value;
    }

    public string CssClass { get; set; }

    #endregion
    
    public string GetAttr(string key)
    {
        return Attributes.TryGetValue(key, out var attribute) ? attribute : string.Empty;
    }

    public void SetAttr(string key, object value)
    {
        if (value == null || string.IsNullOrEmpty(value.ToString()))
        {
            if (Attributes.ContainsKey(key))
                Attributes.Remove(key);
        }
        else
        {
            Attributes[key] = value.ToString();
        }
    }

    public void SetAttr(IDictionary<string, object> values)
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
    public void SetUserValues(string field, object value)
    {
        UserValues[field] = value;
    }
}
