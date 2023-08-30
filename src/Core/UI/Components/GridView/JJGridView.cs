#nullable enable
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataDictionary.Actions.GridToolbar;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Exports.Configuration;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.UI.Components.GridView;
using JJMasterData.Core.Web.Components.Scripts;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Actions;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.FormView;
// ReSharper disable UnusedMember.Local

namespace JJMasterData.Core.Web.Components;

/// <summary>
/// Display the database values in a table,
/// where each field represents a column and each record represents a row.
/// Allows pagination, multiple filters, layout configuration and field sorting
/// </summary>
/// <example>
/// Example
/// <img src="../media/JJGridViewWithLegend.png"/>
/// </example>
public class JJGridView : AsyncComponent
{
    #region "Events"

    public event EventHandler<GridCellEventArgs>? OnRenderCell;

    public event AsyncEventHandler<GridCellEventArgs>? OnRenderCellAsync;
    
    /// <summary>
    /// Event fired when rendering the checkbox used to select the Grid row.
    /// <para/>Fired only when EnableMultSelect property is enabled.
    /// </summary>
    public event EventHandler<GridSelectedCellEventArgs>? OnRenderSelectedCell;
    public event AsyncEventHandler<GridSelectedCellEventArgs>? OnRenderSelectedCellAsync;
    /// <summary>
    /// Event fired to retrieve table data
    /// </summary>
    /// <remarks>
    /// The component uses the following rule to retrieve grid data:
    /// <para/>1) Use the DataSource property;
    /// <para/>2) If the DataSource property is null, try to execute the OnDataLoad action;
    /// <para/>3) If the OnDataLoad action is not implemented, try to retrieve
    /// using the proc informed in the FormElement;
    /// </remarks>

    public event EventHandler<GridDataLoadEventArgs>? OnDataLoad;
    public event AsyncEventHandler<GridDataLoadEventArgs>? OnDataLoadAsync;
    public event EventHandler<ActionEventArgs>? OnRenderAction;
    public event AsyncEventHandler<ActionEventArgs>? OnRenderActionAsync;
    #endregion

    #region "Properties"

    private OrderByData? _currentOrder;
    private string? _selectedRowsId;
    private int _currentPage;
    private GridSettings? _currentSettings;
    private ExportOptions? _currentExportConfig;
    private GridFilter? _filter;
    private GridTable? _table;
    private IList<Dictionary<string,object?>>? _dataSource;
    private ActionsScripts? _actionsScripts;
    private List<FormElementField>? _pkFields;
    private IDictionary<string, object?>? _defaultValues;
    private FormStateData? _formData;
    private ActionMap? _currentActionMap;
    private JJDataImportation? _dataImportation;
    private JJDataExportation? _dataExportation;
    private GridScripts? _gridScripts;
    private ComponentContext? _componentContext;

    internal JJDataImportation DataImportation
    {
        get
        {
            if (_dataImportation != null)
                return _dataImportation;

            _dataImportation = ComponentFactory.DataImportation.Create(FormElement);
            _dataImportation.UserValues = UserValues;
            _dataImportation.ProcessOptions = ImportAction.ProcessOptions;
            _dataImportation.Name = Name + "_dataimp";

            return _dataImportation;
        }
    }

    public JJDataExportation DataExportation
    {
        get
        {
            if (_dataExportation != null)
                return _dataExportation;

            _dataExportation = ComponentFactory.DataExportation.Create(FormElement);
            _dataExportation.Name = Name;
            _dataExportation.IsExternalRoute = IsExternalRoute;
            _dataExportation.ExportOptions = CurrentExportConfig;
            _dataExportation.ShowBorder = CurrentSettings?.ShowBorder ?? false;
            _dataExportation.ShowRowStriped = CurrentSettings?.ShowRowStriped ?? false;
            _dataExportation.UserValues = UserValues;
            _dataExportation.ProcessOptions = ExportAction.ProcessOptions;

            _dataExportation.OnRenderCell += OnRenderCell;
            _dataExportation.OnRenderCellAsync += OnRenderCellAsync;
            
            return _dataExportation;
        }
    }

    private List<FormElementField> PrimaryKeyFields
    {
        get
        {
            if (_pkFields != null) 
                return _pkFields;

            if (FormElement == null)
                throw new ArgumentNullException(nameof(FormElement));

            _pkFields = FormElement.Fields.ToList().FindAll(x => x.IsPk);

            return _pkFields;
        }
    }

    internal async IAsyncEnumerable<FormElementField> GetVisibleFieldsAsync()
    {
        if (FormElement == null)
            throw new ArgumentNullException(nameof(FormElement));

        var defaultValues = await GetDefaultValuesAsync();
        var formData = new FormStateData(defaultValues, UserValues, PageState.List);
        foreach (var f in FormElement.Fields)
        {
            bool isVisible = await ExpressionsService.GetBoolValueAsync(f.VisibleExpression, formData);
            if (isVisible)
                yield return f;
        }
    }

    internal ActionsScripts ActionsScripts =>
        _actionsScripts ??= new ActionsScripts(ExpressionsService, DataDictionaryRepository, UrlHelper, EncryptionService, StringLocalizer);

    internal IFormValuesService FormValuesService { get; }

    /// <summary>
    /// <see cref="FormElement"/>
    /// </summary>
    public FormElement FormElement { get; set; }
    
    /// DataSource is the property responsible for controlling the data source.
    /// The component uses the following rule to retrieve grid data:
    /// <para/>1) Use the DataSource property;
    /// <para/>2) If the DataSource property is null, try to execute the OnDataLoad action;
    /// <para/>3) If the OnDataLoad action is not implemented, try to retrieve
    /// Using the stored procedure informed in the FormElement;
    public IList<Dictionary<string,object?>>? DataSource
    {
        get => _dataSource;
        set
        {
            _dataSource = value;
            if (value == null)
                return;
            IsUserSetDataSource = true;
        }
    }

    private bool IsUserSetDataSource { get; set; }
    public bool ShowTitle { get; set; }

    public bool EnableFilter { get; set; }

    public bool ShowToolbar { get; set; }

    /// <summary>
    /// Retrieve the order of the table,
    /// by default uses the first field of the primary key
    /// </summary>
    /// <returns>Current table order</returns>
    /// <remarks>
    /// For more than one field use comma ex:
    /// "Field1 ASC, Field2 DESC, Field3 ASC"
    /// </remarks>
    public OrderByData CurrentOrder
    {
        get
        {
            if (_currentOrder != null)
                return _currentOrder;
            
            if (!CurrentContext.IsPost)
            {
                if (MaintainValuesOnLoad)
                {
                    var tableOrder = CurrentContext.Session[$"jj-grid-view-order-{Name}"];
                    if (tableOrder != null)
                    {
                        _currentOrder = OrderByData.FromString(tableOrder);
                    }
                }
            }
            else
            {
                _currentOrder = OrderByData.FromString(CurrentContext.Request["grid-view-order-" + Name]);
                if (_currentOrder == null)
                {
                    var tableOrder = CurrentContext.Session[$"jj-grid-view-order-{Name}"];
                    if (tableOrder != null)
                    {
                        _currentOrder = OrderByData.FromString(tableOrder);
                    }
                }
                else
                {
                    _currentOrder = new OrderByData();
                }
            }

            CurrentOrder = _currentOrder ?? new OrderByData();
            return _currentOrder!;
        }
        set
        {
            CurrentContext.Session[$"jj-grid-view-order-{Name}"] = value?.ToQueryParameter();
            _currentOrder = value;
        }
    }

    public int CurrentPage
    {
        get
        {
            if (_currentPage != 0)
            {
                return _currentPage;
            }

            if (CurrentContext.IsPost)
            {
                int currentPage = 1;
                string tablePageId = "grid-view-page-" + Name;
                if (!string.IsNullOrEmpty(CurrentContext.Request[tablePageId]))
                {
                    if (int.TryParse(CurrentContext.Request[tablePageId], out var page))
                        currentPage = page;
                }
                else
                {
                    object tablePage = CurrentContext.Session[$"jjcurrentpage_{Name}"];
                    if (tablePage != null)
                    {
                        if (int.TryParse(tablePage.ToString(), out var page))
                            currentPage = page;
                    }
                }

                CurrentPage = currentPage;
            }
            else
            {
                int page = 1;
                if (MaintainValuesOnLoad)
                {
                    object tablePage = CurrentContext.Session[$"jjcurrentpage_{Name}"];
                    if (tablePage != null)
                    {
                        if (int.TryParse(tablePage.ToString(), out var nAuxPage))
                            page = nAuxPage;
                    }
                }

                _currentPage = page;
            }

            return _currentPage;
        }
        set
        {
            if (MaintainValuesOnLoad)
                CurrentContext.Session[$"jjcurrentpage_{Name}"] = value.ToString();

            _currentPage = value;
        }
    }

    /// <summary>
    /// <see cref="GridSettings"/>
    /// </summary>
    public GridSettings CurrentSettings
    {
        get
        {
            if (_currentSettings != null)
                return _currentSettings;
            
            var action = CurrentActionMap?.GetCurrentAction(FormElement);
            var form = new GridFormSettings(CurrentContext, StringLocalizer);
            if (action is ConfigAction)
            {
                CurrentSettings = form.LoadFromForm();
                return _currentSettings!;
            }

            if (MaintainValuesOnLoad)
                CurrentSettings = CurrentContext.Session.GetSessionValue<GridSettings>($"jjcurrentui_{FormElement.Name}");

            if (_currentSettings == null)
                CurrentSettings = form.LoadFromForm();
            
            return _currentSettings!;
        }
        set
        {
            if (MaintainValuesOnLoad)
                CurrentContext.Session.SetSessionValue($"jjcurrentui_{FormElement.Name}", value);

            _currentSettings = value;
        }
    }

    internal GridFilter Filter => _filter ??= new GridFilter(this);

    internal GridTable Table
    {
        get
        {
            if (_table != null)
                return _table;

            _table = new GridTable(this);
            
            _table.Body.OnRenderAction += OnRenderAction;
            _table.Body.OnRenderActionAsync += OnRenderActionAsync;
            
            _table.Body.OnRenderCell += OnRenderCell;
            _table.Body.OnRenderCellAsync += OnRenderCellAsync;
            
            _table.Body.OnRenderSelectedCell += OnRenderSelectedCell;
            _table.Body.OnRenderSelectedCellAsync += OnRenderSelectedCellAsync;


            return _table;
        }
    }

    public ExportOptions CurrentExportConfig
    {
        get
        {
            if (_currentExportConfig != null)
                return _currentExportConfig;

            _currentExportConfig = new ExportOptions();
            if (CurrentContext.IsPost)
            {
                _currentExportConfig = ExportOptions.LoadFromForm(CurrentContext, Name);
            }

            return _currentExportConfig;
        }
        set => _currentExportConfig = value;
    }

    public static bool EnableAjax => true;

    public bool EnableEditMode { get; set; }

    /// <remarks>
    /// Even when set to false, the grid respects the CurrentOrder property
    /// </remarks>
    public bool EnableSorting { get; set; }

    public bool EnableMultiSelect { get; set; }

    /// <summary>
    /// Keep the grid filters, order and pagination in the session,
    /// and recover on the first page load. (Default = false)
    /// </summary>
    /// <remarks>
    /// When using this property, we recommend changing the object's [Name] parameter.
    /// The [Name] property is used to compose the name of the session variable.
    /// </remarks>
    public bool MaintainValuesOnLoad { get; set; }

    /// <summary>
    /// Show the header when no records are found.
    /// </summary>
    public bool ShowHeaderWhenEmpty { get; set; }

    /// <summary>
    /// Gets or sets the text to be displayed on the empty data row when a JJGridView control contains no records.
    /// </summary>
    /// <remarks>
    /// Default value = (There is no record to be displayed).
    /// <para/>
    /// To hide the columns when displaying the message see the method
    /// <seealso cref="ShowHeaderWhenEmpty"/>.
    /// </remarks>
    public string EmptyDataText { get; set; }

    /// <summary>
    /// Display pagination controls (Default = true)
    /// </summary>
    /// <remarks>
    /// Hide all pagination buttons
    /// but keep the default pagination controls.
    /// <para/>
    /// The Pagination will be displayed if the number of grid records exceeds the minimum number of records on a page.
    /// <para/>
    /// If the CurrentPage property is equal to zero, pagination will not be displayed.
    /// <para/>
    /// If the CurrentUI.TotalPerPage property is equal to zero, pagination will not be displayed.
    /// <para/>
    /// If the TotalRecords property is equal to zero, pagination will not be displayed.
    /// </remarks>
    public bool ShowPagging { get; set; }

    /// <summary>
    /// Key-Value pairs with the errors.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public IDictionary<string, string> Errors { get; } = new Dictionary<string, string>();

    /// <summary>
    /// When reloading the panel, keep the values entered in the form.
    /// Valid only when EnableEditMode property is enabled
    /// (Default=True)
    /// </summary>
    public bool AutoReloadFormFields { get; set; }

    /// <summary>
    /// Values to be replaced by relationship.
    /// If the field name exists in the relationship, the value will be replaced
    /// </summary>
    /// <remarks>
    /// Key = Field name, Value=Field value
    /// </remarks>
    public IDictionary<string, object> RelationValues { get; set; }

    public HeadingSize TitleSize { get; set; }

    public int TotalOfRecords { get; set; }

    public LegendAction LegendAction => ToolBarActions.LegendAction;
    public RefreshAction RefreshAction => ToolBarActions.RefreshAction;
    public FilterAction FilterAction => ToolBarActions.FilterAction;
    public ImportAction ImportAction => ToolBarActions.ImportAction;
    public ExportAction ExportAction => ToolBarActions.ExportAction;
    public ConfigAction ConfigAction => ToolBarActions.ConfigAction;
    public SortAction SortAction => ToolBarActions.SortAction;
    public GridToolbarActionList ToolBarActions => FormElement.Options.GridToolbarActions;
    public GridTableActionList GridActions => FormElement.Options.GridTableActions;
    
    private ActionMap? CurrentActionMap
    {
        get
        {
            if (_currentActionMap != null) 
                return _currentActionMap;
            var encryptedActionMap = CurrentContext.Request["grid-view-action-" + Name];
            if (string.IsNullOrEmpty(encryptedActionMap))
                return null;

            _currentActionMap = EncryptionService.DecryptActionMap(encryptedActionMap);
            return _currentActionMap;
        }
    }

    private string? SelectedRowsId
    {
        get => _selectedRowsId ??= CurrentContext.Request.GetUnvalidated("grid-view-selected-rows" + Name)?.ToString();
        set => _selectedRowsId = value ?? "";
    }

    internal ComponentContext ComponentContext
    {
        get
        {
            if (_componentContext != null)
                return _componentContext.Value;
            
            var resolver = new ComponentContextResolver(this);
            _componentContext = resolver.GetContext();

            return _componentContext.Value;
        }
    }
    
    #endregion

    #region Injected Services
    internal IFieldsService FieldsService { get; }
    internal IExpressionsService ExpressionsService { get; }
    internal IEncryptionService EncryptionService { get; }
    internal IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    internal ComponentFactory ComponentFactory { get; }
    internal IEntityRepository EntityRepository { get; }
    internal JJMasterDataUrlHelper UrlHelper { get; }

    internal GridScripts Scripts => _gridScripts ??= new GridScripts(this);

    internal IHttpContext CurrentContext { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }


    #endregion

    #region "Constructors"

    internal JJGridView(
        FormElement formElement,
        IHttpContext currentContext,
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        JJMasterDataUrlHelper urlHelper,
        IExpressionsService expressionsService,
        IEncryptionService encryptionService,
        IFieldsService fieldsService,
        IFormValuesService formValuesService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        ComponentFactory componentFactory)
    {
        Name = "jj-" + formElement.Name.ToLower();
        ShowTitle = true;
        EnableFilter = true;
        EnableSorting = true;
        ShowHeaderWhenEmpty = true;
        ShowPagging = true;
        ShowToolbar = true;
        EmptyDataText = "No records found";
        AutoReloadFormFields = true;
        RelationValues = new Dictionary<string, object>();
        TitleSize = HeadingSize.H1;
        FormElement = formElement;
        FieldsService = fieldsService;
        ExpressionsService = expressionsService;
        EncryptionService = encryptionService;
        StringLocalizer = stringLocalizer;
        ComponentFactory = componentFactory;
        EntityRepository = entityRepository;
        UrlHelper = urlHelper;
        CurrentContext = currentContext;
        DataDictionaryRepository = dataDictionaryRepository;
        FormValuesService = formValuesService;
    }

    #endregion

    protected override async Task<ComponentResult> BuildResultAsync()
    {
        string lookupRoute = CurrentContext.Request.QueryString("lookup-" + Name);
        if (!string.IsNullOrEmpty(lookupRoute))
            return await GetLookupResult(lookupRoute);

        string context = CurrentContext.Request.QueryString("context");

        if (ComponentContext is ComponentContext.HtmlContent)
        {
            var componentName = CurrentContext.Request.QueryString("componentName");

            if (Name.Equals(componentName))
            {
                return HtmlComponentResult.FromHtmlBuilder(await GetTableHtmlBuilder());
            }
        }
        
        if (ComponentContext is ComponentContext.DataExportation)
        {
            return await GetExportationResult();
        }

        if (ComponentContext is ComponentContext.GridViewRow)
        {
            int rowIndex = int.Parse(CurrentContext.Request.QueryString("gridViewRowIndex"));

            var htmlResponse = await GetTableRowHtmlAsync(rowIndex);

            return new HtmlComponentResult(htmlResponse);
        }

        if (ComponentContext is ComponentContext.SelectAllGridRows)
        {
            string selectedRows = await GetEncryptedSelectedRowsAsync();
            
            return new JsonComponentResult(new {selectedRows});
        }
        
        if (ComponentContext is ComponentContext.SearchBox)
            return await JJSearchBox.GetResultFromComponent(this,FormElement, await GetCurrentFilterAsync(), CurrentContext,ComponentFactory.Controls.GetFactory<SearchBoxFactory>());

        if ("searchBox".Equals(context))
        {
            var objName = CurrentContext.Request.QueryString("componentName");
            if (objName == null || !objName.StartsWith(GridFilter.FilterFieldPrefix))
                return new EmptyComponentResult();

            string filterName = Name[GridFilter.FilterFieldPrefix.Length..];
            if (!FormElement.Fields.Contains(filterName))
                return new EmptyComponentResult();

            var field = FormElement.Fields[filterName];
            var jjSearchBox = await ComponentFactory.Controls.CreateAsync(FormElement,field, new(await GetCurrentFilterAsync(), UserValues, PageState.Filter), Name) as JJSearchBox;
            jjSearchBox!.Name = objName;
            return await jjSearchBox.GetResultAsync();
        }
        
        return new RenderedComponentResult(await GetHtmlBuilderAsync());
    }

    internal async Task<HtmlBuilder> GetHtmlBuilderAsync()
    {
        var html = new HtmlBuilder(HtmlTag.Div);
        html.AppendIf(ShowTitle, GetTitle(_defaultValues).GetHtmlBuilder);

        if (FilterAction.IsVisible)
        {
            html.Append( await Filter.GetFilterHtml());
        }

        if (ShowToolbar)
        {
            html.Append(await GetToolbarHtmlBuilder());
        }

        html.Append(await GetTableHtmlBuilder());

        return html;
    }

    public async Task<IDictionary<string, object?>> GetCurrentFilterAsync() => await Filter.GetCurrentFilter();


    public void SetCurrentFilter(string key, object value)
    {
        Filter.SetCurrentFilter(key, value);
    }

    public async Task<string> GetTableHtmlAsync() => (await GetTableHtmlBuilder()).ToString();

    private async Task<HtmlBuilder> GetTableHtmlBuilder()
    {
        AssertProperties();

        string? currentAction = CurrentContext.Request["grid-view-action-" + Name];

        var html = new HtmlBuilder(HtmlTag.Div);

        if (CheckForSqlCommand())
        {
            var errorMessage = await ExecuteSqlCommand();
            if (errorMessage == null)
                currentAction = null;
            else
                html.AppendComponent(errorMessage);
        }

        await SetDataSource();

        html.WithAttribute("id", $"grid-view-{Name}");
        html.AppendIf(SortAction.IsVisible, GetSortingConfig);

        html.AppendText(GetScriptHtml());
        html.AppendRange(GetHiddenInputs(currentAction));

        html.Append(await Table.GetHtmlBuilder());

        if (DataSource?.Count == 0 && !string.IsNullOrEmpty(EmptyDataText))
        {
            html.Append(await GetNoRecordsAlert());
        }

        var gridPagination = new GridPagination(this);

        html.Append(gridPagination.GetHtmlElement());

        if (ShowToolbar)
        {
            html.Append(await GetSettingsHtml());

            html.Append(await GetExportHtml());

            html.Append(await GetLegendHtml());
        }

        html.Append(HtmlTag.Div, div => { div.WithCssClass("clearfix"); });

        return html;
    }
    
    private IEnumerable<HtmlBuilder> GetHiddenInputs(string? currentAction)
    {
        yield return new HtmlBuilder().AppendHiddenInput($"grid-view-order-{Name}", CurrentOrder.ToQueryParameter() ?? string.Empty);
        yield return new HtmlBuilder().AppendHiddenInput($"grid-view-page-{Name}", CurrentPage.ToString());
        yield return new HtmlBuilder().AppendHiddenInput($"grid-view-action-{Name}", currentAction ?? string.Empty);
        yield return new HtmlBuilder().AppendHiddenInput($"grid-view-row-{Name}", string.Empty);

        if (EnableMultiSelect)
        {
            yield return new HtmlBuilder().AppendHiddenInput($"grid-view-selected-rows{Name}", SelectedRowsId ?? string.Empty);
        }
    }

    internal async Task<string> GetTableRowHtmlAsync(int rowIndex)
    {
        var row = DataSource?[rowIndex];

        return await Table.Body
            .GetTdHtmlList(row, rowIndex)
            .AggregateAsync(string.Empty, (current, td) => current + td);
    }
    
    private async Task<ComponentResult> GetLookupResult(string lookupRoute)
    {
        string fieldName = lookupRoute[GridFilter.FilterFieldPrefix.Length..];
        var field = FormElement.Fields.ToList().Find(x => x.Name.Equals(fieldName));

        if (field == null) 
            return new EmptyComponentResult();

        var lookup = ComponentFactory.Controls.Create<JJLookup>(FormElement, field, new(new FormStateData(new Dictionary<string, object?>(), null, PageState.Filter), null, Name));
        lookup.Name = lookupRoute;
        lookup.ElementMap.EnableElementActions = false;
        return await lookup.GetResultAsync();
    }

    internal JJTitle GetTitle(IDictionary<string, object?>? values = null)
    {
        var title = FormElement.Title;
        var subTitle = FormElement.SubTitle;

        foreach (var field in FormElement.Fields)
        {
            if (values != null && values.TryGetValue(field.Name, out var fieldValue))
            {
                title = title?.Replace($"{{{field.Name}}}", fieldValue?.ToString());
                subTitle = subTitle?.Replace($"{{{field.Name}}}", fieldValue?.ToString());
            }
        }

        var titleComponent = new JJTitle(title, subTitle)
        {
            Size = TitleSize
        };

        return titleComponent;
    }

    internal async Task<HtmlBuilder> GetToolbarHtmlBuilder() => await new GridToolbar(this).GetHtmlBuilderAsync();

    public string? GetFilterHtml() => Filter.GetFilterHtml().ToString();

    public async Task<string> GetToolbarHtml() => (await GetToolbarHtmlBuilder()).ToString();

    private HtmlBuilder GetSortingConfig() => new GridSortingConfig(this).GetHtmlElement();

    public string GetTitleHtml() => GetTitle(_defaultValues).GetHtml();


    private bool CheckForSqlCommand()
    {
        var action = CurrentActionMap?.GetCurrentAction(FormElement);
        return action is SqlCommandAction;
    }

    private async Task<JJMessageBox?> ExecuteSqlCommand()
    {
        var action = CurrentActionMap!.GetCurrentAction(FormElement);
        var gridSqlAction = new GridSqlCommandAction(this);
        return await gridSqlAction.ExecuteSqlCommand(CurrentActionMap, (SqlCommandAction)action);
    }

    private void AssertProperties()
    {
        if (FormElement == null)
            throw new ArgumentNullException(nameof(FormElement));

        if (EnableMultiSelect && PrimaryKeyFields.Count == 0)
            throw new JJMasterDataException(
                    "It is not allowed to enable multiple selection without defining a primary key in the data dictionary");
    }

    private async Task<HtmlBuilder> GetNoRecordsAlert()
    {
        var alert = new JJAlert
        {
            ShowCloseButton = true,
            Color = PanelColor.Default,
            Title = StringLocalizer["No records found."],
            Icon = IconType.InfoCircle
        };

        var hasFilter = await Filter.HasFilter();

        if (!hasFilter) return alert.GetHtmlBuilder();

        alert.Messages.Add("There are filters applied for this query.");
        alert.Icon = IconType.Filter;

        return alert.GetHtmlBuilder();
    }

    private string GetScriptHtml()
    {
        var script = new StringBuilder();

        //Scripts
        script.AppendLine("\t<script type=\"text/javascript\"> ");

        if (EnableMultiSelect)
        {
            script.AppendLine("\t$(document).ready(function () {");
            script.AppendLine("\t\t$(\".jjselect input\").change(function() {");
            script.AppendLine("\t\t\tJJViewHelper.selectItem('" + Name + "', $(this)); ");
            script.AppendLine("\t\t});");
            script.AppendLine("\t});");
        }

        if (EnableEditMode)
        {
            var listFieldsPost = FormElement.Fields.ToList().FindAll(x => x.AutoPostBack);
            string functionname = "do_rowreload_" + Name;
            if (listFieldsPost.Count > 0)
            {
                script.AppendLine("");
                script.Append("\tfunction ");
                script.Append(functionname);
                script.AppendLine("(nRow, objname, objid) { ");
                script.AppendLine("\t\tvar frm = $('form'); ");
                script.AppendLine("\t\tvar surl = frm.attr('action'); ");
                script.AppendLine("\t\tif (surl.includes('?'))");
                script.AppendLine("\t\t\tsurl += '&context=gridViewRow&gridViewRowIndex=' + nRow;");
                script.AppendLine("\t\telse");
                script.AppendLine("\t\t\tsurl += '?context=gridViewRow&gridViewRowIndex=' + nRow;");
                script.AppendLine("");
                script.AppendLine("\t\tsurl += '&componentName=' + objname;");
                script.AppendLine($"\t\tsurl += '&gridViewName={Name}';");
                script.AppendLine("\t\t$.ajax({ ");
                script.AppendLine("\t\tasync: false,");
                script.AppendLine("\t\t\ttype: frm.attr('method'), ");
                script.AppendLine("\t\t\turl: surl, ");
                script.AppendLine("\t\t\tdata: frm.serialize(), ");
                script.AppendLine("\t\t\tsuccess: function (data) { ");
                script.AppendLine($"\t\t\t\t$(\"#grid-view-{Name} #row\" + nRow).html(data); ");
                script.AppendLine($"\t\t\t\tdo_change_{Name}(nRow);");
                script.AppendLine("\t\t\t\tloadJJMasterData(null, \"#row\" + nRow + \" \"); ");
                script.AppendLine("\t\t\t\tjjutil.gotoNextFocus(objid); ");
                script.AppendLine("\t\t\t}, ");
                script.AppendLine("\t\t\terror: function (jqXHR, textStatus, errorThrown) { ");
                script.AppendLine("\t\t\t\tconsole.log(errorThrown); ");
                script.AppendLine("\t\t\t\tconsole.log(textStatus); ");
                script.AppendLine("\t\t\t\tconsole.log(jqXHR); ");
                script.AppendLine("\t\t\t} ");
                script.AppendLine("\t\t}); ");
                script.AppendLine("\t} ");

                script.AppendLine("");
                script.Append("\tfunction ");
                script.AppendFormat("do_change_{0}", Name);
                script.AppendLine("(nRow) { ");
                script.AppendLine("\t\tvar prefixSelector = \"\";");
                script.AppendLine("\t\tif(nRow != null) {");
                script.AppendLine("\t\t\tprefixSelector = \"tr#row\" + nRow + \" \";");
                script.AppendLine("\t\t}");


                foreach (var f in listFieldsPost)
                {
                    //Workaround to JJSearch
                    if (f.Component == FormComponent.Search)
                    {
                        script.Append("\t\t$(prefixSelector + \"");
                        script.Append(".");
                        script.Append(f.Name);
                        script.AppendLine("\").change(function () {");
                        script.AppendLine("\t\tvar obj = $(this);");
                        script.AppendLine("\t\tsetTimeout(function() {");
                        script.AppendLine("\t\t\tvar nRowId = obj.attr(\"gridViewRowIndex\");");
                        script.AppendLine("\t\t\tvar objid = obj.attr(\"id\");");
                        script.Append("\t\t\t");
                        script.Append(functionname);
                        script.Append("(nRowId, \"");
                        script.Append(f.Name);
                        script.AppendLine("\", objid);");
                        script.AppendLine("\t\t\t},200);");
                        script.AppendLine("\t\t});");
                        script.AppendLine("");
                    }
                    else
                    {
                        script.Append("\t\t$(prefixSelector + \"");
                        script.Append(".");
                        script.Append(f.Name);
                        script.AppendLine("\").change(function () {");
                        script.AppendLine("\t\t\tvar obj = $(this);");
                        script.AppendLine("\t\t\tvar nRowId = obj.attr(\"gridViewRowIndex\");");
                        script.AppendLine("\t\t\tvar objid = obj.attr(\"id\");");
                        script.Append("\t\t\t");
                        script.Append(functionname);
                        script.Append("(nRowId, \"");
                        script.Append(f.Name);
                        script.AppendLine("\", objid);");
                        script.AppendLine("\t\t});");
                        script.AppendLine("");
                    }
                }

                script.AppendLine("\t}");

                script.AppendLine("");
                script.AppendLine("\t$(document).ready(function () {");
                script.AppendLine($"\t\tdo_change_{Name}(null);");
                script.AppendLine("\t});");
            }
        }

        script.AppendLine("\t</script> ");

        return script.ToString();
    }

    internal async Task<IDictionary<string, object?>> GetDefaultValuesAsync() => _defaultValues ??=
        await FieldsService.GetDefaultValuesAsync(FormElement, null, PageState.List);

    internal async Task<FormStateData> GetFormDataAsync()
    {
        if (_formData == null)
        {
            var defaultValues = await FieldsService.GetDefaultValuesAsync(FormElement, null, PageState.List);
            var userValues = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            DataHelper.CopyIntoDictionary(userValues, UserValues, false);
            DataHelper.CopyIntoDictionary(userValues, defaultValues, true);

            _formData = new FormStateData(defaultValues, userValues, PageState.List);
        }

        return _formData;
    }
    
    private async Task<HtmlBuilder> GetSettingsHtml()
    {
        var action = ConfigAction;
        var formData = await GetFormDataAsync();
        bool isVisible = await ExpressionsService.GetBoolValueAsync(action.VisibleExpression, formData);
        if (!isVisible)
            return new HtmlBuilder(string.Empty);

        var modal = new JJModalDialog
        {
            Name = "config-modal-" + Name,
            Title = "Configure View"
        };

        var btnOk = new JJLinkButton
        {
            Text = "Ok",
            IconClass = "fa fa-check",
            ShowAsButton = true,
            OnClientClick = Scripts.GetConfigUIScript(ConfigAction, RelationValues)
        };
        modal.Buttons.Add(btnOk);

        var btnCancel = new JJLinkButton
        {
            Text = "Cancel",
            IconClass = "fa fa-times",
            ShowAsButton = true,
            OnClientClick = Scripts.GetCloseConfigUIScript()
        };
        modal.Buttons.Add(btnCancel);

        var form = new GridFormSettings(CurrentContext, StringLocalizer);
        modal.HtmlBuilderContent = form.GetHtmlElement(IsPaggingEnabled(), CurrentSettings);

        return modal.GetHtmlBuilder();
    }

    private async Task<HtmlBuilder> GetExportHtml()
    {
        var action = ExportAction;
        var formData = await GetFormDataAsync();
        bool isVisible = await ExpressionsService.GetBoolValueAsync(action.VisibleExpression, formData);
        if (!isVisible)
            return new HtmlBuilder(string.Empty);

        var modal = new JJModalDialog
        {
            Name = "export-modal-" + Name,
            Title = "Export"
        };

        return modal.GetHtmlBuilder();
    }

    private async Task<HtmlBuilder> GetLegendHtml()
    {
        var action = LegendAction;
        var formData = await GetFormDataAsync();
        bool isVisible = await ExpressionsService.GetBoolValueAsync(action.VisibleExpression, formData);
        if (!isVisible)
            return new HtmlBuilder(string.Empty);

        var legend = new JJLegendView(ComponentFactory.Controls.GetFactory<ComboBoxFactory>(), StringLocalizer)
        {
            ShowAsModal = true,
            FormElement = FormElement,
            Name = "iconlegend_modal_" + Name
        };
        return legend.GetHtmlBuilder();
    }

    internal string GetFieldName(string fieldName, IDictionary<string, object?> row)
    {
        string name = "";
        foreach (var fpk in PrimaryKeyFields)
        {
            if (name.Length > 0)
                name += "_";

            name += row[fpk.Name]!.ToString()!
                .Replace(" ", "_")
                .Replace("'", "")
                .Replace("\"", "");
        }

        name += fieldName;

        return name;
    }


    public IDictionary<string, object> GetSelectedRowId()
    {
        var values = new Dictionary<string, object>();
        string currentRow = CurrentContext.Request["grid-view-row-" + Name];

        if (string.IsNullOrEmpty(currentRow))
            return values;

        var decriptId = EncryptionService.DecryptStringWithUrlUnescape(currentRow);
        var @params = HttpUtility.ParseQueryString(decriptId);

        foreach (string key in @params)
        {
            values.Add(key, @params[key]!);
        }

        return values;
    }

    private async Task<ComponentResult> GetExportationResult()
    {
        string expressionType = CurrentContext.Request.QueryString("dataExportationOperation");
        switch (expressionType)
        {
            case "showOptions":
                return await DataExportation.GetResultAsync();
            case "startProcess":
                {
                    if (IsUserSetDataSource || OnDataLoad != null || OnDataLoadAsync != null)
                    {
                        var result = await GetDataSourceAsync(new EntityParameters
                        {
                            Filters = await GetCurrentFilterAsync(),
                            OrderBy = CurrentOrder,
                            RecordsPerPage = int.MaxValue,
                            CurrentPage = 1
                        });
                        DataExportation.StartExportation(result);
                    }
                    else
                    {
                        try
                        {
                            await ExportFileInBackground();
                        }
                        catch (Exception ex)
                        {
                            var err = new JJValidationSummary(ExceptionManager.GetMessage(ex))
                            {
                                MessageTitle = "Error"
                            };

                            return HtmlComponentResult.FromHtmlBuilder(err.GetHtmlBuilder());
                        }
                    }

                    var html = new DataExportationLog(DataExportation).GetHtmlProcess();


                    return HtmlComponentResult.FromHtmlBuilder(html);
                }
            case "checkProgress":
                {
                    var dto = DataExportation.GetCurrentProgress();
                    return new JsonComponentResult(dto);
                }
            case "stopProcess":
                DataExportation.StopExportation();
                return new JsonComponentResult(new {});
        }

        return new EmptyComponentResult();
    }

    public async Task ExportFileInBackground()
    {
        DataExportation.ExportFileInBackground(await GetCurrentFilterAsync(), CurrentOrder);
    }
    
    public async Task<IList<Dictionary<string,object?>>?> GetDictionaryListAsync()
    {
        await SetDataSource();

        return DataSource;
    }
    
    private async Task SetDataSource()
    {
        if (_dataSource == null || IsUserSetDataSource)
        {
            var result = await GetDataSourceAsync(new EntityParameters
            {
                Filters = await GetCurrentFilterAsync(),
                OrderBy = CurrentOrder,
                RecordsPerPage = CurrentSettings.RecordsPerPage,
                CurrentPage = CurrentPage
            });
            _dataSource = result.Data;
            TotalOfRecords = result.TotalOfRecords;
            //Se estiver paginando e não retornar registros volta para pagina inicial
            if (CurrentPage > 1 && TotalOfRecords == 0)
            {
                CurrentPage = 1;
                result = await GetDataSourceAsync(new EntityParameters
                {
                    Filters = await GetCurrentFilterAsync(),
                    OrderBy = CurrentOrder,
                    RecordsPerPage = CurrentSettings.RecordsPerPage,
                    CurrentPage = CurrentPage
                });
                _dataSource = result.Data;
                TotalOfRecords = result.TotalOfRecords;
            }
        }
    }

    private async Task<DictionaryListResult> GetDataSourceAsync(EntityParameters parameters)
    {
        DataTable dataTable;
        if (IsUserSetDataSource && DataSource != null)
        {

            var dataView = new DataView(EnumerableHelper.ConvertToDataTable(DataSource));
            dataView.Sort = parameters.OrderBy.ToQueryParameter();

            dataTable = dataView.ToTable();
            dataView.Dispose();
        }
        else if (OnDataLoad != null || OnDataLoadAsync != null)
        {
            var args = new GridDataLoadEventArgs
            {
                Filters = parameters.Filters,
                OrderBy = parameters.OrderBy,
                RecordsPerPage = parameters.RecordsPerPage,
                CurrentPage = parameters.CurrentPage,
            };

            OnDataLoad?.Invoke(this, args);

            if (OnDataLoadAsync != null)
            {
                await OnDataLoadAsync.Invoke(this, args);
            }


            TotalOfRecords = args.TotalOfRecords;
            
            return new DictionaryListResult(args.DataSource!, args.TotalOfRecords);
        }
        else
        {
            return await EntityRepository.GetDictionaryListAsync(FormElement, parameters);
        }

        return DictionaryListResult.FromDataTable(dataTable);
    }

    /// <remarks>
    /// Used with the <see cref="EnableEditMode"/> property
    /// </remarks>
    public async Task<List<IDictionary<string, object?>>?> GetGridValuesAsync(int recordsPerPage, int currentPage)
    {
        var result = await GetDataSourceAsync(new EntityParameters
        {
            Filters = await GetCurrentFilterAsync(),
            OrderBy = CurrentOrder,
            RecordsPerPage = recordsPerPage,
            CurrentPage = currentPage
        });

        return await GetGridValuesAsync(result.Data);
    }

    /// <remarks>
    /// Used with the EnableEditMode property
    /// </remarks>
    public async Task<List<IDictionary<string, object?>>?> GetGridValuesAsync(IList<Dictionary<string,object?>>? loadedData = null)
    {
        if (loadedData == null)
        {
            loadedData = await GetDictionaryListAsync();
            if (loadedData == null)
                return null;
        }

        var gridValues = new List<IDictionary<string, object?>>();
        foreach (var row in loadedData)
        {
            string fieldName = GetFieldName("", row);
            var newValues = await FormValuesService.GetFormValuesWithMergedValuesAsync(FormElement, PageState.List, row,
                AutoReloadFormFields, fieldName);
            gridValues.Add(newValues);
        }

        return gridValues;
    }

    /// <remarks>
    /// Used with the EnableMultSelect property
    /// </remarks>
    public List<IDictionary<string, object>> GetSelectedGridValues()
    {
        var listValues = new List<IDictionary<string, object>>();

        if (!EnableMultiSelect)
            return listValues;

        var inputHidden = SelectedRowsId;
        if (string.IsNullOrEmpty(inputHidden))
            return listValues;

        var pkList = inputHidden!.Split(',');

        var pkFields = PrimaryKeyFields;
        foreach (var pk in pkList)
        {
            var values = new Dictionary<string, object>();
            var descriptval = EncryptionService.DecryptStringWithUrlUnescape(pk);
            string[] ids = descriptval.Split(';');
            for (var i = 0; i < pkFields.Count; i++)
            {
                values.Add(pkFields[i].Name, ids[i]);
            }

            values.Add("INTERNALPK", descriptval);
            listValues.Add(values);
        }

        return listValues;
    }

    public void ClearSelectedGridValues()
    {
        SelectedRowsId = string.Empty;
    }

    internal async Task<string> GetEncryptedSelectedRowsAsync()
    {
        var result = await GetDataSourceAsync(new EntityParameters()
        {
            RecordsPerPage = int.MaxValue,
            CurrentPage = 1,
            OrderBy = CurrentOrder,
            Filters = await GetCurrentFilterAsync()
        });
        var selectedKeys = new StringBuilder();
        var hasVal = false;
        foreach (var row in result.Data)
        {
            if (!hasVal)
                hasVal = true;
            else
                selectedKeys.Append(",");

            string values = DataHelper.ParsePkValues(FormElement, row, ';');
            selectedKeys.Append(EncryptionService.EncryptStringWithUrlEscape(values));
        }

        return selectedKeys.ToString();
    }

    /// <summary>
    /// Validate the field and returns a Hashtable with the errors.
    /// </summary>
    /// <returns>
    /// Key = Field name
    /// Value = Message
    /// </returns>
    public async Task<IDictionary<string, object>> ValidateGridFieldsAsync(List<IDictionary<string, object?>> values)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        var errors = new Dictionary<string, object>();
        int line = 0;
        foreach (var row in values)
        {
            line++;
            var formData = new FormStateData(row, UserValues, PageState.List);
            foreach (var field in FormElement.Fields)
            {
                bool enabled = await ExpressionsService.GetBoolValueAsync(field.EnableExpression, formData);
                bool visible = await ExpressionsService.GetBoolValueAsync(field.VisibleExpression, formData);
                if (enabled && visible && field.DataBehavior is not FieldBehavior.ViewOnly)
                {
                    string? val = string.Empty;
                    if (row[field.Name] != null)
                        val = row[field.Name]?.ToString();

                    string objname = GetFieldName(field.Name, row);
                    string err = FieldsService.ValidateField(field, objname, val);
                    if (!string.IsNullOrEmpty(err))
                    {
                        string errMsg = $"{StringLocalizer["Line"]} {line}: {err}";
                        errors.Add(objname, errMsg);
                    }
                }
            }
        }

        return errors;
    }

    internal bool IsPaggingEnabled()
    {
        return !(!ShowPagging || CurrentPage == 0 || CurrentSettings.RecordsPerPage == 0 || TotalOfRecords == 0);
    }

    public void AddToolBarAction(SqlCommandAction action)
    {
        ValidateAction(action);
        ToolBarActions.Add(action);
    }

    public void AddToolBarAction(UrlRedirectAction action)
    {
        ValidateAction(action);
        ToolBarActions.Add(action);
    }

    public void AddToolBarAction(InternalAction action)
    {
        ValidateAction(action);
        ToolBarActions.Add(action);
    }

    public void AddToolBarAction(SubmitAction action)
    {
        ValidateAction(action);
        ToolBarActions.Add(action);
    }

    public void AddToolBarAction(ScriptAction action)
    {
        ValidateAction(action);
        ToolBarActions.Add(action);
    }

    /// <summary>
    /// Remove a custom button in the grid.
    /// </summary>
    ///<remarks>
    /// Only actions of types ScriptAction, UrlRedirectAction or InternalAction can be removed
    ///</remarks>
    public void RemoveToolBarAction(string actionName)
    {
        if (string.IsNullOrEmpty(actionName))
            throw new ArgumentNullException(nameof(actionName));

        var action = ToolBarActions.First(x => x.Name.Equals(actionName));

        switch (action)
        {
            case null:
                throw new ArgumentException(StringLocalizer["Action {0} not found", actionName]);
            case ScriptAction or UrlRedirectAction or InternalAction:
                ToolBarActions.Remove(action);
                break;
            default:
                throw new ArgumentException("This action can not be removed");
        }
    }

    public void AddGridAction(SqlCommandAction action)
    {
        ValidateAction(action);
        GridActions.Add(action);
    }

    public void AddGridAction(UrlRedirectAction action)
    {
        ValidateAction(action);
        GridActions.Add(action);
    }

    public void AddGridAction(InternalAction action)
    {
        ValidateAction(action);
        GridActions.Add(action);
    }

    public void AddGridAction(ScriptAction action)
    {
        ValidateAction(action);
        GridActions.Add(action);
    }

    private void AddGridAction(BasicAction action)
    {
        ValidateAction(action);
        GridActions.Add(action);
    }

    /// <summary>
    /// Remove custom button from grid.
    /// </summary>
    /// <remarks>
    /// Only actions of types ScriptAction, UrlRedirectAction or InternalAction
    /// can be removed
    /// </remarks>
    public void RemoveGridAction(string actionName)
    {
        if (string.IsNullOrEmpty(actionName))
            throw new ArgumentNullException(nameof(actionName));

        var action = GridActions.First(x => x.Name.Equals(actionName));
        switch (action)
        {
            case null:
                throw new ArgumentException(StringLocalizer["Action {0} not found", actionName]);
            case ScriptAction or UrlRedirectAction or InternalAction:
                GridActions.Remove(action);
                break;
            default:
                throw new ArgumentException("This action can not be removed");
        }
    }

    public BasicAction GetToolBarAction(string actionName)
    {
        return ToolBarActions.First(x => x.Name.Equals(actionName));
    }

    public BasicAction GetGridAction(string actionName)
    {
        return GridActions.First(x => x.Name.Equals(actionName));
    }

    /// <summary>
    /// Add or change a value in the CurrentFilter.<br></br>
    /// If it exists, change it, otherwise it includes it.
    /// </summary>
    public async Task SetCurrentFilterAsync(string field, object value)
    {
        var filter = await GetCurrentFilterAsync();
        filter[field] = value;
    }

    /// <summary>
    /// Verify if a action is valid, else, throws an exception.
    /// </summary>
    private static void ValidateAction(BasicAction action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        if (string.IsNullOrEmpty(action.Name))
            throw new ArgumentException("Property name action is not valid");
    }

    public void SetGridOptions(GridUI options)
    {
        FormElement.Options.Grid = options;
    }

    public bool IsExportPost()
    {
        return "export".Equals(CurrentContext.Request["dataExportationOperation"]) && Name.Equals(CurrentContext.Request["gridViewName"]);
    }
}