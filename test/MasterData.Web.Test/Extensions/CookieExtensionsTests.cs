using System.Security.Claims;
using JJMasterData.Core.Extensions;
using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Web.Test.Extensions;

public class CookieExtensionsTests
{
    [Fact]
    public void SetGridOrderCookie_SameUserOnNextRequest_ReadsCookieValue()
    {
        var writeContext = CreateHttpContext("user-1");

        writeContext.SetGridOrderCookie("Customers", "Name asc");

        var readContext = CreateHttpContext("user-1", GetRequestCookieHeader(writeContext));

        Assert.Equal("Name asc", readContext.GetGridOrderCookie("Customers"));
    }

    [Fact]
    public void GetGridOrderCookie_DifferentUserOnNextRequest_IgnoresCookieValue()
    {
        var writeContext = CreateHttpContext("user-1");

        writeContext.SetGridOrderCookie("Customers", "Name asc");

        var readContext = CreateHttpContext("user-2", GetRequestCookieHeader(writeContext));

        Assert.Null(readContext.GetGridOrderCookie("Customers"));
    }

    [Fact]
    public void SetGridFilterCookie_SameUserOnNextRequest_ReadsCookieValue()
    {
        var writeContext = CreateHttpContext("user-1");

        writeContext.SetGridFilterCookie("Customers", new Dictionary<string, object>
        {
            ["Name"] = "Alice"
        });

        var readContext = CreateHttpContext("user-1", GetRequestCookieHeader(writeContext));
        var filter = readContext.GetGridFilterCookie("Customers");

        Assert.NotNull(filter);
        Assert.Equal("Alice", filter["Name"]?.ToString());
    }

    [Fact]
    public void SetGridFilterCookie_UsesApplicationPath()
    {
        var context = CreateHttpContext("user-1");
        context.Request.PathBase = "/app";
        context.Request.Path = "/customers/filter";

        context.SetGridFilterCookie("Customers", new Dictionary<string, object>
        {
            ["Name"] = "Alice"
        });

        Assert.Contains("path=/app", context.Response.Headers.SetCookie.ToString(),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SetGridFilterCookie_WithoutApplicationPath_UsesRootPath()
    {
        var context = CreateHttpContext("user-1");
        context.Request.Path = "/customers/filter";

        context.SetGridFilterCookie("Customers", new Dictionary<string, object>
        {
            ["Name"] = "Alice"
        });

        Assert.Contains("path=/", context.Response.Headers.SetCookie.ToString(),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetGridFilterCookie_TamperedPayload_ReturnsNull()
    {
        var writeContext = CreateHttpContext("user-1");

        writeContext.SetGridFilterCookie("Customers", new Dictionary<string, object>
        {
            ["Name"] = "Alice"
        });

        var cookieHeader = GetRequestCookieHeader(writeContext);
        var tamperedCookieHeader = $"{cookieHeader[..^1]}X";
        var readContext = CreateHttpContext("user-1", tamperedCookieHeader);

        Assert.Null(readContext.GetGridFilterCookie("Customers"));
    }

    private static DefaultHttpContext CreateHttpContext(string userId, string? cookieHeader = null)
    {
        var context = new DefaultHttpContext();
        var services = new ServiceCollection();
        services.AddDataProtection();

        context.Request.Path = "/customers";
        context.RequestServices = services.BuildServiceProvider();
        context.User = new ClaimsPrincipal(
            new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId)],
                authenticationType: "test"));

        if (!string.IsNullOrEmpty(cookieHeader))
            context.Request.Headers.Cookie = cookieHeader;

        return context;
    }
    private static string GetRequestCookieHeader(DefaultHttpContext context)
    {
        var setCookie = context.Response.Headers.SetCookie.ToString();
        var separatorIndex = setCookie.IndexOf(';');
        return separatorIndex >= 0 ? setCookie[..separatorIndex] : setCookie;
    }
}
