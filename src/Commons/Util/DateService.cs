using System;
using JJMasterData.Commons.Localization;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Commons.Util;

public class DateService(IStringLocalizer<MasterDataResources> localizer)
{
    public string GetPhrase(DateTime date)
    {
        var timeDifference = DateTime.Now - date;

        if (date == DateTime.MinValue)
        {
            return localizer["Never"];
        }

        if (timeDifference.TotalSeconds < 60)
        {
            return localizer["Just now"];
        }

        if (timeDifference.TotalMinutes < 60)
        {
            int minutes = (int)timeDifference.TotalMinutes;
            return minutes == 1
                ? localizer["1 minute ago"]
                : localizer["{0} minutes ago", minutes];
        }
        if (timeDifference.TotalHours < 24)
        {
            int hours = (int)timeDifference.TotalHours;
            return hours == 1
                ? localizer["1 hour ago"]
                : localizer["{0} hours ago", hours];
        }
        if (timeDifference.TotalDays < 30)
        {
            int days = (int)timeDifference.TotalDays;
            return days == 1
                ? localizer["1 day ago"]
                : localizer["{0} days ago", days];
        }
        if (timeDifference.TotalDays < 365)
        {
            int months = (int)(timeDifference.TotalDays / 30);
            int remainingDays = (int)(timeDifference.TotalDays % 30);
            if (remainingDays == 1)
            {
                return months == 0
                    ? localizer["1 day ago"]
                    : localizer["{0} months and 1 day ago", months];
            }

            return months == 0
                ? localizer["{0} days ago", remainingDays]
                : localizer["{0} months and {1} days ago", months, remainingDays];
        }
        int years = (int)(timeDifference.TotalDays / 365);
        return years == 1
            ? localizer["1 year ago"]
            : localizer["{0} years ago", years];
    }
}