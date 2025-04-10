namespace JJMasterData.Web.Models;

public sealed class CodeEditorHint
{
    public required string Language { get; init; }
    
    /// <summary>
    /// Text to be inserted when the hint is accepted.
    /// </summary>
    public required string InsertText { get; init; }
    
    /// <summary>
    /// The hint label.
    /// </summary>
    public required string Label { get; init; }
    
    /// <summary>
    /// Details about the hint.
    /// </summary>
    public required string Details { get; init; }
}