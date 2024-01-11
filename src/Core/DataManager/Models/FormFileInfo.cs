using JJMasterData.Core.DataManager.IO;

namespace JJMasterData.Core.DataManager.Models;

public class FormFileInfo
{
    private FormFileContent _content;

    public FormFileContent Content
    {
        get => _content ??= new FormFileContent();
        set => _content = value;
    }

    public FormFileInfo()
    {
        
    }
    public bool IsRenamed { get; set; }
    public bool Deleted { get; set; }

    public string OldName { get; set; }

    public string FileName =>  Content.FileName ?? OldName;
    
    public bool IsInMemory => Content.Bytes != null;
}
