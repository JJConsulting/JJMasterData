using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class LinkButtonGroupFactory(IStringLocalizer<MasterDataResources> stringLocalizer) : IComponentFactory<JJLinkButtonGroup>
{
    public JJLinkButtonGroup Create()
    {
        return new JJLinkButtonGroup(stringLocalizer);
    }
}