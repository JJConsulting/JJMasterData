using System;
using Fluid.Values;

namespace JJMasterData.Core.Html;

public static class HtmlTemplateFunctions
{
    public static FunctionValue FormatDate { get; } = new((args, _) =>
    {
        var obj = args.At(0).ToStringValue();
        var format = args.At(1).ToStringValue();

        return StringValue.Create(DateTime.TryParse(obj, out var dt) ? dt.ToString(format) : obj);
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