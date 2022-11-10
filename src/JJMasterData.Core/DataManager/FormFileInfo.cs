namespace JJMasterData.Core.DataManager;

public class FormFileInfo
{
    private FormFileContent _content;

    public FormFileContent Content 
    {
        get
        {
            if (_content == null)
                _content = new FormFileContent();

            return _content;
        }
        set
        {
            _content = value;
        }
    }

    public bool Deleted { get; set; }

    public string OriginName { get; set; }

    public bool IsInMemory => Content.FileStream != null && Content.FileStream.Length > 0;
}
