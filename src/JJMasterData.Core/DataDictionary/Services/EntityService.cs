using System;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary.Services.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class EntityService : BaseService
{
    public EntityService(IValidationDictionary validationDictionary) :base(validationDictionary)
    {
    }

    private bool ValidateEntity(FormElement formElement, string originName)
    {
        if (ValidateName(formElement.Name) && !originName.ToLower().Equals(formElement.Name.ToLower()))
        {
            if (DicDao.HasDictionary(formElement.Name))
                AddError("Name", Translate.Key("There is already a dictionary with the name " + formElement.Name));
        }

        if (string.IsNullOrEmpty(formElement.TableName))
            AddError("TableName", Translate.Key("Required table name field"));

    
        if (!string.IsNullOrEmpty(formElement.CustomProcNameGet) &&
            !string.IsNullOrEmpty(formElement.CustomProcNameSet))
        { 
            if (formElement.CustomProcNameGet.ToLower().Equals(formElement.CustomProcNameSet.ToLower()))
            {
                AddError("CustomProcNameGet", Translate.Key("Procedure names cannot be identical"));
            }
        }
                

        return IsValid;
    }


    public FormElement EditEntity(FormElement formElement, string entityName)
    {
        if (!ValidateEntity(formElement, entityName))
            return null;



        try
        {
            var dicParser = DicDao.GetDictionary(entityName);

            dicParser.Table.Name = formElement.Name;
            dicParser.Table.TableName = formElement.TableName;
            dicParser.Table.CustomProcNameGet = formElement.CustomProcNameGet;
            dicParser.Table.CustomProcNameSet = formElement.CustomProcNameSet;
            dicParser.Table.Info = formElement.Info;
            dicParser.Form.Title = formElement.Title;
            dicParser.Form.SubTitle = formElement.SubTitle;

            if (!entityName.Equals(formElement.Name))
            {
                DicDao.DelDictionary(entityName);
                DicDao.SetDictionary(dicParser);
            }
            else
            {
                DicDao.SetDictionary(dicParser);
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