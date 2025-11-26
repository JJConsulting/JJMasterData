#nullable enable
using System;
using System.Collections.Generic;
using JJConsulting.FontAwesome;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.UI.Components;

public class TitleFactory(ExpressionsService expressionsService) : IComponentFactory<JJTitle>
{
    public JJTitle Create()
    {
        return new JJTitle();
    }
    
    public JJTitle Create(string title, string subtitle, FontAwesomeIcon? icon = null, List<TitleAction>? actions = null)
    {
        var htmlTitle = Create();
        htmlTitle.Title = title;
        htmlTitle.SubTitle = subtitle;
        htmlTitle.Icon = icon;
        htmlTitle.Actions = actions;
        
        return htmlTitle;
    }

    public JJTitle Create(FormElement formElement, FormStateData formStateData, List<TitleAction>? actions)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));
    
        var htmlTitle = Create();
        htmlTitle.Title = expressionsService.GetExpressionValue(formElement.Title, formStateData) as string;
        htmlTitle.Size = formElement.TitleSize;
        htmlTitle.Icon = formElement.Icon;
        htmlTitle.Actions = actions;
        htmlTitle.SubTitle = expressionsService.GetExpressionValue(formElement.SubTitle, formStateData) as string;

        return htmlTitle;
    }
}