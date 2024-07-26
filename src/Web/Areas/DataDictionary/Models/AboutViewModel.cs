namespace JJMasterData.Web.Areas.DataDictionary.Models;

public sealed class AboutViewModel
{
    public required string ExecutingAssemblyProduct { get; set; }
    public required string ExecutingAssemblyVersion { get; set; }
    public required string ExecutingAssemblyCopyright { get; set; }
    public required DateTime ExecutingAssemblyLastWriteTime { get; set; }
}