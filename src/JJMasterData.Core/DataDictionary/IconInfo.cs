namespace JJMasterData.Core.DataDictionary;

public class IconInfo
{
    public IconType Icon { get; set; }
    public string Description { get; set; }
    public string Unicode { get; set; }
    public string ClassName { get; set; }
    public int Id
    {
        get
        {
            return (int)Icon;
        }
    }


    public IconInfo() { }

    public IconInfo(int iconId, string description, string unicode, string className)
    {
        Icon = (IconType)iconId;
        Description = description;
        Unicode = unicode;
        ClassName = className;
    }

}