﻿using System.Collections.Generic;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

public class JJToolbar : HtmlComponent
{
    public List<HtmlBuilder> Items { get; set; }
    internal JJToolbar()
    {
        Items = [];
    }

    internal override HtmlBuilder BuildHtml()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithCssClass(CssClass)
            .Append(HtmlTag.Div, row =>
            {
                row.WithCssClass("row");
                row.Append(GetHtmlCol());
            });

        return html;
    }

    private HtmlBuilder GetHtmlCol()
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("col-sm-12");

        int totElement = Items.Count;
        for (int i = 0; i < totElement; i++)
        {
            var element = Items[i];
            if (element == null)
                continue;

            if (i != 0)
                element.WithStyle( "margin-right: 3px;");

            div.Append(element);
        }

        return div;
    }



}
