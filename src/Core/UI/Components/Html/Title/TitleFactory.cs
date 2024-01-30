using System;
using System.Threading.Tasks;
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
    
    public JJTitle Create(string title, string subtitle)
    {
        var htmlTitle = Create();
        htmlTitle.Title = title;
        htmlTitle.SubTitle = subtitle;
        return htmlTitle;
    }

    public async Task<JJTitle> CreateAsync(FormElement formElement, FormStateData formStateData)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));
    
        var htmlTitle = Create();
        htmlTitle.Title = await expressionsService.GetExpressionValueAsync(formElement.Title, formStateData) as string;
        htmlTitle.Size = formElement.TitleSize;
        htmlTitle.SubTitle = await expressionsService.GetExpressionValueAsync(formElement.SubTitle, formStateData) as string;

        return htmlTitle;
    }
}