#nullable enable
using System.Diagnostics.CodeAnalysis;

namespace JJMasterData.Core.UI.Components;

public class ComponentResult
{
    public enum ContentType
    {
        RenderedComponent,
        JsonData,
        HtmlData,
        Empty
    }
    public ContentType Type { get; }

    public virtual string? Content { get; }

    public static ComponentResult Empty => new(null, ContentType.Empty);

    protected ComponentResult(string? content, ContentType type)
    {
        Type = type;
        Content = content;
    }
    
    public static implicit operator string?(ComponentResult result) => result.Content;
    public static explicit operator ComponentResult(string content) => new(content, ContentType.RenderedComponent);
}