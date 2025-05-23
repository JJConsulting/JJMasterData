﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Tasks;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataDictionary.Services;

public class FieldService(
        IValidationDictionary validationDictionary,
        IDataDictionaryRepository dataDictionaryRepository,
        IEntityRepository entityRepository,
        IEnumerable<IExpressionProvider> expressionProviders,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : DataDictionaryServiceBase(validationDictionary, dataDictionaryRepository,stringLocalizer)
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

    private async ValueTask<bool> ValidateFieldAsync(FormElement formElement, FormElementField field, string originalName)
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


        var isNotIdentity = field.AutoNum && field.DataType is not FieldType.Int &&
                            field.DataType is not FieldType.UniqueIdentifier;
        
        if (isNotIdentity)
        {
            AddError(nameof(field.AutoNum),
                StringLocalizer[
                    "Field with AutoNum (auto increment) must be of data type int, unencrypted and required"]);
        }

        if (field.DataType is FieldType.DateTime2 && field.Size is < 0 or > 7)
        {
            AddError(nameof(field.DataType), StringLocalizer["Field size must be between 0 and 7 for DateTime2"]);
        }
        
        if (field.DataType != FieldType.Varchar &&
            field.DataType != FieldType.NVarchar &&
            field.DataType != FieldType.Text &&
            field.DataType != FieldType.NText)
        {
            if (field.Filter.Type is FilterMode.Contain)
            {
                AddError(nameof(field.Filter.Type),
                    StringLocalizer["Only fields of type Varchar or Text can be of type Contains."]);
            }
        }

        if (field.DataType is not FieldType.Int && field.Component is FormComponent.Icon)
        {
            AddError(nameof(field.DataType), StringLocalizer["Icon components can only be of type Int"]);
        }

        if (field.DataType is FieldType.Bit && field.Component != FormComponent.CheckBox)
        {
            AddError(nameof(field.DataType),
                StringLocalizer["Bit fields can only be a checkbox (true or false.)"]);
        }

        if (field.NumberOfDecimalPlaces < 0 && field.DataType is FieldType.Decimal or FieldType.Float)
        {
            AddError(nameof(field.NumberOfDecimalPlaces), StringLocalizer["[Number of Decimal Places] cannot be lesser than 0."]);
        }

        if (field.DataType is FieldType.Decimal)
        {
            if (field.NumberOfDecimalPlaces > field.Size)
            {
                AddError(nameof(field.NumberOfDecimalPlaces), StringLocalizer["[Number of Decimal Places] cannot be greater than [Size]."]);
            }

            if (field.Size is < 1 or > 38)
            {
                AddError(nameof(field.Size), StringLocalizer["[Size] must be between 1 and 38."]);
            }
        }
        if (field.Component is FormComponent.CheckBox && field.DataType is not FieldType.Bit && field.DataType is not FieldType.Int)
        {
            AddError(nameof(field.DataType),
                StringLocalizer["Checkbox components can only be of type Int or Boolean"]);
        }

        if (field.Component is FormComponent.Number or FormComponent.Currency)
        {
            if (field.NumberOfDecimalPlaces > 0)
            {
                if (field.DataType is not FieldType.Float and not FieldType.Decimal)
                {
                    AddError(nameof(field.DataType),
                        StringLocalizer["The field [NumberOfDecimalPlaces] cannot be defined with the type "] +
                        field.DataType);
                }

                if (field.IsPk)
                {
                    AddError(nameof(field.DataType),
                        StringLocalizer["The primary key field must not contain [NumberOfDecimalPlaces]"]);
                }
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
        var syncProviders = expressionProviders.GetSyncProvidersPrefixes().ToArray();
        var asyncProviders = expressionProviders.GetAsyncProvidersPrefixes().ToArray();
        
        if (string.IsNullOrWhiteSpace(field.VisibleExpression))
            AddError(nameof(field.VisibleExpression), StringLocalizer["Required [VisibleExpression] field"]);
        if (!ValidateBooleanExpression(field.VisibleExpression))
            AddError(nameof(field.VisibleExpression), StringLocalizer["[{0}]: Valued boolean expression cannot contain boolean operators.", nameof(field.VisibleExpression)]);
        
        if (!ValidateBooleanExpression(field.EnableExpression))
            AddError(nameof(field.EnableExpression), StringLocalizer["[{0}]: Valued boolean expression cannot contain boolean operators.", nameof(field.EnableExpression)]);
        
        if (!ValidateExpression(field.VisibleExpression, syncProviders))
            AddError(nameof(field.VisibleExpression), StringLocalizer["Invalid [VisibleExpression] field"]);

        if (string.IsNullOrWhiteSpace(field.EnableExpression))
            AddError(nameof(field.EnableExpression), StringLocalizer["Required [EnableExpression] field"]);
        if (!ValidateExpression(field.EnableExpression, syncProviders))
            AddError(nameof(field.EnableExpression), StringLocalizer["Invalid [EnableExpression] field"]);
        
        if (!string.IsNullOrEmpty(field.DefaultValue))
        {
            if (!ValidateExpression(field.DefaultValue, asyncProviders))
                AddError(nameof(field.DefaultValue), StringLocalizer["Invalid [DefaultValue] field"]);
        }

        if (!string.IsNullOrEmpty(field.TriggerExpression))
        {
            if (!ValidateExpression(field.TriggerExpression, asyncProviders))
                AddError(nameof(field.TriggerExpression), StringLocalizer["Invalid [TriggerExpression] field"]);
        }
    }

    private ValueTask ValidateDataItemAsync(FormElementField field)
    {
        var dataItem = field.DataItem;
        if (dataItem == null)
        {
            AddError("DataItem", StringLocalizer["DataItem cannot be empty."]);
            return ValueTaskHelper.CompletedTask;
        }

        if (dataItem.DataItemType == DataItemType.SqlCommand)
        {
            if (dataItem.Command == null)
            {
                AddError("Command", StringLocalizer["[Command] required"]);
                return ValueTaskHelper.CompletedTask;
            }
               
            if (string.IsNullOrEmpty(dataItem.Command.Sql))
                AddError(nameof(FormElementDataItem.Command.Sql), StringLocalizer["[Field Command.Sql] required"]);

            if (dataItem.GridBehavior is not DataItemGridBehavior.Id 
                && (!dataItem.Command.Sql.Contains("{SearchId}") || dataItem.Command.Sql.Contains("--{SearchId}")))
            {
                AddError("DataItem.Command.Sql", StringLocalizer["{SearchId} is required at queries not using Id as Grid Behavior."] + "\n" +
                                        StringLocalizer["Check <a href=\"{0}\">the docs</a> for more information.","https://md.jjconsulting.tech/articles/tutorials/search_id.html"]);
            }
        }
        else if (dataItem.DataItemType == DataItemType.Manual)
        {
            RemoveError("DataItem.Command.Sql");
            ValidateManualItems(dataItem.Items);
        }
        else if (dataItem.DataItemType == DataItemType.ElementMap)
        {
            return ValidateDataElementMapAsync(field);
        }

        return ValueTaskHelper.CompletedTask;
    }

    private void ValidateManualItems(List<DataItemValue> items)
    {
        if (items == null || items.Count == 0)
        {
            AddError("DataItem", StringLocalizer["Item list not defined"]);
        }

        if (items != null)
            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                if (string.IsNullOrEmpty(it.Id))
                    AddError("DataItem", StringLocalizer["Item id {0} required", i]);

                if (string.IsNullOrEmpty(it.Description))
                    AddError("DataItem", StringLocalizer["Item description {0} required", i]);
            }
    }

    private async ValueTask ValidateDataElementMapAsync(FormElementField field)
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

        if (dataFile!.MultipleFile && dataFile.ExportAsLink)
        {
            AddError(nameof(dataFile.ExportAsLink),
                StringLocalizer["The [ExportAsLink] field cannot be enabled with [MultipleFile]"]);
        }
    }

    public async Task<bool> SortFieldsAsync(string elementName, string[] fieldsOrder)
    {
        var formElement = await DataDictionaryRepository.GetFormElementAsync(elementName);
        
        SortFields(formElement, fieldsOrder);

        await DataDictionaryRepository.InsertOrReplaceAsync(formElement);
        return true;
    }

    private static void SortFields(FormElement formElement, string[] fieldsOrder)
    {
        var newList = fieldsOrder.Select(fieldName => formElement.Fields[fieldName]).ToList();

        for (var i = 0; i < formElement.Fields.Count; i++)
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

        if (elementMap is null || field.DataItem.ElementMap is null)
        {
            AddError(nameof(elementMap.IdFieldName),
                StringLocalizer["Required [{0}] field", StringLocalizer["Element Map"]]);
            return IsValid;
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
            if (!fieldKey.IsPk && fieldKey.Filter.Type == FilterMode.None)
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

    public async ValueTask<Dictionary<string, string>> GetElementFieldListAsync(string elementName, bool recoverOnlyFilters = false)
    {
        var fields = new Dictionary<string, string> { { string.Empty, StringLocalizer["--Select--"] } };

        if (string.IsNullOrEmpty(elementName))
            return fields;

        var dataEntry = await DataDictionaryRepository.GetFormElementAsync(elementName);
        if (dataEntry == null)
            return fields;

        foreach (var field in dataEntry.Fields.OrderBy(e => e.Name))
        {
            if (recoverOnlyFilters)
            {
                if (field.IsPk || field.Filter.Type is FilterMode.Equal)
                {
                    fields.Add(field.Name, field.Name);
                }
            }
            else
            {
                fields.Add(field.Name, field.Name);
            }
        }

        return fields;
    }

    public async Task<bool> CopyFieldAsync(FormElement formElement, FormElementField field)
    {
        var newField = field.DeepCopy();

        if (formElement.Fields.Contains(newField.Name))
        {
            AddError(newField.Name, StringLocalizer["Name of field already exists"]);
            return IsValid;
        }

        formElement.Fields.Add(newField);
        await DataDictionaryRepository.InsertOrReplaceAsync(formElement);
        return IsValid;
    }

    public Task SetFormElementAsync(FormElement formElement)
    {
        return DataDictionaryRepository.InsertOrReplaceAsync(formElement);
    }

    public async Task ImportFieldsFromTable(string elementName)
    {
        var existingElement = await DataDictionaryRepository.GetFormElementAsync(elementName);
        var elementFromDb = await entityRepository.GetElementFromTableAsync(existingElement.Schema ?? "dbo", existingElement.TableName, existingElement.ConnectionId);
        
        var fields = elementFromDb.Fields;
        foreach (var field in fields)
        {
            if (!existingElement.Fields.Contains(field.Name))
            {
                existingElement.Fields.Add(new(field));
            }
        }

        await DataDictionaryRepository.InsertOrReplaceAsync(existingElement);
    }
}