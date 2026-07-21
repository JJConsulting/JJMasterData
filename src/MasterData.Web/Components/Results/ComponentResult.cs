namespace JJMasterData.Web.Components;

public abstract class ComponentResult
{
    public int StatusCode { get; init; } = 200;
    public abstract string Content { get; }
}