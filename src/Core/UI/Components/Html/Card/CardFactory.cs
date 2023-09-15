using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.Web.Components;

public class CardFactory : IComponentFactory<JJCard>
{
    public JJCard Create()
    {
        return new JJCard();
    }
}