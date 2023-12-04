namespace JJMasterData.Core.UI.Components;

public class AlertFactory : IComponentFactory<JJAlert>
{
    public JJAlert Create()
    {
        return new JJAlert();
    }
}