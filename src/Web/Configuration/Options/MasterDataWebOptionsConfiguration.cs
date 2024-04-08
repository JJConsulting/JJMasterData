using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Core.Configuration.Options;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Web.Configuration.Options;

public sealed class MasterDataWebOptionsConfiguration
{
    public Action<MasterDataWebOptions>? ConfigureWeb { get; set; }
    public Action<MasterDataCoreOptions>? ConfigureCore { get; set; } 
    public Action<MasterDataCommonsOptions>? ConfigureCommons { get; set; }
    public Func<Type,IStringLocalizerFactory,IStringLocalizer> ConfigureDataAnnotations { get; set; } = DefaultDataAnnotationsProviderImplementation;

    private static IStringLocalizer DefaultDataAnnotationsProviderImplementation(
        Type modelType, 
        IStringLocalizerFactory stringLocalizerFactory)
    {
        return stringLocalizerFactory.Create(modelType);
    }
}