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

    public static ComponentResult Empty => new(ContentType.Empty);

    internal ComponentResult(string content, ContentType type)
    {
        Content = content;
        Type = type;
    }
    
    protected ComponentResult(ContentType type)
    {
        Type = type;
    }
    
    public static implicit operator string?(ComponentResult result) => result.Content;
}