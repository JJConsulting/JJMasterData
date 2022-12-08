namespace JJMasterData.Core.DataDictionary;

public record IconInfo(IconType Icon, string Description, string Unicode, string ClassName)
{
    public int Id { get; } = (int)Icon;
    public IconType Icon { get; } = Icon;
    public string Description { get; } = Description;
    public string Unicode { get; } = Unicode;
    public string ClassName { get; } = ClassName;
}