using JJConsulting.Html.Bootstrap.Components;
using JJMasterData.Core.DataDictionary.Models;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components.Factories;

public sealed class LabelFactory(IStringLocalizer<MasterDataResources> stringLocalizer) 
{
    public JJLabel Create(FormElementField field)
    {
        var label = new JJLabel
        {
            LabelFor = field.Name,
            Text = string.IsNullOrEmpty(field.Label) ? field.Name : stringLocalizer[field.Label],
            Tooltip = string.IsNullOrEmpty(field.HelpDescription) ? field.HelpDescription : stringLocalizer[field.HelpDescription],
            IsRequired = field.IsRequired,
            RequiredText = stringLocalizer["Required"]
        };

        return label;
    }
}