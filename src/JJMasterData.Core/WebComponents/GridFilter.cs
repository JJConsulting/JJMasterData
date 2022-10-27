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

        if (fields.Count == 0)
            return "";

        var values = GetCurrentFilter();

        var panel = new DataPanelControl(GridView)
        {
            FieldNamePrefix = FIELD_NAME_PREFIX,
            Values = values
        };

        var html = panel.GetHtmlForm(fields.DeepCopy());
        // html.AppendHiddenInput($"current_filteraction_{GridView.Name}");
        //
        // var sHtml = new StringBuilder();
        // sHtml.Append('\t', 3);
        // sHtml.AppendLine("<!-- Start Painel Grid Filter -->");
        // sHtml.Append('\t', 3);
        // sHtml.AppendLine("<div id=\"pnlgridfilter\">");
        //
        // sHtml.Append('\t', 4);
        // sHtml.Append("<input type=\"hidden\" id=\"current_filteraction_");
        // sHtml.Append(GridView.Name);
        // sHtml.Append("\" name=\"current_filteraction_");
        // sHtml.Append(GridView.Name);
        // sHtml.AppendLine("\" value=\"\" /> ");
        //
        // sHtml.Append('\t', 4);
        // sHtml.Append("<div id=\"gridfilter_");
        // sHtml.Append(GridView.Name);
        // sHtml.AppendLine($"\" class=\"{BootstrapHelper.FormHorizontal}\">");
        // foreach (var f in fields)
        // {
        //     string fldClass;
        //     bool visible = GridView.FieldManager.IsVisible(f, PageState.Filter, values);
        //     if (!visible)
        //     {
        //         continue;
        //     }
        //
        //     FirstOptionMode tempValue = FirstOptionMode.None;
        //     if (f.Component == FormComponent.ComboBox)
        //     {
        //         tempValue = f.DataItem.FirstOption;
        //         if (f.Filter.IsRequired || f.Filter.Type == FilterMode.MultValuesEqual || f.Filter.Type == FilterMode.MultValuesContain)
        //             f.DataItem.FirstOption = FirstOptionMode.None;
        //         else
        //             f.DataItem.FirstOption = FirstOptionMode.All;
        //     }
        //
        //     string name = $"{FIELD_NAME_PREFIX}{f.Name}";
        //     object value = null;
        //     if (values != null && values.Contains(f.Name))
        //         value = values[f.Name];
        //
        //     var reqClass = (f.Filter.IsRequired ? " required" : "");
        //     sHtml.Append('\t', 5);
        //     sHtml.Append($"<div class=\"{BootstrapHelper.FormGroup} {(BootstrapHelper.Version == 3 ? string.Empty : "row")}");
        //     sHtml.Append(reqClass);
        //     sHtml.AppendLine("\">");
        //
        //     sHtml.Append('\t', 6);
        //     if (f.Component == FormComponent.CheckBox)
        //     {
        //         sHtml.AppendLine("<div class=\"col-sm-2\"></div>");
        //     }
        //     else
        //     {
        //         var label = new JJLabel(f);
        //         label.CssClass = "col-sm-2";
        //         label.LabelFor = name;
        //         label.IsRequired = false;
        //         sHtml.AppendLine(label.GetHtml());
        //     }
        //
        //     if (f.DataType is FieldType.Int or FieldType.Float or FieldType.Date or FieldType.DateTime)
        //     {
        //         if (f.Filter.Type == FilterMode.Range)
        //         { 
        //             var field = GridView.FieldManager.GetField(f, PageState.Filter, values, value);
        //
        //             //From
        //             if (values != null && values.Contains(f.Name + "_from"))
        //                 value = values[f.Name + "_from"];
        //
        //             sHtml.Append('\t', 6);
        //             sHtml.AppendLine("<div class=\"col-sm-3\">");
        //             sHtml.Append('\t', 7);
        //             var componentFrom = GridView.FieldManager.GetField(f, PageState.Filter, values, value);
        //             componentFrom.Name = name + "_from";
        //             if (componentFrom is JJBaseControl controlFrom)
        //             {
        //                 controlFrom.PlaceHolder = Translate.Key("From");
        //                 if (!GridView.EnableFilter)
        //                     controlFrom.Enabled = false;
        //             }
        //             sHtml.AppendLine(componentFrom.GetHtml());
        //             sHtml.Append('\t', 6);
        //             sHtml.AppendLine("</div>");
        //
        //             //To
        //             if (values != null && values.Contains(f.Name + "_to"))
        //                 value = values[f.Name + "_to"];
        //
        //             sHtml.Append('\t', 6);
        //             sHtml.AppendLine("<div class=\"col-sm-3\">");
        //             sHtml.Append('\t', 7);
        //
        //             var componentTo = GridView.FieldManager.GetField(f, PageState.Filter, values, value);
        //             componentTo.Name = name + "_to";
        //             if (componentTo is JJBaseControl controlTo)
        //             {
        //                 controlTo.PlaceHolder = Translate.Key("To");
        //                 if (!GridView.EnableFilter)
        //                     controlTo.Enabled = false;
        //             }
        //
        //             sHtml.AppendLine(componentTo.GetHtml());
        //             sHtml.Append('\t', 6);
        //             sHtml.AppendLine("</div>");
        //
        //             sHtml.Append('\t', 6);
        //             sHtml.AppendLine("<div class=\"col-sm-4\">");
        //             //sHtml.AppendLine(GetHtmlListPeriod(f.DataType, name));
        //             sHtml.Append('\t', 6);
        //             sHtml.AppendLine("</div>");
        //         }
        //         else
        //         {
        //             if (!string.IsNullOrEmpty(f.CssClass))
        //                 fldClass = f.CssClass;
        //             else
        //                 fldClass = "col-sm-3";
        //
        //             sHtml.Append('\t', 6);
        //             sHtml.AppendLine($"<div class=\"{fldClass}\">");
        //             sHtml.Append('\t', 7);
        //
        //             sHtml.AppendLine(GetHtmlField(f, value, values, name));
        //
        //             sHtml.Append('\t', 6);
        //             sHtml.AppendLine("</div>");
        //             if (string.IsNullOrEmpty(f.CssClass))
        //             {
        //                 sHtml.Append("\t\t\t\t\t\t");
        //                 sHtml.AppendLine("<div class=\"col-sm-7\"></div>");
        //             }
        //         }
        //     }
        //     else
        //     {
        //         if (!string.IsNullOrEmpty(f.CssClass)
        //             && !f.CssClass.Contains("-11")
        //             && !f.CssClass.Contains("-12"))
        //         {
        //             fldClass = f.CssClass;
        //         }
        //         else
        //         {
        //             fldClass = "col-sm-10";
        //         }
        //
        //         sHtml.Append('\t', 6);
        //         sHtml.AppendLine($"<div class=\"{fldClass}\">");
        //         sHtml.Append('\t', 7);
        //         sHtml.AppendLine(GetHtmlField(f, value, values, name));
        //         sHtml.Append('\t', 6);
        //         sHtml.AppendLine("</div>");
        //     }
        //
        //     sHtml.Append('\t', 5);
        //     sHtml.AppendLine("</div>");
        //
        //     if (f.Component == FormComponent.ComboBox)
        //     {
        //         f.DataItem.FirstOption = tempValue;
        //     }
        //
        // }
        // sHtml.Append('\t', 4);
        // sHtml.AppendLine("</div> ");
        //
        // var listFieldsPost = fields.FindAll(x => x.AutoPostBack);
        // string functionname = "do_reloadgridfilter_" + GridView.Name;
        // if (listFieldsPost.Count > 0)
        // {
        //     sHtml.AppendLine("<script type=\"text/javascript\"> ");
        //     sHtml.AppendLine("");
        //     sHtml.Append("\tfunction ");
        //     sHtml.Append(functionname);
        //     sHtml.AppendLine("(objid) { ");
        //     sHtml.AppendLine("\t\t$('#current_filteraction_" + GridView.Name + "').val('FILTERACTION');");
        //     sHtml.AppendLine("\t\tvar frm = $('form'); ");
        //     sHtml.AppendLine("\t\tvar surl = frm.attr('action'); ");
        //     sHtml.AppendLine("\t\tif (surl.includes('?'))");
        //     sHtml.AppendLine("\t\t\tsurl += '&t=reloadgridfilter&pnlname=" + GridView.Name + "&objname=' + objid;");
        //     sHtml.AppendLine("\t\telse");
        //     sHtml.AppendLine("\t\t\tsurl += '?t=reloadgridfilter&pnlname=" + GridView.Name + "&objname=' + objid;");
        //     sHtml.AppendLine("");
        //
        //     if (GridView.EnableAjax)
        //     {
        //         sHtml.AppendLine("\t\t$.ajax({ ");
        //         sHtml.AppendLine("\t\tasync: true,");
        //         sHtml.AppendLine("\t\t\ttype: frm.attr('method'), ");
        //         sHtml.AppendLine("\t\t\turl: surl, ");
        //         sHtml.AppendLine("\t\t\tdata: frm.serialize(), ");
        //         sHtml.AppendLine("\t\t\tsuccess: function (data) { ");
        //         sHtml.AppendLine("\t\t\t\t$('#pnlgridfilter').html(data); ");
        //         sHtml.AppendLine("\t\t\t\tjjloadform(); ");
        //         sHtml.AppendLine("\t\t\t\tjjutil.gotoNextFocus(objid); ");
        //         sHtml.AppendLine("\t\t\t\t$('#current_filteraction_" + GridView.Name + "').val('');");
        //         sHtml.AppendLine("\t\t\t}, ");
        //         sHtml.AppendLine("\t\t\terror: function (jqXHR, textStatus, errorThrown) { ");
        //         sHtml.AppendLine("\t\t\t\tconsole.log(errorThrown); ");
        //         sHtml.AppendLine("\t\t\t\tconsole.log(textStatus); ");
        //         sHtml.AppendLine("\t\t\t\tconsole.log(jqXHR); ");
        //         sHtml.AppendLine("\t\t\t} ");
        //         sHtml.AppendLine("\t\t}); ");
        //     }
        //     else
        //     {
        //         sHtml.AppendLine("\t\t$(\"form:first\").submit(); ");
        //     }
        //
        //     sHtml.AppendLine("\t} ");
        //     sHtml.AppendLine("");
        //
        //     sHtml.AppendLine("\t$(document).ready(function () {");
        //     foreach (FormElementField f in listFieldsPost)
        //     {
        //         string namePrefix = $"{FIELD_NAME_PREFIX}{f.Name}";
        //         //WorkArroud para gatilhar o select do search
        //         if (f.Component == FormComponent.Search)
        //         {
        //             sHtml.Append("\t\t$(\"");
        //             sHtml.Append("#");
        //             sHtml.Append(namePrefix);
        //             sHtml.AppendLine("\").change(function () {");
        //             sHtml.AppendLine("\t\t\tsetTimeout(function() {");
        //             sHtml.Append("\t\t\t\t");
        //             sHtml.Append(functionname);
        //             sHtml.Append("('");
        //             sHtml.Append(f.Name);
        //             sHtml.AppendLine("');");
        //             sHtml.AppendLine("\t\t\t},200);");
        //             sHtml.AppendLine("\t\t});");
        //             sHtml.AppendLine("");
        //         }
        //         //WorkArroud para gatilhar o campo com mascara no IE Edge
        //         else if (f.Component == FormComponent.Number && f.NumberOfDecimalPlaces > 0)
        //         {
        //             sHtml.Append("\t\t$(\"");
        //             sHtml.Append("#");
        //             sHtml.Append(namePrefix);
        //             sHtml.AppendLine("\").blur(function () {");
        //             sHtml.Append("\t\t\t");
        //             sHtml.Append(functionname);
        //             sHtml.AppendLine("($(this).attr('id'));");
        //             sHtml.AppendLine("\t\t});");
        //             sHtml.AppendLine("");
        //         }
        //         else
        //         {
        //             sHtml.Append("\t\t$(\"");
        //             sHtml.Append("#");
        //             sHtml.Append(namePrefix);
        //             sHtml.AppendLine("\").change(function () {");
        //             sHtml.Append("\t\t\t");
        //             sHtml.Append(functionname);
        //             sHtml.AppendLine("($(this).attr('id'));");
        //             sHtml.AppendLine("\t\t});");
        //             sHtml.AppendLine("");
        //         }
        //     }
        //     sHtml.AppendLine("\t});");
        //     sHtml.AppendLine("</script> ");
        // }
        //
        // sHtml.Append('\t', 3);
        // sHtml.AppendLine("</div> ");
        // sHtml.Append('\t', 3);
        // sHtml.AppendLine("<!-- End Painel Grid Filter -->");
        // sHtml.AppendLine("");

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
}
