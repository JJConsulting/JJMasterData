#nullable disable warnings
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Fluid;
using Fluid.Values;
using JJConsulting.Html;
using JJConsulting.Html.Extensions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Html.Templates;

public static class HtmlTemplateOptions
{
    internal const string ServiceProviderKey = "ServiceProvider";
    internal const string FormElementKey = "FormElement";
    internal const string FormElementFieldKey = "_FormElementField";
    internal const string FormValuesKey = "FormValues";

    public static readonly TemplateOptions Value;

    static HtmlTemplateOptions()
    {
        var options = new TemplateOptions();
        options.Scope.SetValue("isNullOrWhiteSpace", IsNullOrWhiteSpace);
        options.Scope.SetValue("isNullOrEmpty", IsNullOrEmpty);
        options.Scope.SetValue("substring", Substring);
        options.Scope.SetValue("capitalize", Capitalize);
        options.Scope.SetValue("formatDate", FormatDate);
        options.Scope.SetValue("trim", Trim);
        options.Scope.SetValue("trimStart", TrimStart);
        options.Scope.SetValue("trimEnd", TrimEnd);
        options.Scope.SetValue("table", Table);
        options.Scope.SetValue("dateAsText", DateAsText);
        options.Scope.SetValue("urlPath", UrlPath);
        options.Scope.SetValue("appUrl", AppUrl);
        options.Scope.SetValue("localize", Localize);
        options.Scope.SetValue("getFileUrl", GetFileUrl);
        options.Scope.SetValue("isImage", IsImage);
        
        options.ModelNamesComparer = StringComparer.OrdinalIgnoreCase;

        Value = options;
    }
    
    private static readonly FunctionValue DateAsText = new((args, context) =>
    {
        var dateFormatter = GetRequiredService<RelativeDateFormatter>(context);
        var dateArg = args.At(0).ToStringValue();

        if (DateTime.TryParse(dateArg, out var date))
        {
            var phrase = dateFormatter.ToRelativeString(date);
            return StringValue.Create(phrase);
        }

        return StringValue.Empty;
    });

    private static readonly FunctionValue Localize = new((args, context) =>
    {
        var stringLocalizer = GetRequiredService<IStringLocalizer<MasterDataResources>>(context);
        var firstArg = args.At(0).ToStringValue();
        var localizerArgs = args.Values.Skip(1).Select(v => v.ToStringValue()).ToArray();
        var localizedString = stringLocalizer[firstArg, localizerArgs];

        return StringValue.Create(localizedString);
    });

    private static readonly FunctionValue UrlPath = new((_, context) =>
    {
        var httpContextAccessor = GetRequiredService<IHttpContextAccessor>(context);
        return StringValue.Create(httpContextAccessor.HttpContext?.Request.GetApplicationPath());
    });

    private static readonly FunctionValue AppUrl = new((_, context) =>
    {
        var httpContextAccessor = GetRequiredService<IHttpContextAccessor>(context);
        return StringValue.Create(httpContextAccessor.HttpContext?.Request.GetApplicationUri());
    });

    private static readonly FunctionValue GetFileUrl = new((args, context) =>
    {
        if (!TryGetAmbientValue<FormElement>(context, FormElementKey, out var formElement) ||
            !TryGetAmbientValue<FormElementField>(context, FormElementFieldKey, out var field) ||
            !TryGetAmbientValue<Dictionary<string, object?>>(context, FormValuesKey, out var values) ||
            field.DataFile is null)
        {
            return StringValue.Empty;
        }

        var fileName = args.At(0).ToStringValue();
        if (string.IsNullOrWhiteSpace(fileName))
            return StringValue.Empty;

        if(values.Count == 0)
            return StringValue.Empty;
        
        var fileDownloaderFactory = GetRequiredService<FileDownloaderFactory>(context);
        var url = fileDownloaderFactory
            .Create(formElement, field, values, fileName)
            .GetDownloadUrl();
        
        return StringValue.Create(url);
    });

    private static readonly FunctionValue FormatDate = new((args, _) =>
    {
        var obj = args.At(0).ToStringValue();
        var format = args.At(1).ToStringValue();

        return StringValue.Create(DateTime.TryParse(obj, out var dt) ? dt.ToString(format) : obj);
    });

    private static readonly FunctionValue Trim = new((args, _) =>
    {
        var str = args.At(0).ToStringValue();

        return StringValue.Create(str.Trim());
    });

    private static readonly FunctionValue TrimStart = new((args, _) =>
    {
        var str = args.At(0).ToStringValue();

        return StringValue.Create(str.TrimStart());
    });

    private static readonly FunctionValue TrimEnd = new((args, _) =>
    {
        var str = args.At(0).ToStringValue();

        return StringValue.Create(str.TrimEnd());
    });

    private static readonly FunctionValue IsNullOrEmpty = new((args, _) =>
    {
        var str = args.At(0).ToStringValue();
        return BooleanValue.Create(string.IsNullOrEmpty(str));
    });

    private static readonly FunctionValue IsNullOrWhiteSpace = new((args, _) =>
    {
        var str = args.At(0).ToStringValue();
        return BooleanValue.Create(string.IsNullOrWhiteSpace(str));
    });

    private static readonly FunctionValue IsImage = new((args, _) =>
    {
        var fileName = args.At(0).ToStringValue();
        return BooleanValue.Create(IsImageFile(fileName));
    });

    private static readonly FunctionValue Capitalize = new((args, _) =>
    {
        var str = args.At(0).ToStringValue();
        return StringValue.Create(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str?.ToLower() ?? string.Empty));
    });

    private static readonly FunctionValue Substring = new((args, _) =>
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

    private static readonly FunctionValue Table = new((args, _) =>
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
                                    td.AppendText(value?.ToStringValue());
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

    private static bool IsImageFile(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        return Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".png" => true,
            ".jpg" => true,
            ".jpeg" => true,
            _ => false
        };
    }

    private static T GetRequiredService<T>(TemplateContext context) where T : notnull
    {
        if (!TryGetAmbientValue<IServiceProvider>(context, ServiceProviderKey, out var serviceProvider))
            throw new InvalidOperationException("Service provider not available in HTML template context.");

        return serviceProvider.GetRequiredService<T>();
    }

    private static bool TryGetAmbientValue<T>(TemplateContext context, string key, out T value)
    {
        if (context.AmbientValues.TryGetValue(key, out var ambientValue) && ambientValue is T typedValue)
        {
            value = typedValue;
            return true;
        }

        value = default!;
        return false;
    }
}
