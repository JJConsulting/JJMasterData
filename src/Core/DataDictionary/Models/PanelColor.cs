using JJMasterData.Core.UI;

namespace JJMasterData.Core.DataDictionary.Models;

/// <summary>
/// Cor do painel de agrupamento de dados
/// </summary>
public enum PanelColor
{
    Default = 0,
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
    public static string ToButtonColorString(this PanelColor color)
    {
        if (BootstrapHelper.Version >= 4 && color == PanelColor.Default)
            return PanelColor.Light.ToString().ToLower();
        
        return color.ToString().ToLower();
    }
    
    public static string ToLinkColorString(this PanelColor color)
    {
        if (BootstrapHelper.Version >= 4 && color == PanelColor.Default)
            return PanelColor.Secondary.ToString().ToLower();
        
        return color.ToString().ToLower();
    }
}