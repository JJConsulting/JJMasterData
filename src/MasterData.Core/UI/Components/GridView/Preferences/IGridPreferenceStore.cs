namespace JJMasterData.Core.UI.Components;

/// <summary>Stores all preferences for a grid in the implementation's current scope.</summary>
public interface IGridPreferenceStore
{
    /// <summary>Returns saved preferences, or null when none exist. Grid names are case-insensitive.</summary>
    GridPreferences? Get(string gridName);

    /// <summary>Saves the complete preferences, or removes them when null. Writes must be visible immediately.</summary>
    void Set(string gridName, GridPreferences? preferences);
}
