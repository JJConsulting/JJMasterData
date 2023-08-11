namespace JJMasterData.Core.DataManager;

public class FormFileInfo
{
    private FormFileContent _content;

    public FormFileContent Content
    {
        get => _content ??= new FormFileContent();
        set => _content = value;
    }

    public bool Deleted { get; set; }

    public string OriginName { get; set; }

    public string FileName => OriginName ?? Content.FileName;
    
    public bool IsInMemory => Content.Bytes != null;
}
