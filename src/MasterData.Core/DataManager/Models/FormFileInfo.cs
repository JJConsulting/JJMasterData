#nullable enable

using JJMasterData.Core.DataManager.IO;

namespace JJMasterData.Core.DataManager.Models;

public class FormFileInfo
{
    public FormFileInfo()
    {
        
    }
    
    public required FormFileContent Content { get; init; }
    
    public bool IsRenamed { get; set; }
    public bool Deleted { get; set; }

    public string? OldName { get; set; }

    public string FileName =>  Content.FileName ?? OldName!;
    
    public bool IsTemporary { get; set; }
}
