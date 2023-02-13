using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Exports.Configuration;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http;
using Newtonsoft.Json;

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
public class JJGridView : JJBaseView
{
    #region "Events"

    public event EventHandler<GridCellEventArgs> OnRenderCell;

    /// <summary>
    /// Event fired when rendering the checkbox used to select the Grid row.
    /// <para/>Fired only when EnableMultSelect property is enabled.
    /// </summary>
    public event EventHandler<GridSelectedCellEventArgs> OnRenderSelectedCell;

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
    public event EventHandler<GridDataLoadEventArgs> OnDataLoad;

    public event EventHandler<ActionEventArgs> OnRenderAction;

    #endregion

    #region "Properties"

    private string _currentOrder;
    private string _selectedRowsId;
    private int _currentPage;
    private GridSettings _currentSettings;
    private ExportOptions _currentExportConfig;
    private GridFilter _filter;
    private GridTable _table;
    private DataTable _dataSource;
    private ActionManager _actionManager;
    private FieldManager _fieldManager;
    private FormManager _formManager;
    private FormValues _formValues;
    private List<FormElementField> _pkFields;
    private List<FormElementField> _visibleFields;
    private Hashtable _defaultValues;
    private List<BasicAction> _toolBarActions;
    private List<BasicAction> _gridActions;
    private ActionMap _currentActionMap;
    private JJDataImp _dataImp;
    private JJDataExp _dataExp;
    private IEntityRepository _entityRepository;
    
    internal IEntityRepository EntityRepository
    {
        get => _entityRepository ??= JJService.EntityRepository;
        set => _entityRepository = value;
    }

    internal FormManager FormManager
    {
        get
        {
            if (_formManager == null)
            {
                var expManager = new ExpressionManager(UserValues, EntityRepository);
                _formManager = new FormManager(FormElement, expManager);
            }
            return _formManager;
        }
    }

    internal JJDataImp DataImp
    {
        get
        {
            if (_dataImp != null) 
                return _dataImp;

            _dataImp = new JJDataImp(FormElement)
            {
                EntityRepository = EntityRepository,
                UserValues = UserValues,
                ProcessOptions = ImportAction.ProcessOptions,
                Name = Name + "_dataimp"
            };

            return _dataImp;
        }
    }

    public JJDataExp DataExp
    {
        get
        {
            if (_dataExp != null) return _dataExp;
            _dataExp = new JJDataExp(FormElement)
            {
                Name = Name,
                ExportOptions = CurrentExportConfig,
                ShowBorder = CurrentSettings.ShowBorder,
                ShowRowStriped = CurrentSettings.ShowRowStriped,
                EntityRepository = EntityRepository,
                UserValues = UserValues,
                ProcessOptions = ExportAction.ProcessOptions,
                OnRenderCell = OnRenderCell
            };

            return _dataExp;
        }
    }

    private List<FormElementField> PrimaryKeyFields
    {
        get
        {
            if (_pkFields != null) return _pkFields;

            if (FormElement == null)
                throw new ArgumentNullException(nameof(FormElement));

            _pkFields = FormElement.Fields.ToList().FindAll(x => x.IsPk);

            return _pkFields;
        }
    }

    internal List<FormElementField> VisibleFields
    {
        get
        {
            if (_visibleFields != null) return _visibleFields;

            if (FormElement == null)
                throw new ArgumentNullException(nameof(FormElement));

            _visibleFields = new List<FormElementField>();
            var defaultValues = DefaultValues;
            foreach (var f in FormElement.Fields)
            {
                if (FieldManager.IsVisible(f, PageState.List, defaultValues))
                    _visibleFields.Add(f);
            }

            return _visibleFields;
        }
    }

    internal ActionManager ActionManager
    {
        get
        {
            if (_actionManager == null)
                _actionManager = new ActionManager(FormElement, FieldManager.ExpressionManager, Name);

            return _actionManager;
        }
    }

    internal FieldManager FieldManager
    {
        get
        {
            if (_fieldManager == null)
            {
                var exp = new ExpressionManager(UserValues, EntityRepository);
                _fieldManager = new FieldManager(Name, FormElement, exp);
            }
            return _fieldManager;
        }
    }

    internal FormValues FormValues => _formValues ??= new FormValues(FieldManager);

    /// <summary>
    /// <see cref="FormElement"/>
    /// </summary>
    public FormElement FormElement { get; set; }


    /// Datasource is the property responsible for controlling the data source.
    /// The component uses the following rule to retrieve grid data:
    /// <para/>1) Use the DataSource property;
    /// <para/>2) If the DataSource property is null, try to execute the OnDataLoad action;
    /// <para/>3) If the OnDataLoad action is not implemented, try to retrieve
    /// Using the stored procedure informed in the FormElement;
    public DataTable DataSource
    {
        get => _dataSource;
        set
        {
            _dataSource = value;
            if (value == null) return;
            IsUserSetDataSource = true;
            TotalRecords = value.Rows.Count;
        }
    }

    private bool IsUserSetDataSource { get; set; }

    public int TotalRecords { get; set; }

    public bool ShowTitle { get; set; }

    public bool EnableFilter { get; set; }

    public bool ShowToolbar { get; set; }

    public Hashtable CurrentFilter => Filter.GetCurrentFilter();

    /// <summary>
    /// Retrieve the order of the table,
    /// by default uses the first field of the primary key
    /// </summary>
    /// <returns>Current table order</returns>
    /// <remarks>
    /// For more than one field use comma ex:
    /// "Field1 ASC, Field2 DESC, Field3 ASC"
    /// </remarks>
    public string CurrentOrder
    {
        get
        {
            if (_currentOrder != null) return _currentOrder;
            if (!CurrentContext.IsPostBack)
            {
                if (MaintainValuesOnLoad)
                {
                    object tableOrder = CurrentContext.Session[$"jjcurrentorder_{Name}"];
                    if (tableOrder != null)
                    {
                        _currentOrder = tableOrder.ToString();
                    }
                }
            }
            else
            {
                _currentOrder = CurrentContext.Request["current_tableorder_" + Name];
                if (_currentOrder == null)
                {
                    object tableOrder = CurrentContext.Session[$"jjcurrentorder_{Name}"];
                    if (tableOrder != null)
                    {
                        _currentOrder = tableOrder.ToString();
                    }
                }
            }

            CurrentOrder = _currentOrder;
            return _currentOrder;
        }
        set
        {
            CurrentContext.Session[$"jjcurrentorder_{Name}"] = value;
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

            if (CurrentContext.IsPostBack)
            {
                int currentPage = 1;
                string tablePageId = "current_tablepage_" + Name;
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

            // Removing it from here when calling the GetMetadataInfoList() method outside the class does not respect pagination
            var actionMap = CurrentActionMap;
            var action = GetCurrentAction(actionMap);
            if (action is ConfigAction)
            {
                CurrentSettings = GridSettings.LoadFromForm(CurrentContext);
                return _currentSettings;
            }

            if (MaintainValuesOnLoad && FormElement != null)
            {
                CurrentSettings = JJSession.GetSessionValue<GridSettings>($"jjcurrentui_{FormElement.Name}");
            }

            if (_currentSettings == null)
                CurrentSettings = GridSettings.LoadFromForm(CurrentContext);

            if (_currentSettings == null)
                CurrentSettings = new GridSettings();

            return _currentSettings;
        }
        set
        {
            if (MaintainValuesOnLoad && FormElement != null)
                JJSession.SetSessionValue($"jjcurrentui_{FormElement.Name}", value);

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
            
            _table = new GridTable(this)
            {
                Body =
                {
                    OnRenderAction = OnRenderAction,
                    OnRenderSelectedCell = OnRenderSelectedCell,
                    OnRenderCell = OnRenderCell
                }
            };

            return _table;
        }
    }

    public ExportOptions CurrentExportConfig
    {
        get
        {
            if (_currentExportConfig != null) return _currentExportConfig;

            _currentExportConfig = new ExportOptions();
            if (CurrentContext.IsPostBack)
            {
                _currentExportConfig = ExportOptions.LoadFromForm(CurrentContext, Name);
            }

            return _currentExportConfig;
        }
        set => _currentExportConfig = value;
    }

    public bool EnableAjax { get; set; }

    public bool EnableEditMode { get; set; }

    /// <remarks>
    /// Even when set to false, the grid respects the CurrentOrder property
    /// </remarks>
    public bool EnableSorting { get; set; }

    public bool EnableMultSelect { get; set; }

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
    public Hashtable Errors { get; set; }

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
    public Hashtable RelationValues { get; set; }

    public HeadingSize TitleSize { get; set; }

    internal Hashtable DefaultValues
    {
        get
        {
            if (_defaultValues == null)
                _defaultValues = FormManager.GetDefaultValues(null, PageState.List);

            return _defaultValues;
        }
    }

    public LegendAction LegendAction
    {
        get { return (LegendAction)ToolBarActions.Find(x => x is LegendAction); }
    }

    public RefreshAction RefreshAction
    {
        get { return (RefreshAction)ToolBarActions.Find(x => x is RefreshAction); }
    }

    public FilterAction FilterAction => (FilterAction)ToolBarActions.Find(x => x is FilterAction);

    public ImportAction ImportAction => (ImportAction)ToolBarActions.Find(x => x is ImportAction);

    public ExportAction ExportAction => (ExportAction)ToolBarActions.Find(x => x is ExportAction);

    public ConfigAction ConfigAction => (ConfigAction)ToolBarActions.Find(x => x is ConfigAction);

    public SortAction SortAction => (SortAction)ToolBarActions.Find(x => x is SortAction);

    public List<BasicAction> ToolBarActions
    {
        get =>
            _toolBarActions ??= new List<BasicAction>
            {
                new LegendAction(),
                new RefreshAction(),
                new FilterAction(),
                new ImportAction(),
                new ExportAction(),
                new ConfigAction(),
                new SortAction()
            };
        internal set => _toolBarActions = value;
    }

    public List<BasicAction> GridActions
    {
        get => _gridActions ??= new List<BasicAction>();

        internal set => _gridActions = value;
    }

    private ActionMap CurrentActionMap
    {
        get
        {
            if (_currentActionMap != null) return _currentActionMap;
            var criptMap = CurrentContext.Request["current_tableaction_" + Name];
            if (string.IsNullOrEmpty(criptMap))
                return null;

            var jsonMap = Cript.Descript64(criptMap);
            _currentActionMap = JsonConvert.DeserializeObject<ActionMap>(jsonMap);
            return _currentActionMap;
        }
    }

    private string SelectedRowsId
    {
        get => _selectedRowsId ??= CurrentContext.Request.GetUnvalidated("selectedrows_" + Name)?.ToString();
        set => _selectedRowsId = value ?? "";
    }

    #endregion

    #region "Constructors"

    public JJGridView()
    {
        Name = "jjview";
        ShowTitle = true;
        EnableFilter = true;
        EnableAjax = true;
        EnableSorting = true;
        ShowHeaderWhenEmpty = true;
        ShowPagging = true;
        ShowToolbar = true;
        EmptyDataText = "No records found";
        AutoReloadFormFields = true;
        RelationValues = new Hashtable();
        TitleSize = HeadingSize.H1;
    }

    public JJGridView(DataTable table) : this()
    {
        FormElement = new FormElement(table);
        DataSource = table;
    }

    public JJGridView(string elementName) : this()
    {
        GridViewFactory.SetGridViewParams(this, elementName);
    }

    public JJGridView(FormElement formElement) : this()
    {
        FormElement = formElement ?? throw new ArgumentNullException(nameof(formElement));
        Name = "jjview" + formElement.Name.ToLower();
    }


    #endregion

    internal override HtmlBuilder RenderHtml()
    {
        var html = new HtmlBuilder(HtmlTag.Div);
        string lookupRoute = CurrentContext.Request.QueryString("jjlookup_" + Name);

        if (!string.IsNullOrEmpty(lookupRoute))
            return GetLookupHtml(lookupRoute);

        html.AppendElementIf(ShowTitle, GetTitle().GetHtmlBuilder);
        html.AppendElementIf(FilterAction.IsVisible,Filter.GetFilterHtml);
        html.AppendElementIf(ShowToolbar, GetToolbarHtmlBuilder);

        html.AppendElement(GetTableHtmlBuilder());

        return html;
    }

    public string GetTableHtml() => GetTableHtmlBuilder().ToString();

    private HtmlBuilder GetTableHtmlBuilder()
    {
        AssertProperties();
        
        string requestType = CurrentContext.Request.QueryString("t");

        SetDataSource();
        
        if (CheckForExportation(requestType)) 
            return null;
        
        if (CheckForTableRow(requestType, Table)) 
            return null;
        
        if (CheckForSelectAllRows(requestType)) 
            return null;

        var html = new HtmlBuilder(HtmlTag.Div);
        html.WithAttribute("id", $"jjgridview_{Name}");
        html.AppendElementIf(SortAction.IsVisible, GetSortingConfig);
        
        html.AppendText(GetScriptHtml());
        html.AppendRange(GetHiddenInputs());

        html.AppendElement(Table.GetHtmlElement());

        if (DataSource.Rows.Count == 0 && !string.IsNullOrEmpty(EmptyDataText))
        {
            html.AppendElement(GetNoRecordsAlert());
        }
        
        var gridPagination = new GridPagination(this);

        html.AppendElement(gridPagination.GetHtmlElement());

        if (ShowToolbar)
        {
            html.AppendElement(GetSettingsHtml());
            
            html.AppendElement(GetExportHtml());

            html.AppendElement(GetLegendHtml());
        }
        
        html.AppendElement(HtmlTag.Div, div =>
        {
            div.WithCssClass("clearfix");
        });

        if (CheckForAjaxResponse(requestType, html)) 
            return null;

        return html;
    }

    private bool CheckForAjaxResponse(string requestType, HtmlBuilder html)
    {
        string objName = CurrentContext.Request.QueryString("objname");
        if ("ajax".Equals(requestType) && Name.Equals(objName))
        {
            CurrentContext.Response.SendResponse(html.ToString());
            return true;
        }

        return false;
    }

    private IEnumerable<HtmlBuilder> GetHiddenInputs()
    {
        var elementList = new List<HtmlBuilder>();
        
        var (error,currentAction  ) = GetAndExecuteCurrentAction();

        if (!string.IsNullOrEmpty(error))
        {
            elementList.Add(new HtmlBuilder(error));
        }

        elementList.Add(GetHiddenInput($"current_tableorder_{Name}", CurrentOrder));
        elementList.Add(GetHiddenInput($"current_tablepage_{Name}", CurrentPage.ToString()));
        elementList.Add(GetHiddenInput($"current_tableaction_{Name}", currentAction));
        elementList.Add(GetHiddenInput($"current_tablerow_{Name}", string.Empty));

        if (EnableMultSelect)
        {
            elementList.Add(GetHiddenInput($"selectedrows_{Name}", SelectedRowsId));
        }

        return elementList;
    }

    private HtmlBuilder GetHiddenInput(string name, string value)
    {
        var input = new HtmlBuilder(HtmlTag.Input);
        input.WithAttribute("hidden", "hidden");
        input.WithNameAndId(name);
        input.WithValue(value);
        return input;
    }

    private bool CheckForSelectAllRows(string requestType)
    {
        if ("selectall".Equals(requestType))
        {
            string values = GetAllSelectedRows();
            CurrentContext.Response.SendResponse(values);
            return true;
        }

        return false;
    }

    private bool CheckForTableRow(string requestType, GridTable table)
    {
        if ("tablerow".Equals(requestType))
        {
            string gridName = CurrentContext.Request.QueryString("gridName");
            if (Name.Equals(gridName))
            {
                int rowIndex = int.Parse(CurrentContext.Request.QueryString("nRow"));
                var row = DataSource.Rows[rowIndex];

                string responseHtml = string.Empty;
                
                foreach (var td in table.Body.GetTdHtmlList(row, rowIndex))
                {
                    responseHtml += td.ToString();
                }
                
                CurrentContext.Response.SendResponse(responseHtml);
            }

            return true;
        }

        return false;
    }

    private bool CheckForExportation(string requestType)
    {
        if ("tableexp".Equals(requestType))
        {
            string gridName = CurrentContext.Request.QueryString("gridName");
            if (Name.Equals(gridName))
                DoExport();

            return true;
        }

        return false;
    }

    private HtmlBuilder GetLookupHtml(string lookupRoute)
    {
        string fieldName = lookupRoute.Substring(GridFilter.FilterFieldPrefix.Length);
        var field = FormElement.Fields.ToList().Find(x => x.Name.Equals(fieldName));

        if (field == null) return null;

        var lookup = (JJLookup)FieldManager.GetField(field, PageState.Filter, null);
        lookup.Name = lookupRoute;
        lookup.DataItem.ElementMap.EnableElementActions = false;
        return lookup.GetHtmlBuilder();
    }

    internal JJTitle GetTitle()
    {
        var title = new JJTitle(FormElement.Title, FormElement.SubTitle)
        {
            Size = TitleSize
        };
        return title;
    }

    internal HtmlBuilder GetToolbarHtmlBuilder() => new GridToolbar(this).GetHtmlElement();

    public string GetFilterHtml() => Filter.GetFilterHtml().ToString();

    public string GetToolbarHtml() => GetToolbarHtmlBuilder().ToString();

    private HtmlBuilder GetSortingConfig() => new GridSortingConfig(this).GetHtmlElement();

    public string GetTitleHtml() => GetTitle().GetHtml();

    private (string currentAction,string error) GetAndExecuteCurrentAction()
    {
        string error = string.Empty;
        string currentAction = CurrentContext.Request["current_tableaction_" + Name];
        var actionMap = CurrentActionMap;
        var action = GetCurrentAction(actionMap);

        switch (action)
        {
            case SqlCommandAction cmdAction:
                error = _actionManager.ExecuteSqlCommand(this, actionMap, cmdAction);

                currentAction = string.Empty;
                break;
            case PythonScriptAction pyAction:
                error = _actionManager.ExecutePythonScriptAction(this, actionMap, pyAction);

                currentAction = string.Empty;
                break;
        }

        return (error, currentAction);
    }

    private void AssertProperties()
    {
        if (FormElement == null)
            throw new ArgumentNullException(nameof(FormElement));

        if (EnableMultSelect && PrimaryKeyFields.Count == 0)
            throw new JJMasterDataException(
                Translate.Key(
                    "It is not allowed to enable multiple selection without defining a primary key in the data dictionary"));
    }

    private HtmlBuilder GetNoRecordsAlert()
    {
        var alert = new JJAlert
        {
            ShowCloseButton = true,
            Color = PanelColor.Default,
            Title = Translate.Key("No records found."),
            Icon = IconType.InfoCircle
        };

        if (!Filter.HasFilter()) return alert.GetHtmlBuilder();

        alert.Messages.Add("There are filters applied for this query.");
        alert.Icon = IconType.Filter;

        return alert.GetHtmlBuilder();
    }

    private string GetScriptHtml()
    {
        var script = new StringBuilder();

        //Scripts
        script.AppendLine("\t<script type=\"text/javascript\"> ");

        if (EnableMultSelect)
        {
            script.AppendLine("\t$(document).ready(function () {");
            script.AppendLine("\t\t$(\".jjselect input\").change(function() {");
            script.AppendLine("\t\t\tjjview.doSelectItem('" + Name + "', $(this)); ");
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
                script.AppendLine("\t\t\tsurl += '&t=tablerow&nRow=' + nRow;");
                script.AppendLine("\t\telse");
                script.AppendLine("\t\t\tsurl += '?t=tablerow&nRow=' + nRow;");
                script.AppendLine("");
                script.AppendLine("\t\tsurl += '&objname=' + objname;");
                script.AppendLine($"\t\tsurl += '&gridName={Name}';");
                script.AppendLine("\t\t$.ajax({ ");
                script.AppendLine("\t\tasync: false,");
                script.AppendLine("\t\t\ttype: frm.attr('method'), ");
                script.AppendLine("\t\t\turl: surl, ");
                script.AppendLine("\t\t\tdata: frm.serialize(), ");
                script.AppendLine("\t\t\tsuccess: function (data) { ");
                script.AppendLine($"\t\t\t\t$(\"#jjgridview_{Name} #row\" + nRow).html(data); ");
                script.AppendLine($"\t\t\t\tdo_change_{Name}(nRow);");
                script.AppendLine("\t\t\t\tjjloadform(null, \"#row\" + nRow + \" \"); ");
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
                    //WorkArroud para gatilhar o select do search
                    if (f.Component == FormComponent.Search)
                    {
                        script.Append("\t\t$(prefixSelector + \"");
                        script.Append(".");
                        script.Append(f.Name);
                        script.AppendLine("\").change(function () {");
                        script.AppendLine("\t\tvar obj = $(this);");
                        script.AppendLine("\t\tsetTimeout(function() {");
                        script.AppendLine("\t\t\tvar nRowId = obj.attr(\"nRowId\");");
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
                        script.AppendLine("\t\t\tvar nRowId = obj.attr(\"nRowId\");");
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

    private HtmlBuilder GetSettingsHtml()
    {
        var action = ConfigAction;
        bool isVisible =
            ActionManager.Expression.GetBoolValue(action.VisibleExpression, action.Name, PageState.List,
                RelationValues);
        if (!isVisible)
            return new HtmlBuilder(string.Empty);

        var modal = new JJModalDialog
        {
            Name = "config_modal_" + Name,
            Title = "Configure View"
        };

        var btnOk = new JJLinkButton
        {
            Text = "Ok",
            IconClass = "fa fa-check",
            ShowAsButton = true,
            OnClientClick = ActionManager.GetConfigUIScript(ConfigAction, RelationValues)
        };
        modal.Buttons.Add(btnOk);

        var btnCancel = new JJLinkButton
        {
            Text = "Cancel",
            IconClass = "fa fa-times",
            ShowAsButton = true,
            OnClientClick = $"jjview.doConfigCancel('{Name}');"
        };
        modal.Buttons.Add(btnCancel);
        modal.HtmlBuilderContent = CurrentSettings.GetHtmlElement(IsPaggingEnabled());

        return modal.GetHtmlBuilder();
    }

    private HtmlBuilder GetExportHtml()
    {
        var action = ExportAction;
        bool isVisible =
            ActionManager.Expression.GetBoolValue(action.VisibleExpression, action.Name, PageState.List,
                RelationValues);
        if (!isVisible)
            return new HtmlBuilder(string.Empty);

        var modal = new JJModalDialog
        {
            Name = "export_modal_" + Name,
            Title = "Export"
        };

        return modal.GetHtmlBuilder();
    }

    private HtmlBuilder GetLegendHtml()
    {
        var action = LegendAction;
        bool isVisible =
            ActionManager.Expression.GetBoolValue(action.VisibleExpression, action.Name, PageState.List,
                RelationValues);
        if (!isVisible)
            return new HtmlBuilder(string.Empty);

        var legend = new JJLegendView(FormElement)
        {
            ShowAsModal = true,
            Name = "iconlegend_modal_" + Name
        };
        return legend.GetHtmlBuilder();
    }

    internal string GetFieldName(string fieldName, IDictionary row)
    {
        string name = "";
        foreach (var fpk in PrimaryKeyFields)
        {
            if (name.Length > 0)
                name += "_";

            name += row[fpk.Name].ToString()
                ?.Replace(" ", "_")
                .Replace("'", "")
                .Replace("\"", "");
        }

        name += fieldName;

        return name;
    }

    

    public Hashtable GetSelectedRowId()
    {
        var values = new Hashtable();
        string currentRow = CurrentContext.Request["current_tablerow_" + Name];

        if (string.IsNullOrEmpty(currentRow)) return values;

        var decriptId = Cript.Descript64(currentRow);
        var parms = HttpUtility.ParseQueryString(decriptId);

        foreach (string key in parms)
        {
            values.Add(key, parms[key]);
        }

        return values;
    }

    private void DoExport()
    {
        var exp = DataExp;
        string expressionType = CurrentContext.Request.QueryString("exptype");
        switch (expressionType)
        {
            case "showoptions":
                CurrentContext.Response.SendResponse(exp.GetHtml());
                break;
            case "export":
            {
                if (IsUserSetDataSource || OnDataLoad != null)
                {
                    var tot = int.MaxValue;
                    var dt = GetDataTable(CurrentFilter, CurrentOrder, tot, 1, ref tot);
                    exp.DoExport(dt);
                }
                else
                {
                    try
                    {
                        exp.ExportFileInBackground(CurrentFilter, CurrentOrder);
                    }
                    catch (Exception ex)
                    {
                        var err = new JJValidationSummary(ExceptionManager.GetMessage(ex))
                        {
                            MessageTitle = "Error"
                        };

                        CurrentContext.Response.SendResponse(err.GetHtml());
                        return;
                    }
                }

                var html = new DataExpLog(exp.Name).GetHtmlProcess();

                CurrentContext.Response.SendResponse(html.ToString());
                break;
            }
            case "checkProcess":
            {
                var dto = exp.GetCurrentProcess();
                string json = JsonConvert.SerializeObject(dto);
                CurrentContext.Response.SendResponse(json, "text/json");
                break;
            }
            case "stopProcess":
                exp.AbortProcess();
                CurrentContext.Response.SendResponse("{}", "text/json");
                break;
        }
    }

    /// <summary>
    /// Retrieves database records.
    /// </summary>
    /// <returns>
    /// Returns a DataTable with the found records.
    /// If no record is found, returns null.
    /// The component uses the following rule to retrieve grid data:
    /// <para/>1) Use the DataSource property;
    /// <para/>2) If the DataSource property is null, try to execute the OnDataLoad action;
    /// <para/>3) If the OnDataLoad action is not implemented, try to retrieve
    /// Using the stored procedure informed in the <see cref="FormElement"/>;
    /// </returns>
    public DataTable GetDataTable()
    {
        SetDataSource();
        
        return DataSource;
    }

    private void SetDataSource(int totalOfRecords = 0)
    {
        if (_dataSource == null || IsUserSetDataSource)
        {
            _dataSource = GetDataTable(CurrentFilter, CurrentOrder, CurrentSettings.TotalPerPage, CurrentPage, ref totalOfRecords);
            TotalRecords = totalOfRecords;

            //Se estiver paginando e não retornar registros volta para pagina inicial
            if (CurrentPage > 1 && _dataSource.Rows.Count == 0)
            {
                CurrentPage = 1;
                _dataSource = GetDataTable(CurrentFilter, CurrentOrder, CurrentSettings.TotalPerPage, CurrentPage, ref totalOfRecords);
                TotalRecords = totalOfRecords;
            }
        }
    }

    private DataTable GetDataTable(Hashtable filters, string orderBy, int recordsPerPage, int currentPage,
        ref int total)
    {
        DataTable dt;
        if (IsUserSetDataSource)
        {
            var tempdt = DataSource;
            if (tempdt != null)
                total = tempdt.Rows.Count;

            var dv = new DataView(tempdt);
            dv.Sort = orderBy;

            dt = dv.ToTable();
            dv.Dispose();
        }
        else if (OnDataLoad != null)
        {
            var args = new GridDataLoadEventArgs
            {
                Filters = filters,
                OrderBy = orderBy,
                RegporPag = recordsPerPage,
                CurrentPage = currentPage,
                Tot = total
            };
            OnDataLoad.Invoke(this, args);
            total = args.Tot;
            dt = args.DataSource;
        }
        else
        {
            dt = EntityRepository.GetDataTable(FormElement, filters, orderBy, recordsPerPage, currentPage, ref total);
        }

        return dt;
    }

    /// <remarks>
    /// Used with the <see cref="EnableEditMode"/> property
    /// </remarks>
    public List<Hashtable> GetGridValues(int recordPerPage, int currentPage)
    {
        int tot = 1;
        DataTable dt = GetDataTable(CurrentFilter, CurrentOrder, recordPerPage, currentPage, ref tot);

        return GetGridValues(dt);
    }

    /// <remarks>
    /// Used with the EnableEditMode property
    /// </remarks>
    public List<Hashtable> GetGridValues(DataTable dt = null)
    {
        if (dt == null)
        {
            dt = GetDataTable();
            if (dt == null)
                return null;
        }

        var listValues = new List<Hashtable>();
        foreach (DataRow row in dt.Rows)
        {
            var values = new Hashtable();
            for (int i = 0; i < row.Table.Columns.Count; i++)
            {
                values.Add(row.Table.Columns[i].ColumnName, row[i]);
            }

            string prefixValue = GetFieldName("", values);
            var newValues = FormValues.GetFormValues(PageState.List, values, AutoReloadFormFields, prefixValue);
            listValues.Add(newValues);
        }

        return listValues;
    }

    /// <remarks>
    /// Used with the EnableMultSelect property
    /// </remarks>
    public List<Hashtable> GetSelectedGridValues()
    {
        var listValues = new List<Hashtable>();

        if (!EnableMultSelect)
            return listValues;

        string inputHidden = SelectedRowsId;
        if (string.IsNullOrEmpty(inputHidden))
            return listValues;

        string[] pkList = inputHidden.Split(',');

        var pkFields = PrimaryKeyFields;
        foreach (string pk in pkList)
        {
            var values = new Hashtable();
            string descriptval = Cript.Descript64(pk);
            string[] ids = descriptval.Split(';');
            for (int i = 0; i < pkFields.Count; i++)
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

    private string GetAllSelectedRows()
    {
        int tot = 0;
        var dt = GetDataTable(CurrentFilter, CurrentOrder, 999999, 1, ref tot);
        var selectedKeys = new StringBuilder();
        var hasVal = false;
        foreach (DataRow row in dt.Rows)
        {
            if (!hasVal)
                hasVal = true;
            else
                selectedKeys.Append(",");

            string values = DataHelper.ParsePkValues(FormElement, row, ';');
            selectedKeys.Append(Cript.Cript64(values));
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
    public Hashtable ValidateGridFields(List<Hashtable> values)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        var errors = new Hashtable();
        int line = 0;
        foreach (var row in values)
        {
            line++;
            foreach (var field in FormElement.Fields)
            {
                bool enabled = FieldManager.IsEnabled(field, PageState.List, row);
                bool visible = FieldManager.IsVisible(field, PageState.List, row);
                if (enabled && visible && field.DataBehavior is not FieldBehavior.ViewOnly)
                {
                    string val = string.Empty;
                    if (row[field.Name] != null)
                        val = row[field.Name].ToString();

                    string objname = GetFieldName(field.Name, row);
                    string err = FieldValidator.ValidateField(field, objname, val);
                    if (!string.IsNullOrEmpty(err))
                    {
                        string errMsg = $"{Translate.Key("Line")} {line}: {err}";
                        errors.Add(objname, errMsg);
                    }
                }
            }
        }

        return errors;
    }

    internal bool IsPaggingEnabled()
    {
        return !(!ShowPagging || CurrentPage == 0 || CurrentSettings.TotalPerPage == 0 || TotalRecords == 0);
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

        var action = ToolBarActions.Find(x => x.Name.Equals(actionName));

        switch (action)
        {
            case null:
                throw new ArgumentException(Translate.Key("Action {0} not found", actionName));
            case ScriptAction or UrlRedirectAction or InternalAction:
                ToolBarActions.Remove(action);
                break;
            default:
                throw new ArgumentException(Translate.Key("This action can not be removed"));
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

        var action = GridActions.Find(x => x.Name.Equals(actionName));
        switch (action)
        {
            case null:
                throw new ArgumentException(Translate.Key("Action {0} not found", actionName));
            case ScriptAction or UrlRedirectAction or InternalAction:
                GridActions.Remove(action);
                break;
            default:
                throw new ArgumentException(Translate.Key("This action can not be removed"));
        }
    }

    public BasicAction GetToolBarAction(string actionName)
    {
        return ToolBarActions.Find(x => x.Name.Equals(actionName));
    }

    public BasicAction GetGridAction(string actionName)
    {
        return GridActions.Find(x => x.Name.Equals(actionName));
    }

    /// <summary>
    /// Add or change a value in the CurrentFilter.<br></br>
    /// If it exists, change it, otherwise it includes it.
    /// </summary>
    public void SetCurrentFilter(string field, object value)
    {
        if (CurrentFilter.ContainsKey(field))
            CurrentFilter[field] = value;
        else
            CurrentFilter.Add(field, value);
    }

    /// <summary>
    /// Verify if a action is valid, else, throws an exception.
    /// </summary>
    private void ValidateAction(BasicAction action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        if (string.IsNullOrEmpty(action.Name))
            throw new ArgumentException(Translate.Key("Property name action is not valid"));
    }

    public void SetGridOptions(GridUI options)
    {
        GridViewFactory.SetGridOptions(this, options);
    }

    internal BasicAction GetCurrentAction(ActionMap actionMap)
    {
        if (actionMap == null)
            return null;

        return actionMap.ContextAction switch
        {
            ActionSource.Form => null, //TODO: formAction
            ActionSource.Grid => GridActions.Find(x => x.Name.Equals(actionMap.ActionName)),
            ActionSource.Toolbar => ToolBarActions.Find(x => x.Name.Equals(actionMap.ActionName)),
            ActionSource.Field => FormElement.Fields[actionMap.FieldName].Actions.Get(actionMap.ActionName),
            _ => null,
        };
    }

    public bool IsExportPost()
    {
        return "export".Equals(CurrentContext.Request["exptype"]) && Name.Equals(CurrentContext.Request["gridName"]);
    }
}