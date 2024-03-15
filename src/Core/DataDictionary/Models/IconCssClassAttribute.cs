using System;

namespace JJMasterData.Core.DataDictionary.Models;

[AttributeUsage(AttributeTargets.Field)]
public class IconCssClassAttribute(string cssClass) : Attribute
{
    public string CssClass { get; } = cssClass;
}