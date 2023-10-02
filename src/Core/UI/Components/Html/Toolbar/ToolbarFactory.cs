namespace JJMasterData.Core.UI.Components;

public class ToolbarFactory : IComponentFactory<JJToolbar>
{
    public JJToolbar Create()
    {
        return new JJToolbar();
    }
}