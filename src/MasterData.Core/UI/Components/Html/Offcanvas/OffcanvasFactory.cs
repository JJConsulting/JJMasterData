namespace JJMasterData.Core.UI.Components;

public class OffcanvasFactory : IComponentFactory<JJOffcanvas>
{
    public JJOffcanvas Create() => new();
}