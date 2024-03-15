using JJMasterData.Core.UI;

namespace JJMasterData.Core.DataDictionary.Models;

/// <summary>
/// Cor do painel de agrupamento de dados
/// </summary>
public enum BootstrapColor
{
    Default = 0, //At Bootstrap 5+ we will use Secondary depending on the situation
    Primary = 1,
    Success = 2,
    Info = 3,
    Warning = 4,
    Danger = 5,
    //The colors below only works for Bootstrap 5+
    Light = 6,
    Dark = 7,
    Secondary = 8
}

public static class ColorExtensions
{
    public static string ToButtonColorString(this BootstrapColor color)
    {
        if (BootstrapHelper.Version >= 4 && color == BootstrapColor.Default)
            return BootstrapColor.Secondary.ToString().ToLower();
        
        return color.ToString().ToLower();
    }
    
    public static string ToLinkColorString(this BootstrapColor color)
    {
        if (BootstrapHelper.Version >= 4 && color == BootstrapColor.Default)
            return BootstrapColor.Default.ToString().ToLower();
        
        return color.ToString().ToLower();
    }
}