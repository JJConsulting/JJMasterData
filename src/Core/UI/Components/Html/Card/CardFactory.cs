namespace JJMasterData.Core.UI.Components;

public class CardFactory : IComponentFactory<JJCard>
{
    public JJCard Create()
    {
        return new JJCard();
    }
}