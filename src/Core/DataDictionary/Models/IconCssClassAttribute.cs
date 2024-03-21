using System;

namespace JJMasterData.Core.DataDictionary.Models;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class IconCssClassAttribute(string cssClass) : Attribute
{
    public string CssClass { get; } = cssClass;
}