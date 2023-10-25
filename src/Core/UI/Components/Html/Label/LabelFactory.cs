using System;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Models;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class LabelFactory :  IComponentFactory<JJLabel>
{
    private readonly IStringLocalizer<MasterDataResources> _stringLocalizer;

    public LabelFactory(IStringLocalizer<MasterDataResources> stringLocalizer)
    {
        _stringLocalizer = stringLocalizer;
    }
    
    public JJLabel Create()
    {
        return new JJLabel(_stringLocalizer);
    }
    
    public JJLabel Create(FormElementField field)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field), "FormElementField cannot be null");

        var label = Create();
        label.LabelFor = field.Name;
        label.Text = field.LabelOrName;
        label.Tooltip = field.HelpDescription;
        label.IsRequired = field.IsRequired;
        
        return label;
    }
    
}