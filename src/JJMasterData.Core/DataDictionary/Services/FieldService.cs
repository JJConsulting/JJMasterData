using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary.Services.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class FieldService : BaseService
{
    public FieldService(IValidationDictionary validationDictionary) : base(validationDictionary)
    {
    }

    public bool SaveField(string elementName, FormElementField field, string originalName)
    {
        var formElement = DicDao.GetFormElement(elementName);

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
        DicDao.SetFormElement(formElement);

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
                    field.DataItem.Itens.Clear();
                    break;
                case DataItemType.Manual:
                    field.DataFile = null;
                    field.DataItem.Command = null;
                    field.DataItem.ElementMap = null;
                    break;
                case DataItemType.SqlCommand:
                    field.DataFile = null;
                    field.DataItem.ElementMap = null;
                    field.DataItem.Itens.Clear();
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
            if (formElement.Fields.Contains(field.Name))
                AddError(nameof(field.Name), Translate.Key("Name of field already exists"));
        }

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

        if (field.DataType == FieldType.Varchar ||
            field.DataType == FieldType.NVarchar)
        {
            if (field.Size <= 0)
                AddError(nameof(field.Size), Translate.Key("Invalid [Size] field"));
        }
        else
        {
            if (field.Filter.Type == FilterMode.MultValuesContain ||
                field.Filter.Type == FilterMode.MultValuesEqual)
            {
                AddError(nameof(field.Filter.Type),
                    Translate.Key("MULTVALUES filters are only allowed for text type fields"));
            }
        }

        if (field.AutoNum && field.DataType != FieldType.Int)
            AddError(nameof(field.AutoNum), Translate.Key("Field with AutoNum (auto increment) must be of data type int, unencrypted and required"));
        

        if (field.Component == FormComponent.Number ||
            field.Component == FormComponent.Currency)
        {
            if (field.NumberOfDecimalPlaces > 0)
            {
                if (field.DataType != FieldType.Float)
                {
                    AddError(nameof(field.DataType),
                        Translate.Key("The field[NumberOfDecimalPlaces] cannot be defined with the type ") +
                        field.DataType);
                }

                if (field.IsPk)
                    AddError(nameof(field.DataType),
                        Translate.Key("The primary key field must not contain [NumberOfDecimalPlaces]"));
            }
        }
        else if (field.Component == FormComponent.Lookup |
                 field.Component == FormComponent.ComboBox |
                 field.Component == FormComponent.Search)
        {
            ValidateDataItem(field.DataItem);
        }
        else if (field.Component == FormComponent.File)
        {
            ValidateDataFile(field.DataBehavior, field.DataFile);
        }

        return IsValid;
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
                    AddError("Command.Sql","{search_id} is required at queries using ReplaceTextOnGrid. " +
                                           "Check <a href=\"https://portal.jjconsulting.com.br/jjdoc/articles/errors/jj002.html\">JJ002</a> for more information.");
                }
                break;
            }
            case DataItemType.Manual:
                ValidateManualItens(data.Itens);
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
        var formElement = DicDao.GetFormElement(elementName);
        
        var newList = orderFields.Select(fieldName => formElement.Fields[fieldName]).ToList();

        for (int i = 0; i < formElement.Fields.Count; i++)
        {
            formElement.Fields[i] = newList[i];
        }

        DicDao.SetFormElement(formElement);
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
            var dataEntry = DicDao.GetDictionary(elementMap.ElementName);
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

    public bool DeleteField(FormElement formElement, string fieldName)
    {
        var field = formElement.Fields[fieldName];
        formElement.Fields.Remove(field);
        DicDao.SetFormElement(formElement);

        return IsValid;
    }

    public string GetNextFieldName(FormElement formElement, string fieldName)
    {
        string nextField = null;
        if (formElement.Fields.Contains(fieldName))
        {
            int iIndex = formElement.Fields.IndexOf(fieldName);
            if (iIndex >= 0 && iIndex < formElement.Fields.Count - 1)
            {
                nextField = formElement.Fields[iIndex + 1].Name;
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

        var dataEntry = DicDao.GetDictionary(map.ElementName);
        if (dataEntry == null)
            return dicFields;

        foreach (var field in dataEntry.Table.Fields)
        {
            dicFields.Add(field.Name, field.Name);
        }

        return dicFields;
    }
}