using System;
using JJMasterData.Core.DataDictionary.Models;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class LabelFactory(IStringLocalizer<MasterDataResources> stringLocalizer) : IComponentFactory<JJLabel>
{
    public JJLabel Create()
    {
        return new JJLabel();
    }
    
    public JJLabel Create(FormElementField field)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field), @"FormElementField cannot be null");

        var label = Create();
        label.LabelFor = field.Name;
        label.Text = string.IsNullOrEmpty(field.Label) ? field.Name : stringLocalizer[field.Label];
        label.Tooltip = field.HelpDescription;
        label.IsRequired = field.IsRequired;
        label.RequiredText = stringLocalizer["Required"];
        
        return label;
    }
    
}