using System;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Localization;

public class JJMasterDataStringLocalizerFactory : IStringLocalizerFactory
{
    private ResourceManagerStringLocalizerFactory ResourceManagerStringLocalizerFactory { get; }
    private IEntityRepository EntityRepository { get; }
    private IMemoryCache Cache { get; }
    private IOptions<JJMasterDataCommonsOptions> Options { get; }

    public JJMasterDataStringLocalizerFactory(
        ResourceManagerStringLocalizerFactory resourceManagerStringLocalizerFactory,
        IEntityRepository entityRepository,
        IMemoryCache cache,
        IOptions<JJMasterDataCommonsOptions> options)
    {
        ResourceManagerStringLocalizerFactory = resourceManagerStringLocalizerFactory;
        EntityRepository = entityRepository;
        Cache = cache;
        Options = options;
    }

    public IStringLocalizer Create(Type resourceSource)
    {
        var resourceLocalizer = ResourceManagerStringLocalizerFactory.Create(resourceSource) as ResourceManagerStringLocalizer;
        return new JJMasterDataStringLocalizer(resourceSource.ToString(), resourceLocalizer, EntityRepository, Cache, Options);
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        var resourceLocalizer = ResourceManagerStringLocalizerFactory.Create(baseName,location) as ResourceManagerStringLocalizer;
        return new JJMasterDataStringLocalizer($"{baseName}_{location}", resourceLocalizer, EntityRepository, Cache, Options);
    }
}