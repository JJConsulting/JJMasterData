using System;
using System.Collections;
using System.Globalization;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http;

namespace JJMasterData.Core.DataManager;

internal class FormValues
{
    private FormValues _formValues;
    private FormManager _formManager;

    private FormElement FormElement => FieldManager.FormElement;
    private JJHttpContext CurrentContext => JJHttpContext.GetInstance();

    public FieldManager FieldManager { get; private set; }

    public FormValues(FieldManager fieldManager)
    {
        FieldManager = fieldManager;
    }

    public Hashtable RequestFormValues(PageState state, string prefix = "")
    {
        if (FormElement == null)
            throw new ArgumentException(nameof(FormElement));

        var values = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        foreach (var field in FormElement.Fields)
        {
            var fieldName = (prefix ?? string.Empty) + field.Name;
            var value = field.ValidateRequest
                ? CurrentContext.Request.Form(fieldName)
                : CurrentContext.Request.GetUnvalidated(fieldName);

            switch (field.Component)
            {
                case FormComponent.Search:
                {
                    var search = (JJSearchBox)FieldManager.GetField(field, state, values);
                    search.AutoReloadFormFields = true;
                    value = search.SelectedValue;
                    break;
                }
                case FormComponent.Lookup:
                {
                    var lookup = (JJLookup)FieldManager.GetField(field, state, values);
                    lookup.AutoReloadFormFields = true;
                    value = lookup.SelectedValue;
                    break;
                }
                case FormComponent.Slider:
                    if (double.TryParse(value?.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture,
                            out var doubleValue))
                        value = doubleValue;
                    break;
                case FormComponent.Currency:
                case FormComponent.Number:
                    string requestType = CurrentContext.Request.QueryString("t");
                    if (value != null && ("reloadpainel".Equals(requestType) || "tablerow".Equals(requestType) ||
                                          "ajax".Equals(requestType)))
                    {
                        if (double.TryParse(value?.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture,
                                out var numericValue))
                            value = numericValue;
                        else
                            value = 0;
                    }

                    break;
                case FormComponent.CheckBox:
                    value ??= CurrentContext.Request.Form(fieldName + "_hidden");
                    break;
            }

            if (value != null)
            {
                values.Add(field.Name, value);
            }
        }

        return values;
    }

    /// <summary>
    /// Recupera os dados do Form, aplicando o valor padrão e as triggers
    /// </summary> 
    public Hashtable GetFormValues(PageState state, Hashtable values, bool autoReloadFormFields, string prefix = null)
    {
        if (FormElement == null)
            throw new ArgumentNullException(nameof(FormElement));

        var newValues = new Hashtable();
        DataHelper.CopyIntoHash(ref newValues, values, true);

        if (CurrentContext.IsPostBack && autoReloadFormFields)
        {
            _formValues ??= new FormValues(FieldManager);
            var requestedValues = _formValues.RequestFormValues(state, prefix);
            DataHelper.CopyIntoHash(ref newValues, requestedValues, true);
        }

        _formManager ??= new FormManager(FormElement, FieldManager.ExpressionManager);
        return _formManager.MergeWithExpressionValues(newValues, state, !CurrentContext.IsPostBack);
    }
}