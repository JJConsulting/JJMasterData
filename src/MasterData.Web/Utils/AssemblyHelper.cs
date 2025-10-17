using System.Reflection;

namespace JJMasterData.Web.Utils;

public static class AssemblyHelper
{
    public static string GetAssemblyCopyright(Assembly assembly)
    {
        var attributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
        return attributes.Length == 0 ? string.Empty : ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
    }

    public static string GetAssemblyProduct(Assembly assembly)
    {
        var attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
        return attributes.Length == 0 ? string.Empty : ((AssemblyProductAttribute)attributes[0]).Product;
    }

    public static DateTime GetLastWriteTimeUtc(Assembly assembly)
    {
        var fileInfo = new FileInfo(assembly.Location);
        return fileInfo.LastWriteTimeUtc;
    }
}