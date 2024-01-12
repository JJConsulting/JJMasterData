using System.Collections.Generic;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.Http;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI;
using JJMasterData.Core.UI.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Structure;

public class DataDictionaryFormElementFactory(
    IOptions<MasterDataCoreOptions> options,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    IHttpContext httpContext,
    MasterDataUrlHelper urlHelper)
{
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
    private IHttpContext HttpContext { get; } = httpContext;
    private MasterDataUrlHelper UrlHelper { get; } = urlHelper;
    private readonly MasterDataCoreOptions _options = options.Value;

    public FormElement GetFormElement()
    {
        var element = DataDictionaryStructure.GetElement(_options.DataDictionaryTableName);

        var formElement = GetFormElement(element);

        AddActions(formElement);

        formElement.Options.Grid.IsCompact = true;
        formElement.Options.Grid.RecordsPerPage = 10;
        formElement.Options.Grid.UseVerticalLayoutAtFilter = true;

        return formElement;
    }

    private FormElement GetFormElement(Element element)
    {
        var formElement = new FormElement(element)
        {
            Title = new ImageFactory(HttpContext).CreateMasterDataLogo().GetHtml(),
        };

        formElement.SubTitle = StringLocalizer[formElement.SubTitle!];

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
        formElement.Fields[DataDictionaryStructure.Json].VisibleExpression = "exp: '{PageState}' = 'Filter'";
        formElement.Fields[DataDictionaryStructure.Json].HelpDescription =
            StringLocalizer["Filter by any data within the data dictionary structure."];
        formElement.Fields[DataDictionaryStructure.LastModified].Component = FormComponent.DateTime;
        formElement.Fields[DataDictionaryStructure.EnableSynchronism].VisibleExpression = "val:0";
        formElement.Fields[DataDictionaryStructure.EnableSynchronism].Component = FormComponent.CheckBox;
    }

    private void AddActions(FormElement formElement)
    {
        formElement.Options.GridToolbarActions.InsertAction.SetVisible(false);
        formElement.Options.GridToolbarActions.ExportAction.SetVisible(false);
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
            new ScriptAction
            {
                Icon = IconType.SolidCirclePlay,
                Name = "render",
                Text = StringLocalizer["Render"],
                EnableExpression = "exp:'T' <> '{type}'",
                IsGroup = true
            },
            new UrlRedirectAction
            {
                Icon = IconType.FilesO,
                Name = "duplicate",
                Text = StringLocalizer["Duplicate"],
                EnableExpression = "exp:'T' <> '{type}'",
                IsGroup = true
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
                UrlRedirect = UrlHelper.GetUrl("Add", "Element", "DataDictionary")
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
                FormAction = UrlHelper.GetUrl("Delete", "Element", "DataDictionary"),
            },

            new UrlRedirectAction
            {
                Name = "btnAbout",
                Tooltip = StringLocalizer["About"],
                Icon = IconType.InfoCircle,
                ShowAsButton = true,
                IsModal = true,
                IsIframe = false,
                ModalTitle = StringLocalizer["About"],
                ModalSize = ModalSize.ExtraLarge,
                UrlRedirect = UrlHelper.GetUrl("Index", "About", "DataDictionary"),
                Order = 14,
                CssClass = BootstrapHelper.PullRight
            },

            new ScriptAction
            {
                Name = "btnHelp",
                Tooltip = StringLocalizer["Help"],
                Icon = IconType.SolidBookOpen,
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
                UrlRedirect = UrlHelper.GetUrl("Import", "Element", "DataDictionary"),
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
                    $"DataDictionaryUtils.exportElement('{ComponentNameGenerator.Create(_options.DataDictionaryTableName)}', '{UrlHelper.GetUrl("Export", "Element", "DataDictionary")}', '{StringLocalizer["Select one or more dictionaries"]}');"
            },
            
            new UrlRedirectAction
            {
                Name = "btnLog",
                Tooltip = StringLocalizer["Log"],
                Icon = IconType.FileTextO,
                ShowAsButton = true,
                IsModal = true,
                ModalTitle = StringLocalizer["Log"],
                ModalSize = ModalSize.ExtraLarge,
                UrlRedirect = UrlHelper.GetUrl("Index", "Log", "DataDictionary"),
                Order = 11,
                CssClass = BootstrapHelper.PullRight
            },
            
            new UrlRedirectAction
            {
                Name = "btnAppSettings",
                Tooltip = StringLocalizer["Application Options"],
                Icon = IconType.Code,
                ShowAsButton = true,
                IsModal = true,
                ModalSize = ModalSize.ExtraLarge,
                ModalTitle = StringLocalizer["Application Options"],
                UrlRedirect = UrlHelper.GetUrl("Index", "Options", "DataDictionary"),
                Order = 12,
                CssClass = BootstrapHelper.PullRight
            },

            new UrlRedirectAction
            {
                Name = "btnI18n",
                Tooltip = StringLocalizer["Localization"],
                Icon = IconType.Globe,
                ShowAsButton = true,
                IsModal = true,
                ModalTitle = StringLocalizer["Localization"],
                ModalSize = ModalSize.ExtraLarge,
                UrlRedirect = UrlHelper.GetUrl("Index", "Localization", "DataDictionary"),
                Order = 11,
                CssClass = BootstrapHelper.PullRight
            }
        ];
        formElement.Options.GridToolbarActions.AddRange(actions);
    }
}