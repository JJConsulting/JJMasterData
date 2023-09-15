using JJMasterData.Core.UI.Components;
namespace JJMasterData.Core.Web.Components;

public class SpinnerFactory : IComponentFactory<JJSpinner>
{
    public JJSpinner Create()
    {
        return new JJSpinner();
    }
}