#nullable enable

using System;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Html;

public class HtmlTemplateHelper<TResource>(
    IStringLocalizer<TResource> stringLocalizer,
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
            return new ValueTask<FluidValue>(new StringValue(localizedString));
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
            
                return new ValueTask<FluidValue>(new StringValue(phrase));
            }
            else
            {
                return new StringValue(string.Empty);
            }
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

            return new ValueTask<FluidValue>(new StringValue(localizedString));
        });
        return localize;
    }

    public FunctionValue GetUrlPathFunction()
    {
        var urlAction = new FunctionValue((_, _) =>
        {
            return new ValueTask<FluidValue>(new StringValue(httpContext.Request.ApplicationPath));
        });

        return urlAction;
    }
    
    public FunctionValue GetAppUrlFunction()
    {
        var urlAction = new FunctionValue((_, _) =>
        {
            return new ValueTask<FluidValue>(new StringValue(httpContext.Request.ApplicationUri));
        });

        return urlAction;
    }
}