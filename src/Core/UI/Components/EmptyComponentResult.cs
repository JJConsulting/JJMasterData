namespace JJMasterData.Core.UI.Components;

public sealed class EmptyComponentResult : ComponentResult
{
    public static readonly EmptyComponentResult Value = new();
    private EmptyComponentResult()
    {
    }
    public override string Content => string.Empty;
}