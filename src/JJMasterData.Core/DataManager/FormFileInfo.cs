namespace JJMasterData.Core.DataManager;

public class FormFileInfo
{
    public FormFileContent Content {get; set;} = new();

    public bool Deleted { get; set; }

    public string OriginName { get; set; }

    public bool IsInMemory => Content.Bytes != null;
}
