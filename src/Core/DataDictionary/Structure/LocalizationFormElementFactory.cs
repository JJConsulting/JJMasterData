using System.Collections.Generic;
using System.Globalization;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Models;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Structure;

public class LocalizationFormElementFactory(IOptions<MasterDataCommonsOptions> options)
{
    private MasterDataCommonsOptions Options { get; } = options.Value;

    public FormElement GetFormElement()
    {
        var supportedCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            
        var element = MasterDataStringLocalizerElement.GetElement(Options);

        var formElement = new FormElement(element);
        formElement.Options.Grid.ShowTitle = false;
        formElement.Fields["resourceKey"].IsRequired = true;
        formElement.Fields["resourceOrigin"].VisibleExpression = "val:0";
        formElement.Fields["resourceOrigin"].Export = false;
        
        var options = formElement.Options;
        
        options.GridToolbarActions.ImportAction.SetVisible(true);
        options.GridTableActions.ViewAction.SetVisible(false);
        options.GridToolbarActions.FilterAction.ExpandedByDefault = true;

        var cultureField = formElement.Fields["cultureCode"];
        cultureField.IsRequired = true;
        cultureField.Component = FormComponent.ComboBox;
        cultureField.DataItem = new FormElementDataItem
        {
            Items = new List<DataItemValue>(),
            ReplaceTextOnGrid = false
        };
        foreach (var cultureInfo in supportedCultures)
        {
            var item = new DataItemValue
            {
                Id = cultureInfo.Name,
                Description = $"{cultureInfo.Name} {cultureInfo.DisplayName}"
            };
            cultureField.DataItem.Items.Add(item);
        }

        return formElement;
    }
}