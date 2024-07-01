#nullable enable

using JJMasterData.Commons.Localization;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Structure;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.Tasks;
using JJMasterData.Core.UI.Components;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Services;

public class ElementService(
        IFormElementComponentFactory<JJFormView> formViewFactory,
        IValidationDictionary validationDictionary,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        DataDictionaryFormElementFactory dataDictionaryFormElementFactory,
        IMasterDataUrlHelper urlHelper)
    : BaseService(validationDictionary, dataDictionaryRepository, stringLocalizer)
{
    private IFormElementComponentFactory<JJFormView> FormViewFactory { get; } = formViewFactory;
    private DataDictionaryFormElementFactory DataDictionaryFormElementFactory { get; } = dataDictionaryFormElementFactory;
    private IMasterDataUrlHelper UrlHelper { get; } = urlHelper;
    private IEntityRepository EntityRepository { get; } =  entityRepository;

    #region Add Dictionary

    public async Task<FormElement?> CreateEntityAsync(ElementBean elementBean)
    {
        var tableName = elementBean.Name;
        var importFields = elementBean.ImportFields;
        var connectionId = elementBean.ConnectionId;
        
        if (!await ValidateEntityAsync(elementBean))
            return null;

        FormElement formElement;
        if (importFields)
        {
            var element = await EntityRepository.GetElementFromTableAsync(tableName, connectionId);
            element.Name = MasterDataCommonsOptions.RemoveTbPrefix(tableName);
            formElement = new FormElement(element);
        }
        else
        {
            formElement = new FormElement
            {
                TableName = tableName,
                Name = MasterDataCommonsOptions.RemoveTbPrefix(tableName),
                ConnectionId = connectionId,
            };
        }

        await DataDictionaryRepository.InsertOrReplaceAsync(formElement);

        return formElement;
    }

    public async Task<bool> ValidateEntityAsync(ElementBean elementBean)
    {
        var tableName = elementBean.Name;
        var importFields = elementBean.ImportFields;
        var connectionId = elementBean.ConnectionId;
        
        if (ValidateName(tableName))
        {
            if (await DataDictionaryRepository.ExistsAsync(tableName))
            {
                AddError("Name", StringLocalizer["There is already a dictionary with the name "] + tableName);
            }
        }

        if (importFields & IsValid)
        {
            var exists = await EntityRepository.TableExistsAsync(tableName, connectionId);
            if (!exists)
                AddError("Name", StringLocalizer["Table not found"]);
        }

        return IsValid;
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
            formView.GridView.CurrentOrder.AddOrReplace(DataDictionaryStructure.Name, OrderByDirection.Asc);
        }

        formView.ShowTitle = false;
        formView.GridView.ShowTitle = false;
        formView.GridView.EnableMultiSelect = true;
        formView.GridView.FilterAction.ExpandedByDefault = true;
        
        formView.GridView.OnDataLoadAsync += async (_, args) =>
        {
            var filter = DataDictionaryFilter.FromDictionary(args.Filters!);
            var result =
                await DataDictionaryRepository.GetFormElementInfoListAsync(filter, args.OrderBy, args.RecordsPerPage,
                    args.CurrentPage);

            var dictionaryList = result.Data.ConvertAll(info => info.ToDictionary());

            args.DataSource = dictionaryList;
            args.TotalOfRecords = result.TotalOfRecords;
        };

        formView.GridView.OnRenderCellAsync += (_, args) =>
        {
            if (args.Field.Name == DataDictionaryStructure.Name)
            {
                if (args.DataRow.TryGetValue("info", out var info) && !string.IsNullOrWhiteSpace(info?.ToString()))
                {
                    args.HtmlResult.AppendSpan(span =>
                    {
                        span.WithCssClass("fa fa-question-circle help-description");
                        span.WithToolTip(info!.ToString());
                    });
                }
            }

            return ValueTaskHelper.CompletedTask;
        };

        formView.GridView.OnRenderActionAsync += (_, args) =>
        {
            var elementName = args.FieldValues["name"]?.ToString();
            
            switch (args.ActionName)
            {
                case "render":
                    args.LinkButton.OnClientClick =
                        $"window.open('{UrlHelper.Action("Render", "Form", new {Area="MasterData", elementName })}', '_blank').focus();";
                    break;
                case "tools":
                    args.LinkButton.UrlAction = UrlHelper.Action("Index", "Entity", 
                        new { Area="DataDictionary", elementName });
                    args.LinkButton.OnClientClick = "";
                    break;
                case "duplicate":
                    args.LinkButton.UrlAction = UrlHelper.Action("Duplicate", "Element", 
                        new { Area="DataDictionary", elementName });
                    args.LinkButton.OnClientClick = "";
                    break;
            }

            return ValueTaskHelper.CompletedTask;
        };
        
        return formView;
    }
    

    #endregion

    public async Task<byte[]> ExportSingleRowAsync(Dictionary<string, object> row)
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