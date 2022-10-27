using System;
using System.Collections;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Html;
using JJMasterData.Core.Http;

namespace JJMasterData.Core.WebComponents;

internal class GridFilter
{
    private const string FILTERACTION = "FILTERACTION";
    private const string CLEARACTION = "CLEARACTION";
    internal const string FIELD_NAME_PREFIX = "filter_";

    private Hashtable _currentFilter;
    private JJGridView GridView { get; set; }

    public GridFilter(JJGridView grid)
    {
        GridView = grid;
    }

    /// <summary>
    /// Recupera o filtro atual da grid
    /// </summary>
    /// <returns></returns>
    public Hashtable GetCurrentFilter()
    {
        if (_currentFilter != null)
            return _currentFilter;

        //Ação é capturada aqui, pois o usuário pode chamar o metodo as antes do GetHtml
        string sAction = GridView.CurrentContext.Request.Form("current_filteraction_" + GridView.Name);
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

        if (sesssionFilter != null && (GridView.IsPostBack || IsAjaxPost()))
        {
            _currentFilter = sesssionFilter;
            return _currentFilter;
        }

        ApplyCurrentFilter(null);
        return _currentFilter;
    }

    private bool IsAjaxPost()
    {
        return !string.IsNullOrEmpty(GridView.CurrentContext.Request.QueryString("t"));
    }

    /// <summary>
    /// Aplica filtros na grid mantendo os valores padrões
    /// </summary>
    public void ApplyCurrentFilter(Hashtable values)
    {
        //Relation Values
        if (values == null)
        {
            values = GridView.RelationValues;
        }
        else
        {
            foreach (DictionaryEntry r in GridView.RelationValues)
            {
                if (values.ContainsKey(r.Key))
                    values[r.Key] = r.Value;
                else
                    values.Add(r.Key, r.Value);
            }
        }

        //QueryString values
        Hashtable qValues = GetFilterQueryString();
        if (qValues != null)
        {
            foreach (DictionaryEntry r in qValues)
            {
                if (!values.ContainsKey(r.Key))
                    values.Add(r.Key, r.Value);
            }
        }


        //Default values
        var fManager = new FormManager(GridView.FormElement, GridView.UserValues, GridView.DataAccess);
        _currentFilter = fManager.ApplyDefaultValues(values, PageState.List);

        //Save on session
        JJSession.SetSessionValue("jjcurrentfilter_" + GridView.Name, _currentFilter);
    }

    /// <summary>
    /// Renderiza o conteúdo do filtro
    /// </summary>
    /// <returns>
    /// Retorna Html renderizado
    /// </returns>
    public string GetHtmlFilter()
    {
        bool isVisible = GridView.FieldManager.IsVisible(
            GridView.FilterAction, PageState.List, GridView.DefaultValues);

        if (!isVisible)
            return string.Empty;

        if (GridView.FilterAction.ShowAsCollapse &
            GridView.FilterAction.EnableScreenSearch)
        {
            return GetHtmlFilterScreen();
        }

        return GetHtmlFilterDefault();
    }

    private string GetHtmlFilterDefault()
    {
        string requestType = GridView.CurrentContext.Request.QueryString("t");
        string objName = GridView.CurrentContext.Request.QueryString("objname");
        string panelName = GridView.CurrentContext.Request.QueryString("pnlname");

        if ("jjsearchbox".Equals(requestType))
        {
            if (objName == null || !objName.StartsWith(FIELD_NAME_PREFIX))
                return null;

            string oName = objName.Substring(FIELD_NAME_PREFIX.Length);
            if (!GridView.FormElement.Fields.Contains(oName))
                return null;

            var field = GridView.FormElement.Fields[oName];
            var jjSearchBox = GridView.FieldManager.GetField(field, PageState.Filter, GridView.CurrentFilter, null);
            jjSearchBox.Name = objName;
            jjSearchBox.GetHtml();
        }

        var action = GridView.FilterAction;
        var fields = GridView.FormElement.Fields.ToList().FindAll(
            x => x.Filter.Type != FilterMode.None && !x.VisibleExpression.Equals("val:0"));

        foreach(var field in fields)
        {
            if (!GridView.EnableFilter || GridView.RelationValues.ContainsKey(field.Name))
                field.EnableExpression = "val:0";
        }

        if (fields.Count == 0)
            return "";

        var values = GetCurrentFilter();

        var panel = new DataPanelControl(GridView)
        {
            FieldNamePrefix = FIELD_NAME_PREFIX,
            Values = values
        };

        var htmlPanel = panel.GetHtmlForm(fields.DeepCopy());
        htmlPanel.WithAttribute("id", $"gridfilter_{GridView.Name}");

        var html = new HtmlElement(HtmlTag.Div)
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

        StringBuilder sRet = new StringBuilder();
        if (action.ShowAsCollapse)
        {
            var painel = new JJCollapsePanel
            {
                Name = "filter_collapse_" + GridView.Name,
                HtmlElementContent = html,
                Title = "Detailed Filters"
            };
            painel.Buttons.Add(btnDoFilter);
            painel.Buttons.Add(btnCancel);
            painel.ExpandedByDefault = action.ExpandedByDefault;
            sRet.AppendLine(painel.GetHtml());
        }
        else
        {
            var painel = new JJModalDialog
            {
                Name = "filter_modal_" + GridView.Name
            };
            btnDoFilter.OnClientClick += "$('#" + painel.Name + "').modal('hide');";
            btnCancel.OnClientClick += "$('#" + painel.Name + "').modal('hide');";
            painel.HtmlElementContent = html;
            painel.Title = "Detailed Filters";
            painel.Buttons.Add(btnDoFilter);
            painel.Buttons.Add(btnCancel);
            sRet.AppendLine(painel.GetHtml());
        }

        if ("reloadgridfilter".Equals(requestType) && GridView.Name.Equals(panelName))
        {
            GridView.CurrentContext.Response.SendResponse(html.GetElementHtml());
            return null;
        }

        return sRet.ToString();
    }


    private string GetHtmlFilterScreen()
    {
        var body = new StringBuilder();
        body.AppendLine("<div class=\"col-sm-12\">");
        body.Append(GetHtmlToolBarSearch(false));
        body.AppendLine("</div>");

        var painel = new JJCollapsePanel
        {
            Name = "filter_collapse_" + GridView.Name,
            HtmlContent = body.ToString(),
            Title = "Filter",
            ExpandedByDefault = GridView.FilterAction.ExpandedByDefault
        };

        return painel.GetHtml();
    }

    public string GetHtmlToolBarSearch(bool isToolBar = true)
    {
        StringBuilder sHtml = new StringBuilder();
        string searchId = "jjsearch_" + GridView.Name;

        if (isToolBar)
        {
            sHtml.Append($"<div class=\"input-group {BootstrapHelper.PullRight} jjsearch\">");
        }
        else
        {
            sHtml.Append($"<div class=\"{BootstrapHelper.FormGroup} has-feedback jjsearch\">");
            sHtml.Append($"<label class=\"{BootstrapHelper.Label}\">");
            sHtml.Append(Translate.Key("Filter by any field visible in the list"));
            sHtml.Append("</label>");
        }

        sHtml.Append("<input class=\"form-control\" ");
        sHtml.Append("type=\"search\" ");
        if (isToolBar)
        {
            sHtml.Append($"{BootstrapHelper.DataToggle}=\"tooltip\" ");
            sHtml.Append("title=\"");
            sHtml.Append(Translate.Key("Filter by any field visible in the list"));
            sHtml.Append("\" ");
        }
        sHtml.AppendFormat("placeholder=\"{0}\" ", Translate.Key("Filter"));
        sHtml.Append($"value=\"{GridView.CurrentContext.Request.Form(searchId)}\" ");
        sHtml.Append($"id=\"{searchId}\" ");
        sHtml.Append($"name=\"{searchId}\" ");
        sHtml.Append($"onkeyup =\"jjview.doSearch('{GridView.Name}', this);\" />");
        sHtml.Append("<span class=\"fa fa-search form-control-feedback\"></span>");
        sHtml.AppendLine("</div> ");

        return sHtml.ToString();
    }

    /// <summary>
    /// Recupera os filtros do formulário
    /// </summary>
    /// <returns>[Nome da Coluna], [Valor]</returns>
    public Hashtable GetFilterFormValues()
    {
        if (GridView.FormElement == null)
            throw new NullReferenceException(nameof(GridView.FormElement));

        Hashtable values = null;
        var fieldsFilter = GridView.FormElement.Fields.ToList().FindAll(x => x.Filter.Type != FilterMode.None);
        foreach (var f in fieldsFilter)
        {
            string name = $"{FIELD_NAME_PREFIX}{f.Name}";

            if (f.Filter.Type == FilterMode.Range)
            {
                string sfrom = GridView.CurrentContext.Request.Form(name + "_from");
                if (values == null && sfrom != null)
                    values = new Hashtable();

                if (!string.IsNullOrEmpty(sfrom))
                {
                    values.Add(f.Name + "_from", sfrom);
                }

                string sto = GridView.CurrentContext.Request.Form(name + "_to");
                if (!string.IsNullOrEmpty(sto))
                {
                    if (f.DataType == FieldType.DateTime && f.Component == FormComponent.Date)
                    {
                        if (DateTime.TryParse(sto, out var dto))
                            sto = dto.ToShortDateString() + " " + DateTime.MaxValue.ToLongTimeString();
                    }

                    values.Add(f.Name + "_to", sto);
                }

            }
            else
            {
                string value = GridView.CurrentContext.Request.Form(name);

                if (values == null && GridView.CurrentContext.Request.Form(name) != null)
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


    /// <summary>
    /// Recupera os filtros passados por url
    /// </summary>
    /// <returns>[Nome da Coluna], [Valor]</returns>
    public Hashtable GetFilterQueryString()
    {
        if (GridView.FormElement == null)
            return null;

        Hashtable values = null;
        var fieldsFilter = GridView.FormElement.Fields.ToList().FindAll(x => x.Filter.Type != FilterMode.None);
        foreach (var f in fieldsFilter)
        {
            string name = $"{FIELD_NAME_PREFIX}{f.Name}";

            if (f.Filter.Type == FilterMode.Range)
            {
                string sfrom = GridView.CurrentContext.Request.QueryString(name + "_from");
                if (values == null && sfrom != null)
                    values = new Hashtable();

                if (!string.IsNullOrEmpty(sfrom))
                {
                    values.Add(f.Name + "_from", sfrom);
                }

                string sto = GridView.CurrentContext.Request.QueryString(name + "_to");
                if (!string.IsNullOrEmpty(sto))
                {
                    values.Add(f.Name + "_to", sto);
                }
            }
            else
            {
                string val = GridView.CurrentContext.Request.QueryString(name);
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


    /// <summary>
    /// Verifica se existe filtros do usuário aplicados
    /// </summary>
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
