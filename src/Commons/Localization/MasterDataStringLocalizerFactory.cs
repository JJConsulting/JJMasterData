using System;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Providers;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Localization;

public class MasterDataStringLocalizerFactory : IStringLocalizerFactory, IDisposable
{
    private ResourceManagerStringLocalizerFactory ResourceManagerStringLocalizerFactory { get; }
    private IEntityRepository EntityRepository { get; }
    private IMemoryCache Cache { get; }
    private IOptions<MasterDataCommonsOptions> Options { get; }
    public MasterDataStringLocalizerFactory(
        ResourceManagerStringLocalizerFactory resourceManagerStringLocalizerFactory,
        IMemoryCache cache,
        IServiceProvider serviceProvider,
        IOptions<MasterDataCommonsOptions> options)
    {
        using var scope = serviceProvider.CreateScope();
        ResourceManagerStringLocalizerFactory = resourceManagerStringLocalizerFactory;
        EntityRepository = scope.ServiceProvider.GetRequiredService<IEntityRepository>();
        Cache = cache;
        Options = options;
    }

    public IStringLocalizer Create(Type resourceSource)
    {
        var resourceLocalizer = ResourceManagerStringLocalizerFactory.Create(resourceSource) as ResourceManagerStringLocalizer;
        return new MasterDataStringLocalizer(resourceSource.ToString(), resourceLocalizer, EntityRepository, Cache, Options);
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        var resourceLocalizer = ResourceManagerStringLocalizerFactory.Create(baseName,location) as ResourceManagerStringLocalizer;
        return new MasterDataStringLocalizer($"{baseName}_{location}", resourceLocalizer, EntityRepository, Cache, Options);
    }

    public void Dispose()
    {
        Cache?.Dispose();
    }
}