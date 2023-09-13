namespace JJMasterData.Core.DataManager;

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

    public bool Deleted { get; set; }

    public string OldName { get; set; }

    public string FileName => OldName ?? Content.FileName;
    
    public bool IsInMemory => Content.Bytes != null;
}
