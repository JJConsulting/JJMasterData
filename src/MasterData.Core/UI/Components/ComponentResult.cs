#nullable enable

namespace JJMasterData.Core.UI.Components;

public abstract class ComponentResult
{
    public int StatusCode { get; init; } = 200;
    public abstract string Content { get; }
    public static implicit operator string(ComponentResult result) => result.Content;
}