using JJMasterData.Web.Models;
using Microsoft.AspNetCore.Http;

namespace JJMasterData.Web.Services;

public class ThemeService
{
    private readonly HttpContext _httpContext;

    public ThemeService(IHttpContextAccessor contextAccessor)
    {
        _httpContext = contextAccessor.HttpContext!;
    }

    public string GetLogoPath()
    {
        return GetTheme() == ThemeMode.Dark ? "images/JJMasterDataDark.png" : "images/JJMasterData.png";
    }

    public ThemeMode GetTheme()
    {
        return (ThemeMode)int.Parse(_httpContext.Request.Cookies["JJMasterDataTheme"] ?? "0");
    }

    public void SetTheme(ThemeMode theme)
    {
        var options = new CookieOptions
        {
            Expires = DateTimeOffset.MaxValue
        };

        _httpContext.Response.Cookies.Append("JJMasterDataTheme", ((int)theme).ToString(), options);
    }


}