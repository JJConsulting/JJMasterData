using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.Web.Components;

internal class GridFilter
{
    private const string FilterActionName = "FILTERACTION";
    private const string ClearActionName = "CLEARACTION";
    internal const string FilterFieldPrefix = "filter_";

    private IDictionary<string, object> _currentFilter;
    private JJGridView GridView { get; set; }

    private IHttpContext CurrentContext => GridView.CurrentContext;
    private IStringLocalizer<JJMasterDataResources> StringLocalizer => GridView.StringLocalizer;
    public GridFilter(JJGridView grid)
    {
        GridView = grid;
    }
    
    internal async Task<HtmlBuilder> GetFilterHtml()
    {
        var filterAction = GridView.FilterAction;
        var formData = await GridView.GetFormDataAsync();
        bool isVisible = await GridView.ExpressionsService.GetBoolValueAsync(filterAction.VisibleExpression, formData);

        if (!isVisible)
            return new HtmlBuilder(string.Empty);

        if (GridView.FilterAction is { ShowAsCollapse: true, EnableScreenSearch: true })
        {
            var collapse = await GetFilterScreenCollapse();
            return collapse.GetHtmlBuilder();
        }

        return await GetDefaultFilter();
    }

    /// <summary>
    /// Recupera o filtro atual da grid
    /// </summary>
    /// <returns></returns>
    public async Task<IDictionary<string, object>> GetCurrentFilter()
    {
        if (_currentFilter is { Count: > 0 })
            return _currentFilter;

        //Ação é capturada aqui, pois o usuário pode chamar o metodo as antes do GetHtml
        string currentFilterAction = CurrentContext.Request.GetFormValue($"grid-view-filter-action-{GridView.Name}");
        switch (currentFilterAction)
        {
            case FilterActionName:
            {
                var formFilters = await GetFilterFormValues();
                await ApplyCurrentFilter(formFilters);
                return _currentFilter;
            }
            case ClearActionName:
                await ApplyCurrentFilter(null);
                return _currentFilter;
        }

        var sessionFilter = CurrentContext.Session.GetSessionValue<Dictionary<string, object>>(
            $"jjcurrentfilter_{GridView.Name}");
        if (sessionFilter != null && GridView.MaintainValuesOnLoad)
        {
            _currentFilter = sessionFilter;
            return _currentFilter;
        }
        
        var filters = CurrentContext.Request.GetFormValue($"{GridView.Name}-filters");
        if (!string.IsNullOrEmpty(filters))
        {
            var filterJson = GridView.EncryptionService.DecryptStringWithUrlUnescape(filters);
            _currentFilter = JsonConvert.DeserializeObject<Dictionary<string, object>>(filterJson);
            return _currentFilter;
        }

        if (sessionFilter != null && (CurrentContext.Request.IsPost || IsDynamicPost()))
        {
            _currentFilter = sessionFilter;
            return _currentFilter;
        }

        await ApplyCurrentFilter(null);
        return _currentFilter;
    }
    
    /// <summary>
    /// Recupera o filtro atual da grid
    /// </summary>
    /// <returns></returns>
    public void SetCurrentFilter(string name, object value)
    {
        _currentFilter ??= new Dictionary<string, object>();
        
        if(_currentFilter.ContainsKey(name))
        {
            _currentFilter[name] = value;
        }
    }

    private bool IsDynamicPost()
    {
        return !string.IsNullOrEmpty(CurrentContext.Request.QueryString["routeContext"]);
    }
    
    public async Task ApplyCurrentFilter(IDictionary<string, object> values)
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

        _currentFilter = await GridView.FieldsService.MergeWithDefaultValuesAsync(GridView.FormElement,values, PageState.List);

        CurrentContext.Session.SetSessionValue($"jjcurrentfilter_{GridView.Name}", _currentFilter);
    }

    private async Task<HtmlBuilder> GetDefaultFilter()
    {
        
        var action = GridView.FilterAction;
        var fields = GridView.FormElement.Fields.ToList().FindAll(
            field => field.Filter.Type != FilterMode.None && !field.VisibleExpression.Equals("val:0"));

        foreach (var field in fields.Where(field => !GridView.EnableFilter || GridView.RelationValues.ContainsKey(field.Name)))
        {
            field.EnableExpression = "val:0";
        }

        if (fields.Count == 0)
            return new HtmlBuilder(string.Empty);

        var values = await GetCurrentFilter();

        var dataPanelControl = new DataPanelControl(GridView, values)
        {
            FieldNamePrefix = FilterFieldPrefix
        };

        var htmlPanel = await dataPanelControl.GetHtmlForm(fields.DeepCopy());
        htmlPanel.WithAttribute("id", $"current-grid-filter-{GridView.Name}");

        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("id", "pnlgridfilter")
            .AppendHiddenInput($"grid-view-filter-action-{GridView.Name}")
            .Append(htmlPanel);

        var btnDoFilter = new JJLinkButton
        {
            Enabled = GridView.EnableFilter,
            Text = "Filter",
            IconClass = "fa fa-search",
            Type = LinkButtonType.Submit,
            OnClientClick = $"{GridView.Scripts.GetFilterScript()};return false;"
        };

        var btnCancel = new JJLinkButton
        {
            Enabled = GridView.EnableFilter,
            Text = "Clear Filter",
            IconClass = "fa fa-trash",
            ShowAsButton = true,
            OnClientClick = $"{GridView.Scripts.GetClearFilterScript()};return false;"
        };

        
        if (action.ShowAsCollapse)
        {
            var panel = new JJCollapsePanel( GridView.CurrentContext)
            {
                Name = $"filter_collapse_{GridView.Name}",
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
                Name = $"{GridView.Name}-filter-modal"
            };
            btnDoFilter.Attributes.Add(BootstrapHelper.Version >= 5 ? "data-bs-dismiss" : "data-dismiss","modal");
            btnCancel.Attributes.Add(BootstrapHelper.Version >= 5 ? "data-bs-dismiss" : "data-dismiss","modal");

            modal.HtmlBuilderContent = html;
            modal.Title = "Detailed Filters";
            modal.Buttons.Add(btnDoFilter);
            modal.Buttons.Add(btnCancel);
            
            html = modal.GetHtmlBuilder();
        }


        return html;
    }


    private async Task<JJCollapsePanel> GetFilterScreenCollapse()
    {
        var body = new HtmlBuilder(HtmlTag.Div);
        body.WithCssClass("col-sm-12");
        body.Append(await GetHtmlToolBarSearch(isToolBar:false));
        
        var panel = new JJCollapsePanel( GridView.CurrentContext)
        {
            Name = $"filter_collapse_{GridView.Name}",
            HtmlBuilderContent = body,
            Title = "Filter",
            ExpandedByDefault = GridView.FilterAction.ExpandedByDefault
        };

        return panel;
    }

    public async Task<HtmlBuilder> GetHtmlToolBarSearch(bool isToolBar = true)
    {
        string searchId = $"jjsearch_{GridView.Name}";

        var textBox = new JJTextBox( GridView.CurrentContext.Request)
        {
            Attributes =
            {
                { "onkeyup", $"GridViewFilterHelper.searchOnDOM('{GridView.Name}', this);" }
            },
            Tooltip = StringLocalizer["Filter by any field visible in the list"],
            PlaceHolder = StringLocalizer["Filter"],
            CssClass = "jj-icon-search",
            Name = searchId,
            Text = CurrentContext.Request.GetFormValue(searchId)
        };
        
        var html = new HtmlBuilder();
        if (isToolBar)
        {
            await html.AppendAsync(HtmlTag.Div, async div =>
            {
                div.WithCssClass($"{BootstrapHelper.PullRight}");
                await div.AppendControlAsync(textBox);
            });
        }
        else
        {
            await html.AppendAsync(HtmlTag.Div, async div =>
            {
                div.WithCssClass(BootstrapHelper.FormGroup);
                div.WithCssClass("has-feedback jjsearch");
                div.Append(HtmlTag.Label, label =>
                {
                    label.WithCssClass(BootstrapHelper.Label);
                    label.AppendText(StringLocalizer["Filter by any field visible in the list"]);
                });
                await div.AppendControlAsync(textBox);
            });
        }

        return html;
    }

    public async Task<IDictionary<string, object>> GetFilterFormValues()
    {
        if (GridView.FormElement == null)
            throw new NullReferenceException(nameof(GridView.FormElement));

        //Relation Filters
        var values = new Dictionary<string, object>();
        var filters = CurrentContext.Request.GetFormValue($"{GridView.Name}-filters");
        if (!string.IsNullOrEmpty(filters))
        {
            var filterJson = GridView.EncryptionService.DecryptStringWithUrlUnescape(filters);
            values = JsonConvert.DeserializeObject<Dictionary<string, object>>(filterJson);
        }

        var fieldsFilter = GridView.FormElement.Fields.ToList().FindAll(x => x.Filter.Type != FilterMode.None);
        foreach (var f in fieldsFilter)
        {
            string name = $"{FilterFieldPrefix}{f.Name}";

            if (f.Filter.Type == FilterMode.Range)
            {
                string sfrom = CurrentContext.Request.GetFormValue($"{name}_from");
                if (values == null && sfrom != null)
                    values = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(sfrom))
                {
                    values.Add($"{f.Name}_from", sfrom);
                }

                string sto = CurrentContext.Request.GetFormValue($"{name}_to");
                if (!string.IsNullOrEmpty(sto))
                {
                    if (f.DataType is FieldType.DateTime or FieldType.DateTime2 && f.Component == FormComponent.Date)
                    {
                        if (DateTime.TryParse(sto, out var dto))
                            sto = $"{dto.ToShortDateString()} {DateTime.MaxValue.ToLongTimeString()}";
                    }

                    values.Add($"{f.Name}_to", sto);
                }

            }
            else
            {
                object value = CurrentContext.Request.GetFormValue(name);

                if (values == null && CurrentContext.Request.GetFormValue(name) != null)
                    values = new Dictionary<string, object>();

                switch (f.Component)
                {
                    case FormComponent.Cnpj:
                    case FormComponent.Cpf:
                    case FormComponent.CnpjCpf:
                        if (!string.IsNullOrEmpty(value?.ToString()))
                            value = StringManager.ClearCpfCnpjChars(value.ToString());
                        break;
                    case FormComponent.CheckBox:
                        if (string.IsNullOrEmpty(value?.ToString()))
                            value = "0";
                        break;
                    case FormComponent.Search:
                        var search = (JJSearchBox)await GridView.ComponentFactory.Controls.CreateAsync(GridView.FormElement,f, new(values,GridView.UserValues, PageState.Filter), GridView.Name);
                        search.Name = name;
                        search.AutoReloadFormFields = true;
                        value = await search.GetSelectedValueAsync();
                        break;
                    case FormComponent.Lookup:
                        var lookup = (JJLookup)await GridView.ComponentFactory.Controls.CreateAsync(GridView.FormElement,f, new(values,GridView.UserValues, PageState.Filter), GridView.Name);
                        lookup.Name = name;
                        lookup.AutoReloadFormFields = true;
                        value = lookup.SelectedValue;
                        break;
                }

                if (!string.IsNullOrEmpty(value?.ToString()))
                {
                    values ??= new Dictionary<string, object>();
                    values[f.Name] = value;
                }
            }
        }

        return values;
    }



    public IDictionary<string, object>  GetFilterQueryString()
    {
        if (GridView.FormElement == null)
            return null;

        IDictionary<string, object>  values = null;
        var fieldsFilter = GridView.FormElement.Fields.ToList().FindAll(x => x.Filter.Type != FilterMode.None);
        foreach (var f in fieldsFilter)
        {
            string name = $"{FilterFieldPrefix}{f.Name}";

            if (f.Filter.Type == FilterMode.Range)
            {
                string sfrom = CurrentContext.Request.QueryString[$"{name}_from"];
                if (values == null && sfrom != null)
                    values = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(sfrom))
                {
                    values.Add($"{f.Name}_from", sfrom);
                }

                string sto = CurrentContext.Request.QueryString[$"{name}_to"];
                if (!string.IsNullOrEmpty(sto))
                {
                    values.Add($"{f.Name}_to", sto);
                }
            }
            else
            {
                string val = CurrentContext.Request.QueryString[name];
                if (!string.IsNullOrEmpty(val))
                {
                    if (values == null)
                        values = new Dictionary<string, object>();

                    values.Add(f.Name, val);
                }
            }
        }

        return values;
    }
    
    public async Task<bool> HasFilter()
    {
        if (GridView.FormElement == null)
            throw new NullReferenceException(nameof(GridView.FormElement));

        foreach (var item in await GetCurrentFilter())
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
