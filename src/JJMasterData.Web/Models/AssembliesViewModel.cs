using System.Reflection;

namespace JJMasterData.Web.Models;

public class AboutViewModel
{
    public string AssemblyInfoHtml;
    public List<Assembly> Dependencies;
    public string BootstrapVersion;
}