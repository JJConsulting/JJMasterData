#nullable enable

using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Data.Extensions;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DI;
using JJMasterData.Core.Web.Components;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration.Options;

namespace JJMasterData.Core.DataDictionary.Services;

public class ElementService : BaseService
{
    private readonly IEntityRepository _entityRepository;
    private readonly JJMasterDataCommonsOptions _options;
    public ElementService(IValidationDictionary validationDictionary, 
                          IOptions<JJMasterDataCommonsOptions> commonsOptions,
                          IEntityRepository entityRepository, 
                          IDataDictionaryRepository dataDictionaryRepository) 
        : base(validationDictionary, dataDictionaryRepository)
    {
        _entityRepository = entityRepository;
        _options = commonsOptions.Value;
    }

    #region Exec Scripts GET/SET/TABLE

    public async Task<List<string?>> GetScriptsListAsync(string id)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(id);
        Element element = formElement;

        var addedFields = await GetAddedFieldsAsync(element).ToListAsync();
        
        var listScripts = new List<string?>
        {
            _entityRepository.GetScriptCreateTable(element),
            _entityRepository.GetScriptReadProcedure(element),
            _entityRepository.GetScriptWriteProcedure(element),
            _entityRepository.GetAlterTableScript(element, addedFields),
        };

        return listScripts;
    }
    
    public async IAsyncEnumerable<ElementField> GetAddedFieldsAsync(Element element)
    {
        if (!await _entityRepository.TableExistsAsync(element.TableName))
            yield break;
        
        foreach (var field in element.Fields.Where(f => f.DataBehavior == FieldBehavior.Real))
        {
            if (!await _entityRepository.ColumnExistsAsync(element.TableName, field.Name))
            {
                yield return field;
            }
        }
    }
    
    public async Task ExecuteScriptsAsync(string id, string scriptOption)
    {
        var dictionary = await DataDictionaryRepository.GetMetadataAsync(id);
        var element = dictionary;

        switch (scriptOption)
        {
            case "ExecuteProcedures":
                var sql = new StringBuilder();
                sql.AppendLine(_entityRepository.GetScriptWriteProcedure(element));
                sql.AppendLine(_entityRepository.GetScriptReadProcedure(element));
                await _entityRepository.ExecuteBatchAsync(sql.ToString());
                break;
            case "ExecuteCreateDataModel":
                await _entityRepository.CreateDataModelAsync(element);
                break;
            case "ExecuteAlterTable":
                var addedFields = await GetAddedFieldsAsync(element).ToListAsync();
                await _entityRepository.ExecuteBatchAsync(_entityRepository.GetAlterTableScript(element,addedFields));
                break;
        }
    }
    #endregion

    #region Add Dictionary

    public Element CreateEntity(string tableName, bool importFields)
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
                AddError("Name", Translate.Key("There is already a dictionary with the name ") + tableName);
            }
        }

        if (importFields & IsValid)
        {
            var dataAccess = JJService.EntityRepository;
            if (!dataAccess.TableExists(tableName))
                AddError("Name", Translate.Key("Table not found"));

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
            AddError("Name", Translate.Key("Mandatory dictionary name field"));
        }

        if (DataDictionaryRepository.Exists(name))
        {
            AddError("Name", Translate.Key("There is already a dictionary with the name ") + name);
        }

        return IsValid;
    }


    public JJGridView GetFormView()
    {
        var element = new Element(JJServiceCore.Options.DataDictionaryTableName, "Data Dictionaries");
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
        
        var gridView = new JJGridView(formElement)
        {
            Name = "List",
            FilterAction =
            {
                ExpandedByDefault = true
            }
        };
        
        gridView.MaintainValuesOnLoad = true;
        gridView.EnableMultSelect = true;
        gridView.ExportAction.SetVisible(false);
        
        if (!gridView.CurrentFilter.Contains("type"))
            gridView.CurrentFilter.Add("type", "F");

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

    #region Class Source Code Generation
    public string GetClassSourceCode(string dicName)
    {
        string prop = "public @PropType @PropName { get; set; } ";

        var dicParser = DataDictionaryRepository.GetMetadata(dicName);
        var propsBuilder = new StringBuilder();

        foreach (var item in dicParser.Fields.ToList())
        {
            var nameProp = StringManager.GetStringWithoutAccents(item.Name.Replace(" ", "").Replace("-", " ").Replace("_", " "));
            var typeProp = GetTypeProp(item.DataType, item.IsRequired);
            var propField = prop.Replace("@PropName", ToCamelCase(nameProp)).Replace("@PropType", typeProp);

            propsBuilder.AppendLine($"\t[JsonProperty( \"{item.Name}\")] ");
            propsBuilder.AppendLine($"\t[Display(Name = \"{item.Label}\")]");
            propsBuilder.AppendLine("\t"+propField);
            propsBuilder.AppendLine("");

        }

        var resultClass = new StringBuilder();

        resultClass.AppendLine($"public class {dicParser.Name}" + "\r\n{");
        resultClass.AppendLine(propsBuilder.ToString());
        resultClass.AppendLine("\r\n}");

        return resultClass.ToString();
    }

    private string GetTypeProp(FieldType dataTypeField, bool required)
    {
        return dataTypeField switch
        {
            FieldType.Date or FieldType.DateTime or FieldType.DateTime2 => "DateTime",
            FieldType.Float => "double",
            FieldType.Int => "int",
            FieldType.NText or FieldType.NVarchar or FieldType.Text or FieldType.Varchar => required ? "string" : "string?",
            _ => "",
        };
    }

    private string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        
        string formattedValue = string.Empty;
        value.Split(' ').ToList().ForEach(x => formattedValue += x.FirstCharToUpper());

        return formattedValue;

    }
    #endregion

    public byte[] ExportSingleRow(IDictionary row)
    {
        string dictionaryName = row["name"].ToString();
        var metadata = DataDictionaryRepository.GetMetadata(dictionaryName);

        string json = FormElementSerializer.Serialize(metadata);

        return Encoding.Default.GetBytes(json);
    }

    public byte[] ExportMultipleRows(List<Hashtable> selectedRows)
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