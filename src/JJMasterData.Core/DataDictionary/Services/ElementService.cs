using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Options;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Services.Abstractions;
using JJMasterData.Core.WebComponents;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace JJMasterData.Core.DataDictionary.Services;

public class ElementService : BaseService
{
    private readonly IEntityRepository _entityRepository;
    public ElementService(IValidationDictionary validationDictionary, 
                          IEntityRepository entityRepository, 
                          IDictionaryRepository dictionaryRepository) 
        : base(validationDictionary, dictionaryRepository)
    {
        _entityRepository = entityRepository;
    }

    #region Exec Scripts GET/SET/TABLE

    public List<string> GetScriptsDictionary(string id)
    {
        var dictionary = DictionaryRepository.GetMetadata(id);
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
        var dictionary = DictionaryRepository.GetMetadata(id);
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
        DictionaryRepository.ExecInitialSetup();
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
                CustomProcNameGet = JJMasterDataOptions.GetReadProcedureName(tableName),
                CustomProcNameSet = JJMasterDataOptions.GetWriteProcedureName(tableName)
            };
        }
        
        var metadata = new Metadata
        {
            Table = element.DeepCopy(),
            Form = importFields ? new MetadataForm(new FormElement(element)) : new MetadataForm()
        };

        DictionaryRepository.InsertOrReplace(metadata);

        return element;
    }

    public bool ValidateEntity(string tableName, bool importFields)
    {
        if (ValidateName(tableName))
        {
            if (DictionaryRepository.Exists(tableName))
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
            var dicParser = DictionaryRepository.GetMetadata(originName);
            dicParser.Table.Name = newName;
            DictionaryRepository.InsertOrReplace(dicParser);

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

        if (DictionaryRepository.Exists(name))
        {
            AddError("Name", Translate.Key("There is already a dictionary with the name ") + name);
        }

        return IsValid;
    }


    public JJFormView GetFormView()
    {
        var element = MetadataStructure.GetElement();
        var formElement = new FormElement(element);

        formElement.Title = "JJMasterData";
        formElement.Fields["name"].VisibleExpression = "exp:{pagestate} <> 'FILTER'";
        formElement.Fields["namefilter"].VisibleExpression = "exp:{pagestate} = 'FILTER'";

        formElement.Fields["json"].VisibleExpression = "exp:{pagestate} = 'VIEW'";
        formElement.Fields["json"].Component = FormComponent.TextArea;
        formElement.Fields["json"].Export = false;

        formElement.Fields["type"].VisibleExpression = "val:0";
        formElement.Fields["type"].DefaultValue = "val:F";
        formElement.Fields["type"].Component = FormComponent.ComboBox;
        formElement.Fields["type"].DataItem.Items.Add(new DataItemValue("F", "Form"));
        formElement.Fields["type"].DataItem.Items.Add(new DataItemValue("T", "Table"));

        formElement.Fields["owner"].VisibleExpression = "exp:{pagestate} = 'VIEW'";

        formElement.Fields["sync"].VisibleExpression = "exp:{pagestate} <> 'FILTER'";
        formElement.Fields["sync"].Component = FormComponent.ComboBox;
        formElement.Fields["sync"].DataItem.Items.Add(new DataItemValue("1", "Yes"));
        formElement.Fields["sync"].DataItem.Items.Add(new DataItemValue("0", "No"));

        formElement.Fields["modified"].Component = FormComponent.DateTime;

        var formView = new JJFormView(formElement);
        formView.Name = "List";
        formView.FilterAction.ExpandedByDefault = true;
        formView.DataPanel.UISettings.FormCols = 2;
        formView.MaintainValuesOnLoad = true;
        formView.EnableMultSelect = true;
        formView.ExportAction.SetVisible(false);
        formView.EditAction.SetVisible(false);
        formView.InsertAction.SetVisible(false);
        formView.DeleteAction.SetVisible(false);
        formView.DeleteSelectedRowsAction.SetVisible(true);
        formView.DeleteSelectedRowsAction.ConfirmationMessage = "Are you sure you want to DELETE all selected records?";

        formView.ViewAction.IsGroup = true;
        formView.ViewAction.Text = "Details";
        formView.ViewAction.ToolTip = null;

        if (!formView.CurrentFilter.ContainsKey("type"))
            formView.CurrentFilter.Add("type", "F");

        formView.OnDataLoad += FormView_OnDataLoad;

        return formView;
    }

    private void FormView_OnDataLoad(object sender, FormEvents.Args.GridDataLoadEventArgs e)
    {
        int tot = e.Tot;
        e.DataSource = DictionaryRepository.GetDataTable(e.Filters, e.OrderBy, e.RegporPag, e.CurrentPage, ref tot);
        e.Tot = tot;
    }


    #endregion

    #region Class Source Code Generation
    public string GetClassSourceCode(string dicName)
    {
        string prop = "public @PropType @PropName { get; set; } ";

        var dicParser = DictionaryRepository.GetMetadata(dicName);
        var propsBuilder = new StringBuilder();

        foreach (var item in dicParser.Table.Fields.ToList())
        {
            var nameProp = StringManager.NoAccents(item.Name.Replace(" ", "").Replace("-", " ").Replace("_", " "));
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
            FieldType.Date or FieldType.DateTime => "DateTime",
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

    public byte[] Export(List<Hashtable> selectedRows)
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var element in selectedRows)
            {
                string dictionaryName = element["name"].ToString();
                var dicParser = DictionaryRepository.GetMetadata(dictionaryName);
                string json = JsonConvert.SerializeObject(dicParser, Formatting.Indented);

                var jsonFile = archive.CreateEntry(dictionaryName + ".json");
                using var streamWriter = new StreamWriter(jsonFile.Open());
                streamWriter.Write(json);
            }
        }
        return memoryStream.ToArray();
    }

    public bool Import(Stream file)
    {
        string json = new StreamReader(file).ReadToEnd();
        var dicParser = JsonConvert.DeserializeObject<Metadata>(json);
        DictionaryRepository.InsertOrReplace(dicParser);
        //TODO: Validation
        //AddError("Name", "Campo nome do dicionário obrigatório");

        return IsValid;
    }

    public bool JJMasterDataTableExists()
    {
        if (DictionaryRepository is DictionaryDao)
        {
            return JJService.EntityRepository.TableExists(JJService.Options.TableName);
        }

        return true;
    }
}