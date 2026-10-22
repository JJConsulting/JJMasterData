using System.Collections.Generic;
using System.Text.Json;
using JJMasterData.Commons.Serialization;
using JJMasterData.Core.DataManager;

namespace JJMasterData.Core.UI.Components;

/// <summary>Stores one preference document per grid in the current ASP.NET Core session.</summary>
public sealed class SessionGridPreferenceStore(IHttpContextAccessor contextAccessor, IMasterDataUser user) : IGridPreferenceStore
{
    private readonly Dictionary<string, GridPreferences?> _values = new();

    public GridPreferences? Get(string gridName)
    {
        if (contextAccessor.HttpContext == null || string.IsNullOrWhiteSpace(user.Id))
            return null;

        var key = GetKey(gridName);
        if (_values.TryGetValue(key, out var cached))
            return cached;

        var serialized = contextAccessor.HttpContext.Session.GetString(key);
        GridPreferences? preferences = null;
        try
        {
            if (serialized != null)
                preferences = JsonSerializer.Deserialize<GridPreferences>(serialized, MasterDataJsonSerializerOptions.Default);
        }
        catch (JsonException)
        {
            preferences = null;
        }

        _values[key] = preferences;
        return preferences;
    }

    public void Set(string gridName, GridPreferences? preferences)
    {
        if (contextAccessor.HttpContext == null || string.IsNullOrWhiteSpace(user.Id))
            return;

        var session = contextAccessor.HttpContext.Session;
        var key = GetKey(gridName);
        if (preferences == null)
        {
            session.Remove(key);
            _values[key] = null;
            return;
        }

        session.SetString(key, JsonSerializer.Serialize(preferences, MasterDataJsonSerializerOptions.Default));
        _values[key] = preferences;
    }

    private static string GetKey(string gridName) => ".JJMasterData.Grid." + gridName.ToLowerInvariant();
}
