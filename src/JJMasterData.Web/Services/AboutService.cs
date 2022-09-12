using System.Reflection;
using System.Text;

namespace JJMasterData.Web.Services;

public class AboutService
{
    public List<Assembly>? GetJJAssemblies() => AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name.Contains("JJ")).ToList();
    
    public string GetAssemblyInfo()
    {
        var assemblyInfo = new StringBuilder();
        var executingAssembly = Assembly.GetExecutingAssembly();

        assemblyInfo.Append("<b>");
        assemblyInfo.Append(GetAssemblyProduct(executingAssembly));
        assemblyInfo.AppendLine("</b><br>");
        assemblyInfo.Append("Version: ");
        assemblyInfo.Append(executingAssembly.GetName().Version);
        assemblyInfo.AppendLine("<br>");
        assemblyInfo.Append(GetAssemblyCopyright(executingAssembly));
        assemblyInfo.AppendLine("<br>");
        assemblyInfo.AppendLine("All rights reserved.");

        return assemblyInfo.ToString();
    }

    private string GetAssemblyCopyright(Assembly a)
    {
        object[] attributes = a.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
        return attributes.Length == 0 ? string.Empty : ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
    }

    private string GetAssemblyProduct(Assembly a)
    {
        object[] attributes = a.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
        return attributes.Length == 0 ? string.Empty : ((AssemblyProductAttribute)attributes[0]).Product;
    }

}