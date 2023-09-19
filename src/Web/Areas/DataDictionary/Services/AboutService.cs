using System.Reflection;

namespace JJMasterData.Web.Services;

public static class AboutService
{
    public static List<Assembly> GetJJAssemblies() => AppDomain.CurrentDomain.GetAssemblies().Where(a =>
    {
        var name = a.GetName().Name;
        return name != null && name.Contains("JJ");
    }).ToList();

    public static string GetAssemblyCopyright(Assembly assembly)
    {
        object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
        return attributes.Length == 0 ? string.Empty : ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
    }

    public static string GetAssemblyProduct(Assembly assembly)
    {
        object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
        return attributes.Length == 0 ? string.Empty : ((AssemblyProductAttribute)attributes[0]).Product;
    }
    
    public static DateTime GetAssemblyDate(Assembly assembly)
    {
        var fileInfo = new FileInfo(assembly.Location);
        return fileInfo.LastWriteTime;
    }
    

}