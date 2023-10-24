#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Extensions;
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
using JJMasterData.Core.UI.Events.Args;
using JJMasterData.Core.UI.Html;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;

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

    #region Properties
    private RouteContext? _routeContext;
    private OrderByData? _currentOrder;
    private string? _selectedRowsId;
    private int _currentPage;
    private GridSettings? _currentSettings;
    private ExportOptions? _currentExportConfig;
    private GridFilter? _filter;
    private GridTable? _table;
    private IList<Dictionary<string,object?>>? _dataSource;
    private List<FormElementField>? _pkFields;
    private IDictionary<string, object?>? _defaultValues;
    private FormStateData? _formStateData;
    private ActionMap? _currentActionMap;
    private JJDataImportation? _dataImportation;
    private JJDataExportation? _dataExportation;
    private GridScripts? _gridScripts;

    internal JJDataImportation DataImportation
    {
        get
        {
            if (_dataImportation != null)
                return _dataImportation;

            _dataImportation = ComponentFactory.DataImportation.Create(FormElement);
            _dataImportation.UserValues = UserValues;
            _dataImportation.ProcessOptions = ImportAction.ProcessOptions;
            _dataImportation.Name = $"{Name}_dataimp";

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
        foreach (var f in FormElement.Fields.Where(f=>f.DataBehavior is not FieldBehavior.Virtual))
        {
            bool isVisible =  ExpressionsService.GetBoolValue(f.VisibleExpression, formData);
            if (isVisible)
                yield return f;
        }
    }

    internal FormValuesService FormValuesService { get; }

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
            
            var action = CurrentActionMap?.GetAction(FormElement);
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
            var encryptedActionMap = CurrentContext.Request.Form[$"grid-view-action-map-{Name}"];
            if (string.IsNullOrEmpty(encryptedActionMap))
                return null;

            _currentActionMap = EncryptionService.DecryptActionMap(encryptedActionMap);
            return _currentActionMap;
        }
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
    internal FieldsService FieldsService { get; }
    internal ExpressionsService ExpressionsService { get; }

    internal IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    internal IComponentFactory ComponentFactory { get; }
    internal IEntityRepository EntityRepository { get; }

    internal GridScripts Scripts => _gridScripts ??= new GridScripts(this);

    internal IHttpContext CurrentContext { get; }
    internal DataItemService DataItemService { get; }
    internal IEncryptionService EncryptionService { get; }

    #endregion

    #region Constructors

    internal JJGridView(FormElement formElement,
        IHttpContext currentContext,
        IEntityRepository entityRepository,
        IEncryptionService encryptionService,
        DataItemService dataItemService,
        ExpressionsService expressionsService,
        FieldsService fieldsService,
        FormValuesService formValuesService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        IComponentFactory componentFactory)
    {
        Name = $"{ComponentNameGenerator.Create(formElement.Name)}-grid-view";
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
        CurrentContext = currentContext;
        DataItemService = dataItemService;
        FormValuesService = formValuesService;
    }

    #endregion

    protected override async Task<ComponentResult> BuildResultAsync()
    {
        if (ComponentContext is ComponentContext.GridViewReload)
        {
            return new ContentComponentResult(await GetTableHtmlBuilder());
        }
        
        if (ComponentContext is ComponentContext.DataExportation)
        {
            return await GetExportationResult();
        }

        if (ComponentContext is ComponentContext.GridViewRow)
        {
            int rowIndex = int.Parse(CurrentContext.Request.QueryString["gridViewRowIndex"]);

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
        
        if (ComponentContext is ComponentContext.GridViewFilterSearchBox)
        {
            var fieldName = CurrentContext.Request.QueryString["fieldName"];
            var field = FormElement.Fields[fieldName];
            var formStateData = new FormStateData(await GetCurrentFilterAsync(), UserValues, PageState.Filter);
            var jjSearchBox = ComponentFactory.Controls.Create(FormElement,field, formStateData, Name) as JJSearchBox;
            jjSearchBox!.Name = fieldName;
            return await jjSearchBox.GetItemsResult();
        }
        
        return new RenderedComponentResult(await GetHtmlBuilderAsync());
    }

    internal async Task<HtmlBuilder> GetHtmlBuilderAsync()
    {
        var html = new HtmlBuilder(HtmlTag.Div);
        await html.AppendAsync(HtmlTag.Div, async div =>
        {
            div.WithAttribute("id", $"grid-view-{Name}");

            div.AppendIf(ShowTitle, GetTitle(await GetDefaultValuesAsync()).GetHtmlBuilder);

            if (FilterAction.IsVisible)
            {
                div.Append(await Filter.GetFilterHtml());
            }

            if (ShowToolbar)
            {
                div.Append(await GetToolbarHtmlBuilder());
            }

            div.Append(await GetTableHtmlBuilder());
        });


        return html;
    }

    public async Task<IDictionary<string, object?>> GetCurrentFilterAsync()
    {
        return await Filter.GetCurrentFilter();
    }


    public void SetCurrentFilter(string key, object value)
    {
        Filter.SetCurrentFilter(key, value);
    }

    public async Task<string> GetTableHtmlAsync() => (await GetTableHtmlBuilder()).ToString();

    private async Task<HtmlBuilder> GetTableHtmlBuilder()
    {
        AssertProperties();

        string? currentAction = CurrentContext.Request[$"grid-view-action-map-{Name}"];

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

        html.WithAttribute("id", $"grid-view-table-{Name}");

        if (SortAction.IsVisible)
        {
            html.Append(await GetSortingConfigAsync());
        }
        
        await html.AppendIfAsync(SortAction.IsVisible, GetSortingConfigAsync);

        html.AppendText(GetScriptHtml());
        html.AppendRange(GetHiddenInputs(currentAction));

        html.Append(await Table.GetHtmlBuilder());

        if (DataSource?.Count == 0 && !string.IsNullOrEmpty(EmptyDataText))
        {
            html.Append(await GetNoRecordsAlert());
        }

        if (FormElement.Options.Grid.ShowPagging)
        {
            var gridPagination = new GridPagination(this);

            html.Append(gridPagination.GetHtmlBuilder());
        }
        
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
        yield return new HtmlBuilder().AppendHiddenInput($"grid-view-action-map-{Name}", currentAction ?? string.Empty);
        yield return new HtmlBuilder().AppendHiddenInput($"grid-view-row-{Name}", string.Empty);

        if (EnableMultiSelect)
        {
            yield return new HtmlBuilder().AppendHiddenInput($"grid-view-selected-rows-{Name}", SelectedRowsId ?? string.Empty);
        }
    }

    internal async Task<string> GetTableRowHtmlAsync(int rowIndex)
    {
        var row = DataSource?[rowIndex];

        return await Table.Body
            .GetTdHtmlList(row, rowIndex)
            .AggregateAsync(string.Empty, (current, td) => current + td);
    }
    

    internal JJTitle GetTitle(IDictionary<string, object?> values)
    {
        var title = FormElement.Title;
        var subTitle = FormElement.SubTitle;
        
        foreach (var field in FormElement.Fields)
        {
            values.TryGetValue(field.Name, out var fieldValue);
            title = title?.Replace($"{{{field.Name}}}", fieldValue?.ToString());
            subTitle = subTitle?.Replace($"{{{field.Name}}}", fieldValue?.ToString());
        }

        var titleComponent = ComponentFactory.Html.Title.Create(title, subTitle);
        titleComponent.Size = TitleSize;

        return titleComponent;
    }

    internal async Task<HtmlBuilder> GetToolbarHtmlBuilder() => await new GridToolbar(this).GetHtmlBuilderAsync();

    public async Task<HtmlBuilder> GetFilterHtmlAsync() => await Filter.GetFilterHtml();

    public async Task<HtmlBuilder> GetToolbarHtmlAsync() => await GetToolbarHtmlBuilder();

    private async Task<HtmlBuilder> GetSortingConfigAsync() => await new GridSortingConfig(this).GetHtmlBuilderAsync();

    private bool CheckForSqlCommand()
    {
        var action = CurrentActionMap?.GetAction(FormElement);
        return action is SqlCommandAction;
    }

    private async Task<JJMessageBox?> ExecuteSqlCommand()
    {
        var action = CurrentActionMap!.GetAction(FormElement);
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

        if (EnableEditMode)
        {
            var listFieldsPost = FormElement.Fields.ToList().FindAll(x => x.AutoPostBack);
            string functionname = $"do_rowreload_{Name}";
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
                script.AppendLine("\t\t\t\tlistenAllEvents(\"#row\" + nRow + \" \"); ");
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
                        script.Append((string?)f.Name);
                        script.AppendLine("\").change(function () {");
                        script.AppendLine("\t\tvar obj = $(this);");
                        script.AppendLine("\t\tsetTimeout(function() {");
                        script.AppendLine("\t\t\tvar nRowId = obj.attr(\"gridViewRowIndex\");");
                        script.AppendLine("\t\t\tvar objid = obj.attr(\"id\");");
                        script.Append("\t\t\t");
                        script.Append(functionname);
                        script.Append("(nRowId, \"");
                        script.Append((string?)f.Name);
                        script.AppendLine("\", objid);");
                        script.AppendLine("\t\t\t},200);");
                        script.AppendLine("\t\t});");
                        script.AppendLine("");
                    }
                    else
                    {
                        script.Append("\t\t$(prefixSelector + \"");
                        script.Append(".");
                        script.Append((string?)f.Name);
                        script.AppendLine("\").change(function () {");
                        script.AppendLine("\t\t\tvar obj = $(this);");
                        script.AppendLine("\t\t\tvar nRowId = obj.attr(\"gridViewRowIndex\");");
                        script.AppendLine("\t\t\tvar objid = obj.attr(\"id\");");
                        script.Append("\t\t\t");
                        script.Append(functionname);
                        script.Append("(nRowId, \"");
                        script.Append((string?)f.Name);
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

    internal async Task<FormStateData> GetFormStateDataAsync()
    {
        if (_formStateData == null)
        {
            var defaultValues = await FieldsService.GetDefaultValuesAsync(FormElement, null, PageState.List);
            var userValues = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            DataHelper.CopyIntoDictionary(userValues, UserValues, false);
            DataHelper.CopyIntoDictionary(userValues, defaultValues, true);

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
            Title = "Configure View"
        };

        var btnOk = ComponentFactory.Html.LinkButton.Create();
        btnOk.Text = "Ok";
        btnOk.IconClass = "fa fa-check";
        btnOk.ShowAsButton = true;
        btnOk.OnClientClick = Scripts.GetConfigUIScript(ConfigAction, RelationValues);
        modal.Buttons.Add(btnOk);

        var btnCancel = ComponentFactory.Html.LinkButton.Create();
        btnCancel.Text = "Cancel";
        btnCancel.IconClass = "fa fa-times";
        btnCancel.ShowAsButton = true;
        btnCancel.OnClientClick = Scripts.GetCloseConfigUIScript();
        modal.Buttons.Add(btnCancel);

        var form = new GridFormSettings(CurrentContext, StringLocalizer);
        modal.HtmlBuilderContent = form.GetHtmlElement(IsPaggingEnabled(), CurrentSettings);

        return modal.GetHtmlBuilder();
    }

    private async Task<HtmlBuilder> GetExportHtml()
    {
        var action = ExportAction;
        var formData = await GetFormStateDataAsync();
        bool isVisible = ExpressionsService.GetBoolValue(action.VisibleExpression, formData);
        if (!isVisible)
            return new HtmlBuilder(string.Empty);

        var modal = new JJModalDialog
        {
            Name = $"data-exportation-modal-{Name}",
            Title = "Export"
        };

        return modal.GetHtmlBuilder();
    }

    private async Task<HtmlBuilder> GetLegendHtml()
    {
        var action = LegendAction;
        var formData = await GetFormStateDataAsync();
        bool isVisible = ExpressionsService.GetBoolValue(action.VisibleExpression, formData);

        if (!isVisible)
            return new HtmlBuilder(string.Empty);

        var legend = new GridLegendView(ComponentFactory.Controls.ComboBox, StringLocalizer)
        {
            Name = Name,
            ShowAsModal = true,
            FormElement = FormElement
        };
        
        return await legend.GetHtmlBuilderAsync();
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
                            var validationSummary = ComponentFactory.Html.ValidationSummary.Create(ExceptionManager.GetMessage(ex));
                            validationSummary.MessageTitle = StringLocalizer["Error"];
                            return new ContentComponentResult(validationSummary.GetHtmlBuilder());
                        }
                    }

                    var html = new DataExportationLog(DataExportation).GetHtmlProcess();
                    return new ContentComponentResult(html);
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
        DataTable dataTable;
        if (IsUserSetDataSource && DataSource != null)
        {

            var dataView = new DataView(EnumerableHelper.ConvertToDataTable(DataSource.DeepCopy()));
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
            return await EntityRepository.GetDictionaryListResultAsync(FormElement, parameters);
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
    public IDictionary<string, object> ValidateGridFields(List<IDictionary<string, object?>> values)
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
                bool enabled = ExpressionsService.GetBoolValue(field.EnableExpression, formData);
                bool visible = ExpressionsService.GetBoolValue(field.VisibleExpression, formData);
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

    public ActionContext GetActionContext(BasicAction basicAction, FormStateData formStateData)
    {
        return new ActionContext
        {
            Action = basicAction,
            FormElement = FormElement,
            FormStateData = formStateData,
            ParentComponentName = Name,
            IsModal = ComponentContext is ComponentContext.Modal
        };
    }
}