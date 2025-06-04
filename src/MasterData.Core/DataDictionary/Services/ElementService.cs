#nullable enable

using System;
using System.Globalization;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Structure;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.Tasks;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;


namespace JJMasterData.Core.DataDictionary.Services;

public class ElementService(
        IFormElementComponentFactory<JJFormView> formViewFactory,
        IValidationDictionary validationDictionary,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        DataDictionaryFormElementFactory dataDictionaryFormElementFactory,
        DateService dateService,
        IUrlHelper urlHelper)
    : DataDictionaryServiceBase(validationDictionary, dataDictionaryRepository, stringLocalizer)
{
    #region Add Dictionary

    public async Task<FormElement?> CreateEntityAsync(ElementBean elementBean)
    {
        var tableName = elementBean.Name;
        var schema = elementBean.Schema;
        var importFields = elementBean.ImportFields;
        var connectionId = elementBean.ConnectionId;
        
        if (!await ValidateEntityAsync(elementBean))
            return null;

        FormElement formElement;
        if (importFields)
        {
            var element = await entityRepository.GetElementFromTableAsync(schema, tableName, connectionId);
            element.Name = MasterDataCommonsOptions.RemoveTbPrefix(tableName);
            formElement = new FormElement(element);
        }
        else
        {
            formElement = new FormElement
            {
                Schema = schema,
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

        if (importFields && IsValid)
        {
            var exists = await entityRepository.TableExistsAsync(elementBean.Schema, tableName, connectionId);
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
        var formView = formViewFactory.Create(dataDictionaryFormElementFactory.GetFormElement());
        
        formView.GridView.SetCurrentFilter(DataDictionaryStructure.Type, "F");

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
                var info = args.DataRow[DataDictionaryStructure.Info]?.ToString();
                if (!string.IsNullOrWhiteSpace(info))
                {
                    args.HtmlResult!.AppendSpan(span =>
                    {
                        span.WithCssClass("fa fa-question-circle help-description");
                        span.WithToolTip(info);
                    });
                }
            }
            
            if (args.Field.Name == DataDictionaryStructure.TableName)
            {
                var tableName = args.DataRow[DataDictionaryStructure.TableName]?.ToString();
                args.HtmlResult = new HtmlBuilder(HtmlTag.Span)
                    .WithCssClass("font-monospace")
                    .AppendText(tableName);
            }
            
            if (args.Field.Name == DataDictionaryStructure.LastModified)
            {
                var lastModified = (DateTime)args.DataRow[DataDictionaryStructure.LastModified]!;
                args.HtmlResult = new HtmlBuilder(HtmlTag.Span)
                    .WithToolTip(lastModified.ToString(CultureInfo.CurrentCulture))
                    .AppendText(dateService.GetPhrase(lastModified));
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
                        $"window.open('{urlHelper.Action("Render", "Form", new {Area="MasterData", elementName })}', '_blank').focus();";
                    break;
                case "tools":
                    args.LinkButton.UrlAction = urlHelper.Action("Index", "Entity", 
                        new { Area="DataDictionary", elementName });
                    args.LinkButton.OnClientClick = "";
                    break;
                case "duplicate":
                    args.LinkButton.UrlAction = urlHelper.Action("Duplicate", "Element", 
                        new { Area="DataDictionary", elementName });
                    args.LinkButton.OnClientClick = "";
                    break;
            }

            return ValueTaskHelper.CompletedTask;
        };
        
        return formView;
    }
    

    #endregion

    public Task DeleteAsync(string? elementName)
    {
        return DataDictionaryRepository.DeleteAsync(elementName);
    }
}