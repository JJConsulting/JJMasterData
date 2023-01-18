using JJMasterData.Commons.DI;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Options;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Services.Abstractions;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DI;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.DataDictionary.Services;

public class ElementService : BaseService
{
    private readonly IEntityRepository _entityRepository;
    public ElementService(IValidationDictionary validationDictionary, 
                          IEntityRepository entityRepository, 
                          IDataDictionaryRepository dataDictionaryRepository) 
        : base(validationDictionary, dataDictionaryRepository)
    {
        _entityRepository = entityRepository;
    }

    #region Exec Scripts GET/SET/TABLE

    public List<string> GetScriptsDictionary(string id)
    {
        var dictionary = DataDictionaryRepository.GetMetadata(id);
        var element = dictionary.Table;
        var listScripts = new List<string>
        {
            _entityRepository.GetScriptCreateTable(element),
            _entityRepository.GetScriptReadProcedure(element),
            _entityRepository.GetScriptWriteProcedure(element)
        };

        return listScripts;
    }


    public void ExecScripts(string id, string scriptExec)
    {
        var dictionary = DataDictionaryRepository.GetMetadata(id);
        var element = dictionary.Table;

        switch (scriptExec)
        {
            case "Exec":
                var sql = new StringBuilder();
                sql.AppendLine(_entityRepository.GetScriptWriteProcedure(element));
                sql.AppendLine(_entityRepository.GetScriptReadProcedure(element));
                _entityRepository.ExecuteBatch(sql.ToString());
                break;
            case "ExecAll":
                _entityRepository.CreateDataModel(element);
                break;
        }
    }

    public void ExecScriptsMasterData()
    {
        DataDictionaryRepository.CreateStructureIfNotExists();
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
            element  = new Element
            {
                TableName = tableName,
                Name = GetDictionaryName(tableName),
                CustomProcNameGet = JJMasterDataCommonsOptions.GetReadProcedureName(tableName),
                CustomProcNameSet = JJMasterDataCommonsOptions.GetWriteProcedureName(tableName)
            };
        }
        
        var metadata = new Metadata
        {
            Table = element.DeepCopy(),
            MetadataForm = importFields ? new MetadataForm(new FormElement(element)) : new MetadataForm()
        };

        DataDictionaryRepository.InsertOrReplace(metadata);

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
            dicParser.Table.Name = newName;
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
        formElement.Fields[DataDictionaryStructure.Sync].DataItem.Items.Add(new DataItemValue("1", "Yes"));
        formElement.Fields[DataDictionaryStructure.Sync].DataItem.Items.Add(new DataItemValue("0", "No"));
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
        
        if (!gridView.CurrentFilter.ContainsKey("type"))
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

        foreach (var item in dicParser.Table.Fields.ToList())
        {
            var nameProp = StringManager.GetStringWithoutAccents(item.Name.Replace(" ", "").Replace("-", " ").Replace("_", " "));
            var typeProp = GetTypeProp(item.DataType, item.IsRequired);
            var propField = prop.Replace("@PropName", ToCamelCase(nameProp)).Replace("@PropType", typeProp);

            propsBuilder.AppendLine($"\t[DataMember(Name = \"{item.Name}\")] ");
            propsBuilder.AppendLine($"\t[Display(Name = \"{item.Label}\")]");
            propsBuilder.AppendLine("\t"+propField);
            propsBuilder.AppendLine("");

        }

        var resultClass = new StringBuilder();

        resultClass.AppendLine($"public class {dicParser.Table.Name}" + "\r\n{");
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

    public byte[] ExportSingleRow(Hashtable row)
    {
        string dictionaryName = row["name"].ToString();
        var metadata = DataDictionaryRepository.GetMetadata(dictionaryName);

        string json = JsonConvert.SerializeObject(metadata, Formatting.Indented);

        return Encoding.ASCII.GetBytes(json);
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
                string json = JsonConvert.SerializeObject(metadata, Formatting.Indented);

                var jsonFile = archive.CreateEntry(dictionaryName + ".json");
                using var streamWriter = new StreamWriter(jsonFile.Open());
                streamWriter.Write(json);
            }
        }
        return memoryStream.ToArray();
    }

    public bool Import(Stream file)
    {
        file.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(file);
        var dicParser = JsonConvert.DeserializeObject<Metadata>(reader.ReadToEnd());
        DataDictionaryRepository.InsertOrReplace(dicParser);
        //TODO: Validation
        //AddError("Name", "Campo nome do dicionário obrigatório");

        return IsValid;
    }

    public void CreateStructureIfNotExists()
    {
        DataDictionaryRepository.CreateStructureIfNotExists();
    }
}