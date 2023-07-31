#nullable enable

using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Data.Extensions;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.Web.Components;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Core.Options;
using JJMasterData.Core.UI.Components;
using Microsoft.Extensions.Localization;
using JJMasterData.Core.UI.Components.GridView;

namespace JJMasterData.Core.DataDictionary.Services;

public class ElementService : BaseService
{
    private IFormElementComponentFactory<JJGridView> GridViewFactory { get; }
    private readonly IEntityRepository _entityRepository;
    private readonly JJMasterDataCoreOptions _options;

    public ElementService(IFormElementComponentFactory<JJGridView> gridViewFactory,
        IValidationDictionary validationDictionary, 
                          IOptions<JJMasterDataCoreOptions> options,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
                          IEntityRepository entityRepository, 
                          IDataDictionaryRepository dataDictionaryRepository) 
        : base(validationDictionary, dataDictionaryRepository,stringLocalizer)
    {
        GridViewFactory = gridViewFactory;
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
            element  = new FormElement
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


    public async Task<JJGridView> GetGridView()
    {
        var element = new Element(_options.DataDictionaryTableName, "Data Dictionaries");
        element.Fields.AddPK(DataDictionaryStructure.Name, "Dictionary Name", FieldType.NVarchar, 64, false, FilterMode.Equal);
        element.Fields.Add(DataDictionaryStructure.TableName, "Table Name", FieldType.NVarchar, 64, false, FilterMode.MultValuesContain);
        element.Fields.Add(DataDictionaryStructure.Info, "Info", FieldType.NVarchar, 150, false, FilterMode.None);
        element.Fields.Add(DataDictionaryStructure.Sync, "Sync", FieldType.Varchar, 1, false, FilterMode.None);
        element.Fields.Add(DataDictionaryStructure.LastModified, "Last Modified", FieldType.DateTime, 15, true, FilterMode.Range);

        var formElement = new FormElement(element);
        formElement.Fields[DataDictionaryStructure.Sync].VisibleExpression = "exp:{pagestate} <> 'FILTER'";
        formElement.Fields[DataDictionaryStructure.Sync].Component = FormComponent.ComboBox;
        var dataItem = new FormElementDataItem();
        dataItem.Items.Add(new DataItemValue("1", "Yes"));
        dataItem.Items.Add(new DataItemValue("0", "No"));
        formElement.Fields[DataDictionaryStructure.Sync].DataItem = dataItem;
        formElement.Fields[DataDictionaryStructure.LastModified].Component = FormComponent.DateTime;
        formElement.Title = "JJMasterData";
        
        formElement.Options.GridTableActions.Clear();

        var gridView = GridViewFactory.Create(formElement);
        gridView.Name = "List";
        gridView.FilterAction.ExpandedByDefault = true;

        gridView.MaintainValuesOnLoad = true;
        gridView.EnableMultiSelect = true;
        gridView.ExportAction.SetVisible(false);

        var filter = await gridView.GetCurrentFilterAsync();
        
        if (!filter.ContainsKey("type"))
            filter.Add("type", "F");

        gridView.OnDataLoad += FormViewOnDataLoad;

        return gridView;
    }

    private void FormViewOnDataLoad(object sender, FormEvents.Args.GridDataLoadEventArgs e)
    {
        int tot = e.Tot;
        var filter = DataDictionaryFilter.GetInstance(e.Filters);
        string orderBy = string.IsNullOrEmpty(e.OrderBy) ? "name ASC" : e.OrderBy;
        var list = DataDictionaryRepository.GetMetadataInfoList(filter, orderBy, e.RegporPag, e.CurrentPage, ref tot); 
        e.DataSource = list.ToDataTable();
        e.Tot = tot;
    }

    #endregion

    public byte[] ExportSingleRow(IDictionary<string,dynamic>row)
    {
        string dictionaryName = row["name"].ToString();
        var metadata = DataDictionaryRepository.GetMetadata(dictionaryName);

        string json = FormElementSerializer.Serialize(metadata);

        return Encoding.Default.GetBytes(json);
    }

    public byte[] ExportMultipleRows(List<IDictionary<string,dynamic>> selectedRows)
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