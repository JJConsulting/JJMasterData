#nullable enable
using System;
using System.Collections.Generic;
using JJConsulting.FontAwesome;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Models;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.UI.Components.Factories;

public sealed class TitleFactory(ExpressionsService expressionsService)
{
    public JJTitle Create(FormElement formElement, FormStateData formStateData, List<TitleAction>? actions)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));
    
        var htmlTitle = new JJTitle
        {
            Title = expressionsService.GetExpressionValue(formElement.Title, formStateData) as string,
            Size = formElement.TitleSize,
            Icon = formElement.Icon,
            Actions = actions,
            SubTitle = expressionsService.GetExpressionValue(formElement.SubTitle, formStateData) as string
        };

        return htmlTitle;
    }
}