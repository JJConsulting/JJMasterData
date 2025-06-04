#nullable enable
using System.Collections.Generic;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Commons.Localization;

public sealed class MasterDataStringLocalizer<TResourceSource>(IStringLocalizerFactory factory)
    : IStringLocalizer<TResourceSource>
{
    private readonly IStringLocalizer _localizer = factory.Create(typeof(TResourceSource));

    public LocalizedString this[string? name] => _localizer[name!];

    public LocalizedString this[string? name, params object[] arguments] => _localizer[name!, arguments];

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
        _localizer.GetAllStrings(includeParentCultures);
}
