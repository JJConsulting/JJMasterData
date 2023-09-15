using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.Web.Components;

public class AlertFactory : IComponentFactory<JJAlert>
{
    public JJAlert Create()
    {
        return new JJAlert();
    }
}