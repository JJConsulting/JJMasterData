using System;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.Web.Components;

public class TitleFactory : IComponentFactory<JJTitle>
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

    public JJTitle Create(FormElement form)
    {
        if (form == null)
            throw new ArgumentNullException(nameof(form));

        var htmlTitle = Create();
        htmlTitle.Title = form.Title;
        htmlTitle.SubTitle = form.SubTitle;
        return htmlTitle;
    }
}