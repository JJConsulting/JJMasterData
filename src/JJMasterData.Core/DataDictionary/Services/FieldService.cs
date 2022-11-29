using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Services.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class FieldService : BaseService
{
    public FieldService(IValidationDictionary validationDictionary, IDictionaryRepository dictionaryRepository)
        : base(validationDictionary, dictionaryRepository)
    {
    }

    public bool SaveField(string elementName, FormElementField field, string originalName)
    {
        var dictionary = DictionaryRepository.GetMetadata(elementName);
        var formElement = dictionary.GetFormElement();

        RemoveUnusedProperties(ref field);

        if (field.DataFile != null)
        {
            field.DataFile.MaxFileSize *= 1000000;
            field.DataFile.FolderPath = field.DataFile.FolderPath.Trim();
        }

        if (!ValidateFields(formElement, field, originalName))
        {
            if (string.IsNullOrEmpty(field.Name))
                field.Name = originalName;

            return false;
        }

        if (formElement.FormFields.Contains(originalName))
        {
            field.Actions = formElement.FormFields[originalName].Actions;
            formElement.FormFields[originalName] = field;
        }
        else
        {
            formElement.FormFields.Add(field);
        }

        formElement.FormFields[field.Name] = field;
        dictionary.SetFormElement(formElement);
        DictionaryRepository.InsertOrReplace(dictionary);

        return IsValid;
    }

    private void RemoveUnusedProperties(ref FormElementField field)
    {
        if (field.Component == FormComponent.ComboBox |
            field.Component == FormComponent.Search |
            field.Component == FormComponent.Lookup)
        {
            switch (field.DataItem.DataItemType)
            {
                case DataItemType.Dictionary:
                    field.DataFile = null;
                    field.DataItem.Command = null;
                    field.DataItem.Items.Clear();
                    break;
                case DataItemType.Manual:
                    field.DataFile = null;
                    field.DataItem.Command = null;
                    field.DataItem.ElementMap = null;
                    break;
                case DataItemType.SqlCommand:
                    field.DataFile = null;
                    field.DataItem.ElementMap = null;
                    field.DataItem.Items.Clear();
                    break;
            }
        }
        else if (field.Component != FormComponent.Number && field.Component != FormComponent.Slider)
        {
            field.MinValue = null;
            field.MaxValue = null;
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

    public bool ValidateFields(FormElement formElement, FormElementField field, string originalName)
    {
        ValidateName(field.Name);

        if (!string.IsNullOrEmpty(field.Name) && !field.Name.Equals(originalName))
        {
            if (formElement.FormFields.Contains(field.Name))
                AddError(nameof(field.Name), Translate.Key("Name of field already exists"));
        }

        ValidateExpressions(field);

        if (field.DataType is FieldType.Varchar or FieldType.NVarchar)
        {
            if (field.Size <= 0)
                AddError(nameof(field.Size), Translate.Key("Invalid [Size] field"));
        }
        else
        {
            if (field.Filter.Type is FilterMode.MultValuesContain or FilterMode.MultValuesEqual)
            {
                AddError(nameof(field.Filter.Type),
                    Translate.Key("MULTVALUES filters are only allowed for text type fields"));
            }
        }

        if (field.AutoNum && field.DataType != FieldType.Int)
            AddError(nameof(field.AutoNum), Translate.Key("Field with AutoNum (auto increment) must be of data type int, unencrypted and required"));

        if (field.DataType is not FieldType.Varchar or FieldType.NVarchar or FieldType.NText)
        {
            if (field.Filter.Type is FilterMode.Contain)
            {
                AddError(nameof(field.Filter.Type), Translate.Key("Only fields of type VarChar or Text can be of type Contains."));
            }
        }

        if (field.Component is FormComponent.Number or FormComponent.Currency)
        {
            if (field.NumberOfDecimalPlaces > 0)
            {
                if (field.DataType != FieldType.Float)
                {
                    AddError(nameof(field.DataType),
                        Translate.Key("The field [NumberOfDecimalPlaces] cannot be defined with the type ") +
                        field.DataType);
                }

                if (field.IsPk)
                    AddError(nameof(field.DataType),
                        Translate.Key("The primary key field must not contain [NumberOfDecimalPlaces]"));
            }
            else
            {
                return IsValid;
            }
        }
        else if (field.Component is FormComponent.Lookup or FormComponent.ComboBox or FormComponent.Search)
        {
            ValidateDataItem(field.DataItem);
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
            AddError(nameof(field.VisibleExpression), Translate.Key("Required [VisibleExpression] field"));
        else if (!ValidateExpression(field.VisibleExpression, "val:", "exp:"))
            AddError(nameof(field.VisibleExpression), Translate.Key("Invalid [VisibleExpression] field"));

        if (string.IsNullOrWhiteSpace(field.EnableExpression))
            AddError(nameof(field.EnableExpression), Translate.Key("Required [EnableExpression] field"));
        else if (!ValidateExpression(field.EnableExpression, "val:", "exp:"))
            AddError(nameof(field.EnableExpression), Translate.Key("Invalid [EnableExpression] field"));

        if (!string.IsNullOrEmpty(field.DefaultValue))
        {
            if (!ValidateExpression(field.DefaultValue, "val:", "exp:", "sql:", "protheus:"))
                AddError(nameof(field.DefaultValue), Translate.Key("Invalid [DefaultValue] field"));
        }

        if (!string.IsNullOrEmpty(field.TriggerExpression))
        {
            if (!ValidateExpression(field.TriggerExpression, "val:", "exp:", "sql:", "protheus:"))
                AddError(nameof(field.TriggerExpression), Translate.Key("Invalid [TriggerExpression] field"));
        }
    }

    private bool ValidateDataItem(FormElementDataItem data)
    {
        if (data == null)
        {
            AddError("DataItem", Translate.Key("Undefined font settings"));
            return false;
        }

        switch (data.DataItemType)
        {
            case DataItemType.SqlCommand:
                {
                    if (string.IsNullOrEmpty(data.Command.Sql))
                        AddError("Command.Sql", Translate.Key("[Field Command.Sql] required"));

                    if (data.ReplaceTextOnGrid && !data.Command.Sql.Contains("{search_id}"))
                    {
                        AddError("Command.Sql", "{search_id} is required at queries using ReplaceTextOnGrid. " +
                                               "Check <a href=\"https://portal.jjconsulting.com.br/jjdoc/articles/errors/jj002.html\">JJ002</a> for more information.");
                    }
                    break;
                }
            case DataItemType.Manual:
                ValidateManualItens(data.Items);
                break;
            case DataItemType.Dictionary:
                ValidateDataElementMap(data.ElementMap);
                break;
        }

        return IsValid;
    }

    private bool ValidateManualItens(List<DataItemValue> itens)
    {
        if (itens == null || itens.Count == 0)
        {
            AddError("DataItem", Translate.Key("Item list not defined"));
            return false;
        }

        for (int i = 0; i < itens.Count; i++)
        {
            var it = itens[i];
            if (string.IsNullOrEmpty(it.Id))
                AddError("DataItem", Translate.Key("Item id {0} required", i));

            if (string.IsNullOrEmpty(it.Description))
                AddError("DataItem", Translate.Key("Item description {0} required", i));
        }

        return IsValid;
    }

    private bool ValidateDataElementMap(DataElementMap data)
    {
        if (data == null)
        {
            AddError("ElementMap", Translate.Key("Undefined mapping settings"));
            return false;
        }

        if (string.IsNullOrEmpty(data.ElementName))
            AddError(nameof(data.ElementName), Translate.Key("Required field [ElementName]"));

        return IsValid;
    }

    private bool ValidateDataFile(FieldBehavior dataBehavior, FormElementDataFile dataFile)
    {
        if (dataFile == null)
        {
            AddError("DataFile", Translate.Key("Undefined file settings"));
            return false;
        }

        if (dataBehavior == FieldBehavior.Virtual)
            AddError("DataFile", Translate.Key("Fields of type FILE cannot be virtual"));

        if (string.IsNullOrEmpty(dataFile.FolderPath))
            AddError(nameof(dataFile.FolderPath), Translate.Key($"Field [{nameof(dataFile.FolderPath)}] required"));

        if (string.IsNullOrEmpty(dataFile.AllowedTypes))
            AddError(nameof(dataFile.AllowedTypes), Translate.Key("Required [AllowedTypes] field"));

        if (dataFile.MultipleFile & dataFile.ExportAsLink)
            AddError(nameof(dataFile.ExportAsLink),
                Translate.Key("The [ExportAsLink] field cannot be enabled with [MultipleFile]"));

        return IsValid;
    }

    public bool SortFields(string elementName, string[] orderFields)
    {
        var dictionary = DictionaryRepository.GetMetadata(elementName);
        var formElement = dictionary.GetFormElement();
        var newList = orderFields.Select(fieldName => formElement.FormFields[fieldName]).ToList();

        for (int i = 0; i < formElement.FormFields.Count; i++)
        {
            formElement.FormFields[i] = newList[i];
        }
        dictionary.SetFormElement(formElement);
        DictionaryRepository.InsertOrReplace(dictionary);
        return true;
    }

    public bool AddElementMapFilter(FormElementField field, DataElementMapFilter mapFilter)
    {
        var elementMap = field.DataItem.ElementMap;

        if (string.IsNullOrEmpty(mapFilter.FieldName))
            AddError(nameof(mapFilter.FieldName), Translate.Key("Required filter field"));

        if (!string.IsNullOrEmpty(mapFilter.ExpressionValue) &&
            !mapFilter.ExpressionValue.Contains("val:") &&
            !mapFilter.ExpressionValue.Contains("exp:") &&
            !mapFilter.ExpressionValue.Contains("sql:") &&
            !mapFilter.ExpressionValue.Contains("protheus:"))
        {
            AddError(nameof(mapFilter.ExpressionValue), Translate.Key("Invalid filter field"));
        }

        if (string.IsNullOrEmpty(elementMap.FieldKey))
            AddError(nameof(elementMap.FieldKey), Translate.Key("Required [{0}] field", Translate.Key("Field Key")));

        if (string.IsNullOrEmpty(elementMap.ElementName))
            AddError(nameof(elementMap.ElementName), Translate.Key("Required [{0}] field", Translate.Key("Field Key")));

        if (IsValid)
        {
            var dataEntry = DictionaryRepository.GetMetadata(elementMap.ElementName);
            var fieldKey = dataEntry.Table.Fields[elementMap.FieldKey];
            if (!fieldKey.IsPk & fieldKey.Filter.Type == FilterMode.None)
            {
                string err = Translate.Key("Field [{0}] invalid, as it is not PK or not configured as a filter",
                    elementMap.FieldKey);
                AddError(nameof(elementMap.FieldKey), err);
            }
        }

        if (IsValid)
        {
            field.DataItem.ElementMap.MapFilters.Add(mapFilter);
            return true;
        }

        return false;
    }

    public bool DeleteField(string dictionaryName, string fieldName)
    {
        var dictionary = DictionaryRepository.GetMetadata(dictionaryName);
        if (!dictionary.Table.Fields.ContainsKey(fieldName))
            return false;

        var formElement = dictionary.GetFormElement();
        var field = formElement.FormFields[fieldName];
        formElement.FormFields.Remove(field);
        dictionary.SetFormElement(formElement);
        DictionaryRepository.InsertOrReplace(dictionary);

        return IsValid;
    }

    public string GetNextFieldName(string dictionaryName, string fieldName)
    {
        var dictionary = DictionaryRepository.GetMetadata(dictionaryName);
        var element = dictionary.Table;
        string nextField = null;
        if (element.Fields.ContainsKey(fieldName))
        {
            var currentField = element.Fields[fieldName];
            int iIndex = element.Fields.IndexOf(currentField);
            if (iIndex >= 0 && iIndex < element.Fields.Count - 1)
            {
                nextField = element.Fields[iIndex + 1].Name;
            }
        }

        return nextField;
    }

    public Dictionary<string, string> GetElementFieldList(FormElementField currentField)
    {
        var dicFields = new Dictionary<string, string>();
        dicFields.Add(string.Empty, Translate.Key("--Select--"));

        var map = currentField.DataItem.ElementMap;
        if (string.IsNullOrEmpty(map.ElementName))
            return dicFields;

        var dataEntry = DictionaryRepository.GetMetadata(map.ElementName);
        if (dataEntry == null)
            return dicFields;

        foreach (var field in dataEntry.Table.Fields)
        {
            dicFields.Add(field.Name, field.Name);
        }

        return dicFields;
    }

    public bool CopyField(Metadata metadata, FormElementField field)
    {
        var formElement = metadata.GetFormElement();
        var newField = field.DeepCopy();

        if (formElement.FormFields.Contains(newField.Name))
        {
            AddError(newField.Name, Translate.Key("Name of field already exists"));
            return IsValid;
        }

        formElement.FormFields.Add(newField);
        metadata.SetFormElement(formElement);
        DictionaryRepository.InsertOrReplace(metadata);
        return IsValid;
    }

}