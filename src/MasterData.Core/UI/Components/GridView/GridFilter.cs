using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Serialization;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Events.Args;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;


namespace JJMasterData.Core.UI.Components;

internal sealed class GridFilter(JJGridView gridView)
{
    private const string FilterActionName = "filter";
    private const string ClearActionName = "clear";

    internal const string FilterFieldPrefix = "filter_";

    private readonly IHttpContext _currentContext = gridView.CurrentContext;
    private readonly IStringLocalizer<MasterDataResources> _stringLocalizer = gridView.StringLocalizer;

    private Dictionary<string, object> _currentFilter;
    private Dictionary<string, object> _userFilters;

    public string Name => gridView.Name + "-filter";

    public event AsyncEventHandler<GridFilterLoadEventArgs> OnFilterLoadAsync;

    internal async ValueTask<HtmlBuilder> GetFilterHtml()
    {
        var filterAction = gridView.FilterAction;
        var formData = await gridView.GetFormStateDataAsync();
        var filterHtml = new HtmlBuilder(HtmlTag.Div).WithId(Name);
        var isVisible = gridView.ExpressionsService.GetBoolValue(filterAction.VisibleExpression, formData);

        if (!isVisible)
            return new HtmlBuilder(string.Empty);

        if (gridView.FilterAction is { ShowAsCollapse: true, EnableScreenSearch: true })
        {
            var collapse = GetFilterScreenCollapse();
            return filterHtml.AppendComponent(collapse);
        }

        return filterHtml.Append(await GetDefaultFilter());
    }

    public async ValueTask<Dictionary<string, object>> GetCurrentFilterAsync()
    {
        if (_currentFilter != null)
            return _currentFilter;

        _currentFilter ??= new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

        DataHelper.CopyIntoDictionary(_currentFilter, _userFilters);

        //Action is captured here, because the user can call GetCurrentFilterAsync before GetResultAsync()
        var currentFilterAction = _currentContext.Request.Form[$"grid-view-filter-action-{gridView.Name}"];
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

        var sessionFilter = _currentContext.Session.GetSessionValue<Dictionary<string, object>>(
            $"jjcurrentfilter_{gridView.Name}");

        if (sessionFilter != null && gridView.MaintainValuesOnLoad)
        {
            DataHelper.CopyIntoDictionary(_currentFilter, sessionFilter);
            return _currentFilter;
        }

        if (sessionFilter != null && (_currentContext.Request.Form.ContainsFormValues() || IsDynamicPost()))
        {
            DataHelper.CopyIntoDictionary(_currentFilter, sessionFilter);
            return _currentFilter;
        }
        
        await ApplyCurrentFilter(null);

        return _currentFilter ?? new Dictionary<string, object>();
    }

    public void SetCurrentFilter(string name, object value)
    {
        _userFilters ??= new Dictionary<string, object>();
        _userFilters[name] = value;
    }

    private bool IsDynamicPost()
    {
        return !string.IsNullOrEmpty(_currentContext.Request.QueryString["routeContext"]);
    }

    public async ValueTask ApplyCurrentFilter(Dictionary<string, object> values)
    {
        _currentFilter ??= new Dictionary<string, object>();

        if (values == null)
        {
            values = gridView.RelationValues;
        }
        else
        {
            foreach (var r in gridView.RelationValues)
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
            await gridView.FieldValuesService.MergeWithDefaultValuesAsync(gridView.FormElement,
                new FormStateData(values, gridView.UserValues, PageState.List));

        DataHelper.CopyIntoDictionary(values, defaultValues);
        DataHelper.CopyIntoDictionary(_currentFilter, values);

        _currentContext.Session.SetSessionValue($"jjcurrentfilter_{gridView.Name}", _currentFilter);
    }

    private async ValueTask<HtmlBuilder> GetDefaultFilter()
    {
        var action = gridView.FilterAction;
        var fields = gridView.FormElement.Fields.FindAll(
            field => field.Filter.Type != FilterMode.None && !field.VisibleExpression.Equals("val:0"));

        foreach (var field in fields)
        {
            if (gridView.RelationValues.ContainsKey(field.Name))
                field.EnableExpression = "val:0";

            field.IsRequired = false;

            if (field.AutoPostBack)
            {
                field.Attributes["onchange"] = gridView.Scripts.GetReloadFilterScript();
            }
        }

        if (fields.Count == 0)
            return new HtmlBuilder(string.Empty);

        var values = await GetCurrentFilterAsync();

        if (OnFilterLoadAsync != null)
            await OnFilterLoadAsync(gridView, new GridFilterLoadEventArgs { Filters = values });

        var dataPanelControl = new DataPanelForm(gridView, values)
        {
            FieldNamePrefix = FilterFieldPrefix
        };

        var htmlPanel = await dataPanelControl.GetHtmlForm(fields.ConvertAll(f => f.DeepCopy()));
        htmlPanel.WithAttribute("id", $"current-grid-filter-{gridView.Name}");

        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("id", $"{Name}-body")
            .AppendHiddenInput($"grid-view-filter-action-{gridView.Name}")
            .Append(htmlPanel);

        var btnFilter = gridView.ComponentFactory.Html.LinkButton.Create();
        btnFilter.Enabled = gridView.EnableFilter;
        btnFilter.Text = _stringLocalizer["Filter"];
        btnFilter.IconClass = "fa fa-search";
        btnFilter.ShowAsButton = true;
        btnFilter.Type = LinkButtonType.Submit;
        btnFilter.OnClientClick = $"{gridView.Scripts.GetFilterScript()};";

        var btnCancel = gridView.ComponentFactory.Html.LinkButton.Create();
        btnCancel.Enabled = gridView.EnableFilter;
        btnCancel.Text = _stringLocalizer["Clear Filter"];
        btnCancel.IconClass = "fa fa-trash";
        btnCancel.ShowAsButton = true;
        btnCancel.Type = LinkButtonType.Button;
        btnCancel.OnClientClick = $"{gridView.Scripts.GetClearFilterScript()};";

        if (action.ShowAsCollapse)
        {
            var hasFilter = await HasFilter();
            var filterIcon = new JJIcon(IconType.Filter)
            {
                CssClass = "text-info",
                Name = gridView.Name + "-filter-icon",
                Attributes =
                {
                    {"title",_stringLocalizer["Applied Filters"]}
                }
            };

            if (!hasFilter)
            {
                filterIcon.CssClass += " d-none";
            }
            
            var panel = new JJCollapsePanel(gridView.CurrentContext.Request.Form)
            {
                Name = $"filter-collapse-{gridView.Name}",
                HtmlBuilderContent = html,
                TitleIcon = filterIcon,
                Title = _stringLocalizer[action.Text]
            };
            
            panel.Buttons.Add(btnFilter);
            panel.Buttons.Add(btnCancel);
            
            panel.ExpandedByDefault = action.ExpandedByDefault;

            html = panel.GetHtmlBuilder();
            
            html.AppendScriptIf(hasFilter, $"GridViewFilterHelper.showFilterIcon('{gridView.Name}')");
        }
        else
        {
            var modal = new JJModalDialog
            {
                Name = $"{gridView.Name}-filter-modal"
            };
            btnFilter.Attributes.Add(BootstrapHelper.Version >= 5 ? "data-bs-dismiss" : "data-dismiss", "modal");
            btnCancel.Attributes.Add(BootstrapHelper.Version >= 5 ? "data-bs-dismiss" : "data-dismiss", "modal");

            modal.HtmlBuilderContent = html;
            modal.Title = _stringLocalizer["Detailed Filters"];
            modal.Buttons.Add(btnFilter);
            modal.Buttons.Add(btnCancel);

            html = modal.GetHtmlBuilder();
        }


        return html;
    }


    private JJCollapsePanel GetFilterScreenCollapse()
    {
        var body = new HtmlBuilder(HtmlTag.Div);
        body.WithCssClass("col-sm-12");
        body.Append(GetHtmlToolBarSearch(isToolBar: false));

        var panel = new JJCollapsePanel(gridView.CurrentContext.Request.Form)
        {
            Name = $"filter-collapse-{gridView.Name}",
            HtmlBuilderContent = body,
            Title = _stringLocalizer["Filter"],
            ExpandedByDefault = gridView.FilterAction.ExpandedByDefault
        };

        return panel;
    }

    public HtmlBuilder GetHtmlToolBarSearch(bool isToolBar = true)
    {
        var searchId = $"jjsearch_{gridView.Name}";

        var textBox = new JJTextBox(gridView.CurrentContext.Request.Form)
        {
            Attributes =
            {
                { "onkeyup", $"GridViewFilterHelper.searchOnDOM('{gridView.Name}', this);" }
            },
            Tooltip = _stringLocalizer["Filter by any field visible in the list"],
            PlaceHolder = _stringLocalizer["Filter"],
            CssClass = "jj-icon-search",
            Name = searchId,
            Text = _currentContext.Request.Form[searchId]
        };

        var html = new HtmlBuilder();
        if (isToolBar)
        {
            html.Append(HtmlTag.Div, div =>
            {
                div.WithCssClass(BootstrapHelper.PullRight);
                div.Append(textBox.GetHtmlBuilder());
            });
        }
        else
        {
            html.Append(HtmlTag.Div, div =>
            {
                div.WithCssClass(BootstrapHelper.FormGroup);
                div.WithCssClass("has-feedback jjsearch");
                div.Append(HtmlTag.Label, label =>
                {
                    label.WithCssClass(BootstrapHelper.Label);
                    label.AppendText(_stringLocalizer["Filter by any field visible in the list"]);
                });
                div.Append(textBox.GetHtmlBuilder());
            });
        }

        return html;
    }

    private static object ParseFilterValue(FormElementField field, string value)
    {
        var fieldType = field.DataType;
        var component = field.Component;

        if (component is FormComponent.Number && !string.IsNullOrEmpty(value))
            return FormValuesService.HandleNumericComponent(fieldType, value);

        if (fieldType is FieldType.DateTime or FieldType.DateTime2 && component == FormComponent.Date)
        {
            return DateTime.TryParse(value, out var dateTime)
                ? $"{dateTime.ToShortDateString()} {DateTime.MaxValue.ToLongTimeString()}"
                : null;
        }

        return value;
    }

    private async Task<Dictionary<string, object>> GetFilterFormValues()
    {
        if (gridView.FormElement == null)
            throw new NullReferenceException(nameof(gridView.FormElement));

        //Relation Filters
        var values = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
        var filters = _currentContext.Request.Form[$"grid-view-filters-{gridView.Name}"];
        if (!string.IsNullOrEmpty(filters))
        {
            var filterJson = gridView.EncryptionService.DecryptStringWithUrlUnescape(filters);
            values = JsonSerializer.Deserialize<Dictionary<string, object>>(filterJson, MasterDataJsonSerializerOptions.Default)!;
        }

        var fieldsFilter = gridView.FormElement.Fields.FindAll(x => x.Filter.Type != FilterMode.None);

        foreach (var field in fieldsFilter)
        {
            var name = $"{FilterFieldPrefix}{field.Name}";

            if (field.Filter.Type == FilterMode.Range)
            {
                var fromStringValue = _currentContext.Request.Form[$"{name}_from"];
                if (!string.IsNullOrEmpty(fromStringValue))
                {
                    var fromValue = ParseFilterValue(field, fromStringValue);
                    values.Add($"{field.Name}_from", fromValue);
                }

                var toStringValue = _currentContext.Request.Form[$"{name}_to"];

                if (string.IsNullOrEmpty(toStringValue))
                    continue;

                var toValue = ParseFilterValue(field, toStringValue);
                values.Add($"{field.Name}_to", toValue);
            }
            else
            {
                object value = _currentContext.Request.Form[name];

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
                        var search = (JJSearchBox)gridView.ComponentFactory.Controls.Create(gridView.FormElement, field,
                            new(values, gridView.UserValues, PageState.Filter), Name, value);
                        search.Name = name;
                        search.AutoReloadFormFields = true;
                        value = await search.GetSelectedValueAsync();
                        break;
                    case FormComponent.Lookup:
                        var lookup = (JJLookup)gridView.ComponentFactory.Controls.Create(gridView.FormElement, field,
                            new(values, gridView.UserValues, PageState.Filter), Name, value);
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


    private Dictionary<string, object> GetFilterQueryString()
    {
        Dictionary<string, object> values = null;
        var fieldsFilter = gridView.FormElement.Fields.FindAll(x => x.Filter.Type != FilterMode.None);
        foreach (var filter in fieldsFilter)
        {
            var name = $"{FilterFieldPrefix}{filter.Name}";

            if (filter.Filter.Type == FilterMode.Range)
            {
                var fromString = _currentContext.Request.QueryString[$"{name}_from"];
                if (values == null && fromString != null)
                    values = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(fromString))
                {
                    values.Add($"{filter.Name}_from", fromString);
                }

                var toString = _currentContext.Request.QueryString[$"{name}_to"];
                if (!string.IsNullOrEmpty(toString))
                {
                    values?.Add($"{filter.Name}_to", toString);
                }
            }
            else
            {
                var queryStringValue = _currentContext.Request.QueryString[name];

                if (string.IsNullOrEmpty(queryStringValue))
                    continue;
                values ??= new Dictionary<string, object>();

                values.Add(filter.Name, queryStringValue);
            }
        }

        return values;
    }

    public async ValueTask<bool> HasFilter()
    {
        foreach (var item in await GetCurrentFilterAsync())
        {
            if (string.IsNullOrEmpty(item.Value.ToString()))
                continue;

            if (!gridView.FormElement.Fields.TryGetField(item.Key, out var field))
                continue;

            if (field.Filter.Type != FilterMode.None && !field.VisibleExpression.Equals("val:0"))
                return true;
        }

        return false;
    }
}