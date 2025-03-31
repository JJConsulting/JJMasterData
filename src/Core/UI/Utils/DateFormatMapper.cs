using System.Globalization;

namespace JJMasterData.Core.UI;

public static class DateFormatMapper
{
    public static string GetInputmask(CultureInfo cultureInfo, bool includeTime = false)
    {
        var dateFormat = cultureInfo.Name switch
        {
            "pt-BR" => "dd/MM/yyyy",
            "en-US" => "MM/dd/yyyy",
            "de-DE" => "dd.MM.yyyy",
            "es-ES" => "dd/MM/yyyy",
            "es-MX" => "dd/MM/yyyy",
            "es-AR" => "dd/MM/yyyy",
            "fr-FR" => "dd/MM/yyyy",
            "it-IT" => "dd/MM/yyyy",
            "ja-JP" => "yyyy/MM/dd",
            "zh-CN" => "yyyy-MM-dd",
            "ru-RU" => "dd.MM.yyyy",
            _ => "yyyy-MM-dd"
        };

        return includeTime ? $"{dateFormat} HH:MM" : dateFormat;
    }

    public static string GetFlatpickr(CultureInfo cultureInfo, bool includeTime = false)
    {
        var dateFormat = cultureInfo.Name switch
        {
            "pt-BR" => "d/m/Y",
            "en-US" => "m/d/Y",
            "de-DE" => "d.m.Y",
            "es-ES" => "d/m/Y",
            "es-MX" => "d/m/Y",
            "es-AR" => "d/m/Y",
            "fr-FR" => "d/m/Y",
            "it-IT" => "d/m/Y",
            "ja-JP" => "Y/m/d",
            "zh-CN" => "Y-m-d",
            "ru-RU" => "d.m.Y",
            _ => "Y-m-d"
        };

        return includeTime ? $"{dateFormat} H:i" : dateFormat;
    }
}