using System.Collections.Generic;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Structure;

public class DataDictionaryFormElementFactory(
    IOptionsSnapshot<MasterDataCoreOptions> options,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    IUrlHelper urlHelper)
{
    private readonly MasterDataCoreOptions _options = options.Value;

    public FormElement GetFormElement()
    {
        var element = DataDictionaryStructure.GetElement(_options.DataDictionaryTableName);

        var formElement = GetFormElement(element);

        ConfigureActions(formElement);

        formElement.Options.Grid.IsCompact = true;
        formElement.Options.Grid.RecordsPerPage = 10;
        formElement.Options.Grid.MaintainValuesOnLoad = true;
        formElement.Options.Grid.UseVerticalLayoutAtFilter = true;

        return formElement;
    }

    private FormElement GetFormElement(Element element)
    {
        var formElement = new FormElement(element);

        formElement.SubTitle = $"val:{stringLocalizer[formElement.SubTitle!]}";

        ConfigureFields(formElement);

        return formElement;
    }

    private void ConfigureFields(FormElement formElement)
    {
        formElement.Fields[DataDictionaryStructure.Name].CssClass = "col-sm-4";
        formElement.Fields[DataDictionaryStructure.TableName].CssClass = "col-sm-4";
        formElement.Fields[DataDictionaryStructure.Type].VisibleExpression = "val:0";
        formElement.Fields[DataDictionaryStructure.Owner].VisibleExpression = "val:0";
        formElement.Fields[DataDictionaryStructure.Info].VisibleExpression = "val:0";
        formElement.Fields[DataDictionaryStructure.Json].Component = FormComponent.Text;
        formElement.Fields[DataDictionaryStructure.Json].CssClass = "col-sm-4";
        formElement.Fields[DataDictionaryStructure.Json].VisibleExpression = "val:{IsFilter}";
        formElement.Fields[DataDictionaryStructure.Json].HelpDescription =
            stringLocalizer["Filter by any data within the data dictionary structure."];
        formElement.Fields[DataDictionaryStructure.LastModified].Component = FormComponent.DateTime;
        formElement.Fields[DataDictionaryStructure.LastModified].GridAlignment = GridAlignment.Right;
        formElement.Fields[DataDictionaryStructure.EnableSynchronism].VisibleExpression = "val:0";
        formElement.Fields[DataDictionaryStructure.EnableSynchronism].Component = FormComponent.CheckBox;
    }

    private void ConfigureActions(FormElement formElement)
    {
        formElement.Options.GridTableActions.ViewAction.SetVisible(false);
        formElement.Options.GridTableActions.EditAction.SetVisible(false);
        formElement.Options.GridTableActions.DeleteAction.SetVisible(false);
        
        formElement.Options.GridToolbarActions.InsertAction.SetVisible(false);
        formElement.Options.GridToolbarActions.ExportAction.SetVisible(false);
        
        formElement.Options.GridToolbarActions.FilterAction.Text = "Filters";
        
        AddGridTableActions(formElement);

        AddGridToolbarActions(formElement);
    }

    private void AddGridTableActions(FormElement formElement)
    {
        List<BasicAction> actions =
        [
            new UrlRedirectAction
            {
                Icon = IconType.Pencil,
                Name = "tools",
                Tooltip = stringLocalizer["Edit"],
                EnableExpression = "exp:'T' <> '{type}'",
                IsDefaultOption = true
            },
            new UrlRedirectAction
            {
                Icon = IconType.FilesO,
                Name = "duplicate",
                Tooltip = stringLocalizer["Duplicate"],
                EnableExpression = "exp:'T' <> '{type}'",
                IsGroup = false,
            },
            new ScriptAction
            {
                Icon = IconType.SolidCirclePlay,
                Name = "render",
                Tooltip = stringLocalizer["Render"],
                EnableExpression = "exp:'T' <> '{type}'",
                IsGroup = false
            }
        ];
        
        formElement.Options.GridTableActions.AddRange(actions);
    }

    private void AddGridToolbarActions(FormElement formElement)
    {
        List<BasicAction> actions =
        [
            new UrlRedirectAction
            {
                Name = "btnAdd",
                Text = stringLocalizer["New"],
                Icon = IconType.Plus,
                ShowAsButton = true,
                UrlRedirect = urlHelper.Action("Add", "Element", new {Area="DataDictionary"})
            },
            new ScriptAction
            {
                Name = "btnDeleteSelected",
                Order = 0,
                Icon = IconType.Trash,
                Text = stringLocalizer["Delete Selected"],
                IsGroup = false,
                ConfirmationMessage = stringLocalizer["Do you want to delete ALL selected records?"],
                ShowAsButton = true,
                OnClientClick = "deleteSelected();"
            },
            new ScriptAction
            {
                Name = "btnAbout",
                Text = stringLocalizer["About"],
                Icon = IconType.InfoCircle,
                ShowAsButton = false,
                IsGroup = true,
                OnClientClick = $"DataDictionaryUtils.showAbout('{urlHelper.Action("Index", "About", new {Area="DataDictionary"})}')",
                Order = 14,
                CssClass = BootstrapHelper.PullRight
            },

            new ScriptAction
            {
                Name = "btnHelp",
                Text = stringLocalizer["Help"],
                Icon = IconType.QuestionCircle,
                IsGroup = true,
                ShowAsButton = true,
                OnClientClick = "window.open('https://md.jjconsulting.tech', '_blank');",
                Order = 13,
                CssClass = BootstrapHelper.PullRight
            },

            new UrlRedirectAction
            {
                Name = "btnImport",
                Tooltip = stringLocalizer["Import"],
                Icon = IconType.Upload,
                ShowAsButton = true,
                IsModal = true,
                IsIframe = false,
                ModalTitle = "Import",
                UrlRedirect = urlHelper.Action("Import", "Element", new {Area="DataDictionary"}),
                Order = 11,
                CssClass = BootstrapHelper.PullRight
            },

            new ScriptAction
            {
                Name = "btnExport",
                Tooltip = stringLocalizer["Export Selected"],
                Icon = IconType.Download,
                ShowAsButton = true,
                Order = 10,
                CssClass = BootstrapHelper.PullRight,
                OnClientClick =
                    $"DataDictionaryUtils.exportElement('{_options.DataDictionaryTableName.ToLowerInvariant()}', '{urlHelper.Action("Export", "Element", new{Area="DataDictionary"})}', '{stringLocalizer["Select one or more dictionaries"]}');"
            },
            
            new UrlRedirectAction
            {
                Name = "btnLog",
                Text = stringLocalizer["Log"],
                Icon = IconType.Film,
                ShowAsButton = true,
                IsModal = true,
                IsGroup = true,
                ModalTitle = stringLocalizer["Log"],
                ModalSize = ModalSize.ExtraLarge,
                UrlRedirect = urlHelper.Action("Index", "Log", new {Area="DataDictionary", isModal = true}),
                Order = 11,
                CssClass = BootstrapHelper.PullRight
            },
            new UrlRedirectAction
            {
                Name = "btnI18n",
                Text = stringLocalizer["Internationalization"],
                Icon = IconType.SolidEarthAmericas,
                ShowAsButton = true,
                IsModal = true,
                IsGroup = true,
                ModalTitle = stringLocalizer["Internationalization"],
                ModalSize = ModalSize.ExtraLarge,
                UrlRedirect = urlHelper.Action("Index", "Localization", new {Area="DataDictionary", isModal=true}),
                Order = 11,
                CssClass = BootstrapHelper.PullRight
            }
        ];
        formElement.Options.GridToolbarActions.AddRange(actions);
        formElement.Options.GridToolbarActions.RefreshAction.SetVisible(true);
        formElement.Options.GridToolbarActions.ConfigAction.SetVisible(true);
    }
}