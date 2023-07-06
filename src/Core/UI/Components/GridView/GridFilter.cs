using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Web.Components.Scripts;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http;
using JJMasterData.Core.Web.Http.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Core.Extensions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Components;

internal class GridFilter
{
    private const string FILTERACTION = "FILTERACTION";
    private const string CLEARACTION = "CLEARACTION";
    internal const string FilterFieldPrefix = "filter_";

    private IDictionary<string,dynamic>_currentFilter;
    private JJGridView GridView { get; set; }

    private IHttpContext CurrentContext => GridView.CurrentContext;
    private IStringLocalizer<JJMasterDataResources> StringLocalizer => GridView.StringLocalizer;
    public GridFilter(JJGridView grid)
    {
        GridView = grid;
    }

    /// <summary>
    /// Recupera o filtro atual da grid
    /// </summary>
    /// <returns></returns>
    public IDictionary<string,dynamic> GetCurrentFilter()
    {
        if (_currentFilter != null)
            return _currentFilter;

        //Ação é capturada aqui, pois o usuário pode chamar o metodo as antes do GetHtml
        string currentFilterAction = CurrentContext.Request.Form("current_filteraction_" + GridView.Name);
        if (FILTERACTION.Equals(currentFilterAction))
        {
            var formFilters = GetFilterFormValues();
            ApplyCurrentFilter(formFilters);
            return _currentFilter;
        }

        if (CLEARACTION.Equals(currentFilterAction))
        {
            ApplyCurrentFilter(null);
            return _currentFilter;
        }

        var sessionFilter = CurrentContext.Session.GetSessionValue<Dictionary<string,dynamic>>("jjcurrentfilter_" + GridView.Name);
        if (sessionFilter != null && GridView.MaintainValuesOnLoad)
        {
            _currentFilter = sessionFilter;
            return _currentFilter;
        }
        
        var filters = CurrentContext.Request.Form($"jjgridview_{GridView.FormElement.Name}_filters");
        if (!string.IsNullOrEmpty(filters))
        {
            var filterJson = GridView.EncryptionService.DecryptStringWithUrlDecode(filters);
            _currentFilter = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(filterJson);
            return _currentFilter;
        }

        if (sessionFilter != null && (CurrentContext.IsPost || IsAjaxPost()))
        {
            _currentFilter = sessionFilter;
            return _currentFilter;
        }

        ApplyCurrentFilter(null);
        return _currentFilter;
    }

    private bool IsAjaxPost()
    {
        return !string.IsNullOrEmpty(CurrentContext.Request.QueryString("t"));
    }
    
    public void ApplyCurrentFilter(IDictionary<string,dynamic> values)
    {
        if (values == null)
        {
            values = GridView.RelationValues;
        }
        else
        {
            foreach (var r in GridView.RelationValues)
            {
                values[r.Key] = r.Value;
            }
        }
        
        var qValues = GetFilterQueryString();
        if (qValues != null)
        {
            foreach (var r in qValues)
            {
                if (!values.ContainsKey(r.Key))
                    values.Add(r.Key, r.Value);
            }
        }

        if (GridView.FormElement != null)
        {
            _currentFilter = GridView.FieldsService.MergeWithDefaultValues(GridView.FormElement,values, PageState.List);
        }
        
        CurrentContext.Session.SetSessionValue("jjcurrentfilter_" + GridView.Name, _currentFilter);
    }
    
    public HtmlBuilder GetFilterHtml()
    {
        var filterAction = GridView.FilterAction;
        bool isVisible = GridView.ExpressionsService.GetBoolValue(filterAction.VisibleExpression,filterAction.Name,PageState.List,GridView.DefaultValues);

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

        if (JJSearchBox.IsSearchBoxRoute(GridView, GridView.CurrentContext))
            return JJSearchBox.ResponseJson(GridView, GridView.FormElement, GridView.CurrentFilter, GridView.CurrentContext);

        if ("jjsearchbox".Equals(requestType))
        {
            if (objName == null || !objName.StartsWith(FilterFieldPrefix))
                return null;

            string filterName = objName.Substring(FilterFieldPrefix.Length);
            if (!GridView.FormElement.Fields.Contains(filterName))
                return null;

            var field = GridView.FormElement.Fields[filterName];
            var jjSearchBox = GridView.FieldControlFactory.CreateControl(GridView.FormElement,GridView.Name,field, PageState.Filter, GridView.CurrentFilter, GridView.UserValues);
            jjSearchBox.Name = objName;
            jjSearchBox.GetHtml();
        }

        var action = GridView.FilterAction;
        var fields = GridView.FormElement.Fields.ToList().FindAll(
            field => field.Filter.Type != FilterMode.None && !field.VisibleExpression.Equals("val:0"));

        foreach (var field in fields.Where(field => !GridView.EnableFilter || GridView.RelationValues.ContainsKey(field.Name)))
        {
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
            OnClientClick = $"{GridView.GridViewScriptHelper.GetFilterScript(GridView)};return false;"
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
            var panel = new JJCollapsePanel( GridView.CurrentContext)
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
        
        var panel = new JJCollapsePanel( GridView.CurrentContext)
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

        var textBox = new JJTextBox( GridView.CurrentContext)
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

    public IDictionary<string,dynamic> GetFilterFormValues()
    {
        if (GridView.FormElement == null)
            throw new NullReferenceException(nameof(GridView.FormElement));

        //Relation Filters
        var values = new Dictionary<string, dynamic>();
        var filters = CurrentContext.Request.Form($"jjgridview_{GridView.FormElement.Name}_filters");
        if (!string.IsNullOrEmpty(filters))
        {
            var filterJson = GridView.EncryptionService.DecryptStringWithUrlDecode(filters);
            values = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(filterJson);
        }

        var fieldsFilter = GridView.FormElement.Fields.ToList().FindAll(x => x.Filter.Type != FilterMode.None);
        foreach (var f in fieldsFilter)
        {
            string name = $"{FilterFieldPrefix}{f.Name}";

            if (f.Filter.Type == FilterMode.Range)
            {
                string sfrom = CurrentContext.Request.Form(name + "_from");
                if (values == null && sfrom != null)
                    values = new Dictionary<string,dynamic>();

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
                    values = new Dictionary<string,dynamic>();

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
                        var search = (JJSearchBox)GridView.FieldControlFactory.CreateControl(GridView.FormElement,GridView.Name,f, PageState.Filter, values,GridView.UserValues);
                        search.Name = name;
                        search.AutoReloadFormFields = true;
                        value = search.SelectedValue;
                        break;
                    case FormComponent.Lookup:
                        var lookup = (JJLookup)GridView.FieldControlFactory.CreateControl(GridView.FormElement,GridView.Name,f, PageState.Filter, values,GridView.UserValues);
                        lookup.Name = name;
                        lookup.AutoReloadFormFields = true;
                        value = lookup.SelectedValue;
                        break;
                }

                if (!string.IsNullOrEmpty(value))
                {
                    values ??= new Dictionary<string,dynamic>();
                    values.Add(f.Name, value);
                }
            }
        }

        return values;
    }



    public IDictionary<string,dynamic>  GetFilterQueryString()
    {
        if (GridView.FormElement == null)
            return null;

        IDictionary<string,dynamic>  values = null;
        var fieldsFilter = GridView.FormElement.Fields.ToList().FindAll(x => x.Filter.Type != FilterMode.None);
        foreach (var f in fieldsFilter)
        {
            string name = $"{FilterFieldPrefix}{f.Name}";

            if (f.Filter.Type == FilterMode.Range)
            {
                string sfrom = CurrentContext.Request.QueryString(name + "_from");
                if (values == null && sfrom != null)
                    values = new Dictionary<string,dynamic>();

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
                        values = new Dictionary<string,dynamic>();

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

        foreach (var item in GetCurrentFilter())
        {
            if (string.IsNullOrEmpty(item.Value.ToString()))
                continue;

            if (!GridView.FormElement.Fields.Contains(item.Key))
                continue;

            var field = GridView.FormElement.Fields[item.Key];
            if (field.Filter.Type != FilterMode.None && !field.VisibleExpression.Equals("val:0"))
                return true;
        }

        return false;
    }
}
