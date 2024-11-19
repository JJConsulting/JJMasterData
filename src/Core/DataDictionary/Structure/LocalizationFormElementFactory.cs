using System.Globalization;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Structure;

public class LocalizationFormElementFactory(
    IUrlHelper urlHelper,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    IOptionsSnapshot<MasterDataCommonsOptions> masterDataOptions)
{
    public FormElement GetFormElement(CultureInfo[] supportedCultures)
    {
        var element = MasterDataStringLocalizerElement.GetElement(masterDataOptions.Value);
    
        var formElement = new FormElement(element)
        {
            Title = stringLocalizer["Internationalization"],
            Icon = IconType.SolidEarthAmericas,
            Options =
            {
                Grid =
                {
                    ShowTitle = false
                }
            }
        };
        formElement.Fields["resourceKey"].IsRequired = true;
        formElement.Fields["resourceKey"].CssClass = "col-sm-6";
        formElement.Fields["resourceOrigin"].VisibleExpression = "val:0";
        formElement.Fields["resourceOrigin"].Export = false;
        
        var options = formElement.Options;
        
        options.GridToolbarActions.ImportAction.SetVisible(true);
 
        options.GridTableActions.ViewAction.SetVisible(false);
        options.GridToolbarActions.FilterAction.ExpandedByDefault = true;
    
        formElement.Options.GridToolbarActions.FilterAction.Text = "Filters";
        formElement.Options.GridToolbarActions.Add(new UrlRedirectAction
        {
            Name = "download-resources",
            CssClass = "float-end",
            UrlRedirect = urlHelper.Action("DownloadStrings","Localization", new {Area="DataDictionary"}),
            Tooltip = stringLocalizer["Download all strings to create your own translation."],
            Text = stringLocalizer["Download Resources"],
            ShowAsButton = true,
            Order = 4,
            IsGroup = true,
            Icon = IconType.SolidFileCsv
        });
        formElement.Options.GridToolbarActions.ExportAction.Tooltip = "";
        formElement.Options.GridToolbarActions.ExportAction.Text = stringLocalizer["Export"];
        formElement.Options.GridToolbarActions.ExportAction.IsGroup = true;
        var cultureField = formElement.Fields["cultureCode"];
        cultureField.IsRequired = true;
        cultureField.CssClass = "col-sm-6";
        cultureField.Component = FormComponent.Search;
        cultureField.DataItem = new FormElementDataItem
        {
            Items = [],
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

        formElement.Options.Grid.UseVerticalLayoutAtFilter = true;
        formElement.Options.Grid.IsCompact = true;
        
        return formElement;
    }
}