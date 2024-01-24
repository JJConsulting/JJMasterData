using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataDictionary.Services;

public class FieldService(IValidationDictionary validationDictionary,
        IDataDictionaryRepository dataDictionaryRepository,
        IEnumerable<IExpressionProvider> expressionProviders,
        IMemoryCache memoryCache,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : BaseService(validationDictionary, dataDictionaryRepository,stringLocalizer)
{
    public async Task<bool> SaveFieldAsync(string elementName, FormElementField field, string originalName)
    {
        var formElement = await DataDictionaryRepository.GetFormElementAsync(elementName);

        RemoveUnusedProperties(field);

        if (field.DataFile != null)
        {
            field.DataFile.FolderPath = field.DataFile.FolderPath?.Trim();
        }

        var fieldIsValid =await ValidateFieldAsync(formElement, field, originalName);

        if (!fieldIsValid)
        {
            if (string.IsNullOrEmpty(field.Name))
                field.Name = originalName;

            return false;
        }

        if (formElement.Fields.Contains(originalName))
        {
            field.Actions = formElement.Fields[originalName].Actions;
            formElement.Fields[originalName] = field;
        }
        else
        {
            formElement.Fields.Add(field);
        }

        formElement.Fields[field.Name] = field;

        if (IsValid)
        {
            await DataDictionaryRepository.InsertOrReplaceAsync(formElement);
            if (!formElement.UseReadProcedure)
                memoryCache.Remove(formElement.Name + "_ReadScript");
            if (!formElement.UseWriteProcedure)
                memoryCache.Remove(formElement.Name + "_WriteScript");
        }
            

        return IsValid;
    }

    private static void RemoveUnusedProperties(FormElementField field)
    {
        if (field.Component is FormComponent.ComboBox or FormComponent.Search or FormComponent.Lookup or FormComponent.RadioButtonGroup)
        {
            switch (field.DataItem!.DataItemType)
            {
                case DataItemType.ElementMap:
                    field.DataFile = null;
                    field.DataItem.Command = null;
                    field.DataItem.Items = null;
                    break;
                case DataItemType.Manual:
                    field.DataFile = null;
                    field.DataItem.Command = null;
                    field.DataItem.ElementMap = null;
                    break;
                case DataItemType.SqlCommand:
                    field.DataFile = null;
                    field.DataItem.ElementMap = null;
                    field.DataItem.Items = null;
                    break;
            }
        }
        else if (field.Component == FormComponent.File)
        {
            field.DataItem = null;
        }
        else
        {
            field.DataItem = null;
            field.DataFile = null;
        }
    }

    private async Task<bool> ValidateFieldAsync(FormElement formElement, FormElementField field, string originalName)
    {
        ValidateName(field.Name);

        if (!string.IsNullOrEmpty(field.Name) && !field.Name.Equals(originalName))
        {
            if (formElement.Fields.Contains(field.Name))
                AddError(nameof(field.Name), StringLocalizer["Name of field already exists"]);
        }

        ValidateExpressions(field);
        
        if (field.DataType is FieldType.Varchar or FieldType.NVarchar)
        {
            if (field.Size is < -1 or 0)
                AddError(nameof(field.Size), StringLocalizer["Field size must be equal to -1 or larger than 0"]);
        }
        else
        {
            if (field.Filter.Type is FilterMode.MultValuesContain or FilterMode.MultValuesEqual)
            {
                AddError(nameof(field.Filter.Type),StringLocalizer["MULTVALUES filters are only allowed for text type fields"]);
            }
        }


        var isNotIdentity = field.AutoNum && field.DataType is not FieldType.Int &&
                            field.DataType is not FieldType.UniqueIdentifier;
        
        if (isNotIdentity)
            AddError(nameof(field.AutoNum),
                StringLocalizer[
                    "Field with AutoNum (auto increment) must be of data type int, unencrypted and required"]);

        if (field.DataType != FieldType.Varchar && 
            field.DataType != FieldType.NVarchar && 
            field.DataType != FieldType.Text && 
            field.DataType != FieldType.NText)
        {
            if (field.Filter.Type is FilterMode.Contain)
            {
                AddError(nameof(field.Filter.Type),
                    StringLocalizer["Only fields of type VarChar or Text can be of type Contains."]);
            }
        }

        if (field.DataType is FieldType.Bit && field.Component != FormComponent.CheckBox)
        {
            AddError(nameof(field.Filter.Type),
                StringLocalizer["Bit fields can only be a checkbox (true or false.)"]);
        }

        if (field.Component is FormComponent.Number or FormComponent.Currency)
        {
            if (field.NumberOfDecimalPlaces > 0)
            {
                if (field.DataType != FieldType.Float)
                {
                    AddError(nameof(field.DataType),
                        StringLocalizer["The field [NumberOfDecimalPlaces] cannot be defined with the type "] +
                        field.DataType);
                }

                if (field.IsPk)
                    AddError(nameof(field.DataType),
                        StringLocalizer["The primary key field must not contain [NumberOfDecimalPlaces]"]);
            }
            else
            {
                return IsValid;
            }
        }
        else if (field.Component is FormComponent.Lookup or FormComponent.ComboBox or FormComponent.Search or FormComponent.RadioButtonGroup)
        {
            await ValidateDataItemAsync(field);
        }
        else if (field.Component == FormComponent.File)
        {
            ValidateDataFile(field.DataBehavior, field.DataFile);
        }

        return IsValid;
    }

    private void ValidateExpressions(FormElementField field)
    {
        if (string.IsNullOrWhiteSpace(field.VisibleExpression))
            AddError(nameof(field.VisibleExpression), StringLocalizer["Required [VisibleExpression] field"]);
        
        else if (!ValidateExpression(field.VisibleExpression, expressionProviders.GetBooleanProvidersPrefixes()))
            AddError(nameof(field.VisibleExpression), StringLocalizer["Invalid [VisibleExpression] field"]);

        if (string.IsNullOrWhiteSpace(field.EnableExpression))
            AddError(nameof(field.EnableExpression), StringLocalizer["Required [EnableExpression] field"]);
        else if (!ValidateExpression(field.EnableExpression, expressionProviders.GetBooleanProvidersPrefixes()))
            AddError(nameof(field.EnableExpression), StringLocalizer["Invalid [EnableExpression] field"]);

        if (!string.IsNullOrEmpty(field.DefaultValue))
        {
            if (!ValidateExpression(field.DefaultValue, expressionProviders.GetAsyncProvidersPrefixes()))
                AddError(nameof(field.DefaultValue), StringLocalizer["Invalid [DefaultValue] field"]);
        }

        if (!string.IsNullOrEmpty(field.TriggerExpression))
        {
            if (!ValidateExpression(field.TriggerExpression, expressionProviders.GetAsyncProvidersPrefixes()))
                AddError(nameof(field.TriggerExpression), StringLocalizer["Invalid [TriggerExpression] field"]);
        }
    }

    private Task ValidateDataItemAsync(FormElementField field)
    {
        var dataItem = field.DataItem;
        if (dataItem == null)
        {
            AddError("DataItem", StringLocalizer["DataItem cannot be empty."]);
            return Task.CompletedTask;
        }

        if (dataItem.DataItemType == DataItemType.SqlCommand)
        {
            if (dataItem.Command == null)
            {
                AddError("Command", StringLocalizer["[Command] required"]);
                return Task.CompletedTask;
            }
               
            if (string.IsNullOrEmpty(dataItem.Command.Sql))
                AddError("Command.Sql", StringLocalizer["[Field Command.Sql] required"]);

            if (dataItem.GridBehavior is not DataItemGridBehavior.Id && !dataItem.Command.Sql.Contains("{SearchId}"))
            {
                AddError("Command.Sql", "{SearchId} is required at queries not using GridBehavior.Id " +
                                        "Check <a href=\"https://portal.jjconsulting.com.br/jjdoc/articles/errors/search_id.html\">the docs</a> for more information.");
            }
        }
        else if (dataItem.DataItemType == DataItemType.Manual)
        {
            RemoveError("DataItem.Command.Sql");
            ValidateManualItens(dataItem.Items);
        }
        else if (dataItem.DataItemType == DataItemType.ElementMap)
        {
            return ValidateDataElementMapAsync(field);
        }

        return Task.CompletedTask;
    }

    private void ValidateManualItens(IList<DataItemValue> itens)
    {
        if (itens == null || itens.Count == 0)
        {
            AddError("DataItem", StringLocalizer["Item list not defined"]);
        }

        if (itens != null)
            for (int i = 0; i < itens.Count; i++)
            {
                var it = itens[i];
                if (string.IsNullOrEmpty(it.Id))
                    AddError("DataItem", StringLocalizer["Item id {0} required", i]);

                if (string.IsNullOrEmpty(it.Description))
                    AddError("DataItem", StringLocalizer["Item description {0} required", i]);
            }
    }

    private async Task ValidateDataElementMapAsync(FormElementField field)
    {
        var dataItem = field.DataItem;
        
        var elementMap = dataItem?.ElementMap;

        if (elementMap is null)
        {
            AddError(nameof(elementMap), $"{nameof(DataElementMap)} cannot be null.");
            return;
        }
        
        if (string.IsNullOrEmpty(elementMap.ElementName))
        {
            AddError(nameof(elementMap.ElementName), StringLocalizer["Required field [ElementName]"]);
            return;
        }
        
        var childFormElement = await DataDictionaryRepository.GetFormElementAsync(elementMap.ElementName);

        if (childFormElement is null)
        {
            AddError(nameof(elementMap.ElementName),$"Element {elementMap.ElementName} not found at your data source.");
            return;
        }

        var childField = childFormElement.Fields[elementMap.IdFieldName];

        if (field.DataType != childField.DataType && field.DataBehavior is FieldBehavior.Real)
            AddError(nameof(elementMap.DescriptionFieldName), StringLocalizer["[FieldId] DataType must be the same of your field."]);
        
        if(dataItem.ShowIcon && elementMap.IconIdFieldName is null)
            AddError(nameof(elementMap.DescriptionFieldName), StringLocalizer["[FieldIconId] is required."]);
        
        if (elementMap.IdFieldName.Equals(elementMap.DescriptionFieldName))
            AddError(nameof(elementMap.DescriptionFieldName), StringLocalizer["[FieldDescription] can not be equal a [FieldId]"]);
        
        if (dataItem.GridBehavior is DataItemGridBehavior.Description or DataItemGridBehavior.IconWithDescription && string.IsNullOrEmpty(elementMap.DescriptionFieldName))
            AddError(nameof(dataItem.GridBehavior), StringLocalizer["[GridBehavior] requires a [FieldDescription]"]);
    }

    private void ValidateDataFile(FieldBehavior dataBehavior, FormElementDataFile dataFile)
    {
        if (dataFile == null)
        {
            AddError("DataFile", StringLocalizer["Undefined file settings"]);
        }

        if (dataBehavior == FieldBehavior.Virtual)
            AddError("DataFile", StringLocalizer["Fields of type FILE cannot be virtual"]);

        if (string.IsNullOrEmpty(dataFile?.FolderPath))
            AddError(nameof(dataFile.FolderPath), StringLocalizer["Field [{nameof(dataFile.FolderPath)}] required"]);

        if (string.IsNullOrEmpty(dataFile?.AllowedTypes))
            AddError(nameof(dataFile.AllowedTypes), StringLocalizer["Required [AllowedTypes] field"]);

        if (dataFile!.MultipleFile & dataFile.ExportAsLink)
            AddError(nameof(dataFile.ExportAsLink),
                StringLocalizer["The [ExportAsLink] field cannot be enabled with [MultipleFile]"]);
    }

    public async Task<bool> SortFieldsAsync(string elementName, IEnumerable<string> fieldsOrder)
    {
        var formElement = await DataDictionaryRepository.GetFormElementAsync(elementName);
        
        SortFields(fieldsOrder, formElement);

        await DataDictionaryRepository.InsertOrReplaceAsync(formElement);
        return true;
    }

    private static void SortFields(IEnumerable<string> fieldsOrder, FormElement formElement)
    {
        var newList = fieldsOrder.Select(fieldName => formElement.Fields[fieldName]).ToList();

        for (int i = 0; i < formElement.Fields.Count; i++)
        {
            formElement.Fields[i] = newList[i];
        }
    }

    public async Task<bool> AddElementMapFilterAsync(FormElementField field, DataElementMapFilter elementMapFilter)
    {
        var elementMap = field.DataItem!.ElementMap;

        if (string.IsNullOrEmpty(elementMapFilter.FieldName))
            AddError(nameof(elementMapFilter.FieldName), StringLocalizer["Required filter field"]);

        if (!string.IsNullOrEmpty(elementMapFilter.ExpressionValue) &&
            !elementMapFilter.ExpressionValue.Contains("val:") &&
            !elementMapFilter.ExpressionValue.Contains("exp:") &&
            !elementMapFilter.ExpressionValue.Contains("sql:") &&
            !elementMapFilter.ExpressionValue.Contains("protheus:"))
        {
            AddError(nameof(elementMapFilter.ExpressionValue), StringLocalizer["Invalid filter field"]);
        }

        if (string.IsNullOrEmpty(elementMap.IdFieldName))
            AddError(nameof(elementMap.IdFieldName),
                StringLocalizer["Required [{0}] field", StringLocalizer["Field Key"]]);

        if (string.IsNullOrEmpty(elementMap.ElementName))
            AddError(nameof(elementMap.ElementName), StringLocalizer["Required [{0}] field", StringLocalizer["Field Key"]]);

        if (IsValid)
        {
            var childElement = await DataDictionaryRepository.GetFormElementAsync(elementMap.ElementName);
            var fieldKey = childElement.Fields[elementMap.IdFieldName];
            if (!fieldKey.IsPk & fieldKey.Filter.Type == FilterMode.None)
            {
                string err = StringLocalizer["Field [{0}] invalid, as it is not PK or not configured as a filter",
                    elementMap.IdFieldName];
                AddError(nameof(elementMap.IdFieldName), err);
            }
        }

        if (!IsValid) 
            return false;
        
        field.DataItem.ElementMap.MapFilters.Add(elementMapFilter);
            
        return true;
    }

    public async Task<bool> DeleteField(string elementName, string fieldName)
    {
        var formElement = await DataDictionaryRepository.GetFormElementAsync(elementName);
        if (!formElement.Fields.Contains(fieldName))
            return false;
        
        var field = formElement.Fields[fieldName];
        formElement.Fields.Remove(field);
        await DataDictionaryRepository.InsertOrReplaceAsync(formElement);

        return IsValid;
    }

    public async Task<string> GetNextFieldNameAsync(string elementName, string fieldName)
    {
        var formElement = await DataDictionaryRepository.GetFormElementAsync(elementName);
        string nextField = null;
        if (formElement.Fields.Contains(fieldName))
        {
            var currentField = formElement.Fields[fieldName];
            int iIndex = formElement.Fields.IndexOf(currentField.Name);
            if (iIndex >= 0 && iIndex < formElement.Fields.Count - 1)
            {
                nextField = formElement.Fields[iIndex + 1].Name;
            }
        }

        return nextField;
    }

    public async Task<Dictionary<string, string>> GetElementFieldListAsync(DataElementMap elementMap)
    {
        var fields = new Dictionary<string, string> { { string.Empty, StringLocalizer["--Select--"] } };

        if (string.IsNullOrEmpty(elementMap.ElementName))
            return fields;

        var dataEntry = await DataDictionaryRepository.GetFormElementAsync(elementMap.ElementName);
        if (dataEntry == null)
            return fields;

        foreach (var field in dataEntry.Fields)
        {
            fields.Add(field.Name, field.Name);
        }

        return fields;
    }

    public async Task<bool> CopyFieldAsync(FormElement formElement, FormElementField field)
    {
        var newField = ObjectCloner.DeepCopy(field);

        if (formElement.Fields.Contains(newField.Name))
        {
            AddError(newField.Name, StringLocalizer["Name of field already exists"]);
            return IsValid;
        }

        formElement.Fields.Add(newField);
        await DataDictionaryRepository.InsertOrReplaceAsync(formElement);
        return IsValid;
    }
}