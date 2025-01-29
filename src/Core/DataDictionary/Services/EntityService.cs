using System;
using System.Threading.Tasks;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataDictionary.Services;

public class EntityService(IValidationDictionary validationDictionary,
        IDataDictionaryRepository dataDictionaryRepository,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : DataDictionaryServiceBase(validationDictionary, dataDictionaryRepository,stringLocalizer)
{
    private async Task<bool> ValidateEntity(Entity entity, string originName)
    {
        if (ValidateName(entity.Name) && !originName.Equals(entity.Name, StringComparison.OrdinalIgnoreCase))
        {
            if (await DataDictionaryRepository.ExistsAsync(entity.Name))
                AddError("Name", StringLocalizer["There is already a dictionary with the name {0}",entity.Name]);
        }

        if (string.IsNullOrEmpty(entity.TableName))
            AddError("TableName", StringLocalizer["Required table name field"]);

    
        if (!string.IsNullOrEmpty(entity.ReadProcedureName) &&
            !string.IsNullOrEmpty(entity.WriteProcedureName))
        { 
            if (entity.ReadProcedureName.Equals(entity.WriteProcedureName, StringComparison.OrdinalIgnoreCase))
            {
                AddError("CustomProcNameGet", StringLocalizer["Procedure names cannot be identical"]);
            }
        }
                

        return IsValid;
    }


    public async Task<FormElement> EditEntityAsync(Entity entity, string entityName)
    {
        var isValid = await ValidateEntity(entity, entityName);
        if (!isValid)
            return null;
        
        try
        {
            var formElement = await DataDictionaryRepository.GetFormElementAsync(entityName);

            entity.SetFormElement(formElement);
            
            if (!entityName.Equals(formElement.Name))
            {
                await DataDictionaryRepository.DeleteAsync(entityName);

                await UpdateOldElementNameReferences(entityName, formElement);
            }

            await DataDictionaryRepository.InsertOrReplaceAsync(formElement);
            
            return formElement;

        }
        catch (Exception ex)
        {
            AddError("Entity", ex.Message);
            return null;
        }

    }

    private async Task UpdateOldElementNameReferences(string entityName, FormElement formElement)
    {
        var elementList = await DataDictionaryRepository.GetFormElementListAsync();
        bool updateElement = false;
        foreach (var element in elementList)
        {
            foreach (var relationship in element.Relationships)
            {
                if (relationship.ElementRelationship != null &&
                    relationship.ElementRelationship.ChildElement == entityName)
                {
                    relationship.ElementRelationship.ChildElement = formElement.Name;
                    updateElement = true;
                }
            }

            foreach (var field in element.Fields)
            {
                if (field.DataItem?.ElementMap is not null)
                {
                    if (field.DataItem.ElementMap.ElementName == entityName)
                    {
                        field.DataItem.ElementMap.ElementName = formElement.Name;
                        updateElement = true;
                    }
                }
            }

            if (updateElement)
            {
                await DataDictionaryRepository.InsertOrReplaceAsync(element);
                updateElement = false;
            }
        }
    }
}