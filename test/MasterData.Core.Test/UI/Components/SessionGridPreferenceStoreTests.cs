using JJMasterData.Core.Extensions;
using System.Diagnostics.CodeAnalysis;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Http;
using Moq;

namespace JJMasterData.Core.Test.UI.Components;

public class SessionGridPreferenceStoreTests
{
    [Fact]
    public void RepeatedReads_ReuseDeserializedPreferencesWithinTheRequest()
    {
        var session = new MemorySession();
        var (_, writer) = CreateStore(session);
        writer.Set("Customers", new GridPreferences { CurrentPage = 3 });
        var (_, reader) = CreateStore(session);

        var preferences = reader.Get("Customers");

        Assert.NotNull(preferences);
        Assert.Equal(3, preferences.CurrentPage);
        Assert.Same(preferences, reader.Get("CUSTOMERS"));
        reader.Set("Customers", new GridPreferences { CurrentPage = 4 });
        var updated = reader.Get("Customers");
        Assert.NotSame(preferences, updated);
        Assert.Same(updated, reader.Get("CUSTOMERS"));
        Assert.Equal(4, updated!.CurrentPage);
    }

    [Fact]
    public void DirectSessionChanges_AppearOnNextRequest()
    {
        var session = new MemorySession();
        var (_, store) = CreateStore(session);
        store.Set("Customers", new GridPreferences { CurrentPage = 3 });
        var original = store.Get("Customers");

        session.SetString(Assert.Single(session.Keys), "{\"CurrentPage\":4}");

        Assert.Same(original, store.Get("Customers"));
        var (_, next) = CreateStore(session);
        var updated = next.Get("Customers");
        Assert.NotNull(updated);
        Assert.NotSame(original, updated);
        Assert.Equal(4, updated.CurrentPage);
        Assert.Same(updated, next.Get("CUSTOMERS"));
    }

    [Fact]
    public void Preferences_RoundTripAcrossRequestsInTheSameSession()
    {
        var session = new MemorySession();
        var (context, store) = CreateStore(session);
        store.SetSettings("Customers", new GridSettings { RecordsPerPage = 25, IsCompact = false });
        store.SetFilter("Customers", new() { ["Name"] = "Alice", ["Count"] = 3 });
        store.SetOrder("Customers", "Name asc");
        store.SetCurrentPage("Customers", 3);

        var (_, next) = CreateStore(session);
        var settings = next.GetSettings("CUSTOMERS");
        Assert.NotNull(settings);
        Assert.Equal(25, settings.RecordsPerPage);
        Assert.False(settings.IsCompact);
        Assert.Equal("Alice", next.GetFilter("CUSTOMERS")!["Name"]);
        Assert.Equal("3", next.GetFilter("CUSTOMERS")!["Count"]?.ToString());
        Assert.Equal("Name asc", next.GetOrder("CUSTOMERS"));
        Assert.Equal(3, next.GetCurrentPage("CUSTOMERS"));
        Assert.Equal(0, context.Response.Headers.SetCookie.Count);
        Assert.Single(session.Keys);
    }

    [Fact]
    public void NewSession_DoesNotRestorePreviousPreferences()
    {
        var (_, store) = CreateStore(new MemorySession());
        SavePreferences(store);
        var (_, next) = CreateStore(new MemorySession());
        AssertNoPreferences(next);
    }

    [Fact]
    public void ClearingSession_DiscardsPreferences()
    {
        var session = new MemorySession();
        var (_, store) = CreateStore(session);
        SavePreferences(store);
        session.Clear();
        var (_, next) = CreateStore(session);
        AssertNoPreferences(next);
    }

    [Fact]
    public void Preferences_AreIsolatedByGrid()
    {
        var session = new MemorySession();
        var (_, store) = CreateStore(session);
        SavePreferences(store);
        var (_, other) = CreateStore(session);
        AssertNoPreferences(other, "Orders");
    }

    [Fact]
    public void WritesAndRemovals_AreVisibleWithinTheRequest()
    {
        var session = new MemorySession();
        var (_, store) = CreateStore(session);
        SavePreferences(store);
        store.SetSettings("Customers", new GridSettings { RecordsPerPage = 50 });
        Assert.Equal(50, store.GetSettings("Customers")!.RecordsPerPage);
        store.SetSettings("Customers", null);
        store.SetFilter("Customers", null);
        store.SetOrder("Customers", null);
        store.SetCurrentPage("Customers", null);
        AssertNoPreferences(store);
        Assert.Empty(session.Keys);
    }

    [Fact]
    public void EmptyNavigationValues_RemoveSavedPreferences()
    {
        var session = new MemorySession();
        var (_, store) = CreateStore(session);
        SavePreferences(store);
        store.SetFilter("Customers", new());
        store.SetOrder("Customers", "");
        store.SetCurrentPage("Customers", 0);
        Assert.Null(store.GetFilter("Customers"));
        Assert.Null(store.GetOrder("Customers"));
        Assert.Null(store.GetCurrentPage("Customers"));
        Assert.Single(session.Keys);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void MissingUser_DoesNotReadOrWriteSession(string? userId)
    {
        var session = new MemorySession();
        var (_, store) = CreateStore(session, userId);
        SavePreferences(store);
        AssertNoPreferences(store);
        Assert.Empty(session.Keys);
    }

    [Fact]
    public void MissingHttpContext_DoesNotReadOrWriteSession()
    {
        var user = new Mock<IMasterDataUser>();
        user.SetupGet(u => u.Id).Returns("user-1");
        var store = new SessionGridPreferenceStore(new HttpContextAccessor { HttpContext = null }, user.Object);
        SavePreferences(store);
        AssertNoPreferences(store);
    }

    [Fact]
    public void CorruptSessionValues_AreIgnored()
    {
        var session = new MemorySession();
        var (_, store) = CreateStore(session);
        SavePreferences(store);
        foreach (var key in session.Keys.ToArray())
            session.SetString(key, "not-json");
        var (_, next) = CreateStore(session);
        AssertNoPreferences(next);
    }

    private static void SavePreferences(SessionGridPreferenceStore store)
    {
        store.SetSettings("Customers", new GridSettings());
        store.SetFilter("Customers", new() { ["Name"] = "Alice" });
        store.SetOrder("Customers", "Name asc");
        store.SetCurrentPage("Customers", 3);
    }

    private static void AssertNoPreferences(SessionGridPreferenceStore store, string gridName = "Customers")
    {
        Assert.Null(store.GetSettings(gridName));
        Assert.Null(store.GetFilter(gridName));
        Assert.Null(store.GetOrder(gridName));
        Assert.Null(store.GetCurrentPage(gridName));
    }

    private static (DefaultHttpContext Context, SessionGridPreferenceStore Store) CreateStore(ISession session, string? userId = "user-1")
    {
        var context = new DefaultHttpContext { Session = session };
        var user = new Mock<IMasterDataUser>();
        user.SetupGet(u => u.Id).Returns(userId!);
        return (context, new SessionGridPreferenceStore(new HttpContextAccessor { HttpContext = context }, user.Object));
    }

    private sealed class MemorySession : ISession
    {
        private readonly Dictionary<string, byte[]> _values = new();
        public bool IsAvailable => true;
        public string Id { get; } = Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _values.Keys;
        public void Clear() => _values.Clear();
        public void Remove(string key) => _values.Remove(key);
        public void Set(string key, byte[] value) => _values[key] = value;
        public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value) => _values.TryGetValue(key, out value);
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
