using System;
using System.Collections.Generic;
using System.Linq;

namespace JJMasterData.Core.DataDictionary;

public static class IconHelper
{
    //This is a static default value, only loaded on the first time.
    private static readonly IEnumerable<IconType> _icons = Enum.GetValues(typeof(IconType)).OfType<IconType>();
    public static IEnumerable<IconInfo> GetIconInfoList() => _icons.Select(i => i.AsIconInfo());
    public static IEnumerable<DataItemValue> GetIconDataItemList()
    {
        return GetIconInfoList()
            .Select(i => new DataItemValue(i.Id.ToString(), i.Description, i.Icon, "#000000"));
    }
    public static string GetCssClass(this IconType icon)
    {
        return AsIconInfo(icon).ClassName;
    }
    public static IconInfo AsIconInfo(this IconType icon)
    {
        string description = PascalToParamCase(icon.ToString());
        return new IconInfo(icon, description, $@"\u{(int)icon:x4}", $"fa fa-{description}");
    }
    private static string PascalToParamCase(string icon)
    {
        return string.Concat(
            icon.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x.ToString().ToLower() : x.ToString().ToLower())
        ); 
    }
}