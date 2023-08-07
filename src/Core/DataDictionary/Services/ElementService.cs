#nullable enable

using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Data.Extensions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.Web.Components;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Options;
using JJMasterData.Core.UI.Components;
using Microsoft.Extensions.Localization;
using JJMasterData.Core.Web;

namespace JJMasterData.Core.DataDictionary.Services;

public class ElementService : BaseService
{
    private IFormElementComponentFactory<JJFormView> FormViewFactory { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }
    private readonly IEntityRepository _entityRepository;
    private readonly JJMasterDataCoreOptions _options;

    public ElementService(
        IFormElementComponentFactory<JJFormView> formViewFactory,
        IValidationDictionary validationDictionary,
        IOptions<JJMasterDataCoreOptions> options,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        JJMasterDataUrlHelper urlHelper
        )
        : base(validationDictionary, dataDictionaryRepository, stringLocalizer)
    {
        FormViewFactory = formViewFactory;
        UrlHelper = urlHelper;
        _entityRepository = entityRepository;
        _options = options.Value;
    }

    #region Add Dictionary

    public Element? CreateEntity(string tableName, bool importFields)
    {
        if (!ValidateEntity(tableName, importFields))
            return null;

        Element element;
        if (importFields)
        {
            element = _entityRepository.GetElementFromTable(tableName);
        }
        else
        {
            element = new FormElement
            {
                TableName = tableName,
                Name = GetDictionaryName(tableName),
                CustomProcNameGet = _options.GetReadProcedureName(tableName),
                CustomProcNameSet = _options.GetWriteProcedureName(tableName)
            };
        }

        DataDictionaryRepository.InsertOrReplace(new FormElement(element));

        return element;
    }

    public bool ValidateEntity(string tableName, bool importFields)
    {
        if (ValidateName(tableName))
        {
            if (DataDictionaryRepository.Exists(tableName))
            {
                AddError("Name", StringLocalizer["There is already a dictionary with the name "] + tableName);
            }
        }

        if (importFields & IsValid)
        {
            if (!_entityRepository.TableExists(tableName))
                AddError("Name", StringLocalizer["Table not found"]);
        }

        return IsValid;
    }

    public string GetDictionaryName(string tablename)
    {
        string dicname;
        if (tablename.ToLower().StartsWith("tb_"))
            dicname = tablename.Substring(3);
        else if (tablename.ToLower().StartsWith("tb"))
            dicname = tablename.Substring(2);
        else
            dicname = tablename;

        dicname = dicname.Replace('_', ' ');
        dicname = dicname.FirstCharToUpper();
        dicname = dicname.Replace(" ", "");

        return dicname;
    }

    #endregion

    #region Duplicate Entity

    public bool DuplicateEntity(string originName, string newName)
    {
        if (ValidateEntity(newName))
        {
            var dicParser = DataDictionaryRepository.GetMetadata(originName);
            dicParser.Name = newName;
            DataDictionaryRepository.InsertOrReplace(dicParser);
        }

        return IsValid;
    }

    public bool ValidateEntity(string name)
    {
        if (!ValidateName(name))
            return false;

        if (string.IsNullOrWhiteSpace(name))
        {
            AddError("Name", StringLocalizer["Mandatory dictionary name field"]);
        }

        if (DataDictionaryRepository.Exists(name))
        {
            AddError("Name", StringLocalizer["There is already a dictionary with the name "] + name);
        }

        return IsValid;
    }


    public async Task<JJFormView> GetFormViewAsync()
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

        var formElement = new FormElement(element);
        formElement.Fields[DataDictionaryStructure.Sync].VisibleExpression = "exp:{pagestate} <> 'FILTER'";
        formElement.Fields[DataDictionaryStructure.Sync].Component = FormComponent.ComboBox;
        var dataItem = new FormElementDataItem();
        dataItem.Items.Add(new DataItemValue("1", "Yes"));
        dataItem.Items.Add(new DataItemValue("0", "No"));
        formElement.Fields[DataDictionaryStructure.Sync].DataItem = dataItem;
        formElement.Fields[DataDictionaryStructure.LastModified].Component = FormComponent.DateTime;
        formElement.Title = "JJMasterData";

        formElement.Options.GridToolbarActions.InsertAction.VisibleExpression = "val:0";
        formElement.Options.GridTableActions.Clear();

        var formView = FormViewFactory.Create(formElement);
        formView.Name = "List";
        formView.GridView.FilterAction.ExpandedByDefault = true;

        formView.GridView.MaintainValuesOnLoad = true;
        formView.GridView.EnableMultiSelect = true;
        formView.GridView.ExportAction.SetVisible(false);

        var filter = await formView.GridView.GetCurrentFilterAsync();

        if (!filter.ContainsKey("type"))
            filter.Add("type", "F");

        formView.GridView.OnDataLoadAsync += FormViewOnDataLoad;

        var acTools = new UrlRedirectAction
        {
            Icon = IconType.Pencil,
            Name = "tools",
            ToolTip = StringLocalizer["Field Maintenance"],
            EnableExpression = "exp:'T' <> {type}",
            IsDefaultOption = true
        };
        formView.GridView.AddGridAction(acTools);

        var renderBtn = new ScriptAction
        {
            Icon = IconType.Eye,
            Name = "preview",
            Text = StringLocalizer["Preview"],
            EnableExpression = "exp:'T' <> {type}",
            IsGroup = true
        };
        formView.GridView.AddGridAction(renderBtn);

        var btnDuplicate = new UrlRedirectAction
        {
            Icon = IconType.FilesO,
            Name = "duplicate",
            Text = StringLocalizer["Duplicate"],
            EnableExpression = "exp:'T' <> {type}",
            IsGroup = true
        };
        formView.GridView.AddGridAction(btnDuplicate);

        var btnAdd = new UrlRedirectAction
        {
            Name = "btnadd",
            Text = StringLocalizer["New"],
            Icon = IconType.Plus,
            ShowAsButton = true,
            UrlRedirect = UrlHelper.GetUrl("Add","Element", new {Area="DataDictionary"})
        };
        formView.GridView.AddToolBarAction(btnAdd);

        var btnImport = new UrlRedirectAction
        {
            Name = "btnImport",
            ToolTip = StringLocalizer["Import"],
            Icon = IconType.Upload,
            ShowAsButton = true,
            UrlAsPopUp = true,
            PopUpTitle = "Import",
            UrlRedirect = UrlHelper.GetUrl("Import","Element", new {Area="DataDictionary"}),
            Order = 11,
            CssClass = BootstrapHelper.PullRight
        };
        formView.GridView.AddToolBarAction(btnImport);

        var btnExport = new ScriptAction
        {
            Name = "btnExport",
            ToolTip = StringLocalizer["Export Selected"],
            Icon = IconType.Download,
            ShowAsButton = true,
            Order = 10,
            CssClass = BootstrapHelper.PullRight,
            OnClientClick =
                $"DataDictionaryUtils.exportElement('{formView.Name}', '{UrlHelper.GetUrl("Export","Element", new {Area="DataDictionary"})}', '{StringLocalizer["Select one or more dictionaries"]}');"
        };
        formView.GridView.AddToolBarAction(btnExport);

        var btnAbout = new UrlRedirectAction
        {
            Name = "btnAbout",
            ToolTip = StringLocalizer["About"],
            Icon = IconType.InfoCircle,
            ShowAsButton = true,
            UrlAsPopUp = true,
            PopUpTitle = StringLocalizer["About"],
            UrlRedirect =UrlHelper.GetUrl("Index","About", new {Area="DataDictionary"}),
            Order = 13,
            CssClass = BootstrapHelper.PullRight
        };

        formView.GridView.AddToolBarAction(btnAbout);

        var btnLog = new UrlRedirectAction
        {
            Name = "btnLog",
            ToolTip = StringLocalizer["Log"],
            Icon = IconType.FileTextO,
            ShowAsButton = true,
            UrlAsPopUp = true,
            PopUpTitle = StringLocalizer["Log"],
            UrlRedirect =UrlHelper.GetUrl("Index","Log", new {Area="DataDictionary"}),
            Order = 11,
            CssClass = BootstrapHelper.PullRight
        };

        formView.GridView.AddToolBarAction(btnLog);

        var btnSettings = new UrlRedirectAction
        {
            Name = "btnAppSettings",
            ToolTip = StringLocalizer["Application Options"],
            Icon = IconType.Code,
            ShowAsButton = true,
            UrlAsPopUp = true,
            PopUpTitle = StringLocalizer["Application Options"],
            UrlRedirect = UrlHelper.GetUrl("Index","Options", new {Area="DataDictionary"}),
            Order = 12,
            CssClass = BootstrapHelper.PullRight
        };

        formView.GridView.AddToolBarAction(btnSettings);

        var btnResources = new UrlRedirectAction
        {
            Name = "btnResources",
            ToolTip = StringLocalizer["Resources"],
            Icon = IconType.Globe,
            ShowAsButton = true,
            UrlAsPopUp = true,
            PopUpTitle = StringLocalizer["Resources"],
            UrlRedirect = UrlHelper.GetUrl("Index","Resources", new {Area="DataDictionary"}),
            Order = 11,
            CssClass = BootstrapHelper.PullRight
        };

        formView.GridView.AddToolBarAction(btnResources);

        formView.GridView.AddToolBarAction(new SubmitAction()
        {
            Name = "btnDeleteMetadata",
            Order = 0,
            Icon = IconType.Trash,
            Text = StringLocalizer["Delete Selected"],
            IsGroup = false,
            ConfirmationMessage = StringLocalizer["Do you want to delete ALL selected records?"],
            ShowAsButton = true,
            FormAction = UrlHelper.GetUrl("Delete","Element", new {Area="DataDictionary"}),
        });

        formView.GridView.OnRenderAction += OnRenderAction;
        
        return formView;
    }

    private async Task FormViewOnDataLoad(object sender, GridDataLoadEventArgs e)
    {
        var filter = DataDictionaryFilter.GetInstance(e.Filters);
        string orderBy = string.IsNullOrEmpty(e.OrderBy) ? "name ASC" : e.OrderBy;
        var result = await DataDictionaryRepository.GetFormElementInfoListAsync(filter, orderBy, e.RegporPag, e.CurrentPage);
        e.DataSource = result.Data.ToDataTable();
        e.Tot = result.TotalOfRecords;
    }

    #endregion

    public byte[] ExportSingleRow(IDictionary<string, dynamic> row)
    {
        string dictionaryName = row["name"].ToString();
        var metadata = DataDictionaryRepository.GetMetadata(dictionaryName);

        string json = FormElementSerializer.Serialize(metadata);

        return Encoding.Default.GetBytes(json);
    }

    public byte[] ExportMultipleRows(List<IDictionary<string, dynamic>> selectedRows)
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var element in selectedRows)
            {
                string dictionaryName = element["name"].ToString();
                var metadata = DataDictionaryRepository.GetMetadata(dictionaryName);
                string json = FormElementSerializer.Serialize(metadata);

                var jsonFile = archive.CreateEntry(dictionaryName + ".json");
                using var streamWriter = new StreamWriter(jsonFile.Open());
                streamWriter.Write(json);
            }
        }

        return memoryStream.ToArray();
    }

    public async Task<bool> Import(Stream file)
    {
        file.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(file);
        var dicParser = FormElementSerializer.Deserialize(await reader.ReadToEndAsync());

        //TODO: Validation
        //FormElement.Validate()

        await DataDictionaryRepository.InsertOrReplaceAsync(dicParser);


        return IsValid;
    }

    public void CreateStructureIfNotExists()
    {
        DataDictionaryRepository.CreateStructureIfNotExists();
    }
    
    private void OnRenderAction(object? sender, ActionEventArgs e)
    {
        var formName = e.FieldValues["name"]?.ToString();
        switch (e.Action.Name)
        {
            case "preview":
                e.LinkButton.OnClientClick =
                    $"window.open('{UrlHelper.GetUrl("Render", "Form", new { dictionaryName = formName, Area = "MasterData" })}', '_blank').focus();";
                break;
            case "tools":
                e.LinkButton.UrlAction = UrlHelper.GetUrl("Index", "Entity", new { dictionaryName = formName });
                e.LinkButton.OnClientClick = "";
                break;
            case "duplicate":
                e.LinkButton.UrlAction = UrlHelper.GetUrl("Duplicate", "Element", new { dictionaryName = formName });
                e.LinkButton.OnClientClick = "";
                break;
        }
    }
}