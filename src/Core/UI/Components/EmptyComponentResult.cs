namespace JJMasterData.Core.UI.Components;

public class EmptyComponentResult : ComponentResult
{
    public static readonly EmptyComponentResult Value = new();
    private EmptyComponentResult()
    {
    }
    public override string Content => string.Empty;
}