namespace JJMasterData.Core.UI.Components;

public class SpinnerFactory : IComponentFactory<JJSpinner>
{
    public JJSpinner Create()
    {
        return new JJSpinner();
    }
}