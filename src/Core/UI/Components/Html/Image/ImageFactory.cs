namespace JJMasterData.Core.UI.Components;

public class ImageFactory : IComponentFactory<JJImage>
{
    JJImage IComponentFactory<JJImage>.Create()
    {
        return new JJImage(string.Empty);
    }
    
    public JJImage Create(string src)
    {
        return new JJImage(src);
    }
}