using JJMasterData.Commons.Localization;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class LinkButtonFactory : IComponentFactory<JJLinkButton>
{
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; }
  
    
    public LinkButtonFactory(
        IStringLocalizer<MasterDataResources> stringLocalizer)
    {
        StringLocalizer = stringLocalizer;
    }

    public JJLinkButton Create()
    {
        return new JJLinkButton(StringLocalizer);
    }
}