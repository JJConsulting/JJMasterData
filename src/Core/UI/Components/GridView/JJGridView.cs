#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Exportation.Configuration;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.Logging;
using JJMasterData.Core.UI.Events;
using JJMasterData.Core.UI.Events.Args;
using JJMasterData.Core.UI.Html;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

// ReSharper disable UnusedMember.Local

namespace JJMasterData.Core.UI.Components;

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
    #region Events
    

    public event AsyncEventHandler<GridCellEventArgs>? OnRenderCellAsync;
    
    /// <summary>
    /// Event fired when rendering the checkbox used to select the Grid row.
    /// <para/>Fired only when EnableMultiSelect property is enabled.
    /// </summary>
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
    public event GridDataLoadEventHandler? OnDataLoadAsync;
    public event AsyncEventHandler<ActionEventArgs>? OnRenderActionAsync;
    public event AsyncEventHandler<GridFilterLoadEventArgs>? OnFilterLoadAsync;
    public event AsyncEventHandler<GridToolbarActionEventArgs>? OnRenderToolbarActionAsync;
    public event AsyncEventHandler<GridRenderEventArgs>? OnBeforeTableRenderAsync;
    public event AsyncEventHandler<GridRenderEventArgs>? OnAfterTableRenderAsync;
    public event AsyncEventHandler<GridRowEventArgs>? OnRenderRowAsync;
    #endregion

    #region Properties
    private RouteContext? _routeContext;
    private OrderByData? _currentOrder;
    private string? _selectedRowsId;
    private int _currentPage;
    private GridSettings? _currentSettings;
    private GridSettingsForm? _gridSettingsForm;
    private ExportOptions? _currentExportConfig;
    private GridFilter? _filter;
    private GridTable? _table;
    private List<Dictionary<string,object?>>? _dataSource;
    private List<FormElementField>? _pkFields;
    private Dictionary<string, object?>? _defaultValues;
    private FormStateData? _formStateData;
    private ActionMap? _currentActionMap;
    private JJDataImportation? _dataImportation;
    private JJDataExportation? _dataExportation;
    private GridScripts? _gridScripts;
    private GridToolbar? _toolbar;
    
    private readonly FieldValidationService _fieldValidationService;
    private readonly UrlRedirectService _urlRedirectService;
    
    internal JJDataImportation DataImportation
    {
        get
        {
            if (_dataImportation != null)
                return _dataImportation;

            _dataImportation = ComponentFactory.DataImportation.Create(FormElement);
            _dataImportation.UserValues = UserValues;
            _dataImportation.ProcessOptions = ImportAction.ProcessOptions;
            _dataImportation.Name = $"{Name}-importation";

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
            _dataExportation.ExportOptions = CurrentExportConfig;
            _dataExportation.ShowBorder = CurrentSettings.ShowBorder;
            _dataExportation.ShowRowStriped = CurrentSettings.ShowRowStriped;
            _dataExportation.UserValues = UserValues;
            _dataExportation.ProcessOptions = ExportAction.ProcessOptions;
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

            _pkFields = FormElement.Fields.FindAll(x => x.IsPk);

            return _pkFields;
        }
    }

    internal async ValueTask<List<FormElementField>> GetVisibleFieldsAsync()
    {
        if (FormElement == null)
            throw new ArgumentNullException(nameof(FormElement));

        var defaultValues = await GetDefaultValuesAsync();
        var formStateData = new FormStateData(defaultValues, UserValues, PageState.List);
        List<FormElementField> fields = [];
        
        foreach (var field in FormElement.Fields.Where(VisibleAtGrid))
        {
            var isVisible =  ExpressionsService.GetBoolValue(field.VisibleExpression, formStateData);
            if (isVisible)
                fields.Add(field);
        }

        return fields;
    }

    private static bool VisibleAtGrid(FormElementField field)
    {
        return field.DataBehavior is not FieldBehavior.WriteOnly && field.DataBehavior is not FieldBehavior.Virtual;
    }

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
    public List<Dictionary<string,object?>>? DataSource
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
    
    public List<TitleAction>? TitleActions { get; set; }
    
    public bool EnableFilter { get; set; }

    public string? ParentComponentName { get; set; }
    
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
            
            if (!CurrentContext.Request.Form.ContainsFormValues())
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
                _currentOrder = OrderByData.FromString(CurrentContext.Request[$"grid-view-order-{Name}"]);
                if (_currentOrder == null)
                {
                    var tableOrder = CurrentContext.Session[$"jj-grid-view-order-{Name}"];
                    if (tableOrder != null)
                    {
                        _currentOrder = OrderByData.FromString(tableOrder);
                    }
                    else
                    {
                        _currentOrder = new OrderByData();
                    }
                }
            }

            CurrentOrder = _currentOrder ?? new OrderByData();
            return _currentOrder!;
        }
        set
        {
            CurrentContext.Session[$"jj-grid-view-order-{Name}"] = value.ToQueryParameter();
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

            if (CurrentContext.Request.Form.ContainsFormValues())
            {
                int currentPage = 1;
                string tablePageId = $"grid-view-page-{Name}";
                if (!string.IsNullOrEmpty(CurrentContext.Request[tablePageId]))
                {
                    if (int.TryParse(CurrentContext.Request[tablePageId], out var page))
                        currentPage = page;
                }
                else
                {
                    var tablePage = CurrentContext.Session[$"jjcurrentpage_{Name}"];
                    if (tablePage != null)
                    {
                        if (int.TryParse(tablePage, out var page))
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
                    var tablePage = CurrentContext.Session[$"jjcurrentpage_{Name}"];
                    if (tablePage != null)
                    {
                        if (int.TryParse(tablePage, out var nAuxPage))
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

    internal GridSettingsForm GridSettingsForm => _gridSettingsForm ??= new(Name, CurrentContext, StringLocalizer);

    /// <summary>
    /// <see cref="GridSettings"/>
    /// </summary>
    public GridSettings CurrentSettings
    {
        get
        {
            if (_currentSettings != null)
                return _currentSettings;
            
            var action = CurrentActionMap?.GetAction<ConfigAction>(FormElement);
            if (action is not null)
            {
                CurrentSettings = GridSettingsForm.LoadFromForm();
                return _currentSettings!;
            }

            if (MaintainValuesOnLoad)
                CurrentSettings = CurrentContext.Session.GetSessionValue<GridSettings>($"jjcurrentui_{FormElement.Name}");

            if (_currentSettings == null)
                CurrentSettings = GridSettingsForm.LoadFromForm();
            
            return _currentSettings!;
        }
        set
        {
            if (MaintainValuesOnLoad)
                CurrentContext.Session.SetSessionValue($"jjcurrentui_{FormElement.Name}", value);

            _currentSettings = value;
        }
    }

    internal GridFilter Filter
    {
        get
        {
            if (_filter != null) 
                return _filter;
            
            _filter = new GridFilter(this);
            _filter.OnFilterLoadAsync += OnFilterLoadAsync;

            return _filter;
        }
    }

    internal GridTable Table
    {
        get
        {
            if (_table != null)
                return _table;

            _table = new GridTable(this);
            
            _table.Body.OnRenderActionAsync += OnRenderActionAsync;
            _table.Body.OnRenderCellAsync += OnRenderCellAsync;
            _table.Body.OnRenderSelectedCellAsync += OnRenderSelectedCellAsync;
            _table.Body.OnRenderRowAsync += OnRenderRowAsync;

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
            if (CurrentContext.Request.Form.ContainsFormValues())
            {
                _currentExportConfig = ExportOptions.LoadFromForm(CurrentContext.Request.Form, Name);
            }

            return _currentExportConfig;
        }
        set => _currentExportConfig = value;
    }

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
    public string? EmptyDataText { get; set; }

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
    /// If the CurrentUI.RecordsPerPage property is equal to zero, pagination will not be displayed.
    /// <para/>
    /// If the TotalRecords property is equal to zero, pagination will not be displayed.
    /// </remarks>
    public bool ShowPaging { get; set; }

    /// <summary>
    /// Key-Value pairs with the errors.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public Dictionary<string, string> Errors { get; } = new Dictionary<string, string>();

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
    public Dictionary<string, object> RelationValues { get; set; }

    public HeadingSize TitleSize { get; set; } 

    public int TotalOfRecords { get; set; }

    public LegendAction LegendAction => ToolbarActions.LegendAction;
    public RefreshAction RefreshAction => ToolbarActions.RefreshAction;
    public FilterAction FilterAction => ToolbarActions.FilterAction;
    public ImportAction ImportAction => ToolbarActions.ImportAction;
    public ExportAction ExportAction => ToolbarActions.ExportAction;
    public ConfigAction ConfigAction => ToolbarActions.ConfigAction;
    public SortAction SortAction => ToolbarActions.SortAction;
    public InsertAction InsertAction => ToolbarActions.InsertAction;
    
    public ViewAction ViewAction => TableActions.ViewAction;
    public EditAction EditAction => TableActions.EditAction;
    public DeleteAction DeleteAction => TableActions.DeleteAction;
    
    public GridToolbarActionList ToolbarActions => FormElement.Options.GridToolbarActions;
    public GridTableActionList TableActions => FormElement.Options.GridTableActions;
    
    private ActionMap? CurrentActionMap
    {
        get
        {
            if (_currentActionMap != null) 
                return _currentActionMap;
            var encryptedActionMap = CurrentContext.Request.Form[$"grid-view-action-map-{Name}"];
            if (string.IsNullOrEmpty(encryptedActionMap))
                return null;

            _currentActionMap = EncryptionService.DecryptActionMap(encryptedActionMap);
            return _currentActionMap;
        }
        set => _currentActionMap = value;
    }

    private string? SelectedRowsId
    {
        get => _selectedRowsId ??= CurrentContext.Request.Form[$"grid-view-selected-rows-{Name}"];
        set => _selectedRowsId = value ?? "";
    }
    
    protected RouteContext RouteContext
    {
        get
        {
            if (_routeContext != null)
                return _routeContext;

            var factory = new RouteContextFactory(CurrentContext.Request.QueryString, EncryptionService);
            _routeContext = factory.Create();
            
            return _routeContext;
        }
    }
    
    internal ComponentContext ComponentContext => RouteContext.ComponentContext;

    
    #endregion

    #region Injected Services
    internal ExpressionsService ExpressionsService { get; }
    internal FormValuesService FormValuesService { get; }
    internal FieldValuesService FieldValuesService { get; }
    internal IStringLocalizer<MasterDataResources> StringLocalizer { get; }

    internal HtmlTemplateService HtmlTemplateService { get; }
    internal IComponentFactory ComponentFactory { get; }
    internal IEntityRepository EntityRepository { get; }

    internal GridScripts Scripts => _gridScripts ??= new GridScripts(this);

    internal IHttpContext CurrentContext { get; }
    internal DataItemService DataItemService { get; }
    internal IEncryptionService EncryptionService { get; }

    private GridToolbar Toolbar
    {
        get
        {
            if (_toolbar is null)
            {
                _toolbar = new GridToolbar(this);
                _toolbar.OnRenderToolbarActionAsync += OnRenderToolbarActionAsync;
            }

            return _toolbar;
        }
    }

    internal ILogger<JJGridView> Logger { get; }
    internal FieldFormattingService FieldFormattingService { get; }

    #endregion

    #region Constructors

    internal JJGridView(
        FormElement formElement,
        IHttpContext currentContext,
        IEntityRepository entityRepository,
        IEncryptionService encryptionService,
        DataItemService dataItemService,
        ExpressionsService expressionsService,
        FormValuesService formValuesService,
        FieldFormattingService fieldFormattingService,
        FieldValuesService fieldValuesService,
        FieldValidationService fieldValidationService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        UrlRedirectService urlRedirectService,
        HtmlTemplateService htmlTemplateService,
        ILogger<JJGridView> logger,
        IComponentFactory componentFactory)
    {
        Name = formElement.Name.ToLowerInvariant();
        FormElement = formElement;
        ShowTitle =  formElement.Options.Grid.ShowTitle;
        EnableFilter = true;
        EnableSorting = formElement.Options.Grid.EnableSorting;
        ShowHeaderWhenEmpty = formElement.Options.Grid.ShowHeaderWhenEmpty;
        ShowPaging = formElement.Options.Grid.ShowPagging;
        ShowToolbar = formElement.Options.Grid.ShowToolBar;
        EmptyDataText = formElement.Options.Grid.EmptyDataText;
        AutoReloadFormFields = true;
        RelationValues = new Dictionary<string, object>();
        TitleSize = formElement.TitleSize;
        
        ExpressionsService = expressionsService;
        EncryptionService = encryptionService;
        StringLocalizer = stringLocalizer;
        _urlRedirectService = urlRedirectService;
        HtmlTemplateService = htmlTemplateService;
        Logger = logger;
        ComponentFactory = componentFactory;
        EntityRepository = entityRepository;
        CurrentContext = currentContext;
        DataItemService = dataItemService;
        FormValuesService = formValuesService;
        FieldValuesService = fieldValuesService;
        _fieldValidationService = fieldValidationService;
        FieldFormattingService = fieldFormattingService;
    }

    #endregion

    protected override async Task<ComponentResult> BuildResultAsync()
    {
        if (!RouteContext.CanRender(FormElement))
            return EmptyComponentResult.Value;
        
        if (ComponentContext is ComponentContext.GridViewReload)
            return new ContentComponentResult(await GetTableHtmlBuilder());
        
        if (ComponentContext is ComponentContext.DataExportation)
            return await GetExportationResult();

        if (ComponentContext is ComponentContext.DownloadFile)
            return ComponentFactory.Downloader.Create().GetDownloadResult();
        
        if (ComponentContext is ComponentContext.GridViewRow)
        {
            int rowIndex = int.Parse(CurrentContext.Request.QueryString["gridViewRowIndex"]);

            await SetDataSource();
            
            var htmlResponse = await GetTableRowHtmlAsync(rowIndex);

            return new ContentComponentResult(new HtmlBuilder(htmlResponse));
        }

        if (ComponentContext is ComponentContext.GridViewSelectAllRows)
        {
            string selectedRows = await GetEncryptedSelectedRowsAsync();
            
            return new JsonComponentResult(new {selectedRows});
        }

        if (ComponentContext is ComponentContext.GridViewFilterReload)
        {
            var html = await Filter.GetFilterHtml();
            
            return new ContentComponentResult(html);
        }
        
        if (ComponentContext is ComponentContext.SearchBoxFilter)
        {
            var fieldName = CurrentContext.Request.QueryString["fieldName"];
            var field = FormElement.Fields[fieldName];
            var formStateData = new FormStateData(await GetCurrentFilterAsync(), UserValues, PageState.Filter);
            var jjSearchBox = ComponentFactory.Controls.Create(FormElement,field, formStateData, Name) as JJSearchBox;
            jjSearchBox!.Name = GridFilter.FilterFieldPrefix + jjSearchBox.Name;
            return await jjSearchBox.GetItemsResult();
        }

        if (ComponentContext is ComponentContext.UrlRedirect)
        {
            return await _urlRedirectService.GetUrlRedirectResult(this,CurrentActionMap);
        }

        HtmlBuilder? sqlActionError = null;

        if (TryGetSqlAction(out var sqlCommandAction))
        {
            var sqlResult = await ExecuteSqlCommand(sqlCommandAction);
            if (sqlResult == EmptyComponentResult.Value)
                CurrentActionMap = null;
            else if (sqlResult is RedirectComponentResult redirectResult)
            {
                return redirectResult;
            }
            else if(sqlResult is RenderedComponentResult result)
            {
                sqlActionError = result.HtmlBuilder;
            }
        }

        var gridHtml = await GetHtmlBuilderAsync();

        if (sqlActionError is not null)
        {
            gridHtml.Append(sqlActionError);
        }
        
        return new RenderedComponentResult(gridHtml);
    }

    internal async Task<HtmlBuilder> GetHtmlBuilderAsync()
    {
        var html = new HtmlBuilder(HtmlTag.Div);
        html.WithAttribute("id", Name);
        await html.AppendAsync(HtmlTag.Div, async div =>
        {
            if (ShowTitle)
                div.AppendComponent(GetTitle());
            
            if (FilterAction.IsVisible)
                div.Append(await Filter.GetFilterHtml());

            if (OnBeforeTableRenderAsync is not null)
            {
                await OnBeforeTableRenderAsync(this, new()
                {
                    HtmlBuilder = div
                });
            }

            if (ShowToolbar)
                div.Append(await GetToolbarHtmlBuilder());

            div.Append(await GetTableHtmlBuilder());
            
            if (OnAfterTableRenderAsync is not null)
            {
                await OnAfterTableRenderAsync(this, new()
                {
                    HtmlBuilder = div
                });
            }
        });


        return html;
    }

    public ValueTask<Dictionary<string, object?>> GetCurrentFilterAsync()
    {
        return Filter.GetCurrentFilterAsync();
    }


    public void SetCurrentFilter(string key, object value)
    {
        Filter.SetCurrentFilter(key, value);
    }

    public async Task<string> GetTableHtmlAsync() => (await GetTableHtmlBuilder()).ToString();

    private async Task<HtmlBuilder> GetTableHtmlBuilder()
    {
        AssertProperties();
        
        var html = new HtmlBuilder(HtmlTag.Div);

        await SetDataSource();

        var totalPages = (int)Math.Ceiling(TotalOfRecords / (double)CurrentSettings.RecordsPerPage);
        
        html.WithAttribute("id", $"grid-view-table-{Name}");

        if (SortAction.IsVisible)
            await html.AppendAsync(GetSortingConfigAsync);
        
        html.AppendRange(GetHiddenInputs());

        if (CurrentPage <= 0)
        {
            html.AppendComponent(GetPaginationWarningAlert(totalPages));
        }
        else if (CurrentPage > totalPages && totalPages != 0)
        {
            html.AppendComponent(GetPaginationWarningAlert(totalPages));
        }
        else
        {
            html.Append(await Table.GetHtmlBuilder());
            
            if (DataSource?.Count == 0 && !string.IsNullOrEmpty(EmptyDataText))
                html.Append(await GetNoRecordsAlert());
            
            if (IsPagingEnabled())
            {
                var gridPagination = new GridPagination(this);

                html.Append(gridPagination.GetHtmlBuilder());
            }
        }
        
        if (ShowToolbar)
        {
            html.Append(await GetSettingsHtml());

            html.Append(await GetExportHtml());

            html.Append(await GetCaptionHtml());
        }

        html.Append(HtmlTag.Div, div => div.WithCssClass("clearfix"));

        return html;
    }

    private JJAlert GetPaginationWarningAlert(int totalPages)
    {
        return new JJAlert
        {
            Title = StringLocalizer["Warning"],
            InnerHtml = new HtmlBuilder(HtmlTag.Span)
                .AppendText(StringLocalizer["Page must be between 1 and {0}.",totalPages])
                .AppendLink("Click here to go to the first page.", $"javascript:{Scripts.GetPaginationScript(1)}"),
            Color = BootstrapColor.Warning,
            Icon = IconType.SolidTriangleExclamation
        };
    }

    private IEnumerable<HtmlBuilder> GetHiddenInputs()
    {
        yield return new HtmlBuilder().AppendHiddenInput($"grid-view-order-{Name}", CurrentOrder.ToQueryParameter() ?? string.Empty);
        yield return new HtmlBuilder().AppendHiddenInput($"grid-view-page-{Name}", CurrentPage.ToString());
        yield return new HtmlBuilder().AppendHiddenInput($"grid-view-action-map-{Name}", EncryptionService.EncryptObject(CurrentActionMap) ?? string.Empty);
        yield return new HtmlBuilder().AppendHiddenInput($"grid-view-row-{Name}", string.Empty);

        if (EnableMultiSelect)
        {
            yield return new HtmlBuilder().AppendHiddenInput($"grid-view-selected-rows-{Name}", SelectedRowsId ?? string.Empty);
        }
    }

    internal async Task<string> GetTableRowHtmlAsync(int rowIndex)
    {
        var row = DataSource?[rowIndex];

        string result = string.Empty;
        foreach (var builder in await Table.Body.GetTdHtmlList(row ?? new Dictionary<string, object?>(), rowIndex))
            result += builder;
        
        return result;
    }
    
    public string GetTitleHtml()
    {
        return GetTitle().GetHtml();
    }
    
    internal JJTitle GetTitle()
    {
        return ComponentFactory.Html.Title.Create(FormElement, new FormStateData(RelationValues!,UserValues, PageState.List), TitleActions);
    }

    internal ValueTask<HtmlBuilder> GetToolbarHtmlBuilder() => Toolbar.GetHtmlBuilderAsync();

    public ValueTask<HtmlBuilder> GetFilterHtmlAsync() => Filter.GetFilterHtml();

    public ValueTask<HtmlBuilder> GetToolbarHtmlAsync() => GetToolbarHtmlBuilder();

    private Task<HtmlBuilder> GetSortingConfigAsync() => new GridSortingConfig(this).GetHtmlBuilderAsync();

    private bool TryGetSqlAction(out SqlCommandAction? sqlCommandAction)
    {
        var action = CurrentActionMap?.GetAction(FormElement);
        if (action is SqlCommandAction sqlAction)
        {
            sqlCommandAction = sqlAction;
            return true;
        }

        sqlCommandAction = null;
        
        return false;
    }

    private Task<ComponentResult> ExecuteSqlCommand(SqlCommandAction? action)
    {
        if (action is null)
            throw new JJMasterDataException("Action not found at your FormElement");
        
        var gridSqlAction = new GridSqlCommandAction(this);
        
        return gridSqlAction.ExecuteSqlCommand(CurrentActionMap, action);
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
            Color = BootstrapColor.Default,
            Title = StringLocalizer[EmptyDataText!],
            Icon = IconType.InfoCircle
        };

        var hasFilter = await Filter.HasFilter();

        if (!hasFilter)
            return alert.GetHtmlBuilder();

        alert.Messages.Add(StringLocalizer["There are filters applied for this query."]);
        alert.Icon = IconType.Filter;

        return alert.GetHtmlBuilder();
    }

    internal async ValueTask<Dictionary<string, object?>> GetDefaultValuesAsync() => _defaultValues ??=
        await FieldValuesService.GetDefaultValuesAsync(FormElement, new FormStateData(new Dictionary<string, object?>(),UserValues, PageState.List));

    internal async ValueTask<FormStateData> GetFormStateDataAsync()
    {
        if (_formStateData == null)
        {
            var defaultValues = await FieldValuesService.GetDefaultValuesAsync(FormElement, new FormStateData(new Dictionary<string, object?>(RelationValues!),UserValues, PageState.List));
            var userValues = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            DataHelper.CopyIntoDictionary(userValues, RelationValues!, true);
            DataHelper.CopyIntoDictionary(userValues, UserValues);

            _formStateData = new FormStateData(defaultValues, userValues, PageState.List);
        }

        return _formStateData;
    }
    
    private async Task<HtmlBuilder> GetSettingsHtml()
    {
        var action = ConfigAction;
        var formData = await GetFormStateDataAsync();
        bool isVisible = ExpressionsService.GetBoolValue(action.VisibleExpression, formData);
        if (!isVisible)
            return new HtmlBuilder(string.Empty);

        var modal = new JJModalDialog
        {
            Name = $"config-modal-{Name}",
            Size = ModalSize.Small,
            Title = StringLocalizer["Configure Grid"]
        };

        var btnOk = ComponentFactory.Html.LinkButton.Create();
        btnOk.Text = StringLocalizer["Ok"];
        btnOk.IconClass = "fa fa-check";
        btnOk.ShowAsButton = true;
        btnOk.OnClientClick = Scripts.GetGridSettingsScript(ConfigAction, RelationValues);
        modal.Buttons.Add(btnOk);

        var btnCancel = ComponentFactory.Html.LinkButton.Create();
        btnCancel.Text = StringLocalizer["Cancel"];
        btnCancel.IconClass = "fa fa-times";
        btnCancel.ShowAsButton = true;
        btnCancel.OnClientClick = Scripts.GetCloseConfigUIScript();
        modal.Buttons.Add(btnCancel);
        modal.HtmlBuilderContent = GridSettingsForm.GetHtmlBuilder(CanCustomPaging(), CurrentSettings);

        return modal.GetHtmlBuilder();
    }

    private bool CanCustomPaging() => IsPagingEnabled() && CurrentSettings.RecordsPerPage % 5 == 0 && CurrentSettings.RecordsPerPage <= 50;

    private async ValueTask<HtmlBuilder> GetExportHtml()
    {
        var action = ExportAction;
        var formData = await GetFormStateDataAsync();
        var isVisible = ExpressionsService.GetBoolValue(action.VisibleExpression, formData);
        if (!isVisible)
            return new HtmlBuilder(string.Empty);

        var modal = new JJModalDialog
        {
            Name = $"data-exportation-modal-{Name}",
            Title = StringLocalizer["Export"]
        };

        return modal.GetHtmlBuilder();
    }

    private async Task<HtmlBuilder> GetCaptionHtml()
    {
        var action = LegendAction;
        var formData = await GetFormStateDataAsync();
        var isVisible = ExpressionsService.GetBoolValue(action.VisibleExpression, formData);

        if (!isVisible)
            return new HtmlBuilder(string.Empty);

        var captionView = new GridCaptionView(action.Tooltip,ComponentFactory.Controls.ComboBox, StringLocalizer)
        {
            Name = Name,
            ShowAsModal = true,
            FormElement = FormElement
        };
        
        return await captionView.GetHtmlBuilderAsync();
    }

    internal string GetFieldName(string fieldName, Dictionary<string, object?> row)
    {
        string name = "";
        foreach (var fpk in PrimaryKeyFields)
        {
            if (name.Length > 0)
                name += "_";
                
            if(row.TryGetValue(fpk.Name, out var fieldValue))
            {
                name += fieldValue?.ToString()
                    ?.Replace(" ", "_")
                    ?.Replace("'", "")
                    ?.Replace("\"", "");
            }
        }

        name += fieldName;

        return name;
    }


    public Dictionary<string, object> GetSelectedRowId()
    {
        var values = new Dictionary<string, object>();
        string currentRow = CurrentContext.Request[$"grid-view-row-{Name}"];

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
        string expressionType = CurrentContext.Request.QueryString["dataExportationOperation"];
        switch (expressionType)
        {
            case "showOptions":
                return await DataExportation.GetResultAsync();
            case "startProcess":
                {
                    if (IsUserSetDataSource)
                    {
                        var result = await GetDataSourceAsync(new EntityParameters
                        {
                            Filters = await GetCurrentFilterAsync(),
                            OrderBy = CurrentOrder,
                            RecordsPerPage = int.MaxValue,
                            CurrentPage = 1
                        });
                        var exportationResult = await DataExportation.ExecuteExportationAsync(result);
                        return exportationResult;
                    }

                    try
                    {
                        await ExportFileInBackground();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Error executing DataExportation.");
                        var errorMessage = StringLocalizer[ExceptionManager.GetMessage(ex)];
                        var validationSummary =ComponentFactory.Html.ValidationSummary.Create(errorMessage);
                        validationSummary.MessageTitle = StringLocalizer["Error"];
                        return new ContentComponentResult(validationSummary.GetHtmlBuilder());
                    }

                    var html = new DataExportationLog(DataExportation).GetLoadingHtml();
                    return new ContentComponentResult(html);
                }
            case "checkProgress":
                {
                    var dto = DataExportation.GetCurrentProgress();
                    return new JsonComponentResult(dto);
                }
            case "stopProcess":
                DataExportation.StopImportation();
                return new JsonComponentResult(new {});
        }

        return EmptyComponentResult.Value;
    }

    public async ValueTask ExportFileInBackground()
    {
        DataExportation.ExportFileInBackground(await GetCurrentFilterAsync(), CurrentOrder);
    }
    
    public async Task<List<Dictionary<string,object?>>?> GetDictionaryListAsync()
    {
        await SetDataSource();

        return DataSource;
    }
    
    private async Task SetDataSource()
    {
        if (DataSource == null && !IsUserSetDataSource)
        {
            var filters = await GetCurrentFilterAsync();
            var result = await GetDataSourceAsync(new EntityParameters
            {
                Filters = filters,
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
        try
        {
            if (IsUserSetDataSource && DataSource != null)
            {

                using var dataView = new DataView(EnumerableHelper.ConvertToDataTable( new List<Dictionary<string,object?>>(DataSource)));
                dataView.Sort = parameters.OrderBy.ToQueryParameter();
                var dataTable = dataView.ToTable();
            
                return DictionaryListResult.FromDataTable(dataTable);
            }

            if (OnDataLoadAsync != null)
            {
                var args = new GridDataLoadEventArgs
                {
                    Filters = parameters.Filters,
                    OrderBy = parameters.OrderBy,
                    RecordsPerPage = parameters.RecordsPerPage,
                    CurrentPage = parameters.CurrentPage,
                };
            
                await OnDataLoadAsync.Invoke(this, args);
            
                if (args.DataSource is not null)
                {
                    TotalOfRecords = args.TotalOfRecords;
            
                    return new DictionaryListResult(args.DataSource, args.TotalOfRecords);
                }
            }

            return await EntityRepository.GetDictionaryListResultAsync(FormElement, parameters);
        }
        catch (Exception ex)
        {
            Logger.LogGridViewDataSourceException(ex, FormElement.Name);
            throw;
        }
    }

    /// <remarks>
    /// Used with the <see cref="EnableEditMode"/> property
    /// </remarks>
    public async Task<List<Dictionary<string, object?>>?> GetGridValuesAsync(int recordsPerPage, int currentPage)
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
    public async Task<List<Dictionary<string, object?>>?> GetGridValuesAsync(List<Dictionary<string,object?>>? loadedData = null)
    {
        if (loadedData == null)
        {
            loadedData = await GetDictionaryListAsync();
            if (loadedData == null)
                return null;
        }

        var gridValues = new List<Dictionary<string, object?>>();
        foreach (var row in loadedData)
        {
            string fieldName = GetFieldName("", row);
            var newValues = await FormValuesService.GetFormValuesWithMergedValuesAsync(FormElement, new FormStateData(row!, UserValues, PageState.List),
                AutoReloadFormFields, fieldName);
            gridValues.Add(newValues);
        }

        return gridValues;
    }

    /// <remarks>
    /// Used with the EnableMultiSelect property
    /// </remarks>
    public List<Dictionary<string, object>> GetSelectedGridValues()
    {
        var listValues = new List<Dictionary<string, object>>();

        if (!EnableMultiSelect)
            return listValues;

        var inputHidden = SelectedRowsId;
        if (inputHidden == null || string.IsNullOrEmpty(inputHidden))
            return listValues;

        var pkList = inputHidden.Split(',');

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
        var result = await GetDataSourceAsync(new EntityParameters
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
                selectedKeys.Append(',');

            string values = DataHelper.ParsePkValues(FormElement, row, ';');
            selectedKeys.Append((string?)EncryptionService.EncryptStringWithUrlEscape(values));
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
    public Dictionary<string, string> ValidateGridFields(List<Dictionary<string, object?>> values)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        var errors = new Dictionary<string, string>();
        int line = 0;
        foreach (var row in values)
        {
            line++;
            var formData = new FormStateData(row, UserValues, PageState.List);
            foreach (var field in FormElement.Fields)
            {
                bool enabled = ExpressionsService.GetBoolValue(field.EnableExpression, formData);
                bool visible = ExpressionsService.GetBoolValue(field.VisibleExpression, formData);
                if (enabled && visible && field.DataBehavior is not FieldBehavior.ViewOnly)
                {
                    string? val = string.Empty;
                    if (row[field.Name] != null)
                        val = row[field.Name]?.ToString();

                    string objname = GetFieldName(field.Name, row);
                    string err = _fieldValidationService.ValidateField(field, objname, val);
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

    internal bool IsPagingEnabled()
    {
        return !(
            !ShowPaging 
            || CurrentPage == 0 
            || CurrentSettings.RecordsPerPage == 0 
            || TotalOfRecords == 0);
    }

    public void AddToolbarAction(SqlCommandAction action)
    {
        ValidateAction(action);
        ToolbarActions.Add(action);
    }

    public void AddToolbarAction(UrlRedirectAction action)
    {
        ValidateAction(action);
        ToolbarActions.Add(action);
    }

    public void AddToolbarAction(InternalAction action)
    {
        ValidateAction(action);
        ToolbarActions.Add(action);
    }

    public void AddToolbarAction(ScriptAction action)
    {
        ValidateAction(action);
        ToolbarActions.Add(action);
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

        var action = ToolbarActions.First(x => x.Name.Equals(actionName));

        switch (action)
        {
            case null:
                throw new ArgumentException(StringLocalizer["Action {0} not found", actionName]);
            case ScriptAction or UrlRedirectAction or InternalAction:
                ToolbarActions.Remove(action);
                break;
            default:
                throw new ArgumentException("This action can not be removed");
        }
    }

    public void AddGridAction(SqlCommandAction action)
    {
        ValidateAction(action);
        TableActions.Add(action);
    }

    public void AddGridAction(UrlRedirectAction action)
    {
        ValidateAction(action);
        TableActions.Add(action);
    }

    public void AddGridAction(InternalAction action)
    {
        ValidateAction(action);
        TableActions.Add(action);
    }

    public void AddGridAction(ScriptAction action)
    {
        ValidateAction(action);
        TableActions.Add(action);
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

        var action = TableActions.First(x => x.Name.Equals(actionName));
        switch (action)
        {
            case null:
                throw new ArgumentException(StringLocalizer["Action {0} not found", actionName]);
            case ScriptAction or UrlRedirectAction or InternalAction:
                TableActions.Remove(action);
                break;
            default:
                throw new ArgumentException("This action can not be removed");
        }
    }

    public BasicAction GetToolBarAction(string actionName)
    {
        return ToolbarActions.First(x => x.Name.Equals(actionName));
    }

    public BasicAction GetGridAction(string actionName)
    {
        return TableActions.First(x => x.Name.Equals(actionName));
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
    
    public void SetGridOptions(GridUI options) => FormElement.Options.Grid = options;

    public bool IsExportPost()
    {
        return "startProcess".Equals(CurrentContext.Request["dataExportationOperation"]) && Name.Equals(CurrentContext.Request["gridViewName"]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool HasAction() => CurrentActionMap is not null;

    public ActionContext GetActionContext(BasicAction basicAction, FormStateData formStateData)
    {
        return new ActionContext
        {
            Action = basicAction,
            FormElement = FormElement,
            FormStateData = formStateData,
            ParentComponentName = Name
        };
    }
}