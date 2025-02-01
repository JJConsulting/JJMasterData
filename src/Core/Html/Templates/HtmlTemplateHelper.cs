#nullable enable

using System;
using System.Linq;
using Fluid;
using Fluid.Values;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Html.Templates;

public class HtmlTemplateHelper(
    IStringLocalizer<MasterDataResources> stringLocalizer,
    DateService dateService,
    IHttpContext httpContext)
{
    public FilterDelegate GetLocalizeFilter()
    {
        return (input, args, _) =>
        {
            var inputString = input.ToStringValue();
            var argsValues = args.Values;

            string localizedString;

            if (argsValues is not null)
            {
                var localizerArgs = argsValues.Select(v => v.ToStringValue()).ToArray();
                localizedString = stringLocalizer[inputString, localizerArgs.ToArray()].Value;
            }
            else
            {
                localizedString = stringLocalizer[inputString];
            }
            return StringValue.Create(localizedString);
        };
    }
    
    public FunctionValue GetDatePhraseFunction()
    {
        var localize = new FunctionValue((args, _) =>
        {
            var dateArg = args.At(0).ToStringValue();
            
            if (DateTime.TryParse(dateArg, out var date))
            {
                var phrase = dateService.GetPhrase(date);

                return StringValue.Create(phrase);
            }

            return StringValue.Empty;
        });
        return localize;
    }

    public FunctionValue GetLocalizeFunction()
    {
        var localize = new FunctionValue((args, _) =>
        {
            var firstArg = args.At(0).ToStringValue();
            var localizerArgs = args.Values.Skip(1).Select(v => v.ToStringValue()).ToArray();

            var localizedString = stringLocalizer[firstArg, localizerArgs.ToArray()];

            return StringValue.Create(localizedString);
        });
        return localize;
    }

    public FunctionValue GetUrlPathFunction()
    {
        var urlAction = new FunctionValue((_, _) => StringValue.Create(httpContext.Request.ApplicationPath));

        return urlAction;
    }
    
    public FunctionValue GetAppUrlFunction()
    {
        var urlAction = new FunctionValue((_, _) => StringValue.Create(httpContext.Request.ApplicationUri));

        return urlAction;
    }
    
    public static FunctionValue FormatDate { get; } = new((args, _) =>
    {
        var obj = args.At(0).ToStringValue();
        var format = args.At(1).ToStringValue();

        return StringValue.Create(DateTime.TryParse(obj, out var dt) ? dt.ToString(format) : obj);
    });
    
    public static FunctionValue Trim { get; } = new((args, _) =>
    {
        var str = args.At(0).ToStringValue();

        return StringValue.Create(str.Trim());
    });
    
    public static FunctionValue TrimStart { get; } = new((args, _) =>
    {
        var str = args.At(0).ToStringValue();

        return StringValue.Create(str.TrimStart());
    });
    
    public static FunctionValue TrimEnd { get; } = new((args, _) =>
    {
        var str = args.At(0).ToStringValue();

        return StringValue.Create(str.TrimEnd());
    });
    
    public static FunctionValue IsNullOrEmpty { get; } = new((args, _) =>
    {
        var str = args.At(0).ToStringValue();
        return BooleanValue.Create(string.IsNullOrEmpty(str));
    });

    public static FunctionValue IsNullOrWhiteSpace { get; } = new((args, _) =>
    {
        var str = args.At(0).ToStringValue();
        return BooleanValue.Create(string.IsNullOrWhiteSpace(str));
    });

    public static FunctionValue Substring { get; } = new((args, _) =>
    {
        if (args.Count < 2)
        {
            return new StringValue("Error: Not enough arguments");
        }

        var str = args.At(0).ToObjectValue().ToString()!;
        if (!int.TryParse(args.At(1).ToStringValue(), out var startIndex))
        {
            return new StringValue("Error: Invalid start index");
        }

        int length = 0;

        if (args.Count > 2 && !int.TryParse(args.At(2).ToStringValue(), out length))
        {
            return new StringValue("Error: Invalid length");
        }

        var substring = args.Count > 2
            ? str.Substring(startIndex, length)
            : str[startIndex..];

        return new StringValue(substring);
    });
}