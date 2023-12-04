using System;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataDictionary.Services;

public class EntityService(IValidationDictionary validationDictionary,
        IDataDictionaryRepository dataDictionaryRepository,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : BaseService(validationDictionary, dataDictionaryRepository,stringLocalizer)
{
    private async Task<bool> ValidateEntity(Element formElement, string originName)
    {
        if (ValidateName(formElement.Name) && !originName.ToLower().Equals(formElement.Name.ToLower()))
        {
            if (await DataDictionaryRepository.ExistsAsync(formElement.Name))
                AddError("Name", StringLocalizer["There is already a dictionary with the name {0}",formElement.Name]);
        }

        if (string.IsNullOrEmpty(formElement.TableName))
            AddError("TableName", StringLocalizer["Required table name field"]);

    
        if (!string.IsNullOrEmpty(formElement.ReadProcedureName) &&
            !string.IsNullOrEmpty(formElement.WriteProcedureName))
        { 
            if (formElement.ReadProcedureName.ToLower().Equals(formElement.WriteProcedureName.ToLower()))
            {
                AddError("CustomProcNameGet", StringLocalizer["Procedure names cannot be identical"]);
            }
        }
                

        return IsValid;
    }


    public async Task<FormElement> EditEntityAsync(FormElement entity, string entityName)
    {
        var isValid = await ValidateEntity(entity, entityName);
        if (!isValid)
            return null;
        
        try
        {
            var formElement = await DataDictionaryRepository.GetFormElementAsync(entityName);

            formElement.Name = entity.Name;
            formElement.TableName = entity.TableName;
            formElement.ReadProcedureName = entity.ReadProcedureName;
            formElement.WriteProcedureName = entity.WriteProcedureName;
            formElement.Info = entity.Info;
            formElement.Title = entity.Title;
            formElement.SubTitle = entity.SubTitle;
            formElement.UseReadProcedure = entity.UseReadProcedure;
            formElement.UseWriteProcedure = entity.UseWriteProcedure;
            
            if (!entityName.Equals(formElement.Name))
            {
                await DataDictionaryRepository.DeleteAsync(entityName);
                await DataDictionaryRepository.InsertOrReplaceAsync(formElement);
            }
            else
            {
                await DataDictionaryRepository.InsertOrReplaceAsync(formElement);
            }

            return formElement;

        }
        catch (Exception ex)
        {
            AddError("Entity", ex.Message);
            return null;
        }

    }


}