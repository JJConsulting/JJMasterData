using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Events.Args;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace JJMasterData.Core.UI.Components;

internal class GridFilter(JJGridView gridView)
{
    private const string FilterActionName = "filter";
    private const string ClearActionName = "clear";
    
    internal const string FilterFieldPrefix = "filter_";

    private Dictionary<string, object> _currentFilter;
    private Dictionary<string, object> _userFilters;
    private JJGridView GridView { get; } = gridView;

    private IHttpContext CurrentContext => GridView.CurrentContext;
    private IStringLocalizer<MasterDataResources> StringLocalizer => GridView.StringLocalizer;
    public string Name => GridView.Name + "-filter";

    public event AsyncEventHandler<GridFilterLoadEventArgs> OnFilterLoadAsync;
    
    internal async Task<HtmlBuilder> GetFilterHtml()
    {
        var filterAction = GridView.FilterAction;
        var formData = await GridView.GetFormStateDataAsync();
        var filterHtml = new HtmlBuilder(HtmlTag.Div).WithId(Name);
        bool isVisible = GridView.ExpressionsService.GetBoolValue(filterAction.VisibleExpression, formData);

        if (!isVisible)
            return new HtmlBuilder(string.Empty);

        if (GridView.FilterAction is { ShowAsCollapse: true, EnableScreenSearch: true })
        {
            var collapse = await GetFilterScreenCollapse();
            return filterHtml.AppendComponent(collapse);
        }

        return filterHtml.Append(await GetDefaultFilter());
    }

    /// <summary>
    /// Recupera o filtro atual da grid
    /// </summary>
    /// <returns></returns>
    public async Task<Dictionary<string, object>> GetCurrentFilterAsync()
    {
        if (_currentFilter != null)
            return _currentFilter;
        
        _currentFilter ??= new Dictionary<string, object>();
        
        DataHelper.CopyIntoDictionary(_currentFilter, _userFilters);
     
        //Action is captured here, because the user can call GetCurrentFilterAsync before GetResultAsync()
        var currentFilterAction = CurrentContext.Request.Form[$"grid-view-filter-action-{GridView.Name}"];
        switch (currentFilterAction)
        {
            case FilterActionName:
            {
                var filterFormValues = await GetFilterFormValues();
                await ApplyCurrentFilter(filterFormValues);
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
            DataHelper.CopyIntoDictionary(_currentFilter, sessionFilter);
            return _currentFilter;
        }
        
        if (sessionFilter != null && (CurrentContext.Request.Form.ContainsFormValues() || IsDynamicPost()))
        {
            DataHelper.CopyIntoDictionary(_currentFilter, sessionFilter);
            return _currentFilter;
        }

        
        await ApplyCurrentFilter(null);
        
        return _currentFilter ?? new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Recupera o filtro atual da grid
    /// </summary>
    /// <returns></returns>
    public void SetCurrentFilter(string name, object value)
    {
        _userFilters ??= new Dictionary<string, object>();
        _userFilters[name] = value;
    }

    private bool IsDynamicPost()
    {
        return !string.IsNullOrEmpty(CurrentContext.Request.QueryString["routeContext"]);
    }
    
    public async Task ApplyCurrentFilter(Dictionary<string, object> values)
    {
        _currentFilter ??= new Dictionary<string, object>();

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

        var queryString = GetFilterQueryString();
        if (queryString != null)
        {
            foreach (var r in queryString)
            {
                if (!values.ContainsKey(r.Key))
                    values.Add(r.Key, r.Value);
            }
        }

        var defaultValues =
            await GridView.FieldsService.MergeWithDefaultValuesAsync(GridView.FormElement, new FormStateData(values,GridView.UserValues, PageState.List));
        
        DataHelper.CopyIntoDictionary(values, defaultValues);
        DataHelper.CopyIntoDictionary(_currentFilter, values);
        
        CurrentContext.Session.SetSessionValue($"jjcurrentfilter_{GridView.Name}", _currentFilter);
    }

    private async Task<HtmlBuilder> GetDefaultFilter()
    {
        var action = GridView.FilterAction;
        var fields = GridView.FormElement.Fields.ToList().FindAll(
            field => field.Filter.Type != FilterMode.None && !field.VisibleExpression.Equals("val:0"));

        foreach (var field in fields)
        {
            if (GridView.RelationValues.ContainsKey(field.Name))
                field.EnableExpression = "val:0";

            field.IsRequired = false;

            if (field.AutoPostBack)
            {
                field.Attributes["onchange"] = GridView.Scripts.GetReloadFilterScript();
            }
        }

        if (fields.Count == 0)
            return new HtmlBuilder(string.Empty);
        
        var values = await GetCurrentFilterAsync();

        if (OnFilterLoadAsync != null)
            await OnFilterLoadAsync(GridView, new GridFilterLoadEventArgs { Filters = values });

        var dataPanelControl = new DataPanelControl(GridView, values)
        {
            FieldNamePrefix = FilterFieldPrefix
        };
        
        var htmlPanel = await dataPanelControl.GetHtmlForm(fields.ConvertAll(f=>f.DeepCopy()));
        htmlPanel.WithAttribute("id", $"current-grid-filter-{GridView.Name}");

        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("id", $"{Name}-body")
            .AppendHiddenInput($"grid-view-filter-action-{GridView.Name}")
            .Append(htmlPanel);

        var btnDoFilter = GridView.ComponentFactory.Html.LinkButton.Create();
        btnDoFilter.Enabled = GridView.EnableFilter;
        btnDoFilter.Text = GridView.StringLocalizer["Filter"];
        btnDoFilter.IconClass = "fa fa-search";
        btnDoFilter.ShowAsButton = true;
        btnDoFilter.Type = LinkButtonType.Submit;
        btnDoFilter.OnClientClick = $"{GridView.Scripts.GetFilterScript()};return false;";

        var btnCancel = GridView.ComponentFactory.Html.LinkButton.Create();
        btnCancel.Enabled = GridView.EnableFilter;
        btnCancel.Text = GridView.StringLocalizer["Clear Filter"];
        btnCancel.IconClass = "fa fa-trash";
        btnCancel.ShowAsButton = true;
        btnCancel.OnClientClick = $"{GridView.Scripts.GetClearFilterScript()};return false;";

        if (action.ShowAsCollapse)
        {
            var panel = new JJCollapsePanel( GridView.CurrentContext.Request.Form)
            {
                Name = $"grid-view-filter-collapse-{GridView.Name}",
                HtmlBuilderContent = html,
                TitleIcon = action.ShowIconAtCollapse ? new JJIcon(action.Icon) : null,
                Title = GridView.StringLocalizer[action.Text]
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
            modal.Title = GridView.StringLocalizer["Detailed Filters"];
            modal.Buttons.Add(btnDoFilter);
            modal.Buttons.Add(btnCancel);
            
            html = modal.GetHtmlBuilder();
        }


        return html;
    }


    private async Task<JJCollapsePanel> GetFilterScreenCollapse()
    {
        var body = new Div();
        body.WithCssClass("col-sm-12");
        body.Append(await GetHtmlToolBarSearch(isToolBar:false));
        
        var panel = new JJCollapsePanel( GridView.CurrentContext.Request.Form)
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

        var textBox = new JJTextBox( GridView.CurrentContext.Request.Form)
        {
            Attributes =
            {
                { "onkeyup", $"GridViewFilterHelper.searchOnDOM('{GridView.Name}', this);" }
            },
            Tooltip = StringLocalizer["Filter by any field visible in the list"],
            PlaceHolder = StringLocalizer["Filter"],
            CssClass = "jj-icon-search",
            Name = searchId,
            Text = CurrentContext.Request.Form[searchId]
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

    private static object ParseFilterValue(FormElementField field,string value)
    {
        var fieldType = field.DataType;
        var component = field.Component;

        if (component is FormComponent.Number && !string.IsNullOrEmpty(value))
            return FormValuesService.HandleNumericComponent(fieldType, value);
    
        if (fieldType is FieldType.DateTime or FieldType.DateTime2 && component == FormComponent.Date)
        {
            return DateTime.TryParse(value, out var dateTime) ?
                $"{dateTime.ToShortDateString()} {DateTime.MaxValue.ToLongTimeString()}" : null;
        }
    
        return value;
    }
    
    private async Task<Dictionary<string, object>> GetFilterFormValues()
    {
        if (GridView.FormElement == null)
            throw new NullReferenceException(nameof(GridView.FormElement));

        //Relation Filters
        var values = new Dictionary<string, object>();
        var filters = CurrentContext.Request.Form[$"grid-view-filters-{GridView.Name}"];
        if (!string.IsNullOrEmpty(filters))
        {
            var filterJson = GridView.EncryptionService.DecryptStringWithUrlUnescape(filters);
            values = JsonConvert.DeserializeObject<Dictionary<string, object>>(filterJson)!;
        }

        var fieldsFilter = GridView.FormElement.Fields.ToList().FindAll(x => x.Filter.Type != FilterMode.None);

        foreach (var field in fieldsFilter)
        {
            var name = $"{FilterFieldPrefix}{field.Name}";

            if (field.Filter.Type == FilterMode.Range)
            {
                var fromStringValue = CurrentContext.Request.Form[$"{name}_from"];
                if (!string.IsNullOrEmpty(fromStringValue))
                {
                    object fromValue = ParseFilterValue(field,fromStringValue);
                    values.Add($"{field.Name}_from", fromValue);
                }

                var toStringValue = CurrentContext.Request.Form[$"{name}_to"];
                if (!string.IsNullOrEmpty(toStringValue))
                {
                    object toValue = ParseFilterValue(field, toStringValue);
                    values.Add($"{field.Name}_to", toValue);
                }
            }
            else
            {
                object value = CurrentContext.Request.Form[name];

                switch (field.Component)
                {
                    case FormComponent.Cnpj:
                    case FormComponent.Cpf:
                    case FormComponent.CnpjCpf:
                        if (!string.IsNullOrEmpty(value?.ToString()))
                            value = StringManager.ClearCpfCnpjChars(value.ToString());
                        break;
                    case FormComponent.CheckBox:
                        value = StringManager.ParseBool(value) ? "1" : "0";
                        break;
                    case FormComponent.Search:
                        var search = (JJSearchBox)GridView.ComponentFactory.Controls.Create(GridView.FormElement,field, new(values,GridView.UserValues, PageState.Filter),Name,value);
                        search.Name = name;
                        search.AutoReloadFormFields = true;
                        value = await search.GetSelectedValueAsync();
                        break;
                    case FormComponent.Lookup:
                        var lookup = (JJLookup)GridView.ComponentFactory.Controls.Create(GridView.FormElement,field, new(values,GridView.UserValues, PageState.Filter),Name, value);
                        lookup.Name = name;
                        lookup.AutoReloadFormFields = true;
                        value = lookup.SelectedValue?.ToString();
                        break;
                    default:
                        value = ParseFilterValue(field, value?.ToString());
                        break;
                }

                if (!string.IsNullOrEmpty(value?.ToString()))
                {
                    values[field.Name] = value;
                }
            }
        }

        return values;
    }


    private Dictionary<string, object>  GetFilterQueryString()
    {
        Dictionary<string, object>  values = null;
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
                    values?.Add($"{f.Name}_to", sto);
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

        foreach (var item in await GetCurrentFilterAsync())
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
