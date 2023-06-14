#nullable enable
using System;
using System.Collections;
using System.Globalization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http;
using System;
using System.Collections;
using System.Globalization;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.DataManager;

//TODO: Refactoring FormValues to FormRequest
//RequestFormValues to GetValues
internal class FormValues
{

    private FormElement FormElement => FieldManager.FormElement;
    private IHttpContext CurrentContext => JJHttpContext.GetInstance();

    public FieldManager FieldManager { get; private set; }

    public FormValues(FieldManager fieldManager)
    {
        FieldManager = fieldManager;
    }

    public Hashtable RequestFormValues(PageState state, string? prefix = null)
    {
        if (FormElement == null)
            throw new ArgumentException(nameof(FormElement));

        var values = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        foreach (var field in FormElement.Fields)
        {
            var fieldName = (prefix ?? string.Empty) + field.Name;
            object? value = field.ValidateRequest ? CurrentContext.Request.Form(fieldName) : CurrentContext.Request.GetUnvalidated(fieldName);

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
                case FormComponent.Currency:
                case FormComponent.Number:
                    string requestType = CurrentContext.Request.QueryString("t");
                    if (value != null && ("reloadpainel".Equals(requestType) || "tablerow".Equals(requestType) || "ajax".Equals(requestType)))
                    {
                        if (double.TryParse(value.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var numericValue))
                            value = numericValue;
                        else
                            value = 0;
                    }
                    break;
                case FormComponent.CheckBox:
                    value ??= CurrentContext.Request.Form(fieldName + "_hidden") ?? "0";
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
    public IDictionary GetFormValues(PageState state, IDictionary? values, bool autoReloadFormFields, string? prefix = null)
    {
        if (FormElement == null)
            throw new ArgumentNullException(nameof(FormElement));

        IDictionary newValues = new Hashtable();
        DataHelper.CopyIntoHash(ref newValues, values, true);

        if (CurrentContext.IsPost && autoReloadFormFields)
        {
            var formValues = new FormValues(FieldManager);
            var requestedValues = formValues.RequestFormValues(state, prefix);
            DataHelper.CopyIntoHash(ref newValues, requestedValues, true);
        }

        var formManager = new FormManager(FormElement, FieldManager.ExpressionManager);
        return formManager.MergeWithExpressionValues(newValues, state, !CurrentContext.IsPost);
    }

    public IDictionary? GetDatabaseValuesFromPk(FormElement element)
    {
        if (!CurrentContext.HasContext())
            return null;

        string criptPkval = CurrentContext.Request["jjform_pkval_" + element.Name];
        if (string.IsNullOrEmpty(criptPkval))
            return null;

        string parsedPkval = Cript.Descript64(criptPkval);
        var filters = DataHelper.GetPkValues(element, parsedPkval, '|');
        var entityRepository = FieldManager.ExpressionManager.EntityRepository;
        return entityRepository.GetFields(FormElement, filters);
    }

}
