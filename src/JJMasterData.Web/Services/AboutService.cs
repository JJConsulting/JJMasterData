using System.Reflection;
using System.Text;

namespace JJMasterData.Web.Services;

public class AboutService
{
    public List<Assembly> GetJJAssemblies() => AppDomain.CurrentDomain.GetAssemblies().Where(a =>
    {
        var name = a.GetName().Name;
        return name != null && name.Contains("JJ");
    }).ToList();

    public string GetAssemblyCopyright(Assembly assembly)
    {
        object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
        return attributes.Length == 0 ? string.Empty : ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
    }

    public string GetAssemblyProduct(Assembly assembly)
    {
        object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
        return attributes.Length == 0 ? string.Empty : ((AssemblyProductAttribute)attributes[0]).Product;
    }

}