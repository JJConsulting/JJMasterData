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
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Structure;
using JJMasterData.Core.Http;
using JJMasterData.Core.UI.Components;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Services;

public class ElementService(IFormElementComponentFactory<JJFormView> formViewFactory,
        IValidationDictionary validationDictionary,
        IOptions<MasterDataCoreOptions> options,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        DataDictionaryFormElementFactory dataDictionaryFormElementFactory,
        MasterDataUrlHelper urlHelper)
    : BaseService(validationDictionary, dataDictionaryRepository, stringLocalizer)
{
    private IFormElementComponentFactory<JJFormView> FormViewFactory { get; } = formViewFactory;
    private DataDictionaryFormElementFactory DataDictionaryFormElementFactory { get; } = dataDictionaryFormElementFactory;
    private MasterDataUrlHelper UrlHelper { get; } = urlHelper;
    private IEntityRepository EntityRepository { get; } =  entityRepository;

    private readonly MasterDataCoreOptions _options = options.Value;

    #region Add Dictionary

    public async Task<Element?> CreateEntityAsync(string tableName, bool importFields)
    {
        if (!await ValidateEntityAsync(tableName, importFields))
            return null;

        Element element;
        if (importFields)
        {
            element = await EntityRepository.GetElementFromTableAsync(tableName);
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
            var exists = await EntityRepository.TableExistsAsync(tableName);
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

    public async Task<bool> DuplicateEntityAsync(string originalElementName, string newName)
    {
        bool originalElementExists = await DataDictionaryRepository.ExistsAsync(originalElementName);
        if (!originalElementExists)
        {
            AddError("OriginalElementName", StringLocalizer["Original Element Name {0} does not exists", originalElementName]);
        }
            
        
        if (await ValidateEntityAsync(newName))
        {
            var dicParser = await DataDictionaryRepository.GetFormElementAsync(originalElementName);
            dicParser.Name = newName;
            await DataDictionaryRepository.InsertOrReplaceAsync(dicParser);
        }

        return IsValid;
    }

    private async Task<bool> ValidateEntityAsync(string name)
    {
        var isNullOrWhitespace = string.IsNullOrWhiteSpace(name);
        switch (isNullOrWhitespace)
        {
            case true:
                AddError("Name", StringLocalizer["[New Element Name] field is required."]);
                break;
            case false when await DataDictionaryRepository.ExistsAsync(name):
                AddError("Name", StringLocalizer["Element {0} already exists.", name]);
                break;
        }

        return IsValid;
    }


    public JJFormView GetFormView()
    {
        var formView = FormViewFactory.Create(DataDictionaryFormElementFactory.GetFormElement());
        
        formView.GridView.SetCurrentFilter(DataDictionaryStructure.Type,"F");

        if (!formView.GridView.CurrentOrder.Any())
        {
            formView.GridView.CurrentOrder.AddOrReplace(DataDictionaryStructure.LastModified, OrderByDirection.Desc);
        }
        
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

        formView.GridView.OnRenderActionAsync += (_, args) =>
        {
            var elementName = args.FieldValues["name"]?.ToString();
            
            switch (args.ActionName)
            {
                case "render":
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

    public async Task<byte[]> ExportMultipleRowsAsync(List<Dictionary<string, object>> selectedRows)
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

    public Task CreateStructureIfNotExistsAsync()
    {
        return DataDictionaryRepository.CreateStructureIfNotExistsAsync();
    }
}