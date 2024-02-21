using System.Collections.Generic;
using System.Globalization;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Models;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Structure;

public class LocalizationFormElementFactory(IOptionsSnapshot<MasterDataCommonsOptions> masterDataOptions)
{
    public FormElement GetFormElement()
    {
        var supportedCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            
        var element = MasterDataStringLocalizerElement.GetElement(masterDataOptions.Value);

        var formElement = new FormElement(element);
        formElement.Options.Grid.ShowTitle = false;
        formElement.Fields["resourceKey"].IsRequired = true;
        formElement.Fields["resourceOrigin"].VisibleExpression = "val:0";
        formElement.Fields["resourceOrigin"].Export = false;
        
        var options = formElement.Options;
        
        options.GridToolbarActions.ImportAction.SetVisible(true);
        options.GridTableActions.ViewAction.SetVisible(false);
        options.GridToolbarActions.FilterAction.ExpandedByDefault = true;

        formElement.Options.GridToolbarActions.FilterAction.Text = "Filters";
        formElement.Options.GridToolbarActions.FilterAction.ShowIconAtCollapse = true;
        
        var cultureField = formElement.Fields["cultureCode"];
        cultureField.IsRequired = true;
        cultureField.Component = FormComponent.Search;
        cultureField.DataItem = new FormElementDataItem
        {
            Items = new List<DataItemValue>(),
            GridBehavior = DataItemGridBehavior.Id
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

        formElement.Options.Grid.IsCompact = true;
        
        return formElement;
    }
}