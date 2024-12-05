using System;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Models;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class LabelFactory(IStringLocalizer<MasterDataResources> stringLocalizer) : IComponentFactory<JJLabel>
{
    public JJLabel Create()
    {
        return new JJLabel(stringLocalizer);
    }
    
    public JJLabel Create(FormElementField field)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field), @"FormElementField cannot be null");

        var label = Create();
        label.LabelFor = field.Name;
        label.Text = field.LabelOrName;
        label.Tooltip = field.HelpDescription;
        label.IsRequired = field.IsRequired;
        
        return label;
    }
    
}