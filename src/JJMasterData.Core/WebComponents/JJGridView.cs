using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Exports.Configuration;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Html;
using JJMasterData.Core.Http;
using Newtonsoft.Json;

namespace JJMasterData.Core.WebComponents;

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
    private GridUI _currentUI;
    private ExportOptions _currentExportConfig;
    private GridFilter _filter;
    private DataTable _dataSource;
    private ActionManager _actionManager;
    private FieldManager _fieldManager;
    private List<FormElementField> _pkFields;
    private List<FormElementField> _visibleFields;
    private Hashtable _defaultValues;
    private List<BasicAction> _toolBarActions;
    private List<BasicAction> _gridActions;
    private ActionMap _currentActionMap;
    private JJDataImp _dataImp;
    private JJDataExp _dataExp;
    private DataDictionaryManager _dataDictionaryManager;

    internal JJDataImp DataImp
    {
        get
        {
            if (_dataImp != null) return _dataImp;

            _dataImp = new JJDataImp(FormElement, DataAccess)
            {
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
                ShowBorder = CurrentUI.ShowBorder,
                ShowRowStriped = CurrentUI.ShowRowStriped,
                DataAccess = DataAccess,
                UserValues = UserValues
            };
            _dataExp.OnRenderCell += OnRenderCell;
            _dataExp.ProcessOptions = ExportAction.ProcessOptions;

            return _dataExp;
        }
    }

    private List<FormElementField> PrimaryKeyFields
    {
        get
        {
            if (_pkFields != null) return _pkFields;

            if (FormElement == null)
                throw new Exception("FormElement inválido");

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
                throw new Exception("FormElement inválido");

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

    internal ActionManager ActionManager => _actionManager ??= new ActionManager(this, FormElement);

    internal FieldManager FieldManager => _fieldManager ??= new FieldManager(this, FormElement);


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
            if (!IsPostBack)
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

            if (IsPostBack)
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
    /// <see cref="GridUI"/>
    /// </summary>
    public GridUI CurrentUI
    {
        get
        {
            if (_currentUI != null)
                return _currentUI;

            // Removing it from here when calling the GetDataTable() method outside the class does not respect pagination
            var actionMap = CurrentActionMap;
            var action = GetCurrentAction(actionMap);
            if (action is ConfigAction)
            {
                CurrentUI = GridUI.LoadFromForm(CurrentContext);
                return _currentUI;
            }

            if (MaintainValuesOnLoad && FormElement != null)
            {
                CurrentUI = JJSession.GetSessionValue<GridUI>($"jjcurrentui_{FormElement.Name}");
            }

            if (_currentUI == null)
                CurrentUI = GridUI.LoadFromForm(CurrentContext);

            if (_currentUI == null)
                CurrentUI = new GridUI();

            return _currentUI;
        }
        set
        {
            if (MaintainValuesOnLoad && FormElement != null)
                JJSession.SetSessionValue($"jjcurrentui_{FormElement.Name}", value);

            _currentUI = value;
        }
    }

    internal GridFilter Filter => _filter ??= new GridFilter(this);

    public ExportOptions CurrentExportConfig
    {
        get
        {
            if (_currentExportConfig != null) return _currentExportConfig;

            _currentExportConfig = new ExportOptions();
            if (IsPostBack)
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

    private DataDictionaryManager DataDictionaryManager =>
        _dataDictionaryManager ??= new DataDictionaryManager(FormElement);

    internal Hashtable DefaultValues
    {
        get
        {
            if (_defaultValues != null) return _defaultValues;

            //Default field values.
            var formManager = new FormManager(FormElement, UserValues, DataAccess);
            _defaultValues = formManager.GetDefaultValues(null, PageState.List);

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
        get => _selectedRowsId ??= CurrentContext.Request.Form("selectedrows_" + Name);
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
        WebComponentFactory.SetGridViewParams(this, elementName);
    }

    public JJGridView(FormElement formElement) : this()
    {
        FormElement = formElement ?? throw new ArgumentNullException(nameof(formElement));
    }

    public JJGridView(FormElement formElement, IDataAccess dataAccess) : this(formElement)
    {
        DataAccess = dataAccess;
    }

    #endregion

    internal override HtmlElement RenderHtmlElement()
    {
        var html = new HtmlElement(HtmlTag.Div);
        string lookupRoute = CurrentContext.Request.QueryString("jjlookup_" + Name);

        if (!string.IsNullOrEmpty(lookupRoute))
            return GetLookupHtml(lookupRoute);

        html.AppendElementIf(ShowTitle, GetTitle().GetHtmlElement);
        html.AppendElementIf(FilterAction.IsVisible, new HtmlElement(GetFilterHtml()));
        html.AppendElementIf(ShowToolbar, GetToolbarHtmlElement);

        html.AppendElement(GetTableHtmlElement());

        return html;
    }

    private HtmlElement GetTableHtmlElement()
    {
        AssertProperties();
        
        string requestType = CurrentContext.Request.QueryString("t");
        
        var table = new GridTable(this);
        table.Body.OnRenderAction += OnRenderAction;
        table.Body.OnRenderSelectedCell += OnRenderSelectedCell;
        table.Body.OnRenderCell += OnRenderCell;
        
        if (CheckForExportation(requestType)) 
            return null;
        
        if (CheckForTableRow(requestType, table)) 
            return null;
        
        if (CheckForSelectAllRows(requestType)) 
            return null;
        
        GetDataTable();
        
        var html = new HtmlElement(HtmlTag.Div);
        html.WithAttribute("id", $"jjgridview_{Name}");
        html.AppendElementIf(SortAction.IsVisible, GetSortingConfig);
        
        html.AppendText(GetHtmlScript());
        html.AppendRange(GetHiddenInputs());
        html.AppendElement(table.GetHtmlElement());

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

    private bool CheckForAjaxResponse(string requestType, HtmlElement html)
    {
        string objName = CurrentContext.Request.QueryString("objname");
        if ("ajax".Equals(requestType) && Name.Equals(objName))
        {
            CurrentContext.Response.SendResponse(html.GetElementHtml());
            return true;
        }

        return false;
    }

    private IList<HtmlElement> GetHiddenInputs()
    {
        var elementList = new List<HtmlElement>();
        
        var (currentAction, error ) = GetAndExecuteCurrentAction();

        if (!string.IsNullOrEmpty(error))
        {
            elementList.Add(new HtmlElement(error));
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

    private HtmlElement GetHiddenInput(string name, string value)
    {
        var input = new HtmlElement(HtmlTag.Input);
        input.WithAttribute("hidden", "hidden");
        input.WithNameAndId(name);
        input.WithValue(value);
        return input;
    }

    private bool CheckForSelectAllRows(string requestType)
    {
        if ("selectall".Equals(requestType))
        {
            string values = DoSelectAllRows();
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
                string responseHtml = table.Body.GetRowHtmlElement(row, rowIndex, true).GetElementHtml();

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

    private HtmlElement GetLookupHtml(string lookupRoute)
    {
        string fieldName = lookupRoute.Substring(GridFilter.FIELD_NAME_PREFIX.Length);
        var field = FormElement.Fields.ToList().Find(x => x.Name.Equals(fieldName));

        if (field == null) return null;

        var lookup = (JJLookup)FieldManager.GetField(field, PageState.Filter, null, null);
        lookup.Name = lookupRoute;
        lookup.DataItem.ElementMap.EnableElementActions = false;
        return lookup.GetHtmlElement();
    }

    internal JJTitle GetTitle()
    {
        var title = new JJTitle(FormElement.Title, FormElement.SubTitle)
        {
            Size = TitleSize
        };
        return title;
    }

    internal HtmlElement GetToolbarHtmlElement() => new GridToolbar(this).GetHtmlElement();

    public string GetFilterHtml() => Filter.GetHtmlFilter();

    public string GetToolbarHtml() => GetToolbarHtmlElement().GetElementHtml();

    private HtmlElement GetSortingConfig() => new GridSortingConfig(this).GetHtmlElement();

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
            throw new Exception(
                Translate.Key(
                    "It is not allowed to enable multiple selection without defining a primary key in the data dictionary"));
    }

    private HtmlElement GetNoRecordsAlert()
    {
        var alert = new JJAlert
        {
            ShowCloseButton = true,
            Color = PanelColor.Default,
            Title = Translate.Key("No records found."),
            Icon = IconType.InfoCircle
        };

        if (!Filter.HasFilter()) return alert.GetHtmlElement();

        alert.Messages.Add("There are filters applied for this query.");
        alert.Icon = IconType.Filter;

        return alert.GetHtmlElement();
    }

    private string GetHtmlScript()
    {
        var html = new StringBuilder();

        //Scripts
        html.AppendLine("\t<script type=\"text/javascript\"> ");

        if (EnableMultSelect)
        {
            html.AppendLine("\t$(document).ready(function () {");
            html.AppendLine("\t\t$(\".jjselect input\").change(function() {");
            html.AppendLine("\t\t\tjjview.doSelectItem('" + Name + "', $(this)); ");
            html.AppendLine("\t\t});");
            html.AppendLine("\t});");
        }

        if (EnableEditMode)
        {
            var listFieldsPost = FormElement.Fields.ToList().FindAll(x => x.AutoPostBack);
            string functionname = "do_rowreload_" + Name;
            if (listFieldsPost.Count > 0)
            {
                html.AppendLine("");
                html.Append("\tfunction ");
                html.Append(functionname);
                html.AppendLine("(nRow, objname, objid) { ");
                html.AppendLine("\t\tvar frm = $('form'); ");
                html.AppendLine("\t\tvar surl = frm.attr('action'); ");
                html.AppendLine("\t\tif (surl.includes('?'))");
                html.AppendLine("\t\t\tsurl += '&t=tablerow&nRow=' + nRow;");
                html.AppendLine("\t\telse");
                html.AppendLine("\t\t\tsurl += '?t=tablerow&nRow=' + nRow;");
                html.AppendLine("");
                html.AppendLine("\t\tsurl += '&objname=' + objname;");
                html.AppendLine($"\t\tsurl += '&gridName={Name}';");
                html.AppendLine("\t\t$.ajax({ ");
                html.AppendLine("\t\tasync: false,");
                html.AppendLine("\t\t\ttype: frm.attr('method'), ");
                html.AppendLine("\t\t\turl: surl, ");
                html.AppendLine("\t\t\tdata: frm.serialize(), ");
                html.AppendLine("\t\t\tsuccess: function (data) { ");
                html.AppendLine($"\t\t\t\t$(\"#jjgridview_{Name} #row\" + nRow).html(data); ");
                html.AppendLine($"\t\t\t\tdo_change_{Name}(nRow);");
                html.AppendLine("\t\t\t\tjjloadform(null, \"#row\" + nRow + \" \"); ");
                html.AppendLine("\t\t\t\tjjutil.gotoNextFocus(objid); ");
                html.AppendLine("\t\t\t}, ");
                html.AppendLine("\t\t\terror: function (jqXHR, textStatus, errorThrown) { ");
                html.AppendLine("\t\t\t\tconsole.log(errorThrown); ");
                html.AppendLine("\t\t\t\tconsole.log(textStatus); ");
                html.AppendLine("\t\t\t\tconsole.log(jqXHR); ");
                html.AppendLine("\t\t\t} ");
                html.AppendLine("\t\t}); ");
                html.AppendLine("\t} ");

                html.AppendLine("");
                html.Append("\tfunction ");
                html.AppendFormat("do_change_{0}", Name);
                html.AppendLine("(nRow) { ");
                html.AppendLine("\t\tvar prefixSelector = \"\";");
                html.AppendLine("\t\tif(nRow != null) {");
                html.AppendLine("\t\t\tprefixSelector = \"tr#row\" + nRow + \" \";");
                html.AppendLine("\t\t}");


                foreach (FormElementField f in listFieldsPost)
                {
                    //WorkArroud para gatilhar o select do search
                    if (f.Component == FormComponent.Search)
                    {
                        html.Append("\t\t$(prefixSelector + \"");
                        html.Append(".");
                        html.Append(f.Name);
                        html.AppendLine("\").change(function () {");
                        html.AppendLine("\t\tvar obj = $(this);");
                        html.AppendLine("\t\tsetTimeout(function() {");
                        html.AppendLine("\t\t\tvar nRowId = obj.attr(\"nRowId\");");
                        html.AppendLine("\t\t\tvar objid = obj.attr(\"id\");");
                        html.Append("\t\t\t");
                        html.Append(functionname);
                        html.Append("(nRowId, \"");
                        html.Append(f.Name);
                        html.AppendLine("\", objid);");
                        html.AppendLine("\t\t\t},200);");
                        html.AppendLine("\t\t});");
                        html.AppendLine("");
                    }
                    else
                    {
                        html.Append("\t\t$(prefixSelector + \"");
                        html.Append(".");
                        html.Append(f.Name);
                        html.AppendLine("\").change(function () {");
                        html.AppendLine("\t\t\tvar obj = $(this);");
                        html.AppendLine("\t\t\tvar nRowId = obj.attr(\"nRowId\");");
                        html.AppendLine("\t\t\tvar objid = obj.attr(\"id\");");
                        html.Append("\t\t\t");
                        html.Append(functionname);
                        html.Append("(nRowId, \"");
                        html.Append(f.Name);
                        html.AppendLine("\", objid);");
                        html.AppendLine("\t\t});");
                        html.AppendLine("");
                    }
                }

                html.AppendLine("\t}");

                html.AppendLine("");
                html.AppendLine("\t$(document).ready(function () {");
                html.AppendLine($"\t\tdo_change_{Name}(null);");
                html.AppendLine("\t});");
            }
        }

        html.AppendLine("\t</script> ");

        return html.ToString();
    }

    public string GetHtmlAction(Hashtable values)
    {
        //Actions
        var basicActions = GridActions.OrderBy(x => x.Order).ToList();
        var listAction = basicActions.FindAll(x => x.IsVisible && !x.IsGroup);
        var listActionGroup = basicActions.FindAll(x => x.IsVisible && x.IsGroup);

        var html = new StringBuilder();
        foreach (var action in listAction)
        {
            html.AppendLine("\t\t\t\t\t<td class=\"table-action\">");
            html.Append("\t\t\t\t\t\t");
            var link = ActionManager.GetLinkGrid(action, values);
            var onRender = OnRenderAction;
            if (onRender != null)
            {
                var args = new ActionEventArgs(action, link, values);
                onRender.Invoke(this, args);
                if (args.HtmlResult != null)
                {
                    html.AppendLine(args.HtmlResult);
                    link = null;
                }
            }

            if (link != null)
                html.AppendLine(link.GetHtml());

            html.AppendLine("\t\t\t\t\t</td>");
        }

        if (listActionGroup.Count > 0)
        {
            html.AppendLine("\t\t\t\t\t<td class=\"table-action\">");
            html.AppendLine($"\t\t\t\t\t\t<div class=\"{BootstrapHelper.InputGroupBtn}\">");
            html.AppendLine(
                $"\t\t\t\t\t\t\t<{(BootstrapHelper.Version == 3 ? "button" : "a")} type=\"button\" class=\"btn-link dropdown-toggle\" {BootstrapHelper.DataToggle}=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">");
            html.Append('\t', 8);
            html.Append("<span class=\"caret\" ");
            html.Append($"{BootstrapHelper.DataToggle}=\"tooltip\" ");
            html.AppendFormat("title=\"{0}\">", Translate.Key("More Options"));
            html.AppendLine("</span>");
            html.AppendLine($"\t\t\t\t\t\t\t</{(BootstrapHelper.Version == 3 ? "button" : "a")}>");
            html.AppendLine("\t\t\t\t\t\t\t<ul class=\"dropdown-menu dropdown-menu-right\">");
            foreach (var action in listActionGroup)
            {
                var link = ActionManager.GetLinkGrid(action, values);
                var onRender = OnRenderAction;
                if (onRender != null)
                {
                    var args = new ActionEventArgs(action, link, values);
                    onRender.Invoke(this, args);
                }

                if (link is not { Visible: true }) continue;

                if (action.DividerLine)
                    html.AppendLine("\t\t\t\t\t\t\t\t<li role=\"separator\" class=\"divider\"></li>");

                html.AppendLine("\t\t\t\t\t\t\t\t<li class=\"dropdown-item\">");
                html.Append("\t\t\t\t\t\t\t\t\t");
                html.AppendLine(link.GetHtml());
                html.AppendLine("\t\t\t\t\t\t\t\t</li>");
            }

            html.AppendLine("\t\t\t\t\t\t\t</ul>");
            html.AppendLine("\t\t\t\t\t\t</div>");
            html.AppendLine("\t\t\t\t\t</td>");
        }


        return html.ToString();
    }

    private HtmlElement GetSettingsHtml()
    {
        var action = ConfigAction;
        bool isVisible =
            ActionManager.Expression.GetBoolValue(action.VisibleExpression, action.Name, PageState.List,
                RelationValues);
        if (!isVisible)
            return new HtmlElement(string.Empty);

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

        modal.HtmlContent = CurrentUI.GetHtmlElement(IsPaggingEnabled()).GetElementHtml();

        return modal.GetHtmlElement();
    }

    private HtmlElement GetExportHtml()
    {
        var action = ExportAction;
        bool isVisible =
            ActionManager.Expression.GetBoolValue(action.VisibleExpression, action.Name, PageState.List,
                RelationValues);
        if (!isVisible)
            return new HtmlElement(string.Empty);

        var modal = new JJModalDialog
        {
            Name = "export_modal_" + Name,
            Title = "Export"
        };

        return modal.GetHtmlElement();
    }

    private HtmlElement GetLegendHtml()
    {
        var action = LegendAction;
        bool isVisible =
            ActionManager.Expression.GetBoolValue(action.VisibleExpression, action.Name, PageState.List,
                RelationValues);
        if (!isVisible)
            return new HtmlElement(string.Empty);

        var legend = new JJLegendView(FormElement, DataAccess)
        {
            ShowAsModal = true,
            Name = "iconlegend_modal_" + Name
        };
        return legend.GetHtmlElement();
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

    internal string GetPkValues(DataRow row, char separator = ';')
    {
        string name = "";
        foreach (var fpk in PrimaryKeyFields)
        {
            if (name.Length > 0)
                name += separator.ToString();

            name += row[fpk.Name].ToString();
        }

        return name;
    }

    internal string GetPkValues(IDictionary row, char separator = ';')
    {
        string name = "";
        foreach (var fpk in PrimaryKeyFields)
        {
            if (name.Length > 0)
                name += separator.ToString();

            name += row[fpk.Name].ToString();
        }

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

                var html = new HtmlBuilder();
                html.StartElement(new DataExpLog(exp.Name).GetHtmlProcess());

                CurrentContext.Response.SendResponse(html.RenderHtml());
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
        int tot = 0;
        if (_dataSource == null || IsUserSetDataSource)
        {
            _dataSource = GetDataTable(CurrentFilter, CurrentOrder, CurrentUI.TotalPerPage, CurrentPage, ref tot);
            TotalRecords = tot;

            //Se estiver paginando e não retornar registros volta para pagina inicial
            if (CurrentPage > 1 && _dataSource.Rows.Count == 0)
            {
                CurrentPage = 1;
                _dataSource = GetDataTable(CurrentFilter, CurrentOrder, CurrentUI.TotalPerPage, CurrentPage, ref tot);
                TotalRecords = tot;
            }
        }

        return DataSource;
    }

    /// <summary>
    /// <inheritdoc cref="GetDataTable"/>
    /// </summary>
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
            var result = DataDictionaryManager.GetDataTable(filters, orderBy, recordsPerPage, currentPage);
            dt = result.Result;
            total = result.Total;
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
    private List<Hashtable> GetGridValues(DataTable dt = null)
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
            var newValues =
                FieldManager.GetFormValues(prefixValue, FormElement, PageState.List, values, AutoReloadFormFields);
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

        string[] listPk = inputHidden.Split(',');

        List<FormElementField> pkFields = PrimaryKeyFields;
        foreach (string pk in listPk)
        {
            Hashtable values = new();
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

    private string DoSelectAllRows()
    {
        int tot = 0;
        var dt = GetDataTable(CurrentFilter, CurrentOrder, 999999, 1, ref tot);
        var sIds = new StringBuilder();
        var hasVal = false;
        foreach (DataRow row in dt.Rows)
        {
            if (!hasVal)
                hasVal = true;
            else
                sIds.Append(",");

            string values = GetPkValues(row);
            sIds.Append(Cript.Cript64(values));
        }

        return sIds.ToString();
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
            foreach (var f in FormElement.Fields)
            {
                bool enable = FieldManager.IsEnable(f, PageState.List, row);
                bool visible = FieldManager.IsVisible(f, PageState.List, row);
                if (enable && visible && f.DataBehavior != FieldBehavior.ViewOnly)
                {
                    string val = "";
                    if (row[f.Name] != null)
                        val = row[f.Name].ToString();

                    string objname = GetFieldName(f.Name, row);
                    string err = FieldValidator.ValidateField(f, objname, val);
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
        return !(!ShowPagging || CurrentPage == 0 || CurrentUI.TotalPerPage == 0 || TotalRecords == 0);
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

    public void SetGridOptions(UIGrid options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options), "Grid Options");

        EnableAjax = true;
        EnableSorting = options.EnableSorting;
        EnableMultSelect = options.EnableMultSelect;
        MaintainValuesOnLoad = options.MaintainValuesOnLoad;
        ShowPagging = options.ShowPagging;
        ShowToolbar = options.ShowToolBar;

        if (!GridUI.HasFormValues(CurrentContext) | !ShowToolbar | !ConfigAction.IsVisible)
        {
            GridUI ui = null;
            if (MaintainValuesOnLoad && FormElement != null)
                ui = JJSession.GetSessionValue<GridUI>($"jjcurrentui_{FormElement.Name}");

            if (ui == null)
            {
                ui = CurrentUI;
                ui.ShowRowHover = options.ShowRowHover;
                ui.ShowRowStriped = options.ShowRowStriped;
                ui.ShowBorder = options.ShowBorder;
                ui.TotalPerPage = options.TotalPerPage;
                ui.TotalPaginationButtons = options.TotalPaggingButton;
                ui.IsHeaderFixed = options.HeaderFixed;
            }

            CurrentUI = ui;
        }

        ShowHeaderWhenEmpty = options.ShowHeaderWhenEmpty;
        EmptyDataText = options.EmptyDataText;
    }

    internal BasicAction GetCurrentAction(ActionMap actionMap)
    {
        if (actionMap == null)
            return null;

        return actionMap.ContextAction switch
        {
            ActionOrigin.Form => null, //TODO: formAction
            ActionOrigin.Grid => GridActions.Find(x => x.Name.Equals(actionMap.ActionName)),
            ActionOrigin.Toolbar => ToolBarActions.Find(x => x.Name.Equals(actionMap.ActionName)),
            ActionOrigin.Field => FormElement.Fields[actionMap.FieldName].Actions.Get(actionMap.ActionName),
            _ => null,
        };
    }

    public bool IsExportPost()
    {
        var actionMap = CurrentActionMap;
        var action = GetCurrentAction(actionMap);
        return action is ExportAction;
    }
}