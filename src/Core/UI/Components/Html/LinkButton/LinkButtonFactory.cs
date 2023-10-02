using JJMasterData.Commons.Localization;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class LinkButtonFactory : IComponentFactory<JJLinkButton>
{
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
  
    
    public LinkButtonFactory(
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        StringLocalizer = stringLocalizer;
    }

    public JJLinkButton Create()
    {
        return new JJLinkButton(StringLocalizer);
    }
}