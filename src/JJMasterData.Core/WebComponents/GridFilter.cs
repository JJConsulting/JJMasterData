using System;
using System.Collections;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Http;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Classe responsável por renderizar o filtro da Grid.
/// </summary>
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
        string t = GridView.CurrentContext.Request.QueryString("t");
        string objname = GridView.CurrentContext.Request.QueryString("objname");
        string pnlname = GridView.CurrentContext.Request.QueryString("pnlname");

        if ("jjsearchbox".Equals(t))
        {
            if (objname == null || !objname.StartsWith(FIELD_NAME_PREFIX))
                return null;

            string oName = objname.Substring(FIELD_NAME_PREFIX.Length);
            if (!GridView.FormElement.Fields.Contains(oName))
                return null;

            var f = GridView.FormElement.Fields[oName];
            var jjSearchBox = GridView.FieldManager.GetField(f, PageState.Filter, GridView.CurrentFilter, null);
            jjSearchBox.Name = objname;
            jjSearchBox.GetHtml();
        }

        var action = GridView.FilterAction;
        var fields = GridView.FormElement.Fields.ToList().FindAll(
            x => x.Filter.Type != FilterMode.None && !x.VisibleExpression.Equals("val:0"));

        if (fields.Count == 0)
            return "";

        Hashtable values = GetCurrentFilter();
        string reqClass;
        var html = new StringBuilder();
        html.Append('\t', 3);
        html.AppendLine("<!-- Start Painel Grid Filter -->");
        html.Append('\t', 3);
        html.AppendLine("<div id=\"pnlgridfilter\">");

        html.Append('\t', 4);
        html.Append("<input type=\"hidden\" id=\"current_filteraction_");
        html.Append(GridView.Name);
        html.Append("\" name=\"current_filteraction_");
        html.Append(GridView.Name);
        html.AppendLine("\" value=\"\" /> ");

        html.Append('\t', 4);
        html.Append("<div id=\"gridfilter_");
        html.Append(GridView.Name);
        html.AppendLine($"\" class=\"{BootstrapHelper.FormHorizontal}\">");
        foreach (var f in fields)
        {
            string fldClass;
            bool visible = GridView.FieldManager.IsVisible(f, PageState.Filter, values);
            if (!visible)
            {
                continue;
            }

            FirstOptionMode tempValue = FirstOptionMode.None;
            if (f.Component == FormComponent.ComboBox)
            {
                tempValue = f.DataItem.FirstOption;
                if (f.Filter.IsRequired || f.Filter.Type == FilterMode.MultValuesEqual || f.Filter.Type == FilterMode.MultValuesContain)
                    f.DataItem.FirstOption = FirstOptionMode.None;
                else
                    f.DataItem.FirstOption = FirstOptionMode.All;
            }

            string name = $"{FIELD_NAME_PREFIX}{f.Name}";
            object value = null;
            if (values != null && values.Contains(f.Name))
                value = values[f.Name];

            reqClass = (f.Filter.IsRequired ? " required" : "");
            html.Append('\t', 5);
            html.Append($"<div class=\"{BootstrapHelper.FormGroup} {(BootstrapHelper.Version == 3 ? string.Empty : "row")}");
            html.Append(reqClass);
            html.AppendLine("\">");

            html.Append('\t', 6);
            if (f.Component == FormComponent.CheckBox)
            {
                html.AppendLine("<div class=\"col-sm-2\"></div>");
            }
            else
            {
                var label = new JJLabel(f);
                label.CssClass = "col-sm-2";
                label.LabelFor = name;
                label.IsRequired = false;
                html.AppendLine(label.GetHtml());
            }

            if (f.DataType == FieldType.Int ||
                f.DataType == FieldType.Float ||
                f.DataType == FieldType.Date ||
                f.DataType == FieldType.DateTime)
            {
                if (f.Filter.Type == FilterMode.Range)
                {
                    //From
                    if (values != null && values.Contains(f.Name + "_from"))
                        value = values[f.Name + "_from"];

                    html.Append('\t', 6);
                    html.AppendLine("<div class=\"col-sm-3\">");
                    html.Append('\t', 7);
                    var componentFrom = GridView.FieldManager.GetField(f, PageState.Filter, values, value);
                    componentFrom.Name = name + "_from";
                    if (componentFrom is JJBaseControl controlFrom)
                    {
                        controlFrom.PlaceHolder = Translate.Key("From");
                        if (!GridView.EnableFilter)
                            controlFrom.Enabled = false;
                    }
                    html.AppendLine(componentFrom.GetHtml());
                    html.Append('\t', 6);
                    html.AppendLine("</div>");

                    //To
                    if (values != null && values.Contains(f.Name + "_to"))
                        value = values[f.Name + "_to"];

                    html.Append('\t', 6);
                    html.AppendLine("<div class=\"col-sm-3\">");
                    html.Append('\t', 7);

                    var componentTo = GridView.FieldManager.GetField(f, PageState.Filter, values, value);
                    componentTo.Name = name + "_to";
                    if (componentTo is JJBaseControl controlTo)
                    {
                        controlTo.PlaceHolder = Translate.Key("To");
                        if (!GridView.EnableFilter)
                            controlTo.Enabled = false;
                    }

                    html.AppendLine(componentTo.GetHtml());
                    html.Append('\t', 6);
                    html.AppendLine("</div>");

                    html.Append('\t', 6);
                    html.AppendLine("<div class=\"col-sm-4\">");
                    html.AppendLine(GetHtmlListPeriod(f.DataType, name));
                    html.Append('\t', 6);
                    html.AppendLine("</div>");
                }
                else
                {
                    if (!string.IsNullOrEmpty(f.CssClass))
                        fldClass = f.CssClass;
                    else
                        fldClass = "col-sm-3";

                    html.Append('\t', 6);
                    html.AppendLine($"<div class=\"{fldClass}\">");
                    html.Append('\t', 7);

                    html.AppendLine(GetHtmlField(f, value, values, name));

                    html.Append('\t', 6);
                    html.AppendLine("</div>");
                    if (string.IsNullOrEmpty(f.CssClass))
                    {
                        html.Append("\t\t\t\t\t\t");
                        html.AppendLine("<div class=\"col-sm-7\"></div>");
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(f.CssClass)
                    && !f.CssClass.Contains("-11")
                    && !f.CssClass.Contains("-12"))
                {
                    fldClass = f.CssClass;
                }
                else
                {
                    fldClass = "col-sm-10";
                }

                html.Append('\t', 6);
                html.AppendLine($"<div class=\"{fldClass}\">");
                html.Append('\t', 7);
                html.AppendLine(GetHtmlField(f, value, values, name));
                html.Append('\t', 6);
                html.AppendLine("</div>");
            }

            html.Append('\t', 5);
            html.AppendLine("</div>");

            if (f.Component == FormComponent.ComboBox)
            {
                f.DataItem.FirstOption = tempValue;
            }

        }
        html.Append('\t', 4);
        html.AppendLine("</div> ");

        var listFieldsPost = fields.FindAll(x => x.AutoPostBack);
        string functionname = "do_reloadgridfilter_" + GridView.Name;
        if (listFieldsPost.Count > 0)
        {
            html.AppendLine("<script type=\"text/javascript\"> ");
            html.AppendLine("");
            html.Append("\tfunction ");
            html.Append(functionname);
            html.AppendLine("(objid) { ");
            html.AppendLine("\t\t$('#current_filteraction_" + GridView.Name + "').val('FILTERACTION');");
            html.AppendLine("\t\tvar frm = $('form'); ");
            html.AppendLine("\t\tvar surl = frm.attr('action'); ");
            html.AppendLine("\t\tif (surl.includes('?'))");
            html.AppendLine("\t\t\tsurl += '&t=reloadgridfilter&pnlname=" + GridView.Name + "&objname=' + objid;");
            html.AppendLine("\t\telse");
            html.AppendLine("\t\t\tsurl += '?t=reloadgridfilter&pnlname=" + GridView.Name + "&objname=' + objid;");
            html.AppendLine("");

            if (GridView.EnableAjax)
            {
                html.AppendLine("\t\t$.ajax({ ");
                html.AppendLine("\t\tasync: true,");
                html.AppendLine("\t\t\ttype: frm.attr('method'), ");
                html.AppendLine("\t\t\turl: surl, ");
                html.AppendLine("\t\t\tdata: frm.serialize(), ");
                html.AppendLine("\t\t\tsuccess: function (data) { ");
                html.AppendLine("\t\t\t\t$('#pnlgridfilter').html(data); ");
                html.AppendLine("\t\t\t\tjjloadform(); ");
                html.AppendLine("\t\t\t\tjjutil.gotoNextFocus(objid); ");
                html.AppendLine("\t\t\t\t$('#current_filteraction_" + GridView.Name + "').val('');");
                html.AppendLine("\t\t\t}, ");
                html.AppendLine("\t\t\terror: function (jqXHR, textStatus, errorThrown) { ");
                html.AppendLine("\t\t\t\tconsole.log(errorThrown); ");
                html.AppendLine("\t\t\t\tconsole.log(textStatus); ");
                html.AppendLine("\t\t\t\tconsole.log(jqXHR); ");
                html.AppendLine("\t\t\t} ");
                html.AppendLine("\t\t}); ");
            }
            else
            {
                html.AppendLine("\t\t$(\"form:first\").submit(); ");
            }

            html.AppendLine("\t} ");
            html.AppendLine("");

            html.AppendLine("\t$(document).ready(function () {");
            foreach (FormElementField f in listFieldsPost)
            {
                string namePrefix = $"{FIELD_NAME_PREFIX}{f.Name}";
                //WorkArroud para gatilhar o select do search
                if (f.Component == FormComponent.Search)
                {
                    html.Append("\t\t$(\"");
                    html.Append("#");
                    html.Append(namePrefix);
                    html.AppendLine("\").change(function () {");
                    html.AppendLine("\t\t\tsetTimeout(function() {");
                    html.Append("\t\t\t\t");
                    html.Append(functionname);
                    html.Append("('");
                    html.Append(f.Name);
                    html.AppendLine("');");
                    html.AppendLine("\t\t\t},200);");
                    html.AppendLine("\t\t});");
                    html.AppendLine("");
                }
                //WorkArroud para gatilhar o campo com mascara no IE Edge
                else if (f.Component == FormComponent.Number && f.NumberOfDecimalPlaces > 0)
                {
                    html.Append("\t\t$(\"");
                    html.Append("#");
                    html.Append(namePrefix);
                    html.AppendLine("\").blur(function () {");
                    html.Append("\t\t\t");
                    html.Append(functionname);
                    html.AppendLine("($(this).attr('id'));");
                    html.AppendLine("\t\t});");
                    html.AppendLine("");
                }
                else
                {
                    html.Append("\t\t$(\"");
                    html.Append("#");
                    html.Append(namePrefix);
                    html.AppendLine("\").change(function () {");
                    html.Append("\t\t\t");
                    html.Append(functionname);
                    html.AppendLine("($(this).attr('id'));");
                    html.AppendLine("\t\t});");
                    html.AppendLine("");
                }
            }
            html.AppendLine("\t});");
            html.AppendLine("</script> ");
        }

        html.Append('\t', 3);
        html.AppendLine("</div> ");
        html.Append('\t', 3);
        html.AppendLine("<!-- End Painel Grid Filter -->");
        html.AppendLine("");

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
                HtmlContent = html.ToString(),
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
            painel.HtmlContent = html.ToString();
            painel.Title = "Detailed Filters";
            painel.Buttons.Add(btnDoFilter);
            painel.Buttons.Add(btnCancel);
            sRet.AppendLine(painel.GetHtml());
        }

        if ("reloadgridfilter".Equals(t) && GridView.Name.Equals(pnlname))
        {
            GridView.CurrentContext.Response.SendResponse(html.ToString());
            return null;
        }

        return sRet.ToString();
    }

    private string GetHtmlField(FormElementField f, object value, Hashtable formValues, string name)
    {
        var baseField = GridView.FieldManager.GetField(f, PageState.Filter, formValues, value);
        baseField.Name = name;
        if (baseField is JJTextGroup textGroup)
        {
            if (f.Filter.Type == FilterMode.MultValuesContain ||
                f.Filter.Type == FilterMode.MultValuesEqual)
            {
                textGroup.Attributes.Add("data-role", "tagsinput");
                textGroup.MaxLength = 0;
            }
        }

        else if (baseField is JJComboBox comboBox)
        {
            if (f.Filter.Type == FilterMode.MultValuesEqual)
            {
                comboBox.MultiSelect = true;
            }
        }

        if (baseField is JJBaseControl control)
        {
            if (!GridView.EnableFilter ||
                GridView.RelationValues.ContainsKey(f.Name))
            {
                control.Enabled = false;
            }
        }

        return baseField.GetHtml();
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



    /// <summary>
    /// Recupera lista de atalhos para periodos de datas
    /// </summary>
    /// <param name="dataType"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    private string GetHtmlListPeriod(FieldType dataType, string fieldName)
    {
        if (dataType != FieldType.Date &&
            dataType != FieldType.DateTime)
        {
            return string.Empty;
        }

        var now = DateTime.Today;
        var html = new StringBuilder();
        html.Append('\t', 7);
        html.AppendLine("<div class=\"dropdown\">");
        html.Append('\t', 8);
        html.Append($"<button class=\"{BootstrapHelper.DefaultButton} dropdown-toggle\" ");
        html.Append("type =\"button\" ");
        html.AppendFormat("id=\"dropdown_{0}\" ", fieldName);
        html.Append($"{BootstrapHelper.DataToggle}=\"dropdown\" ");
        html.Append("aria-haspopup=\"true\" ");
        html.Append("aria-expanded=\"true\">");
        html.Append(Translate.Key("Periods"));
        html.Append("&nbsp;<span class=\"caret\"></span>");
        html.AppendLine("</button>");
        html.Append('\t', 8);
        html.Append("<ul class=\"dropdown-menu\" ");
        html.AppendFormat("aria-labelledby=\"dropdown_{0}\">\r\n", fieldName);

        html.Append('\t', 9);
        html.Append("<li class=\"dropdown-item\">");
        html.Append("<a href=\"#\" onclick=\"");
        if (dataType == FieldType.DateTime)
        {
            html.AppendFormat("$('#{0}_from').val('{1} 00:00'); ", fieldName, now.ToShortDateString());
            html.AppendFormat("$('#{0}_to').val('{1} 23:59'); ", fieldName, now.ToShortDateString());
        }
        else
        {
            html.AppendFormat("$('#{0}_from').val('{1}'); ", fieldName, now.ToShortDateString());
            html.AppendFormat("$('#{0}_to').val('{1}'); ", fieldName, now.ToShortDateString());
        }
        html.Append("\">");
        html.Append(Translate.Key("Today"));
        html.Append("</a>");
        html.AppendLine("</li>");

        string yesterdayDate = now.AddDays(-1).ToShortDateString();
        html.Append('\t', 9);
        html.Append("<li class=\"dropdown-item\">");
        html.Append("<a href=\"#\" onclick=\"");
        if (dataType == FieldType.DateTime)
        {
            html.AppendFormat("$('#{0}_from').val('{1} 00:00'); ", fieldName, yesterdayDate);
            html.AppendFormat("$('#{0}_to').val('{1} 23:59'); ", fieldName, yesterdayDate);
        }
        else
        {
            html.AppendFormat("$('#{0}_from').val('{1}'); ", fieldName, yesterdayDate);
            html.AppendFormat("$('#{0}_to').val('{1}'); ", fieldName, yesterdayDate);
        }
        html.Append("\">");
        html.Append(Translate.Key("Yesterday"));
        html.Append("</a>");
        html.AppendLine("</li>");

        DateTime dtMonthFrom = new DateTime(now.Year, now.Month, 1);
        DateTime dtMonthTo = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
        html.Append('\t', 9);
        html.Append("<li class=\"dropdown-item\">");
        html.Append("<a href=\"#\" onclick=\"");
        if (dataType == FieldType.DateTime)
        {
            html.AppendFormat("$('#{0}_from').val('{1} 00:00'); ", fieldName, dtMonthFrom.ToShortDateString());
            html.AppendFormat("$('#{0}_to').val('{1} 23:59'); ", fieldName, dtMonthTo.ToShortDateString());
        }
        else
        {
            html.AppendFormat("$('#{0}_from').val('{1}'); ", fieldName, dtMonthFrom.ToShortDateString());
            html.AppendFormat("$('#{0}_to').val('{1}'); ", fieldName, dtMonthTo.ToShortDateString());
        }
        html.Append("\">");
        html.Append(Translate.Key("This month"));
        html.Append("</a>");
        html.AppendLine("</li>");

        DateTime lastMonth = DateTime.Now.AddMonths(-1);
        DateTime dtLastMonthFrom = new DateTime(lastMonth.Year, lastMonth.Month, 1);
        DateTime dtLastMonthTo = new DateTime(lastMonth.Year, lastMonth.Month, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month));
        html.Append('\t', 9);
        html.Append("<li class=\"dropdown-item\">");
        html.Append("<a href=\"#\" onclick=\"");
        if (dataType == FieldType.DateTime)
        {
            html.AppendFormat("$('#{0}_from').val('{1} 00:00'); ", fieldName, dtLastMonthFrom.ToShortDateString());
            html.AppendFormat("$('#{0}_to').val('{1} 23:59'); ", fieldName, dtLastMonthTo.ToShortDateString());
        }
        else
        {
            html.AppendFormat("$('#{0}_from').val('{1}'); ", fieldName, dtLastMonthFrom.ToShortDateString());
            html.AppendFormat("$('#{0}_to').val('{1}'); ", fieldName, dtLastMonthTo.ToShortDateString());
        }
        html.Append("\">");
        html.Append(Translate.Key("Last month"));
        html.Append("</a>");
        html.AppendLine("</li>");

        DateTime lastQuarter = DateTime.Now.AddMonths(-3);
        DateTime dtLastQuarterFrom = new DateTime(lastQuarter.Year, lastQuarter.Month, 1);
        html.Append('\t', 9);
        html.Append("<li class=\"dropdown-item\">");
        html.Append("<a href=\"#\" onclick=\"");
        if (dataType == FieldType.DateTime)
        {
            html.AppendFormat("$('#{0}_from').val('{1} 00:00'); ", fieldName, dtLastQuarterFrom.ToShortDateString());
            html.AppendFormat("$('#{0}_to').val('{1} 23:59'); ", fieldName, dtMonthTo.ToShortDateString());
        }
        else
        {
            html.AppendFormat("$('#{0}_from').val('{1}'); ", fieldName, dtLastQuarterFrom.ToShortDateString());
            html.AppendFormat("$('#{0}_to').val('{1}'); ", fieldName, dtMonthTo.ToShortDateString());
        }
        html.Append("\">");
        html.Append(Translate.Key("Last three months"));
        html.Append("</a>");
        html.AppendLine("</li>");

        html.Append('\t', 9);
        html.AppendLine("<li role=\"separator\" class=\"divider\"></li>");

        html.Append('\t', 9);
        html.Append("<li class=\"dropdown-item\">");
        html.Append("<a href=\"#\" onclick=\"");
        html.AppendFormat("$('#{0}_from').val(''); ", fieldName);
        html.AppendFormat("$('#{0}_to').val(''); ", fieldName);
        html.Append("\">");
        html.Append(Translate.Key("Clear"));
        html.Append("</a>");
        html.AppendLine("</li>");

        html.Append('\t', 8);
        html.AppendLine("<ul>");
        html.Append('\t', 7);
        html.AppendLine("</div>");

        return html.ToString();
    }

}
