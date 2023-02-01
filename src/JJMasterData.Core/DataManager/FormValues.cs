using System;
using System.Collections;
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

    public Hashtable RequestFormValues(PageState state, string prefix = null)
    {
        if (FormElement == null)
            throw new ArgumentException(nameof(FormElement));

        var values = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        foreach (var f in FormElement.Fields)
        {
            var objname = (prefix == null ? prefix : string.Empty) + f.Name;
            var value = f.ValidateRequest ? CurrentContext.Request.Form(objname) : CurrentContext.Request.GetUnvalidated(objname);

            switch (f.Component)
            {
                case FormComponent.Search:
                {
                    var search = (JJSearchBox)FieldManager.GetField(f, state, values);
                    search.AutoReloadFormFields = true;
                    value = search.SelectedValue;
                    break;
                }
                case FormComponent.Lookup:
                {
                    var lookup = (JJLookup)FieldManager.GetField(f, state, values);
                    lookup.AutoReloadFormFields = true;
                    value = lookup.SelectedValue;
                    break;
                }
                default:
                {
                    if (f.Component is FormComponent.Number or FormComponent.Currency)
                    {
                        string requestType = CurrentContext.Request.QueryString("t");
                        if (value != null && "reloadpainel".Equals(requestType) || "tablerow".Equals(requestType) || "ajax".Equals(requestType))
                        {
                            if (double.TryParse(value?.ToString(), out var numericValue))
                                value = numericValue;
                            else
                                value = 0;
                        }
                    }
                    else if (f.Component == FormComponent.CheckBox)
                    {
                        value ??= CurrentContext.Request.Form(objname + "_hidden") ?? "0";
                    }
                    break;
                }
            }
             
            if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
            {
                values.Add(f.Name, value);
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

        var newvalues = new Hashtable();
        DataHelper.CopyIntoHash(ref newvalues, values, true);
        
        if (CurrentContext.IsPostBack && autoReloadFormFields)
        {
            _formValues ??= new FormValues(FieldManager);
            var requestedValues = _formValues.RequestFormValues(state, prefix);
            DataHelper.CopyIntoHash(ref newvalues, requestedValues, true);
        }
        
        _formManager ??= new FormManager(FormElement, FieldManager.ExpressionManager);
        return _formManager.MergeWithExpressionValues(newvalues, state, !CurrentContext.IsPostBack);
    }

}
