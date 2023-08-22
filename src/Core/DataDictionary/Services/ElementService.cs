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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataDictionary.Factories;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Options;
using JJMasterData.Core.UI.Components;
using Microsoft.Extensions.Localization;
using JJMasterData.Core.Web;
using Newtonsoft.Json;

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
                Name = GetDictionaryName(tableName),
                CustomProcNameGet = _options.GetReadProcedureName(tableName),
                CustomProcNameSet = _options.GetWriteProcedureName(tableName)
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

    public async Task<bool> DuplicateEntityAsync(string originName, string newName)
    {
        if (await ValidateEntityAsync(newName))
        {
            var dicParser = await DataDictionaryRepository.GetMetadataAsync(originName);
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
        formView.GridView.SetCurrentFilter("type","F");

        formView.GridView.EnableMultiSelect = true;
        
        formView.GridView.FilterAction.ExpandedByDefault = true;
        formView.GridView.OnDataLoadAsync += async (_, args) =>
        {
            var filter = DataDictionaryFilter.FromDictionary(args.Filters);
            var result =
                await DataDictionaryRepository.GetFormElementInfoListAsync(filter, args.OrderBy, args.RecordsPerPage,
                    args.CurrentPage);

            var dictionaryList = new List<Dictionary<string, dynamic?>>();
            
            foreach (var info in result.Data)
            {
                var dictionary = new Dictionary<string, dynamic?>
                {
                    { "info", info.Info },
                    { "type", "F"},
                    { "modified", info.Modified },
                    { "name", info.Name },
                    { "sync", info.Sync },
                    { "tablename", info.TableName }
                };
    
                dictionaryList.Add(dictionary);
            }

            args.DataSource = dictionaryList;
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

    public async Task<byte[]> ExportSingleRowAsync(IDictionary<string, object> row)
    {
        var dictionaryName = row["name"].ToString()!;
        var metadata = await DataDictionaryRepository.GetMetadataAsync(dictionaryName);

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
                var dictionaryName = element["name"].ToString()!;
                var metadata = await DataDictionaryRepository.GetMetadataAsync(dictionaryName);
                
                var json = FormElementSerializer.Serialize(metadata,settings =>
                {
                    settings.Formatting = Formatting.Indented;
                });

                var jsonFile = archive.CreateEntry(dictionaryName + ".json");
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