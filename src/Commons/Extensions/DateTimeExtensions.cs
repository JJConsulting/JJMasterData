using System;

namespace JJMasterData.Commons.Extensions;

public static class DateTimeExtensions
{
    /// <summary>
    /// Converts a DateTime to a human-friendly l10n string.
    /// <br/>
    /// Example:
    /// <br/>
    /// pt-BR: 10/08/2022 23:42:24
    /// <br/>
    /// en-US: 08/10/2022 11:42:24 PM
    /// </summary>
    public static string ToDateTimeString(this DateTime dateTime)
    {
        return $"{dateTime.ToShortDateString()} {dateTime.ToLongTimeString()}";
    }
}