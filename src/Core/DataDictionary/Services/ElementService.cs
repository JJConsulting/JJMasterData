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


    public async Task<JJFormView> GetFormViewAsync()
    {
        var formView = await FormViewFactory.CreateAsync(_options.DataDictionaryTableName);
        return formView;
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
}