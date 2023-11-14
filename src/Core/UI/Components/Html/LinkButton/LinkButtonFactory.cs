using JJMasterData.Commons.Localization;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class LinkButtonFactory(IStringLocalizer<MasterDataResources> stringLocalizer) : IComponentFactory<JJLinkButton>
{
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;


    public JJLinkButton Create()
    {
        return new JJLinkButton(StringLocalizer);
    }
}