using System.Runtime.CompilerServices;

namespace JJMasterData.Core.UI.Components;

public class LinkButtonFactory : IComponentFactory<JJLinkButton>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public JJLinkButton Create() => new();
}