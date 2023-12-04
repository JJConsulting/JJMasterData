using System.Reflection;

namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

public class AboutViewModel
{
    public required List<Assembly> Dependencies { get; set; }
    public required string ExecutingAssemblyProduct { get; set; }
    public required string ExecutingAssemblyVersion { get; set; }
    public required string ExecutingAssemblyCopyright { get; set; }
    public required DateTime ExecutingAssemblyLastWriteTime { get; set; }
}