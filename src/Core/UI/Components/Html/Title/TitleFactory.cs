using System;
using System.Drawing;
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
    
    public JJTitle Create(string title, string subtitle, IconType? icon)
    {
        var htmlTitle = Create();
        htmlTitle.Title = title;
        htmlTitle.SubTitle = subtitle;
        htmlTitle.Icon = icon;
        
        return htmlTitle;
    }

    public JJTitle Create(FormElement formElement, FormStateData formStateData)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));
    
        var htmlTitle = Create();
        htmlTitle.Title = expressionsService.GetExpressionValue(formElement.Title, formStateData) as string;
        htmlTitle.Size = formElement.TitleSize;
        htmlTitle.Icon = formElement.Icon;
        htmlTitle.SubTitle = expressionsService.GetExpressionValue(formElement.SubTitle, formStateData) as string;

        return htmlTitle;
    }
}