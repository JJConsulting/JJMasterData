using System;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Localization;

public sealed class MasterDataStringLocalizerFactory : IStringLocalizerFactory
{
    private ResourceManagerStringLocalizerFactory ResourceManagerStringLocalizerFactory { get; }
    private IEntityRepository EntityRepository { get; }
    private IMemoryCache Cache { get; }
    private IOptionsMonitor<MasterDataCommonsOptions> Options { get; }

    public MasterDataStringLocalizerFactory(
        ResourceManagerStringLocalizerFactory resourceManagerStringLocalizerFactory,
        IMemoryCache cache,
        IServiceProvider serviceProvider,
        IOptionsMonitor<MasterDataCommonsOptions> options)
    {
        using var scope = serviceProvider.CreateScope();
        ResourceManagerStringLocalizerFactory = resourceManagerStringLocalizerFactory;
        EntityRepository = scope.ServiceProvider.GetRequiredService<IEntityRepository>();
        Cache = cache;
        Options = options;
    }

    public IStringLocalizer Create(Type resourceSource)
    {
        var resourceLocalizer = (ResourceManagerStringLocalizer)ResourceManagerStringLocalizerFactory.Create(resourceSource);
        return new MasterDataStringLocalizer(resourceSource.ToString(), resourceLocalizer, EntityRepository, Cache, Options);
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        var resourceLocalizer = (ResourceManagerStringLocalizer)ResourceManagerStringLocalizerFactory.Create(baseName,location);
        return new MasterDataStringLocalizer($"{baseName}_{location}", resourceLocalizer, EntityRepository, Cache, Options);
    }
}