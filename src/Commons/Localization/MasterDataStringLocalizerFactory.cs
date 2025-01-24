using System;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Localization;

public sealed class MasterDataStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly ResourceManagerStringLocalizerFactory _resourceManagerStringLocalizerFactory;
    private readonly IEntityRepository _entityRepository;
    private readonly IOptionsMonitor<MasterDataCommonsOptions> _options;

    public MasterDataStringLocalizerFactory(
        ResourceManagerStringLocalizerFactory resourceManagerStringLocalizerFactory,
        IServiceProvider serviceProvider,
        IOptionsMonitor<MasterDataCommonsOptions> options)
    {
        using var scope = serviceProvider.CreateScope();
        _resourceManagerStringLocalizerFactory = resourceManagerStringLocalizerFactory;
        _entityRepository = scope.ServiceProvider.GetRequiredService<IEntityRepository>();
        _options = options;
    }

    public IStringLocalizer Create(Type resourceSource)
    {
        var resourceLocalizer = _resourceManagerStringLocalizerFactory.Create(resourceSource);
        return new MasterDataStringLocalizer(resourceSource.ToString(), resourceLocalizer, _entityRepository, _options);
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        var resourceLocalizer = _resourceManagerStringLocalizerFactory.Create(baseName, location);
        return new MasterDataStringLocalizer($"{baseName}_{location}", resourceLocalizer, _entityRepository, _options);
    }
}