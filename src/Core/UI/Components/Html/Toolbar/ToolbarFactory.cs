using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.Web.Components;

public class ToolbarFactory : IComponentFactory<JJToolbar>
{
    public JJToolbar Create()
    {
        return new JJToolbar();
    }
}