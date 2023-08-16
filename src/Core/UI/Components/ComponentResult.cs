#nullable enable
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

    public string? Content { get; }

    public bool IsRenderedComponent => Type is ContentType.RenderedComponent;

    public bool IsDataResult => !IsRenderedComponent && Type is not ContentType.Empty;

    public static ComponentResult Empty => new(null, ContentType.Empty);
    
    public ComponentResult(string? content, ContentType type)
    {
        Type = type;
        Content = content;
    }
    
    public static implicit operator string?(ComponentResult result) => result.Content;
    public static explicit operator ComponentResult(string content) => new(content, ContentType.RenderedComponent);
}