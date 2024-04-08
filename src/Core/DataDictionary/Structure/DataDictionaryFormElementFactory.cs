using System.Collections.Generic;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI;
using JJMasterData.Core.UI.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Structure;

public class DataDictionaryFormElementFactory(
    IOptionsSnapshot<MasterDataCoreOptions> options,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    IHttpContext httpContext,
    IMasterDataUrlHelper urlHelper)
{
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
    private IHttpContext HttpContext { get; } = httpContext;
    private IMasterDataUrlHelper UrlHelper { get; } = urlHelper;
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
        var formElement = new FormElement(element)
        {
            Title = $"val:{new ImageFactory(HttpContext).CreateMasterDataLogo().GetHtml()}",
        };

        formElement.SubTitle = $"val:{StringLocalizer[formElement.SubTitle!]}";

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
            StringLocalizer["Filter by any data within the data dictionary structure."];
        formElement.Fields[DataDictionaryStructure.LastModified].Component = FormComponent.DateTime;
        formElement.Fields[DataDictionaryStructure.LastModified].GridAlignment = GridAlignment.Right;
        formElement.Fields[DataDictionaryStructure.EnableSynchronism].VisibleExpression = "val:0";
        formElement.Fields[DataDictionaryStructure.EnableSynchronism].Component = FormComponent.CheckBox;
    }

    private void ConfigureActions(FormElement formElement)
    {
        formElement.Options.GridToolbarActions.InsertAction.SetVisible(false);
        formElement.Options.GridToolbarActions.ExportAction.SetVisible(false);
        
        formElement.Options.GridToolbarActions.FilterAction.Text = "Filters";
        formElement.Options.GridToolbarActions.FilterAction.ShowIconAtCollapse = true;
        
        formElement.Options.GridTableActions.Clear();
        
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
                Tooltip = StringLocalizer["Edit"],
                EnableExpression = "exp:'T' <> '{type}'",
                IsDefaultOption = true
            },
            new UrlRedirectAction
            {
                Icon = IconType.FilesO,
                Name = "duplicate",
                Tooltip = StringLocalizer["Duplicate"],
                EnableExpression = "exp:'T' <> '{type}'",
                IsGroup = false,
            },
            new ScriptAction
            {
                Icon = IconType.SolidCirclePlay,
                Name = "render",
                Tooltip = StringLocalizer["Render"],
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
                Text = StringLocalizer["New"],
                Icon = IconType.Plus,
                ShowAsButton = true,
                UrlRedirect = UrlHelper.Action("Add", "Element", new {Area="DataDictionary"})
            },

            new SubmitAction
            {
                Name = "btnDeleteSelected",
                Order = 0,
                Icon = IconType.Trash,
                Text = StringLocalizer["Delete Selected"],
                IsGroup = false,
                ConfirmationMessage = StringLocalizer["Do you want to delete ALL selected records?"],
                ShowAsButton = true,
                FormAction = UrlHelper.Action("Delete", "Element", new {Area="DataDictionary"}),
            },

            new UrlRedirectAction
            {
                Name = "btnAbout",
                Text = StringLocalizer["About"],
                Icon = IconType.InfoCircle,
                ShowAsButton = false,
                IsModal = true,
                IsIframe = false,
                IsGroup = true,
                ModalTitle = StringLocalizer["About"],
                ModalSize = ModalSize.ExtraLarge,
                UrlRedirect = UrlHelper.Action("Index", "About", new {Area="DataDictionary"}),
                Order = 14,
                CssClass = BootstrapHelper.PullRight
            },

            new ScriptAction
            {
                Name = "btnHelp",
                Text = StringLocalizer["Help"],
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
                Tooltip = StringLocalizer["Import"],
                Icon = IconType.Upload,
                ShowAsButton = true,
                IsModal = true,
                IsIframe = false,
                ModalTitle = "Import",
                UrlRedirect = UrlHelper.Action("Import", "Element", new {Area="DataDictionary"}),
                Order = 11,
                CssClass = BootstrapHelper.PullRight
            },

            new ScriptAction
            {
                Name = "btnExport",
                Tooltip = StringLocalizer["Export Selected"],
                Icon = IconType.Download,
                ShowAsButton = true,
                Order = 10,
                CssClass = BootstrapHelper.PullRight,
                OnClientClick =
                    $"DataDictionaryUtils.exportElement('{ComponentNameGenerator.Create(_options.DataDictionaryTableName)}', '{UrlHelper.Action("Export", "Element", new{Area="DataDictionary"})}', '{StringLocalizer["Select one or more dictionaries"]}');"
            },
            
            new UrlRedirectAction
            {
                Name = "btnLog",
                Text = StringLocalizer["Log"],
                Icon = IconType.Film,
                ShowAsButton = true,
                IsModal = true,
                IsGroup = true,
                ModalTitle = StringLocalizer["Log"],
                ModalSize = ModalSize.ExtraLarge,
                UrlRedirect = UrlHelper.Action("Index", "Log", new {Area="DataDictionary"}),
                Order = 11,
                CssClass = BootstrapHelper.PullRight
            },
            
            new UrlRedirectAction
            {
                Name = "btnAppSettings",
                Text = StringLocalizer["Application Settings"],
                Icon = IconType.SolidToolbox,
                ShowAsButton = true,
                IsGroup = true,
                UrlRedirect = UrlHelper.Action("Index", "Settings", new {Area="DataDictionary"}),
                Order = 12,
                CssClass = BootstrapHelper.PullRight
            },

            new UrlRedirectAction
            {
                Name = "btnI18n",
                Text = StringLocalizer["Internationalization"],
                Icon = IconType.SolidEarthAmericas,
                ShowAsButton = true,
                IsModal = true,
                IsGroup = true,
                ModalTitle = StringLocalizer["Internationalization"],
                ModalSize = ModalSize.ExtraLarge,
                UrlRedirect = UrlHelper.Action("Index", "Localization", new {Area="DataDictionary", isModal=true}),
                Order = 11,
                CssClass = BootstrapHelper.PullRight
            }
        ];
        formElement.Options.GridToolbarActions.AddRange(actions);
    }
}