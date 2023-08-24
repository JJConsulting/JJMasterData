using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.Options;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Factories;

public class DataDictionaryFormElementFactory 
{
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private IHttpContext HttpContext { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }
    private readonly JJMasterDataCoreOptions _options;

    public DataDictionaryFormElementFactory(
        IOptions<JJMasterDataCoreOptions> options,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        IHttpContext httpContext,
        JJMasterDataUrlHelper urlHelper
    )
    {
        StringLocalizer = stringLocalizer;
        HttpContext = httpContext;
        UrlHelper = urlHelper;
        _options = options.Value;
    }
    
    public FormElement GetFormElement()
    {
        var element = DataDictionaryStructure.GetElement(_options.DataDictionaryTableName);

        var formElement = GetFormElement(element);

        AddActions(formElement);

        return formElement;
    }

    private FormElement GetFormElement(Element element)
    {
        var formElement = new FormElement(element);

        var appPath = HttpContext.Request.ApplicationPath;

        var baseUrl = string.IsNullOrEmpty(appPath) ? "/" : appPath;
        
        var image = new HtmlBuilder(HtmlTag.Img)
            .WithAttribute("src",
                baseUrl + "_content/JJMasterData.Web/images/JJMasterData.png")
            .WithAttribute("style", "width:8%;height:8%;");
        
        formElement.Title = image.ToString();
        
        ConfigureFields(formElement);
        
        return formElement;
    }

    private void ConfigureFields(FormElement formElement)
    {
        formElement.Fields[DataDictionaryStructure.Name].VisibleExpression = "exp:{PageState} <> 'FILTER'";
        formElement.Fields[DataDictionaryStructure.Type].VisibleExpression = "val:0";
        formElement.Fields[DataDictionaryStructure.Owner].VisibleExpression= "val:0";
        formElement.Fields[DataDictionaryStructure.Json].Component = FormComponent.Text;
        formElement.Fields[DataDictionaryStructure.Json].VisibleExpression = "exp: {PageState} = 'FILTER'";
        formElement.Fields[DataDictionaryStructure.Json].HelpDescription = StringLocalizer["Filter for any data inside the structure of the metadata"];
        formElement.Fields[DataDictionaryStructure.LastModified].Component = FormComponent.DateTime;
        formElement.Fields[DataDictionaryStructure.EnableApi].VisibleExpression = "val:0";
        formElement.Fields[DataDictionaryStructure.EnableApi].Component = FormComponent.CheckBox;
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
        var acTools = new UrlRedirectAction
        {
            Icon = IconType.Pencil,
            Name = "tools",
            ToolTip = StringLocalizer["Field Maintenance"],
            EnableExpression = "exp:'T' <> {type}",
            IsDefaultOption = true
        };
        formElement.Options.GridTableActions.Add(acTools);

        var renderBtn = new ScriptAction
        {
            Icon = IconType.Eye,
            Name = "preview",
            Text = StringLocalizer["Preview"],
            EnableExpression = "exp:'T' <> {type}",
            IsGroup = true
        };
        formElement.Options.GridTableActions.Add(renderBtn);

        var btnDuplicate = new UrlRedirectAction
        {
            Icon = IconType.FilesO,
            Name = "duplicate",
            Text = StringLocalizer["Duplicate"],
            EnableExpression = "exp:'T' <> {type}",
            IsGroup = true
        };
        formElement.Options.GridTableActions.Add(btnDuplicate);
    }

    private void AddGridToolbarActions(FormElement formElement)
    {
        var btnAdd = new UrlRedirectAction
        {
            Name = "btnadd",
            Text = StringLocalizer["New"],
            Icon = IconType.Plus,
            ShowAsButton = true,
            UrlRedirect = UrlHelper.GetUrl("Add", "Element", "DataDictionary")
        };
        formElement.Options.GridToolbarActions.Add(btnAdd);

        var btnImport = new UrlRedirectAction
        {
            Name = "btnImport",
            ToolTip = StringLocalizer["Import"],
            Icon = IconType.Upload,
            ShowAsButton = true,
            UrlAsPopUp = true,
            PopUpTitle = "Import",
            UrlRedirect = UrlHelper.GetUrl("Import", "Element", "DataDictionary"),
            Order = 11,
            CssClass = BootstrapHelper.PullRight
        };
        formElement.Options.GridToolbarActions.Add(btnImport);

        var btnExport = new ScriptAction
        {
            Name = "btnExport",
            ToolTip = StringLocalizer["Export Selected"],
            Icon = IconType.Download,
            ShowAsButton = true,
            Order = 10,
            CssClass = BootstrapHelper.PullRight,
            OnClientClick =
                $"DataDictionaryUtils.exportElement('{_options.DataDictionaryTableName}', '{UrlHelper.GetUrl("Export", "Element", "DataDictionary")}', '{StringLocalizer["Select one or more dictionaries"]}');"
        };
        formElement.Options.GridToolbarActions.Add(btnExport);

        var btnAbout = new UrlRedirectAction
        {
            Name = "btnAbout",
            ToolTip = StringLocalizer["About"],
            Icon = IconType.InfoCircle,
            ShowAsButton = true,
            UrlAsPopUp = true,
            PopUpTitle = StringLocalizer["About"],
            UrlRedirect = UrlHelper.GetUrl("Index", "About", "DataDictionary"),
            Order = 13,
            CssClass = BootstrapHelper.PullRight
        };

        formElement.Options.GridToolbarActions.Add(btnAbout);

        var btnLog = new UrlRedirectAction
        {
            Name = "btnLog",
            ToolTip = StringLocalizer["Log"],
            Icon = IconType.FileTextO,
            ShowAsButton = true,
            UrlAsPopUp = true,
            PopUpTitle = StringLocalizer["Log"],
            UrlRedirect = UrlHelper.GetUrl("Index", "Log", "DataDictionary"),
            Order = 11,
            CssClass = BootstrapHelper.PullRight
        };

        formElement.Options.GridToolbarActions.Add(btnLog);

        var btnSettings = new UrlRedirectAction
        {
            Name = "btnAppSettings",
            ToolTip = StringLocalizer["Application Options"],
            Icon = IconType.Code,
            ShowAsButton = true,
            UrlAsPopUp = true,
            PopUpTitle = StringLocalizer["Application Options"],
            UrlRedirect = UrlHelper.GetUrl("Index", "Options", "DataDictionary"),
            Order = 12,
            CssClass = BootstrapHelper.PullRight
        };

        formElement.Options.GridToolbarActions.Add(btnSettings);

        var btnResources = new UrlRedirectAction
        {
            Name = "btnI18n",
            ToolTip = StringLocalizer["Internationalization"],
            Icon = IconType.Globe,
            ShowAsButton = true,
            UrlAsPopUp = true,
            PopUpTitle = StringLocalizer["Internationalization"],
            UrlRedirect = UrlHelper.GetUrl("Index", "Localization", "DataDictionary"),
            Order = 11,
            CssClass = BootstrapHelper.PullRight
        };

        formElement.Options.GridToolbarActions.Add(btnResources);

        formElement.Options.GridToolbarActions.Add(new SubmitAction
        {
            Name = "btnDeleteMetadata",
            Order = 0,
            Icon = IconType.Trash,
            Text = StringLocalizer["Delete Selected"],
            IsGroup = false,
            ConfirmationMessage = StringLocalizer["Do you want to delete ALL selected records?"],
            ShowAsButton = true,
            FormAction = UrlHelper.GetUrl("Delete", "Element", "DataDictionary"),
        });
    }
}