using System.Reflection;

namespace JJMasterData.Web.Models;

public class AboutViewModel
{
    public string? AssemblyInfoHtml { get; set; }
    public List<Assembly>? Dependencies { get; set; }
    public string? BootstrapVersion { get; set; }
    public string? ExecutingAssemblyProduct { get; set; }
    public string? ExecutingAssemblyVersion { get; set; }
    
    public string? ExecutingAssemblyCopyright { get; set; }
}