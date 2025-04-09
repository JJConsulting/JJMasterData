using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Commons.Localization;

public sealed class MasterDataStringLocalizerFactory(
    ResourceManagerStringLocalizerFactory resourceManagerStringLocalizerFactory,
    IServiceScopeFactory serviceScopeFactory)
    : IStringLocalizerFactory
{
    public IStringLocalizer Create(Type resourceSource)
    {
        var resourceLocalizer = resourceManagerStringLocalizerFactory.Create(resourceSource);
        return new MasterDataStringLocalizer(resourceSource.ToString(), resourceLocalizer, serviceScopeFactory);
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        var resourceLocalizer = resourceManagerStringLocalizerFactory.Create(baseName, location);
        return new MasterDataStringLocalizer($"{baseName}_{location}", resourceLocalizer, serviceScopeFactory);
    }
}