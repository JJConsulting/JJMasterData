#nullable enable

using System;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Html;

public static class HtmlTemplateHelper
{
    public static FilterDelegate GetLocalizeFilter(IStringLocalizer stringLocalizer)
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
            return new ValueTask<FluidValue>(new StringValue(localizedString));
        };
    }
    
    public static FunctionValue GetLocalizeFunction(IStringLocalizer stringLocalizer)
    {
        var localize = new FunctionValue((args, _) =>
        {
            var firstArg = args.At(0).ToStringValue();
            var localizerArgs = args.Values.Skip(1).Select(v => v.ToStringValue()).ToArray();

            var localizedString = stringLocalizer[firstArg, localizerArgs.ToArray()];

            return new ValueTask<FluidValue>(new StringValue(localizedString));
        });
        return localize;
    }
    
    public static FunctionValue GetUrlPathFunction(IHttpContext httpContext)
    {
        var urlAction = new FunctionValue((_, _) =>
        {
            return new ValueTask<FluidValue>(new StringValue(httpContext.Request.ApplicationPath));
        });
        
        return urlAction;
    }
    
    public static FunctionValue FormatDate { get; } = new((args, _) =>
    {
        var obj = args.At(0).ToStringValue();
        var format = args.At(1).ToStringValue();
    
        return StringValue.Create(DateTime.TryParse(obj, out var dt) ? dt.ToString(format) : obj);
    });
    
    public static FunctionValue IsNullOrEmpty{ get; } = new((args, _) =>
    {
        var str = args.At(0).ToStringValue();
        return BooleanValue.Create(string.IsNullOrEmpty(str));
    });
    
    public static FunctionValue IsNullOrWhiteSpace { get; } = new((args, _) =>
    {
        var str = args.At(0).ToStringValue();
        return BooleanValue.Create(string.IsNullOrWhiteSpace(str));
    });
    
    public static FunctionValue Substring{ get; } = new((args, _) =>
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