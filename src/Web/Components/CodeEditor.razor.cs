using Microsoft.AspNetCore.Components;

namespace JJMasterData.Web.Components;

public partial class CodeEditor
{
    [Parameter] 
    public required string Name { get; set; }

    [Parameter] 
    public required string Language { get; set; }

    [Parameter]
    public string? Value { get; set; }
    
    [Parameter]
    public int Height { get; set; } = 500;
}