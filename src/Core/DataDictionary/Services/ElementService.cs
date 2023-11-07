#nullable enable

using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Structure;
using JJMasterData.Core.Http;
using JJMasterData.Core.UI.Components;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Services;

public class ElementService : BaseService
{
    private IFormElementComponentFactory<JJFormView> FormViewFactory { get; }
    private IEncryptionService EncryptionService { get; }
    private DataDictionaryFormElementFactory DataDictionaryFormElementFactory { get; }
    private MasterDataUrlHelper UrlHelper { get; }
    private readonly IEntityRepository _entityRepository;
    private readonly MasterDataCoreOptions _options;

    public ElementService(
        IFormElementComponentFactory<JJFormView> formViewFactory,
        IValidationDictionary validationDictionary,
        IOptions<MasterDataCoreOptions> options,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IEntityRepository entityRepository,
        IEncryptionService encryptionService,
        IDataDictionaryRepository dataDictionaryRepository,
        DataDictionaryFormElementFactory dataDictionaryFormElementFactory,
        MasterDataUrlHelper urlHelper
        )
        : base(validationDictionary, dataDictionaryRepository, stringLocalizer)
    {
        FormViewFactory = formViewFactory;
        EncryptionService = encryptionService;
        DataDictionaryFormElementFactory = dataDictionaryFormElementFactory;
        UrlHelper = urlHelper;
        _entityRepository = entityRepository;
        _options = options.Value;
    }

    #region Add Dictionary

    public async Task<Element?> CreateEntityAsync(string tableName, bool importFields)
    {
        if (!await ValidateEntityAsync(tableName, importFields))
            return null;

        Element element;
        if (importFields)
        {
            element = await _entityRepository.GetElementFromTableAsync(tableName);
        }
        else
        {
            element = new FormElement
            {
                TableName = tableName,
                Name = GetElementName(tableName),
                ReadProcedureName = _options.GetReadProcedureName(tableName),
                WriteProcedureName = _options.GetWriteProcedureName(tableName)
            };
        }

        await DataDictionaryRepository.InsertOrReplaceAsync(new FormElement(element));

        return element;
    }

    public async Task<bool> ValidateEntityAsync(string tableName, bool importFields)
    {
        if (ValidateName(tableName))
        {
            if (await DataDictionaryRepository.ExistsAsync(tableName))
            {
                AddError("Name", StringLocalizer["There is already a dictionary with the name "] + tableName);
            }
        }

        if (importFields & IsValid)
        {
            var exists = await _entityRepository.TableExistsAsync(tableName);
            if (!exists)
                AddError("Name", StringLocalizer["Table not found"]);
        }

        return IsValid;
    }

    public static string GetElementName(string tablename)
    {
        string elementName;
        if (tablename.ToLower().StartsWith("tb_"))
            elementName = tablename[3..];
        else if (tablename.ToLower().StartsWith("tb"))
            elementName = tablename[2..];
        else
            elementName = tablename;

        elementName = elementName.Replace('_', ' ');
        elementName = elementName.FirstCharToUpper();
        elementName = elementName.Replace(" ", "");
        
        return elementName;
    }

    #endregion

    #region Duplicate Entity

    public async Task<bool> DuplicateEntityAsync(string originName, string newName)
    {
        if (await ValidateEntityAsync(newName))
        {
            var dicParser = await DataDictionaryRepository.GetFormElementAsync(originName);
            dicParser.Name = newName;
            await DataDictionaryRepository.InsertOrReplaceAsync(dicParser);
        }

        return IsValid;
    }

    public async Task<bool> ValidateEntityAsync(string name)
    {
        if (!ValidateName(name))
            return false;

        if (string.IsNullOrWhiteSpace(name))
        {
            AddError("Name", StringLocalizer["Mandatory dictionary name field"]);
        }

        if (await DataDictionaryRepository.ExistsAsync(name))
        {
            AddError("Name", StringLocalizer["There is already a dictionary with the name "] + name);
        }

        return IsValid;
    }


    public JJFormView GetFormView()
    {
        var formView = FormViewFactory.Create(DataDictionaryFormElementFactory.GetFormElement());
        formView.GridView.SetCurrentFilter(DataDictionaryStructure.Type,"F");
        
        formView.GridView.EnableMultiSelect = true;
        
        formView.GridView.FilterAction.ExpandedByDefault = true;
        formView.GridView.OnDataLoadAsync += async (_, args) =>
        {
            var filter = DataDictionaryFilter.FromDictionary(args.Filters!);
            var result =
                await DataDictionaryRepository.GetFormElementInfoListAsync(filter, args.OrderBy, args.RecordsPerPage,
                    args.CurrentPage);

            var dictionaryList = result.Data.Select(info => info.ToDictionary()).ToList();

            args.DataSource = dictionaryList;
            args.TotalOfRecords = result.TotalOfRecords;
        };

        formView.GridView.OnRenderActionAsync += (sender, args) =>
        {
            var elementName = args.FieldValues["name"]?.ToString();
            
            switch (args.ActionName)
            {
                case "preview":
                    args.LinkButton.OnClientClick =
                        $"window.open('{UrlHelper.GetUrl("Render", "Form", "MasterData", new { elementName })}', '_blank').focus();";
                    break;
                case "tools":
                    args.LinkButton.UrlAction = UrlHelper.GetUrl("Index", "Entity", "DataDictionary",
                        new { elementName });
                    args.LinkButton.OnClientClick = "";
                    break;
                case "duplicate":
                    args.LinkButton.UrlAction = UrlHelper.GetUrl("Duplicate", "Element", "DataDictionary",
                        new { elementName });
                    args.LinkButton.OnClientClick = "";
                    break;
            }

            return Task.CompletedTask;
        };
        
        return formView;
    }
    

    #endregion

    public async Task<byte[]> ExportSingleRowAsync(IDictionary<string, object> row)
    {
        var elementName = row["name"].ToString();
        var metadata = await DataDictionaryRepository.GetFormElementAsync(elementName);

        var json = FormElementSerializer.Serialize(metadata, settings =>
        {
            settings.Formatting = Formatting.Indented;
        });

        return Encoding.Default.GetBytes(json);
    }

    public async Task<byte[]> ExportMultipleRowsAsync(List<IDictionary<string, object>> selectedRows)
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var element in selectedRows)
            {
                var elementName = element["name"].ToString();
                var metadata = await DataDictionaryRepository.GetFormElementAsync(elementName);
                
                var json = FormElementSerializer.Serialize(metadata,settings =>
                {
                    settings.Formatting = Formatting.Indented;
                });

                var jsonFile = archive.CreateEntry($"{elementName}.json");
#if NET
                await 
#endif
                using var streamWriter = new StreamWriter(jsonFile.Open());
                await streamWriter.WriteAsync(json);

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

    public async Task CreateStructureIfNotExistsAsync()
    {
       await DataDictionaryRepository.CreateStructureIfNotExistsAsync();
    }
}