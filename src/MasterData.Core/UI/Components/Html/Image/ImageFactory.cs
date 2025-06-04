using System.Diagnostics.CodeAnalysis;

namespace JJMasterData.Core.UI.Components;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
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