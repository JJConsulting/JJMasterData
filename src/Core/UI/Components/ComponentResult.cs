#nullable enable

namespace JJMasterData.Core.UI.Components;

public abstract class ComponentResult
{
    public abstract string? Content { get; }
    public static implicit operator string?(ComponentResult result) => result.Content;
}