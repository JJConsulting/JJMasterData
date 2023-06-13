using System;
using System.Collections;
using System.Linq;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http;

namespace JJMasterData.Core.Web.Components;

internal class GridFilter
{
    private const string FILTERACTION = "FILTERACTION";
    private const string CLEARACTION = "CLEARACTION";
    internal const string FilterFieldPrefix = "filter_";

    private IDictionary _currentFilter;
    private JJGridView GridView { get; set; }

    private JJHttpContext CurrentContext => GridView.CurrentContext;

    public GridFilter(JJGridView grid)
    {
        GridView = grid;
    }

    /// <summary>
    /// Recupera o filtro atual da grid
    /// </summary>
    /// <returns></returns>
    public IDictionary GetCurrentFilter()
    {
        if (_currentFilter != null)
            return _currentFilter;

        //Ação é capturada aqui, pois o usuário pode chamar o metodo as antes do GetHtml
        string sAction = CurrentContext.Request.Form("current_filteraction_" + GridView.Name);
        if (FILTERACTION.Equals(sAction))
        {
            var formFilters = GetFilterFormValues();
            ApplyCurrentFilter(formFilters);
            return _currentFilter;
        }

        if (CLEARACTION.Equals(sAction))
        {
            ApplyCurrentFilter(null);
            return _currentFilter;
        }

        Hashtable sesssionFilter = JJSession.GetSessionValue<Hashtable>("jjcurrentfilter_" + GridView.Name);
        if (sesssionFilter != null && GridView.MaintainValuesOnLoad)
        {
            _currentFilter = sesssionFilter;
            return _currentFilter;
        }

        if (sesssionFilter != null && (CurrentContext.IsPostBack || IsAjaxPost()))
        {
            _currentFilter = sesssionFilter;
            return _currentFilter;
        }

        ApplyCurrentFilter(null);
        return _currentFilter;
    }

    private bool IsAjaxPost()
    {
        return !string.IsNullOrEmpty(CurrentContext.Request.QueryString("t"));
    }
    
    public void ApplyCurrentFilter(IDictionary values)
    {
        if (values == null)
        {
            values = GridView.RelationValues;
        }
        else
        {
            foreach (DictionaryEntry r in GridView.RelationValues)
            {
                if (values.Contains(r.Key))
                    values[r.Key] = r.Value;
                else
                    values.Add(r.Key, r.Value);
            }
        }
        
        Hashtable qValues = GetFilterQueryString();
        if (qValues != null)
        {
            foreach (DictionaryEntry r in qValues)
            {
                if (!values.Contains(r.Key))
                    values.Add(r.Key, r.Value);
            }
        }

        if (GridView.FormElement != null)
        {
            var formManager = new FormManager(GridView.FormElement, GridView.FieldManager.ExpressionManager);
            _currentFilter = formManager.MergeWithDefaultValues(values, PageState.List);
        }
        
        JJSession.SetSessionValue("jjcurrentfilter_" + GridView.Name, _currentFilter);
    }
    
    public HtmlBuilder GetFilterHtml()
    {
        bool isVisible = GridView.FieldManager.IsVisible(
            GridView.FilterAction, PageState.List, GridView.DefaultValues);

        if (!isVisible)
            return new HtmlBuilder(string.Empty);

        if (GridView.FilterAction.ShowAsCollapse &&
            GridView.FilterAction.EnableScreenSearch)
        {
            return GetFilterScreenHtml().RenderHtml();
        }

        return GetDefautFilterHtml();
    }

    private HtmlBuilder GetDefautFilterHtml()
    {
        string requestType = CurrentContext.Request.QueryString("t");
        string objName = CurrentContext.Request.QueryString("objname");
        string panelName = CurrentContext.Request.QueryString("pnlname");

        if ("jjsearchbox".Equals(requestType))
        {
            if (objName == null || !objName.StartsWith(FilterFieldPrefix))
                return null;

            string filterName = objName.Substring(FilterFieldPrefix.Length);
            if (!GridView.FormElement.Fields.Contains(filterName))
                return null;

            var field = GridView.FormElement.Fields[filterName];
            var jjSearchBox = GridView.FieldManager.GetField(field, PageState.Filter, GridView.CurrentFilter);
            jjSearchBox.Name = objName;
            jjSearchBox.GetHtml();
        }

        var action = GridView.FilterAction;
        var fields = GridView.FormElement.Fields.ToList().FindAll(
            field => field.Filter.Type != FilterMode.None && !field.VisibleExpression.Equals("val:0"));

        foreach(var field in fields)
        {
            if (!GridView.EnableFilter || GridView.RelationValues.Contains(field.Name))
                field.EnableExpression = "val:0";
        }

        if (fields.Count == 0)
            return new HtmlBuilder(string.Empty);

        var values = GetCurrentFilter();

        var dataPanelControl = new DataPanelControl(GridView)
        {
            FieldNamePrefix = FilterFieldPrefix,
            Values = values
        };

        var htmlPanel = dataPanelControl.GetHtmlForm(fields.DeepCopy());
        htmlPanel.WithAttribute("id", $"gridfilter_{GridView.Name}");

        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("id", "pnlgridfilter")
            .AppendHiddenInput($"current_filteraction_{GridView.Name}")
            .AppendElement(htmlPanel);

        var btnDoFilter = new JJLinkButton
        {
            Enabled = GridView.EnableFilter,
            Text = "Filter",
            IconClass = "fa fa-search",
            Type = LinkButtonType.Submit,
            OnClientClick = $"return jjview.doFilter('{GridView.Name}','{GridView.EnableAjax.ToString().ToLower()}');"
        };

        var btnCancel = new JJLinkButton
        {
            Enabled = GridView.EnableFilter,
            Text = "Clear Filter",
            IconClass = "fa fa-trash",
            ShowAsButton = true,
            OnClientClick = $"jjview.doClearFilter('{GridView.Name}','{GridView.EnableAjax.ToString().ToLower()}');"
        };

        
        if (action.ShowAsCollapse)
        {
            var panel = new JJCollapsePanel
            {
                Name = "filter_collapse_" + GridView.Name,
                HtmlBuilderContent = html,
                Title = "Detailed Filters"
            };
            panel.Buttons.Add(btnDoFilter);
            panel.Buttons.Add(btnCancel);
            panel.ExpandedByDefault = action.ExpandedByDefault;

            html = panel.GetHtmlBuilder();
        }
        else
        {
            var modal = new JJModalDialog
            {
                Name = "filter_modal_" + GridView.Name
            };
            btnDoFilter.Attributes.Add(BootstrapHelper.Version >= 5 ? "data-bs-dismiss" : "data-dismiss","modal");
            btnCancel.Attributes.Add(BootstrapHelper.Version >= 5 ? "data-bs-dismiss" : "data-dismiss","modal");

            modal.HtmlBuilderContent = html;
            modal.Title = "Detailed Filters";
            modal.Buttons.Add(btnDoFilter);
            modal.Buttons.Add(btnCancel);
            
            html = modal.GetHtmlBuilder();
        }

        if ("reloadgridfilter".Equals(requestType) && GridView.Name.Equals(panelName))
        {
            CurrentContext.Response.SendResponse(html.ToString());
            return null;
        }

        return html;
    }


    private JJCollapsePanel GetFilterScreenHtml()
    {
        var body = new HtmlBuilder(HtmlTag.Div);
        body.WithCssClass("col-sm-12");
        body.AppendElement(GetHtmlToolBarSearch(isToolBar:false));
        
        var panel = new JJCollapsePanel
        {
            Name = "filter_collapse_" + GridView.Name,
            HtmlBuilderContent = body,
            Title = "Filter",
            ExpandedByDefault = GridView.FilterAction.ExpandedByDefault
        };

        return panel;
    }

    public HtmlBuilder GetHtmlToolBarSearch(bool isToolBar = true)
    {
        string searchId = "jjsearch_" + GridView.Name;

        var textBox = new JJTextBox()
        {
            Attributes =
            {
                { "onkeyup", $"jjview.doSearch('{GridView.Name}', this);" }
            },
            ToolTip = Translate.Key("Filter by any field visible in the list"),
            PlaceHolder = Translate.Key("Filter"),
            CssClass = "jj-icon-search",
            Name = searchId,
            Text = CurrentContext.Request.Form(searchId)
        };
        
        var html = new HtmlBuilder();
        if (isToolBar)
        {
            html.AppendElement(HtmlTag.Div, div =>
            {
                div.WithCssClass($"{BootstrapHelper.PullRight}");
                div.AppendElement(textBox);
            });
        }
        else
        {
            html.AppendElement(HtmlTag.Div, div =>
            {
                div.WithCssClass(BootstrapHelper.FormGroup);
                div.WithCssClass("has-feedback jjsearch");
                div.AppendElement(HtmlTag.Label, label =>
                {
                    label.WithCssClass(BootstrapHelper.Label);
                    label.AppendText(Translate.Key("Filter by any field visible in the list"));
                });
                div.AppendElement(textBox);
            });
        }

        return html;
    }

    public Hashtable GetFilterFormValues()
    {
        if (GridView.FormElement == null)
            throw new NullReferenceException(nameof(GridView.FormElement));

        Hashtable values = null;
        var fieldsFilter = GridView.FormElement.Fields.ToList().FindAll(x => x.Filter.Type != FilterMode.None);
        foreach (var f in fieldsFilter)
        {
            string name = $"{FilterFieldPrefix}{f.Name}";

            if (f.Filter.Type == FilterMode.Range)
            {
                string sfrom = CurrentContext.Request.Form(name + "_from");
                if (values == null && sfrom != null)
                    values = new Hashtable();

                if (!string.IsNullOrEmpty(sfrom))
                {
                    values.Add(f.Name + "_from", sfrom);
                }

                string sto = CurrentContext.Request.Form(name + "_to");
                if (!string.IsNullOrEmpty(sto))
                {
                    if (f.DataType is FieldType.DateTime or FieldType.DateTime2 && f.Component == FormComponent.Date)
                    {
                        if (DateTime.TryParse(sto, out var dto))
                            sto = dto.ToShortDateString() + " " + DateTime.MaxValue.ToLongTimeString();
                    }

                    values.Add(f.Name + "_to", sto);
                }

            }
            else
            {
                string value = CurrentContext.Request.Form(name);

                if (values == null && CurrentContext.Request.Form(name) != null)
                    values = new Hashtable();

                switch (f.Component)
                {
                    case FormComponent.Cnpj:
                    case FormComponent.Cpf:
                    case FormComponent.CnpjCpf:
                        if (!string.IsNullOrEmpty(value))
                            value = StringManager.ClearCpfCnpjChars(value);
                        break;
                    case FormComponent.CheckBox:
                        if (string.IsNullOrEmpty(value))
                            value = "0";
                        break;
                    case FormComponent.Search:
                        var search = (JJSearchBox)GridView.FieldManager.GetField(f, PageState.Filter, values);
                        search.Name = name;
                        search.AutoReloadFormFields = true;
                        value = search.SelectedValue;
                        break;
                    case FormComponent.Lookup:
                        var lookup = (JJLookup)GridView.FieldManager.GetField(f, PageState.Filter, values);
                        lookup.Name = name;
                        lookup.AutoReloadFormFields = true;
                        value = lookup.SelectedValue;
                        break;
                }

                if (!string.IsNullOrEmpty(value))
                {
                    values ??= new Hashtable();
                    values.Add(f.Name, value);
                }
            }
        }

        return values;
    }
    
    public Hashtable GetFilterQueryString()
    {
        if (GridView.FormElement == null)
            return null;

        Hashtable values = null;
        var fieldsFilter = GridView.FormElement.Fields.ToList().FindAll(x => x.Filter.Type != FilterMode.None);
        foreach (var f in fieldsFilter)
        {
            string name = $"{FilterFieldPrefix}{f.Name}";

            if (f.Filter.Type == FilterMode.Range)
            {
                string sfrom = CurrentContext.Request.QueryString(name + "_from");
                if (values == null && sfrom != null)
                    values = new Hashtable();

                if (!string.IsNullOrEmpty(sfrom))
                {
                    values.Add(f.Name + "_from", sfrom);
                }

                string sto = CurrentContext.Request.QueryString(name + "_to");
                if (!string.IsNullOrEmpty(sto))
                {
                    values.Add(f.Name + "_to", sto);
                }
            }
            else
            {
                string val = CurrentContext.Request.QueryString(name);
                if (!string.IsNullOrEmpty(val))
                {
                    if (values == null)
                        values = new Hashtable();

                    values.Add(f.Name, val);
                }
            }
        }

        return values;
    }
    
    public bool HasFilter()
    {
        if (GridView.FormElement == null)
            throw new NullReferenceException(nameof(GridView.FormElement));

        foreach (DictionaryEntry item in GetCurrentFilter())
        {
            if (string.IsNullOrEmpty(item.Value.ToString()))
                continue;

            if (!GridView.FormElement.Fields.Contains(item.Key.ToString()))
                continue;

            var field = GridView.FormElement.Fields[item.Key.ToString()];
            if (field.Filter.Type != FilterMode.None && !field.VisibleExpression.Equals("val:0"))
                return true;
        }

        return false;
    }
}
