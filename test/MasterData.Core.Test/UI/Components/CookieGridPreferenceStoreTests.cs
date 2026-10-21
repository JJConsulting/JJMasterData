using JJMasterData.Core.Extensions;
using JJMasterData.Core.Configuration;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Moq;

namespace JJMasterData.Core.Test.UI.Components;

public class CookieGridPreferenceStoreTests
{
    private readonly EphemeralDataProtectionProvider _protection = new();

    [Fact]
    public void Preferences_RoundTripInASingleSessionCookie()
    {
        var (context, store) = CreateStore();
        store.Set("Customers", new GridPreferences
        {
            Settings = new GridSettings { RecordsPerPage = 25, IsCompact = false },
            Filter = new() { ["Name"] = "Alice" },
            Order = "Name asc",
            CurrentPage = 3
        });

        var cookie = Assert.Single(GetCookies(context));
        Assert.Null(cookie.Expires);
        Assert.Null(cookie.MaxAge);
        Assert.True(cookie.HttpOnly);
        Assert.Equal("/app/customers", cookie.Path.Value);

        var (_, next) = CreateStore(cookieHeader: GetRequestCookies(context));
        var preferences = next.Get("CUSTOMERS");
        Assert.NotNull(preferences);
        Assert.Equal("Alice", preferences.Filter!["Name"]?.ToString());
        Assert.Equal("Name asc", preferences.Order);
        Assert.Equal(3, preferences.CurrentPage);
        var settings = preferences.Settings;
        Assert.NotNull(settings);
        Assert.Equal(25, settings.RecordsPerPage);
        Assert.False(settings.IsCompact);
    }

    [Fact]
    public void NavigationState_RoundTripsWithSessionCookies()
    {
        var (context, store) = CreateStore();
        store.SetFilter("Customers", new() { ["Name"] = "Alice" });
        store.SetOrder("Customers", "Name asc");
        store.SetCurrentPage("Customers", 3);

        Assert.All(GetCookies(context), cookie => Assert.Null(cookie.Expires));
        Assert.Single(GetCookies(context).Select(cookie => cookie.Name.ToString()).Distinct());
        var (_, next) = CreateStore(cookieHeader: GetRequestCookies(context));
        Assert.Equal("Alice", next.GetFilter("Customers")!["Name"]?.ToString());
        Assert.Equal("Name asc", next.GetOrder("Customers"));
        Assert.Equal(3, next.GetCurrentPage("Customers"));
    }

    [Theory]
    [InlineData("other-user", "/app", "Customers")]
    [InlineData("user-1", "/app", "Orders")]
    public void Preferences_AreIsolated(string userId, string pathBase, string gridName)
    {
        var (context, store) = CreateStore();
        store.SetSettings("Customers", new GridSettings());
        store.SetFilter("Customers", new() { ["Name"] = "Alice" });
        store.SetOrder("Customers", "Name asc");
        store.SetCurrentPage("Customers", 3);

        var (_, other) = CreateStore(userId, GetRequestCookies(context), pathBase);
        Assert.Null(other.GetSettings(gridName));
        Assert.Null(other.GetFilter(gridName));
        Assert.Null(other.GetOrder(gridName));
        Assert.Null(other.GetCurrentPage(gridName));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void MissingUser_DisablesPersistence(string? userId)
    {
        var (context, store) = CreateStore(userId);
        store.SetSettings("Customers", new GridSettings());
        store.SetFilter("Customers", new() { ["Name"] = "Alice" });
        store.SetOrder("Customers", "Name asc");
        store.SetCurrentPage("Customers", 2);
        Assert.Empty(GetCookies(context));
        Assert.Null(store.GetSettings("Customers"));
        Assert.Null(store.GetOrder("Customers"));
        Assert.Null(store.GetFilter("Customers"));
        Assert.Null(store.GetCurrentPage("Customers"));
    }

    [Fact]
    public void WritesAndDeletes_AreVisibleWithinTheRequest()
    {
        var (context, store) = CreateStore();
        store.SetSettings("Customers", new GridSettings { RecordsPerPage = 25 });
        store.SetFilter("Customers", new() { ["Name"] = "Alice" });
        store.SetOrder("Customers", "Name asc");
        store.SetCurrentPage("Customers", 2);

        var (_, next) = CreateStore(cookieHeader: GetRequestCookies(context));
        next.SetSettings("Customers", new GridSettings { RecordsPerPage = 50 });
        Assert.Equal(50, next.GetSettings("Customers")!.RecordsPerPage);
        next.SetSettings("Customers", null);
        next.SetFilter("Customers", new());
        next.SetOrder("Customers", null);
        next.SetCurrentPage("Customers", null);
        Assert.Null(next.GetSettings("Customers"));
        Assert.Null(next.GetFilter("Customers"));
        Assert.Null(next.GetOrder("Customers"));
        Assert.Null(next.GetCurrentPage("Customers"));
    }

    [Fact]
    public void TamperedAndLegacyCookies_AreIgnored()
    {
        var (context, store) = CreateStore();
        store.SetSettings("Customers", new GridSettings());
        var cookie = Assert.Single(GetCookies(context));
        var (_, tampered) = CreateStore(cookieHeader: $"{cookie.Name}=invalid; .JJMasterData.Grid.customers.Settings={{}}");
        Assert.Null(tampered.GetSettings("Customers"));
    }

    [Theory]
    [InlineData("not-json")]
    [InlineData("{\"Settings\":{\"recordsPerPage\":-1}}")]
    [InlineData("{\"Settings\":{\"totalPaginationButtons\":0}}")]
    public void InvalidSettings_AreIgnored(string serialized)
    {
        var (context, store) = CreateStore();
        store.SetSettings("Customers", new GridSettings());
        var cookie = Assert.Single(GetCookies(context));
        var payload = _protection.CreateProtector("JJMasterData.GridPreferences.v2", cookie.Name.ToString()).Protect(serialized);
        var (_, next) = CreateStore(cookieHeader: $"{cookie.Name}={payload}");
        Assert.Null(next.GetSettings("Customers"));
    }

    [Fact]
    public void LargeFilter_IsChunkedAndRestored()
    {
        var (context, store) = CreateStore();
        var value = new string('x', 6000);
        store.SetFilter("Customers", new() { ["Name"] = value });
        Assert.True(GetCookies(context).Count > 1);
        var (_, next) = CreateStore(cookieHeader: GetRequestCookies(context));
        Assert.Equal(value, next.GetFilter("Customers")!["Name"]?.ToString());
    }

    [Fact]
    public void Settings_UseSecureCookiesForHttps()
    {
        var (context, store) = CreateStore();
        context.Request.Scheme = "https";
        store.SetSettings("Customers", new GridSettings());
        Assert.True(Assert.Single(GetCookies(context)).Secure);
    }

    [Fact]
    public void Settings_LastWriteWins()
    {
        var (context, store) = CreateStore();
        store.SetSettings("Customers", new GridSettings { RecordsPerPage = 10 });
        store.SetSettings("Customers", new GridSettings { RecordsPerPage = 25 });
        Assert.Equal(25, store.GetSettings("Customers")!.RecordsPerPage);
        var (_, next) = CreateStore(cookieHeader: GetRequestCookies(context));
        Assert.Equal(25, next.GetSettings("Customers")!.RecordsPerPage);
    }

    [Fact]
    public void Settings_DeletionRemovesCookie()
    {
        var (context, store) = CreateStore();
        store.SetSettings("Customers", new GridSettings());
        store.SetSettings("Customers", null);
        var cookie = Assert.Single(GetCookies(context));
        Assert.Empty(cookie.Value.ToString());
        Assert.True(cookie.Expires < DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Registration_PreservesCustomStore()
    {
        var services = new ServiceCollection();
        var custom = Mock.Of<IGridPreferenceStore>();
        services.AddSingleton(custom);
        services.AddJJMasterDataCore();
        using var provider = services.BuildServiceProvider();
        Assert.Same(custom, provider.GetRequiredService<IGridPreferenceStore>());
    }

    private (DefaultHttpContext Context, IGridPreferenceStore Store) CreateStore(
        string? userId = "user-1", string? cookieHeader = null, string pathBase = "/app")
    {
        var context = new DefaultHttpContext();
        context.Request.PathBase = pathBase;
        context.Request.Path = "/customers";
        context.Request.Headers.Cookie = cookieHeader;
        var user = new Mock<IMasterDataUser>();
        user.SetupGet(u => u.Id).Returns(userId!);
        var store = new CookieGridPreferenceStore(new HttpContextAccessor { HttpContext = context }, user.Object, _protection);
        return (context, store);
    }

    private static IList<SetCookieHeaderValue> GetCookies(DefaultHttpContext context) =>
        SetCookieHeaderValue.ParseList(context.Response.Headers.SetCookie.Select(value => value!).ToArray());

    private static string GetRequestCookies(DefaultHttpContext context) =>
        string.Join("; ", GetCookies(context)
            .GroupBy(cookie => cookie.Name.ToString())
            .Select(group => group.Last())
            .Select(cookie => $"{cookie.Name}={cookie.Value}"));
}
