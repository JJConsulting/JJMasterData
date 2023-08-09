using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.Options;
using JJMasterData.Core.Web;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Factories;

public class JJMasterDataFormElementFactory : IFormElementFactory
{
    public string ElementName => _options.DataDictionaryTableName;

    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }
    private readonly JJMasterDataCoreOptions _options;

    public JJMasterDataFormElementFactory(
        IOptions<JJMasterDataCoreOptions> options,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        JJMasterDataUrlHelper urlHelper
    )
    {
        StringLocalizer = stringLocalizer;
        UrlHelper = urlHelper;
        _options = options.Value;
    }
    
    public FormElement GetFormElement()
    {
        var element = GetElement();

        var formElement = GetFormElement(element);

        AddActions(formElement);

        return formElement;
    }

    private static FormElement GetFormElement(Element element)
    {
        var formElement = new FormElement(element);
        formElement.Fields[DataDictionaryStructure.Sync].VisibleExpression = "exp:{pagestate} <> 'FILTER'";
        formElement.Fields[DataDictionaryStructure.Sync].Component = FormComponent.ComboBox;
        var dataItem = new FormElementDataItem();
        dataItem.Items.Add(new DataItemValue("1", "Yes"));
        dataItem.Items.Add(new DataItemValue("0", "No"));
        formElement.Fields[DataDictionaryStructure.Sync].DataItem = dataItem;
        formElement.Fields[DataDictionaryStructure.LastModified].Component = FormComponent.DateTime;
        formElement.Title = "JJMasterData";
        return formElement;
    }

    private Element GetElement()
    {
        var element = new Element(_options.DataDictionaryTableName, "Data Dictionaries");
        element.Fields.AddPK(DataDictionaryStructure.Name, "Dictionary Name", FieldType.NVarchar, 64, false,
            FilterMode.Equal);
        element.Fields.Add(DataDictionaryStructure.TableName, "Table Name", FieldType.NVarchar, 64, false,
            FilterMode.MultValuesContain);
        element.Fields.Add(DataDictionaryStructure.Info, "Info", FieldType.NVarchar, 150, false, FilterMode.None);
        element.Fields.Add(DataDictionaryStructure.Sync, "Sync", FieldType.Varchar, 1, false, FilterMode.None);
        element.Fields.Add(DataDictionaryStructure.LastModified, "Last Modified", FieldType.DateTime, 15, true,
            FilterMode.Range);
        return element;
    }

    private void AddActions(FormElement formElement)
    {
        formElement.Options.GridToolbarActions.InsertAction.VisibleExpression = "val:0";
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
                $"DataDictionaryUtils.exportElement('{ElementName}', '{UrlHelper.GetUrl("Export", "Element", "DataDictionary")}', '{StringLocalizer["Select one or more dictionaries"]}');"
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
            Name = "btnResources",
            ToolTip = StringLocalizer["Resources"],
            Icon = IconType.Globe,
            ShowAsButton = true,
            UrlAsPopUp = true,
            PopUpTitle = StringLocalizer["Resources"],
            UrlRedirect = UrlHelper.GetUrl("Index", "Resources", "DataDictionary"),
            Order = 11,
            CssClass = BootstrapHelper.PullRight
        };

        formElement.Options.GridToolbarActions.Add(btnResources);

        formElement.Options.GridToolbarActions.Add(new SubmitAction()
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