

// ReSharper disable once CheckNamespace
namespace JJConsulting.FontAwesome;

public static class FontAwesomeExtensions
{
        
    //Feature 'extensions' is not available. Please use language version 14.0 or greater.
    public static int GetId(this FontAwesomeIcon icon)
    {
        return FontAwesomeIconExtensions.get_Id(icon);
    }
    
    //Feature 'extensions' is not available. Please use language version 14.0 or greater.
    public static string GetCssClass(this FontAwesomeIcon icon)
    {
        return FontAwesomeIconExtensions.get_CssClass(icon);
    }
}