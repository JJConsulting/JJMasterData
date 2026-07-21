using System.Threading.Tasks;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.Extensions.Localization;


namespace JJMasterData.Core.DataDictionary.Services;

public class ElementService(
        IValidationDictionary validationDictionary,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository)
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
            element.Schema = schema;
            element.ConnectionId = connectionId;
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



    #endregion

    public Task DeleteAsync(string elementName)
    {
        return DataDictionaryRepository.DeleteAsync(elementName);
    }
}
