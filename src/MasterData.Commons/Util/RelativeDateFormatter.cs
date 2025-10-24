using System;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Commons.Util;

public sealed class RelativeDateFormatter(IStringLocalizer<MasterDataResources> localizer)
{
    public string ToRelativeString(DateTime date)
    {
        var timeDifference = date.Kind == DateTimeKind.Utc ? 
            DateTime.UtcNow - date : DateTime.Now - date;

        if (date == DateTime.MinValue)
            return localizer["Never"];

        if (timeDifference.TotalSeconds < 60)
            return localizer["Just now"];

        if (timeDifference.TotalMinutes < 60)
        {
            var minutes = (int)timeDifference.TotalMinutes;
            return minutes == 1
                ? localizer["1 minute ago"]
                : localizer["{0} minutes ago", minutes];
        }
        if (timeDifference.TotalHours < 24)
        {
            var hours = (int)timeDifference.TotalHours;
            return hours == 1
                ? localizer["1 hour ago"]
                : localizer["{0} hours ago", hours];
        }
        if (timeDifference.TotalDays < 30)
        {
            var days = (int)timeDifference.TotalDays;
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
                if (months == 0)
                    return localizer["1 day ago"];
                
                if (months == 1)
                    return localizer["A month and 1 day ago"];

                return localizer["{0} months and 1 day ago", months];
            }

            if (months == 0)
                return localizer["{0} days ago", remainingDays];
            
            if (months == 1 && remainingDays == 0)
                return localizer["A month ago"];

            if (months == 1)
                return localizer["A month ago and {0} days ago", remainingDays];
            
            if(remainingDays == 0)
                return localizer["{0} months ago", months];
            
            return localizer["{0} months and {1} days ago", months, remainingDays];
        }
        
        var years = (int)(timeDifference.TotalDays / 365);
        
        return years == 1
            ? localizer["1 year ago"]
            : localizer["{0} years ago", years];
    }
}