using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Json;
using JJMasterData.Commons.Serialization;
using JJMasterData.Core.DataManager;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// Browser-local preferences protected and isolated by user, application path and grid.
/// All preferences use browser session cookies without an explicit expiration.
/// </summary>
public sealed class CookieGridPreferenceStore(
    IHttpContextAccessor contextAccessor,
    IMasterDataUser user,
    IDataProtectionProvider protectionProvider) : IGridPreferenceStore
{
    private readonly ChunkingCookieManager _cookies = new();
    private readonly Dictionary<string, string?> _values = new();

    public GridPreferences? Get(string gridName)
    {
        var context = contextAccessor.HttpContext;
        var scope = user.Id;
        if (context == null || string.IsNullOrWhiteSpace(scope))
            return null;

        var key = GetKey(scope, gridName);
        if (!_values.TryGetValue(key, out var serialized))
        {
            var value = _cookies.GetRequestCookie(context, key);
            if (string.IsNullOrEmpty(value))
                return null;

            try
            {
                serialized = GetProtector(key).Unprotect(value);
            }
            catch (CryptographicException)
            {
                return null;
            }
        }

        if (serialized == null)
            return null;

        try
        {
            return JsonSerializer.Deserialize<GridPreferences>(serialized, MasterDataJsonSerializerOptions.Default);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public void Set(string gridName, GridPreferences? preferences)
    {
        var context = contextAccessor.HttpContext;
        var scope = user.Id;
        if (context == null || string.IsNullOrWhiteSpace(scope))
            return;

        var key = GetKey(scope, gridName);
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            Path = context.Request.PathBase.Add(context.Request.Path).ToUriComponent(),
            SameSite = SameSiteMode.Lax,
            Secure = context.Request.IsHttps
        };

        if (preferences == null)
        {
            _values[key] = null;
            _cookies.DeleteCookie(context, key, cookieOptions);
            return;
        }

        var serialized = JsonSerializer.Serialize(preferences, MasterDataJsonSerializerOptions.Default);
        _values[key] = serialized;
        _cookies.AppendResponseCookie(context, key, GetProtector(key).Protect(serialized), cookieOptions);
    }

    private static string GetKey(string user, string gridName)
    {
        return $".JJMasterData.Grid." +
               $"{Uri.EscapeDataString(user)}." +
               $"{Uri.EscapeDataString(gridName.ToLowerInvariant())}";
    }

    private IDataProtector GetProtector(string key)
    {
        return protectionProvider.CreateProtector("JJMasterData.GridPreferences.v2", key);
    }
}
