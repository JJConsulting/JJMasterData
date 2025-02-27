#nullable enable

using System;
using System.Globalization;
using System.Linq;
using System.Web;
using Fluid.Values;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Html.Templates;

public class HtmlTemplateFunctions(
    IStringLocalizer<MasterDataResources> stringLocalizer,
    DateService dateService,
    IHttpContext httpContext)
{
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

    public static readonly FunctionValue FormatDate = new((args, _) =>
    {
        var obj = args.At(0).ToStringValue();
        var format = args.At(1).ToStringValue();

        return StringValue.Create(DateTime.TryParse(obj, out var dt) ? dt.ToString(format) : obj);
    });

    public static readonly FunctionValue Trim = new((args, _) =>
    {
        var str = args.At(0).ToStringValue();

        return StringValue.Create(str.Trim());
    });

    public static readonly FunctionValue TrimStart = new((args, _) =>
    {
        var str = args.At(0).ToStringValue();

        return StringValue.Create(str.TrimStart());
    });

    public static readonly FunctionValue TrimEnd = new((args, _) =>
    {
        var str = args.At(0).ToStringValue();

        return StringValue.Create(str.TrimEnd());
    });

    public static readonly FunctionValue IsNullOrEmpty = new((args, _) =>
    {
        var str = args.At(0).ToStringValue();
        return BooleanValue.Create(string.IsNullOrEmpty(str));
    });

    public static readonly FunctionValue IsNullOrWhiteSpace = new((args, _) =>
    {
        var str = args.At(0).ToStringValue();
        return BooleanValue.Create(string.IsNullOrWhiteSpace(str));
    });

    public static readonly FunctionValue Capitalize = new((args, _) =>
    {
        var str = args.At(0).ToStringValue();
        return StringValue.Create(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str?.ToLower() ?? string.Empty));
    });

    public static readonly FunctionValue Substring = new((args, _) =>
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

    public static readonly FunctionValue Table = new((args, _) =>
    {
        var arrayValue = args.At(0).ToObjectValue() as object[];

        if (arrayValue == null || arrayValue.Length == 0)
            return StringValue.Empty;

        var dataSource = arrayValue
            .Select(v => (IFluidIndexable)v)
            .ToList();

        var headers = dataSource[0].Keys;

        var builder = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("table-responsive");

        builder.Append(HtmlTag.Table, table =>
        {
            table.WithCssClass("table table-striped table-hover");

            table.Append(HtmlTag.Thead, thead =>
            {
                thead.Append(HtmlTag.Tr, tr =>
                {
                    foreach (var header in headers)
                    {
                        tr.Append(HtmlTag.Th, th => th.AppendText(header));
                    }
                });
            });

            table.Append(HtmlTag.Tbody, tbody =>
            {
                foreach (var row in dataSource)
                {
                    tbody.Append(HtmlTag.Tr, tr =>
                    {
                        foreach (var header in headers)
                        {
                            tr.Append(HtmlTag.Td, td =>
                            {
                                if (row.TryGetValue(header, out var value))
                                {
                                    td.AppendText(HttpUtility.HtmlEncode(value?.ToStringValue() ?? ""));
                                }
                                else
                                {
                                    td.AppendText("");
                                }
                            });
                        }
                    });
                }
            });
        });

        return new StringValue(builder.ToString(), encode: false);
    });
}