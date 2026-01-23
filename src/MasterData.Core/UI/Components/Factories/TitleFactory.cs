#nullable enable
using System;
using System.Collections.Generic;
using JJConsulting.FontAwesome;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Models;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components.Factories;

public sealed class TitleFactory(ExpressionsService expressionsService, IStringLocalizer<MasterDataResources> stringLocalizer)
{
    public JJTitle Create(FormElement formElement, FormStateData formStateData, List<TitleAction>? actions)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));

        var title = expressionsService.GetExpressionValue(formElement.Title, formStateData) as string ?? string.Empty;
        var subtitle = expressionsService.GetExpressionValue(formElement.SubTitle, formStateData) as string ??
                       string.Empty;
        var htmlTitle = new JJTitle
        {
            Title = stringLocalizer[title],
            Size = formElement.TitleSize,
            Icon = formElement.Icon,
            Actions = actions,
            SubTitle = stringLocalizer[subtitle]
        };

        return htmlTitle;
    }
}