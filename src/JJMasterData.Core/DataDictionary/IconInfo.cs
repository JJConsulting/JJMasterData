namespace JJMasterData.Core.DataDictionary;

public record IconInfo(int Id, string Description, string Unicode, string ClassName)
{
    public int Id { get; } = Id;
    public IconType Icon { get; } = (IconType)Id;
    public string Description { get; } = Description;
    public string Unicode { get; } = Unicode;
    public string ClassName { get; } = ClassName;
}