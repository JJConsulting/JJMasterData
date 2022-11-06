using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JJMasterData.Core.DataManager;

public class FormManager
{
    private ExpressionManager _expression;
    private Hashtable _userValues;
    private Factory _factory;
    private IDataAccess _dataAccess;

    /// <inheritdoc cref="WebComponents.JJBaseView.UserValues"/>
    public Hashtable UserValues
    {
        get => _userValues ??= new Hashtable();
        set
        {
            _expression = null;
            _userValues = value;
        }
    }

    /// <inheritdoc cref="Commons.Dao.DataAccess"/>
    public IDataAccess DataAccess
    {
        get => _dataAccess ??= JJService.DataAccess;
        set => _dataAccess = value;
    }

    /// <inheritdoc cref="Commons.Dao.Entity.Factory"/>
    public Factory Factory
    {
        get => _factory ??= new Factory(DataAccess);
        set => _factory = value;
    }

    /// <inheritdoc cref="ExpressionManager"/>
    public ExpressionManager Expression => _expression ??= new(UserValues, DataAccess);

    /// <inheritdoc cref="FormElement"/>
    public FormElement FormElement { get; set; }


    public FormManager(FormElement formElement)
    {
        FormElement = formElement;
    }

    public FormManager(FormElement formElement, Hashtable userValues, IDataAccess dataAccess) : this(formElement)
    {
        UserValues = userValues;
        DataAccess = dataAccess;
    }


    /// <summary>
    /// Validates form fields and returns a list of errors found
    /// </summary>
    /// <param name="formValues">Form values</param>
    /// <param name="pageState">Context</param>
    /// <param name="enableErrorLink">Add html link in error fields</param>
    /// <returns>
    /// Key = Field name
    /// Value = Error message
    /// </returns>
    public Hashtable ValidateFields(Hashtable formValues, PageState pageState, bool enableErrorLink)
    {
        if (formValues == null)
            throw new ArgumentNullException(nameof(formValues));

        var errors = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

        foreach (var field in FormElement.Fields)
        {
            bool isVisible = Expression.GetBoolValue(field.VisibleExpression, field.Name, pageState, formValues);
            if (!isVisible)
                continue;

            bool isEnable = Expression.GetBoolValue(field.EnableExpression, field.Name, pageState, formValues);
            if (!isEnable)
                continue;

            string value;
            if (formValues.Contains(field.Name) && formValues[field.Name] != null)
                value = formValues[field.Name].ToString();
            else
                value = "";

            var error = FieldValidator.ValidateField(field, field.Name, value, enableErrorLink);
            if (!string.IsNullOrEmpty(error))
                errors.Add(field.Name, error);
        }
        return errors;
    }

    /// <summary>
    /// Apply default and triggers expression values
    /// </summary> 
    /// <param name="formValues">Form values</param>
    /// <param name="pageState">Context</param>
    /// <param name="replaceNullValues">Change the field's default value even if it is empty</param>
    /// <returns>
    /// Returns a new hashtable with the updated values
    /// </returns>
    public Hashtable MergeWithExpressionValues(Hashtable formValues, PageState pageState, bool replaceNullValues)
    {
        if (formValues == null)
            throw new ArgumentNullException(Translate.Key("Invalid parameter or not found"), nameof(formValues));

        var newvalues = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        foreach (var f in FormElement.Fields)
        {
            if (formValues.Contains(f.Name))
            {
                object val = DataHelper.ClearSpecialChars(f, formValues[f.Name]);
                newvalues.Add(f.Name, val);
            }
        }

        ApplyDefaultValues(ref newvalues, pageState, replaceNullValues);
        ApplyTriggerValues(ref newvalues, pageState);

        return newvalues;
    }

    public Hashtable GetDefaultValues(Hashtable formValues, PageState state)
    {
        var filters = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        if (FormElement != null)
        {
            var list = FormElement.Fields
                .ToList()
                .FindAll(x => !string.IsNullOrEmpty(x.DefaultValue));
            foreach (var e in list)
            {
                string val = Expression.GetDefaultValue(e, state, formValues);
                if (!string.IsNullOrEmpty(val))
                {
                    filters.Add(e.Name, val);
                }
            }
        }

        return filters;
    }

    public Hashtable MergeWithDefaultValues(Hashtable formValues, PageState pageState)
    {
        var values = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        if (formValues != null)
        {
            foreach (DictionaryEntry v in formValues)
                values.Add(v.Key, v.Value);
        }

        ApplyDefaultValues(ref values, pageState, false);
        return values;
    }

    private void ApplyDefaultValues(ref Hashtable formValues, PageState pageState, bool replaceNullValues)
    {
        if (FormElement != null)
            return;

        var defaultValues = GetDefaultValues(formValues, pageState);
        if (defaultValues == null)
            return;

        foreach (DictionaryEntry d in defaultValues)
        {
            if (!formValues.Contains(d.Key))
            {
                formValues.Add(d.Key, d.Value);
            }
            else
            {
                if ((formValues[d.Key] == null || string.IsNullOrEmpty(formValues[d.Key].ToString()))
                    && replaceNullValues)
                {
                    formValues[d.Key] = d.Value;
                }
            }
        }
    }

    private void ApplyTriggerValues(ref Hashtable formValues, PageState pageState)
    {
        var listFields = FormElement.Fields
            .ToList()
            .FindAll(x => !string.IsNullOrEmpty(x.TriggerExpression));
        foreach (var e in listFields)
        {
            string val = Expression.GetTriggerValue(e, pageState, formValues);
            if (val != null)
            {
                if (formValues.Contains(e.Name))
                    formValues[e.Name] = val;
                else
                    formValues.Add(e.Name, val);
            }
        }
    }

    public List<DataItemValue> GetDataItemValues(FormElementDataItem DataItem, Hashtable formValues, PageState pageState)
    {
        if (DataItem == null)
            return null;

        var values = new List<DataItemValue>();
        if (DataItem.Command != null && !string.IsNullOrEmpty(DataItem.Command.Sql))
        {

            string sql = DataItem.Command.Sql;
            if (sql.Contains("{"))
            {
                var exp = new ExpressionManager(UserValues, DataAccess);
                sql = exp.ParseExpression(sql, pageState, false, formValues);
            }

            DataTable dt = DataAccess.GetDataTable(sql);
            foreach (DataRow row in dt.Rows)
            {
                var item = new DataItemValue();
                item.Id = row[0].ToString();
                item.Description = row[1].ToString().Trim();
                if (DataItem.ShowImageLegend)
                {
                    item.Icon = (IconType)int.Parse(row[2].ToString());
                    item.ImageColor = row[3].ToString();
                }
                values.Add(item);
            }
        }
        else
        {
            values = DataItem.Items;
        }

        return values;
    }

}