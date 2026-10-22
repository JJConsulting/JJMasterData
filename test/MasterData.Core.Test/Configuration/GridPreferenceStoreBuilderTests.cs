using JJMasterData.Core.Configuration;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace JJMasterData.Core.Test.Configuration;

public class GridPreferenceStoreBuilderTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DefaultStore_UsesAvailableSessionRegardlessOfRegistrationOrder(bool registerSessionFirst)
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IMasterDataUser>());
        services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { Session = Mock.Of<ISession>() }
        });
        if (registerSessionFirst)
            services.AddSession();
        services.AddJJMasterDataCore();
        if (!registerSessionFirst)
            services.AddSession();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IGridPreferenceStore>();
        Assert.IsType<SessionGridPreferenceStore>(store);
        Assert.Same(store, scope.ServiceProvider.GetRequiredService<IGridPreferenceStore>());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DefaultStore_UsesCookiesWithoutAnAvailableSession(bool registerSession)
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IMasterDataUser>());
        services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = new DefaultHttpContext() });
        services.AddJJMasterDataCore();
        if (registerSession)
            services.AddSession();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.IsType<CookieGridPreferenceStore>(scope.ServiceProvider.GetRequiredService<IGridPreferenceStore>());
    }

    [Fact]
    public void ExplicitCookieStore_OverridesAutomaticSessionSelection()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IMasterDataUser>());
        services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { Session = Mock.Of<ISession>() }
        });
        services.AddJJMasterDataCore().WithGridPreferenceStore<CookieGridPreferenceStore>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.IsType<CookieGridPreferenceStore>(scope.ServiceProvider.GetRequiredService<IGridPreferenceStore>());
    }

    [Fact]
    public void WithGridSessionPreferenceStore_ReplacesDefaultAndReturnsBuilder()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IMasterDataUser>());
        var builder = services.AddJJMasterDataCore();

        Assert.Same(builder, builder.WithGridSessionPreferenceStore());
        var descriptor = Assert.Single(services, s => s.ServiceType == typeof(IGridPreferenceStore));
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.IsType<SessionGridPreferenceStore>(scope.ServiceProvider.GetRequiredService<IGridPreferenceStore>());
    }

    [Fact]
    public void WithGridPreferenceStore_UsesTypeAndRespectsRequestScopes()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IMasterDataUser>());
        services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor());
        var builder = services.AddJJMasterDataCore();
        Assert.Same(builder, builder.WithGridPreferenceStore<SessionGridPreferenceStore>());

        using var provider = services.BuildServiceProvider();
        using var first = provider.CreateScope();
        using var second = provider.CreateScope();
        var firstStore = first.ServiceProvider.GetRequiredService<IGridPreferenceStore>();
        Assert.IsType<SessionGridPreferenceStore>(firstStore);
        Assert.Same(firstStore, first.ServiceProvider.GetRequiredService<IGridPreferenceStore>());
        Assert.NotSame(firstStore, second.ServiceProvider.GetRequiredService<IGridPreferenceStore>());
    }

    [Fact]
    public void WithGridPreferenceStore_FactoryRunsOncePerScopeAndLastSelectionWins()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { Session = Mock.Of<ISession>() }
        });
        services.AddScoped(_ => Mock.Of<IGridPreferenceStore>());
        var builder = services.AddJJMasterDataCore();
        builder.WithGridSessionPreferenceStore();
        var expected = Mock.Of<IGridPreferenceStore>();
        var calls = 0;
        Assert.Same(builder, builder.WithGridPreferenceStore(_ =>
        {
            calls++;
            return expected;
        }));

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.Same(expected, scope.ServiceProvider.GetRequiredService<IGridPreferenceStore>());
        Assert.Same(expected, scope.ServiceProvider.GetRequiredService<IGridPreferenceStore>());
        Assert.Equal(1, calls);
        using var second = provider.CreateScope();
        Assert.Same(expected, second.ServiceProvider.GetRequiredService<IGridPreferenceStore>());
        Assert.Equal(2, calls);
        Assert.Single(services, s => s.ServiceType == typeof(IGridPreferenceStore));
    }
}
