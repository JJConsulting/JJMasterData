using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using System;
using System.Collections;
using JJMasterData.Core.Http;

namespace JJMasterData.Core.WebComponents;

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
            var val = f.ValidateRequest ? CurrentContext.Request.Form(objname) : CurrentContext.Request.GetUnvalidated(objname);

            if (f.Component == FormComponent.Search)
            {
                var search = (JJSearchBox)FieldManager.GetField(f, state, values);
                search.AutoReloadFormFields = true;
                val = search.SelectedValue;
            }
            else if (f.Component == FormComponent.Lookup)
            {
                var lookup = (JJLookup)FieldManager.GetField(f, state, values);
                lookup.AutoReloadFormFields = true;
                val = lookup.SelectedValue;
            }
            else if (f.Component == FormComponent.Number |
                     f.Component == FormComponent.Currency)
            {
                //When the post is run via ajax with the serialize() function,
                //I don't know why the decimal point is changed to a dot.
                //WorkAround Serialize()
                string t = CurrentContext.Request.QueryString("t");
                if (val != null && "reloadpainel".Equals(t) | "tablerow".Equals(t) | "ajax".Equals(t))
                {
                    string sVal = val.ToString().Replace(" ", "").Replace(".", ",");
                    if (double.TryParse(sVal, out var nVal))
                        val = nVal;
                    else
                        val = 0;
                }
            }
            else if (f.Component == FormComponent.CheckBox)
            {
                val ??= CurrentContext.Request.Form(objname + "_hidden") ?? "0";
            }
             
            if (val != null)
            {
                values.Add(f.Name, val);
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
        
        _formManager ??= new FormManager(FormElement, FieldManager.Expression);
        return _formManager.MergeWithExpressionValues(newvalues, state, !CurrentContext.IsPostBack);
    }

}
