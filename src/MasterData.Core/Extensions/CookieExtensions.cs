using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text.Json;
using JJMasterData.Commons.Serialization;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Extensions;

public static class CookieExtensions
{
    private const string Prefix = ".JJMasterData.";
    private const string GridCookiePrefix = "Grid.";
    private const string FilterCookieSuffix = ".Filter";
    private const string OrderCookieSuffix = ".Order";
    private const string PageCookieSuffix = ".Page";
    private const string SettingsCookieSuffix = ".Settings";

    extension(HttpContext httpContext)
    {
        public Dictionary<string, object> GetGridFilterCookie(string gridName)
        {
            var cookieValue = httpContext.GetDecodedCookieValue(GetGridCookieKey(gridName, FilterCookieSuffix));
            if (string.IsNullOrEmpty(cookieValue))
                return null;

            try
            {
                var unprotectedValue = httpContext.GetGridFilterCookieProtector().Unprotect(cookieValue);
                var filter = JsonSerializer.Deserialize<Dictionary<string, object>>(unprotectedValue, MasterDataJsonSerializerOptions.Default);

                if (filter == null || filter.Count == 0)
                    return null;

                return filter;
            }
            catch (CryptographicException)
            {
                return null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public void SetGridFilterCookie(string gridName, Dictionary<string, object> filterValues)
        {
            var serializedValue = filterValues is { Count: > 0 }
                ? JsonSerializer.Serialize(filterValues, MasterDataJsonSerializerOptions.Default)
                : null;

            serializedValue = serializedValue == null
                ? null
                : httpContext.GetGridFilterCookieProtector().Protect(serializedValue);

            httpContext.SetCookieValue(GetGridCookieKey(gridName, FilterCookieSuffix), serializedValue);
        }

        public string GetGridOrderCookie(string gridName)
        {
            return httpContext.GetDecodedCookieValue(GetGridCookieKey(gridName, OrderCookieSuffix));
        }

        public void SetGridOrderCookie(string gridName, string orderBy)
        {
            httpContext.SetCookieValue(GetGridCookieKey(gridName, OrderCookieSuffix), string.IsNullOrEmpty(orderBy) ? null : orderBy);
        }

        public int? GetGridCurrentPageCookie(string gridName)
        {
            var cookieValue = httpContext.GetDecodedCookieValue(GetGridCookieKey(gridName, PageCookieSuffix));
            if (string.IsNullOrEmpty(cookieValue))
                return null;

            return int.TryParse(cookieValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var page)
                ? page
                : null;
        }

        public void SetGridCurrentPageCookie(string gridName, int page)
        {
            httpContext.SetCookieValue(GetGridCookieKey(gridName, PageCookieSuffix), page.ToString(CultureInfo.InvariantCulture));
        }

        public GridSettings GetGridSettingsCookie(string gridName)
        {
            var cookieValue = httpContext.GetDecodedCookieValue(GetGridCookieKey(gridName, SettingsCookieSuffix));
            if (string.IsNullOrEmpty(cookieValue))
                return null;

            return JsonSerializer.Deserialize<GridSettings>(cookieValue, MasterDataJsonSerializerOptions.Default);
        }

        public void SetGridSettingsCookie(string gridName, GridSettings settings)
        {
            var serializedValue = settings == null
                ? null
                : JsonSerializer.Serialize(settings, MasterDataJsonSerializerOptions.Default);

            httpContext.SetCookieValue(GetGridCookieKey(gridName, SettingsCookieSuffix), serializedValue);
        }
   
        private void SetCookieValue(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                httpContext.Response.Cookies.Delete(key, httpContext.CreateCookieOptions());
                return;
            }

            httpContext.Response.Cookies.Append(key, Uri.EscapeDataString(value), httpContext.CreateCookieOptions());
        }
        
        private CookieOptions CreateCookieOptions()
        {
            return new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                Path = httpContext.Request.Path,
                SameSite = SameSiteMode.Lax,
                Secure = httpContext.Request.IsHttps
            };
        }

        private string GetDecodedCookieValue(string key)
        {
            var value = httpContext.Request.Cookies[key];
            return string.IsNullOrEmpty(value) ? value : Uri.UnescapeDataString(value);
        }

        private IDataProtector GetGridFilterCookieProtector()
        {
            var provider = httpContext.RequestServices.GetRequiredService<IDataProtectionProvider>();
            return provider.CreateProtector("JJMasterData GridView filter cookie.");
        }
    }
    
    private static string GetGridCookieKey(string gridName, string suffix = null)
    {
        return $"{Prefix}{GridCookiePrefix}{gridName.ToLowerInvariant()}{suffix}";
    }
}
