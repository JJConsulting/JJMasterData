﻿using System;

namespace JJMasterData.Core.DataDictionary.Models;

/// <summary>
/// Represents the state of a component.
/// </summary>
///  <div class="NOTE">
///<h5>NOTE</h5>
///<p>You can change the PageState of a dictionary from the code to create dynamic user experiences.</p>
///</div>
public enum PageState
{
    /// <summary>
    /// Represents the dictionary in grid list mode
    /// </summary>
    List = 1,

    /// <summary>
    /// Represents the dictionary showing a specific record in view mode
    /// </summary>
    View = 2,

    /// <summary>
    /// Represents the dictionary creating a record
    /// </summary>
    Insert = 3,

    /// <summary>
    /// Represents the dictionary updating a record
    /// </summary>
    Update = 4,

    /// <summary>
    /// Represents the dictionary filter
    /// </summary>
    Filter = 5,

    /// <summary>
    /// Represents the dictionary importation grid
    /// </summary>
    Import = 6,

    /// <summary>
    /// Represents the dictionary deleting a record
    /// </summary>
    Delete = 7
}

internal static class PageStateExtensions
{
    internal static string GetPageStateName(this PageState state)
    {
        return state switch
        {
            PageState.List => "List",
            PageState.View => "View",
            PageState.Insert => "Insert",
            PageState.Update => "Update",
            PageState.Filter => "Filter",
            PageState.Import => "Import",
            PageState.Delete => "Delete",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }
}