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
using JJMasterData.Core.DataDictionary.Factories;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Options;
using JJMasterData.Core.UI.Components;
using Microsoft.Extensions.Localization;
using JJMasterData.Core.Web;

namespace JJMasterData.Core.DataDictionary.Services;

public class ElementService : BaseService
{
    private IFormElementComponentFactory<JJFormView> FormViewFactory { get; }
    private DataDictionaryFormElementFactory DataDictionaryFormElementFactory { get; }
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
        DataDictionaryFormElementFactory dataDictionaryFormElementFactory,
        JJMasterDataUrlHelper urlHelper
        )
        : base(validationDictionary, dataDictionaryRepository, stringLocalizer)
    {
        FormViewFactory = formViewFactory;
        DataDictionaryFormElementFactory = dataDictionaryFormElementFactory;
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
            dicname = tablename[3..];
        else if (tablename.ToLower().StartsWith("tb"))
            dicname = tablename[2..];
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


    public JJFormView GetFormView()
    {
        var formView = FormViewFactory.Create(DataDictionaryFormElementFactory.GetFormElement());
        formView.GridView.SetCurrentFilter("type","F");

        formView.GridView.EnableMultiSelect = true;
        
        formView.GridView.FilterAction.ExpandedByDefault = true;
        formView.GridView.OnDataLoadAsync += async (sender, args) =>
        {
            var filter = DataDictionaryFilter.GetInstance(args.Filters);
            string orderBy = string.IsNullOrEmpty(args.OrderBy) ? "name ASC" : args.OrderBy;
            var result =
                await DataDictionaryRepository.GetFormElementInfoListAsync(filter, orderBy, args.RecordsPerPage,
                    args.CurrentPage);
            args.DataSource = result.Data.ToDataTable();
            args.TotalOfRecords = result.TotalOfRecords;
        };

        formView.GridView.OnRenderAction += (sender, args) =>
        {
            var formName = args.FieldValues["name"]?.ToString();
            switch (args.Action.Name)
            {
                case "preview":
                    args.LinkButton.OnClientClick =
                        $"window.open('{UrlHelper.GetUrl("Render", "Form", "MasterData", new { dictionaryName = formName })}', '_blank').focus();";
                    break;
                case "tools":
                    args.LinkButton.UrlAction = UrlHelper.GetUrl("Index", "Entity", "DataDictionary",
                        new { dictionaryName = formName });
                    args.LinkButton.OnClientClick = "";
                    break;
                case "duplicate":
                    args.LinkButton.UrlAction = UrlHelper.GetUrl("Duplicate", "Element", "DataDictionary",
                        new { dictionaryName = formName });
                    args.LinkButton.OnClientClick = "";
                    break;
            }
        };
        
        return formView;
    }
    

    #endregion

    public byte[] ExportSingleRow(IDictionary<string, object> row)
    {
        string dictionaryName = row["name"].ToString();
        var metadata = DataDictionaryRepository.GetMetadata(dictionaryName);

        string json = FormElementSerializer.Serialize(metadata);

        return Encoding.Default.GetBytes(json);
    }

    public byte[] ExportMultipleRows(List<IDictionary<string, object>> selectedRows)
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
}