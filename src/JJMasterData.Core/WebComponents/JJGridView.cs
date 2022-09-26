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
using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Http;
using Newtonsoft.Json;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Exibe os valores do banco de dados em uma tabela, 
/// onde cada campo representa uma coluna e cada registro representa uma linha.
/// Permite paginação, multiplos filtros, configuração de layout e ordenação de campos
/// </summary>
/// <example>
/// Exemplo
///[!code-cs[Example](../../../doc/JJMasterData.Sample/JJFormViewBasicExample.aspx.cs)]
/// <img src="../media/JJGridViewWithLegend.png"/>
/// </example>
public class JJGridView : JJBaseView
{
    #region "Events"

    /// <summary>
    /// Evento disparado ao renderizar o conteúdo HTML da celula
    /// </summary>
    /// <example>
    /// Exemplo da pagina
    ///[!code-html[Example](../../../doc/JJMasterData.Sample/JJGridViewRenderCell.aspx)]
    ///[!code-cs[Example](../../../doc/JJMasterData.Sample/JJGridViewRenderCell.aspx.cs)]
    /// Exemplo objeto
    ///[!code-cs[Example](../../../doc/JJMasterData.Sample/Model/Cliente.cs)]
    /// </example>
    public event EventHandler<GridCellEventArgs> OnRenderCell;

    /// <summary>
    /// Evento disparado ao renderizar o checkbox utilizado para selecionar a linha da Grid.
    /// <para/>Disparado somente quando a propriedade EnableMultSelect estiver habilitada.
    /// </summary>       
    public event EventHandler<GridSelectedCellEventArgs> OnRenderSelectedCell;

    /// <summary>
    /// Evento disparado para recuperar os dados da tabela
    /// </summary>
    /// <remarks>
    /// O componente utiliza seguinte regra para recuperar os dados da grid:
    /// <para/>1) Utiliza a propriedade DataSource;
    /// <para/>2) Se a propriedade DataSource for nula, tenta executar a ação OnDataLoad;
    /// <para/>3) Se a ação OnDataLoad não for implementada, tenta recuperar 
    /// utilizando a proc informada no FormElement;
    /// </remarks>
    public event EventHandler<GridDataLoadEventArgs> OnDataLoad;

    /// <summary>
    /// Evento disparado ao renderizar as ações
    /// </summary>
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

    /// <summary>
    /// List of the fields in the grid with a default value.
    /// </summary>
    private List<FormElementField> VisibleFields
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

    /// <summary>
    /// Funções úteis para manipular ações
    /// </summary>
    internal ActionManager ActionManager => _actionManager ??= new ActionManager(this, FormElement);

    /// <summary>
    /// Funções úteis para manipular campos no formulário
    /// </summary>
    internal FieldManager FieldManager => _fieldManager ??= new FieldManager(this, FormElement);

    /// <summary>
    /// Configurações pré-definidas do formulário
    /// </summary>
    /// <example>
    /// Exemplo da pagina
    /// [!code-cshtml[Example](../../../doc/JJMasterData.Sample/JJGridViewWithLegend.aspx)]
    /// [!code-cs[Example](../../../doc/JJMasterData.Sample/JJGridViewWithLegend.aspx.cs)]
    /// Exemplo objeto
    /// [!code-cs[Example](../../../doc/JJMasterData.Sample/Model/Prospect.cs)]
    /// O Resultado html ficará parecido com esse:
    /// <img src="../media/JJGridViewWithLegend.png"/>
    /// </example>
    public FormElement FormElement { get; set; }

    /// <summary>
    /// Tabela com os dados
    /// </summary>
    /// <remarks>
    /// Datasource é propriedade responsável por controlar a origem de dados.
    /// O componente utiliza seguinte regra para recuperar os dados da grid:
    /// <para/>1) Utiliza a propriedade DataSource;
    /// <para/>2) Se a propriedade DataSource for nula, tenta executar a ação OnDataLoad;
    /// <para/>3) Se a ação OnDataLoad não for implementada, tenta recuperar 
    /// utilizando a proc informada no FormElement;
    /// </remarks>
    public DataTable DataSource
    {
        get => _dataSource;
        set
        {
            _dataSource = value;
            if (value == null) return;
            IsUserSetDataSource = true;
            TotalReg = value.Rows.Count;
        }
    }

    /// <summary>
    /// Indica se o usuário preecheu manualmente o DataSource
    /// </summary>
    private bool IsUserSetDataSource { get; set; }

    /// <summary>
    /// Quantidade total de registros existentes no banco
    /// </summary>
    public int TotalReg { get; set; }

    /// <summary>
    /// Exibir título no cabeçalho da página
    /// </summary>
    public bool ShowTitle { get; set; }

    /// <summary>
    /// Habilitar filtros (Default = true) 
    /// </summary>
    /// <remarks>
    /// Desabilita todos os campos e os botões de filtros 
    /// impedindo o usuário de aplicar ou alterar os filtros.
    /// </remarks>
    public bool EnableFilter { get; set; }

    /// <summary>
    /// Exibir toolbar (Default = true) 
    /// </summary>
    public bool ShowToolbar { get; set; }

    /// <summary>
    /// Get = Recupera o filtro atual<para/>
    /// </summary>
    public Hashtable CurrentFilter => Filter.GetCurrentFilter();

    /// <summary>
    /// Recupera a ordenação da tabela, 
    /// por padrão utiliza o primeiro campo da chave primária
    /// </summary>
    /// <returns>Ordem atual da tabela</returns>
    /// <remarks>
    /// Para mais de um campo utilize virgula ex:
    /// "Campo1 ASC, Campo2 DESC, Campo3 ASC"
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
                    object tableorder = CurrentContext.Session[string.Format("jjcurrentorder_{0}", Name)];
                    if (tableorder != null)
                    {
                        _currentOrder = tableorder.ToString();
                    }
                }
            }
            else
            {
                _currentOrder = CurrentContext.Request["current_tableorder_" + Name];
                if (_currentOrder == null)
                {
                    object tableorder = CurrentContext.Session[string.Format("jjcurrentorder_{0}", Name)];
                    if (tableorder != null)
                    {
                        _currentOrder = tableorder.ToString();
                    }
                }
            }
            CurrentOrder = _currentOrder;
            return _currentOrder;
        }
        set
        {
            CurrentContext.Session[string.Format("jjcurrentorder_{0}", Name)] = value;
            _currentOrder = value;
        }
    }

    /// <summary>
    /// Recupera a pagina atual da Grid
    /// </summary>
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
                int page = 1;
                string tablePageId = "current_tablepage_" + Name;
                if (!string.IsNullOrEmpty(CurrentContext.Request[tablePageId]))
                {
                    int nAuxPage;
                    if (int.TryParse(CurrentContext.Request[tablePageId], out nAuxPage))
                        page = nAuxPage;
                }
                else
                {
                    object tablePage = CurrentContext.Session[string.Format("jjcurrentpage_{0}", Name)];
                    if (tablePage != null)
                    {
                        int nAuxPage;
                        if (int.TryParse(tablePage.ToString(), out nAuxPage))
                            page = nAuxPage;
                    }
                }

                CurrentPage = page;
            }
            else
            {
                int page = 1;
                if (MaintainValuesOnLoad)
                {
                    object tablePage = CurrentContext.Session[string.Format("jjcurrentpage_{0}", Name)];
                    if (tablePage != null)
                    {
                        int nAuxPage;
                        if (int.TryParse(tablePage.ToString(), out nAuxPage))
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
                CurrentContext.Session[string.Format("jjcurrentpage_{0}", Name)] = value.ToString();

            _currentPage = value;
        }
    }

    /// <summary>
    /// Recupera as configurações da interface do usuário
    /// </summary>
    public GridUI CurrentUI
    {
        get
        {
            if (_currentUI != null)
                return _currentUI;

            //Se remover daqui ao chamar o metodo GetDataTable() fora da classe não respeita a paginação
            var actionMap = CurrentActionMap;
            var action = GetCurrentAction(actionMap);
            if (action is ConfigAction)
            {
                CurrentUI = GridUI.LoadFromForm(CurrentContext);
                return _currentUI;
            }

            if (MaintainValuesOnLoad && FormElement != null)
            {
                CurrentUI = JJSession.GetSessionValue<GridUI>(string.Format("jjcurrentui_{0}", FormElement.Name));
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
                JJSession.SetSessionValue(string.Format("jjcurrentui_{0}", FormElement.Name), value);

            _currentUI = value;
        }
    }

    /// <summary>
    /// Objeto responsável por renderizar o filtro
    /// </summary>
    internal GridFilter Filter
    {
        get
        {
            if (_filter == null)
                _filter = new GridFilter(this);

            return _filter;
        }
    }

    /// <summary>
    /// Recupera as configurações de exportação 
    /// </summary>
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

    /// <summary>
    /// Realizar os posts via Ajax (Default = true)
    /// </summary>
    public bool EnableAjax { get; set; }

    /// <summary>
    /// Habilitar todos os campos para edição (Default = false)
    /// </summary>
    public bool EnableEditMode { get; set; }

    /// <summary>
    /// Habilita Ordenação das colunas (Default = true)
    /// </summary>
    /// <remarks>
    /// Habilita ou não o link nos titulos permitindo a ordenação.
    /// Mesmo quando configurado como falso, a grid respeita a propriedade CurrentOrder
    /// </remarks>
    public bool EnableSorting { get; set; }

    /// <summary>
    /// Permite selecionar multiplas linhas na Grid 
    /// habilitando um checkbox na primeira coluna. (Defaut = false)
    /// </summary>
    public bool EnableMultSelect { get; set; }

    /// <summary>
    /// Mantem os filtros, ordem e paginação da grid na sessão, 
    /// e recupera na primeira carga da pagina. (Default = false)
    /// </summary>
    /// <remarks>
    /// Ao utilizar esta propriedade, recomendamos alterar o parametro [Name] do objeto.
    /// A propriedade [Name] é utilizada para compor o nome da variável de sessão.
    /// </remarks>
    public bool MaintainValuesOnLoad { get; set; }



    /// <summary>
    /// Obtém ou define um valor que indica se o cabeçalho da gridview ficará visível quando não existir dados.
    /// </summary>
    /// <remarks>
    /// Valor padrão = (Verdadeiro).
    /// <para/>
    /// Para alterar o texto da mensagem veja o método EmptyDataText
    /// </remarks>
    public bool ShowHeaderWhenEmpty { get; set; }

    /// <summary>
    /// Obtém ou define o texto a ser exibido na linha de dados vazia quando um controle JJGridView não contém registros.
    /// </summary>
    /// <remarks>
    /// Valor padrão = (Não existe registro para ser exibido).
    /// <para/>
    /// Para ocultar as colunas ao exibir a mensagem veja o método
    /// <seealso cref="ShowHeaderWhenEmpty"/>.
    /// </remarks>
    public string EmptyDataText { get; set; }

    /// <summary>
    /// Exibe os controles de paginação (Default = true) 
    /// </summary>
    /// <remarks>
    /// Oculta todos os botões da paginação 
    /// porem mantem os controles de paginação pré-definidos.
    /// <para/>
    /// A Paginãção será exibida se o numero de registros da grid ultrapassar a quantidade minima de registros em uma pagina.
    /// <para/>
    /// Se a propriedade CurrentPage for igual zero  a paginação não será exibida.
    /// <para/>
    /// Se a propriedade CurrentUI.TotalPerPage for igual zero a paginação não será exibida.
    /// <para/>
    /// Se a propriedade TotalReg for igual zero a paginação não será exibida.
    /// </remarks>
    public bool ShowPagging { get; set; }

    /// <summary>
    /// Campos com erro.
    /// Key=Nome do campo, Value=Descricão do erro
    /// </summary>
    /// <remarks>
    /// Utilizado quando é permitido alteração de valores na grid
    /// permitindo marcar exatamente o campo que esta com erro.
    /// </remarks>
    public Hashtable Erros { get; set; }

    /// <summary>
    /// Ao recarregar o painel, manter os valores digitados no formuário.
    /// Valido somente quando a propriedade EnableEditMode estiver ativada
    /// (Default=True)
    /// </summary>
    public bool AutoReloadFormFields { get; set; }

    /// <summary>
    /// Valores a serem substituídos por relacionamento.
    /// Se o nome do campo existir no relaciomento o valor será substituido
    /// </summary>
    /// <remarks>
    /// Key = Nome do campo, Value=Valor do campo
    /// </remarks>
    public Hashtable RelationValues { get; set; }

    /// <summary>
    /// Tamanho da fonte do titulo (default(H1)
    /// </summary>
    public HeadingSize SizeTitle { get; set; }

    private DataDictionaryManager DataDictionaryManager => _dataDictionaryManager ??= new DataDictionaryManager(FormElement);

    internal Hashtable DefaultValues
    {
        get
        {
            if (_defaultValues != null) return _defaultValues;

            //Valor padrão dos campos
            var formManager = new FormManager(FormElement, UserValues, DataAccess);
            _defaultValues = formManager.GetDefaultValues(null, PageState.List);

            return _defaultValues;
        }
    }

    public LegendAction LegendAction
    {
        get
        {
            return (LegendAction)ToolBarActions.Find(x => x is LegendAction);
        }
    }

    public RefreshAction RefreshAction
    {
        get
        {
            return (RefreshAction)ToolBarActions.Find(x => x is RefreshAction);
        }
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
        SizeTitle = HeadingSize.H1;
    }

    public JJGridView(DataTable table) : this()
    {
        FormElement = new FormElement(table);
        DataSource = table;
    }

    public JJGridView(string elementName) : this()
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName), "Nome do dicionário nao pode ser vazio");

        Name = "jjview" + elementName.ToLower();
        var dicParser = GetDictionary(elementName);
        FormElement = dicParser.GetFormElement();
        SetGridOptions(dicParser.UIOptions.Grid);
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

    protected override string RenderHtml()
    {
        StringBuilder html = new();

        //Lookup Route
        string lookupRoute = CurrentContext.Request.QueryString("jjlookup_" + Name);
        if (!string.IsNullOrEmpty(lookupRoute))
        {
            string fieldName = lookupRoute.Substring(GridFilter.FIELD_NAME_PREFIX.Length);
            var f = FormElement.Fields.ToList().Find(x => x.Name.Equals(fieldName));
            if (f != null)
            {
                var lookup = (JJLookup)FieldManager.GetField(f, PageState.Filter, null, null);
                lookup.Name = lookupRoute;
                lookup.DataItem.ElementMap.EnableElementActions = false;
                return lookup.GetHtml();
            }
        }

        if (ShowTitle)
            html.Append(GetHtmlTitle());

        if (FilterAction.IsVisible)
            html.Append(GetHtmlFilter());

        if (ShowToolbar)
            html.Append(GetHtmlGridToolbar());

        html.Append(GetHtmlTable());

        return html.ToString();
    }


    /// <summary>
    /// Renderiza o conteúdo do título
    /// </summary>
    /// <returns>
    /// Retorna Html renderizado
    /// </returns>
    public string GetHtmlTitle()
    {
        var title = new JJTitle(FormElement.Title, FormElement.SubTitle);
        title.Size = SizeTitle;
        return title.GetHtml();
    }

    /// <summary>
    /// Renderiza o conteúdo do filtro
    /// </summary>
    /// <returns>
    /// Retorna Html renderizado
    /// </returns>
    public string GetHtmlFilter() => Filter.GetHtmlFilter();

    /// <summary>
    /// Renderiza o conteúdo da barra de ferramentar da grid
    /// </summary>
    /// <returns>
    /// Retorna Html renderizado
    /// </returns>
    public string GetHtmlGridToolbar()
    {
        StringBuilder html = new();
        html.AppendLine("<!-- Start Toolbar -->");
        html.AppendLine($"<div class=\"{BootstrapHelper.FormGroup}\"> ");
        html.AppendLine("\t<div class=\"row\"> ");
        html.AppendLine("\t\t<div class=\"col-sm-12\"> ");

        var listActions = ToolBarActions.OrderBy(x => x.Order).ToList();
        foreach (var action in listActions)
        {
            if (action is FilterAction filterAction)
            {
                bool isVisible = FieldManager.IsVisible(action, PageState.List, DefaultValues);
                if (!isVisible)
                    continue;

                if (filterAction.EnableScreenSearch)
                {
                    html.Append('\t', 3);
                    html.Append(Filter.GetHtmlToolBarSeach());
                    continue;
                }
            }

            var linkButton = ActionManager.GetLinkToolBar(action, DefaultValues);
            if (linkButton.Visible)
            {
                if (action is ExportAction)
                {
                    if (DataExp.IsRunning())
                    {
                        linkButton.Spinner.Name = "dataexp_spinner_" + Name;
                        linkButton.Spinner.Visible = true;
                    }

                }
                else if (action is ImportAction)
                {
                    if (DataImp.IsRunning())
                        linkButton.Spinner.Visible = true;
                }
            }


            if (BootstrapHelper.Version != 3)
            {
                linkButton.CssClass += $" {BootstrapHelper.MarginRight}-1";
            }


            html.Append('\t', 3);

            html.AppendLine(linkButton.GetHtml());
        }
        html.AppendLine("\t\t</div> ");


        html.AppendLine("\t</div> ");
        html.AppendLine("</div> ");
        html.AppendLine("<!-- End Toolbar -->");
        html.AppendLine("");

        return html.ToString();
    }

    /// <summary>
    /// Renderiza a modal para configurar a ordem dos registros da grid
    /// </summary>
    /// <returns>
    /// Retorna Html renderizado
    /// </returns>
    private string GetHtmlConfigSorting()
    {
        var configsorting = new GridConfigSorting(this);
        return configsorting.GetHtml();
    }

    /// <summary>
    /// Renderiza o conteúdo da tabela
    /// </summary>
    /// <returns>
    /// Retorna Html renderizado
    /// </returns>
    public string GetHtmlTable()
    {
        if (FormElement == null)
            throw new ArgumentNullException(nameof(FormElement));

        if (EnableMultSelect && PrimaryKeyFields.Count == 0)
            throw new Exception(
                Translate.Key("It is not allowed to enable multiple selection without defining a primary key in the data dictionary"));

        var html = new StringBuilder();

        //Açoes da grid
        string sAction = CurrentContext.Request["current_tableaction_" + Name];
        var actionMap = CurrentActionMap;
        var action = GetCurrentAction(actionMap);
        //if (action is ExportAction)
        //{
        //    DoExport();
        //    return null;
        //}
        //else 
        if (action is SqlCommandAction cmdAction)
        {
            string sRet = DoSqlCommand(actionMap, cmdAction);
            html.AppendLine(sRet);
            sAction = string.Empty;
        }
        else if (action is PythonScriptAction pyAction)
        {
            string sRet = DoPythonScriptAction(actionMap, pyAction);
            html.AppendLine(sRet);
            sAction = string.Empty;
        }



        //Se o post for via ajax
        string tRequest = CurrentContext.Request.QueryString("t");
        if ("tableexp".Equals(tRequest))
        {
            string gridName = CurrentContext.Request.QueryString("gridName");
            if (Name.Equals(gridName))
                DoExport();

            return null;
        }

        //Recupera os valores selecionados
        var listSelectedValues = GetSelectedGridValues();

        //Recupera os dados
        GetDataTable();


        if ("tablerow".Equals(tRequest))
        {
            string gridName = CurrentContext.Request.QueryString("gridName");
            if (Name.Equals(gridName))
            {
                int nRowId = int.Parse(CurrentContext.Request.QueryString("nRow"));
                var row = DataSource.Rows[nRowId];
                html.Append(GetHtmlRow(row, nRowId, true, listSelectedValues));

                CurrentContext.Response.SendResponse(html.ToString());
            }

            return null;
        }

        if ("selectall".Equals(tRequest))
        {
            string values = DoSelectAllRows();
            CurrentContext.Response.SendResponse(values);
        }


        html.AppendLine("");
        html.AppendLine($"<div id=\"jjgridview_{Name}\">");

        if (SortAction.IsVisible)
            html.Append(GetHtmlConfigSorting());

        html.AppendLine("\t<!-- Start Table -->");

        //Scripts
        html.AppendLine(GetHtmlScript());

        //Input Hiddens
        html.Append("\t<input type=\"hidden\" id=\"current_tableorder_");
        html.Append(Name);
        html.Append("\" name=\"current_tableorder_");
        html.Append(Name);
        html.Append("\" value=\"");
        html.Append(CurrentOrder);
        html.AppendLine("\" /> ");

        html.Append("\t<input type=\"hidden\" id=\"current_tablepage_");
        html.Append(Name);
        html.Append("\" name=\"current_tablepage_");
        html.Append(Name);
        html.Append("\" value=\"");
        html.Append(CurrentPage);
        html.AppendLine("\" /> ");

        html.Append("\t<input type=\"hidden\" id=\"current_tableaction_");
        html.Append(Name);
        html.Append("\" name=\"current_tableaction_");
        html.Append(Name);
        html.Append("\" value=\"");
        html.Append(sAction);
        html.AppendLine("\" /> ");

        html.Append("\t<input type=\"hidden\" id=\"current_tablerow_");
        html.Append(Name);
        html.Append("\" name=\"current_tablerow_");
        html.Append(Name);
        html.AppendLine("\" value=\"\" /> ");

        if (EnableMultSelect)
        {
            html.Append("\t<input type=\"hidden\" id=\"selectedrows_");
            html.Append(Name);
            html.Append("\" name=\"selectedrows_");
            html.Append(Name);
            html.Append("\" value=\"");
            html.Append(SelectedRowsId);
            html.AppendLine("\" /> ");
        }

        //Inicio Grid
        if (CurrentUI.IsResponsive)
            html.AppendLine("\t<div class=\"table-responsive\">");

        html.Append("\t\t<table class=\"table");
        if (CurrentUI.ShowBorder)
            html.Append(" table-bordered");

        if (CurrentUI.ShowRowHover)
            html.Append(" table-hover");

        if (CurrentUI.ShowRowStriped)
            html.Append(" table-striped");

        if (CurrentUI.HeaderFixed)
            html.Append(" table-fix-head");

        html.AppendLine("\">");

        //Cabeçalho
        html.AppendLine("\t\t\t<thead>");
        html.AppendLine(GetHtmlHeader());
        html.AppendLine("\t\t\t</thead>");

        //Itens
        html.AppendLine($"\t\t\t<tbody id=\"table_{Name}\">");
        int nRow = -1;
        foreach (DataRow row in DataSource.Rows)
        {
            nRow++;
            html.Append(GetHtmlRow(row, nRow, false, listSelectedValues));
        }
        html.AppendLine("\t\t\t</tbody>");


        html.AppendLine("\t\t</table>");

        if (CurrentUI.IsResponsive)
            html.AppendLine("\t</div>");

        if (DataSource.Rows.Count == 0 && !string.IsNullOrEmpty(EmptyDataText))
        {
            var alert = new JJAlert();
            alert.ShowCloseButton = true;
            alert.Color = PanelColor.Default;
            alert.Icon = IconType.InfoCircle;
            alert.Title = EmptyDataText;
            if (Filter.HasFilter())
            {
                alert.Icon = IconType.Filter;
                alert.Messages.Add("There are filters applied for this query");
            }
            
            html.AppendLine(alert.GetHtml());
        }

        html.AppendLine("\t<!-- End Table -->");
        html.AppendLine("");

        //Paginação
        html.Append(GetHtmlPaging(listSelectedValues));

        if (ShowToolbar)
        {
            //Modal para configuração de layout
            html.Append(GetHtmlSetup());

            //Modal para exportação
            html.Append(GetHtmlExport());

            //Modal para legenda
            html.Append(GetHtmlLegend());
        }

        html.AppendLine("</div>");
        html.AppendLine("<div class=\"clearfix\"></div>");

        //Se o post for via ajax
        string postType = CurrentContext.Request.QueryString("t");
        string objName = CurrentContext.Request.QueryString("objname");
        if ("ajax".Equals(postType) && Name.Equals(objName))
        {
            CurrentContext.Response.SendResponse(html.ToString());
            return null;
        }

        return html.ToString();
    }




    /// <summary>
    /// Renderiza todos os scripts relacionados a grid
    /// </summary>
    /// <returns>
    /// Retorna Html renderizado
    /// </returns>
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
                html.AppendLine(string.Format("\t\t\t\tdo_change_{0}(nRow);", Name));
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
                html.AppendLine(string.Format("\t\tdo_change_{0}(null);", Name));
                html.AppendLine("\t});");
            }
        }

        html.AppendLine("\t</script> ");

        return html.ToString();
    }

    /// <summary>
    /// Renderiza o cabeçalho da grid
    /// </summary>
    /// <returns>
    /// Retorna Html renderizado
    /// </returns>
    private string GetHtmlHeader()
    {
        char TAB = '\t';

        if (DataSource.Rows.Count == 0 && !ShowHeaderWhenEmpty)
            return string.Empty;

        //Actions
        var basicActions = GridActions.OrderBy(x => x.Order).ToList();
        var listAction = basicActions.FindAll(x => x.IsVisible && !x.IsGroup);
        var listActionGroup = basicActions.FindAll(x => x.IsVisible && x.IsGroup);

        var html = new StringBuilder();
        html.Append(TAB, 4);
        html.AppendLine("<tr>");

        if (EnableMultSelect)
        {
            bool hasPages = true;
            if (!IsPaggingEnable())
            {
                hasPages = false;
            }
            else
            {
                int totalPages = (int)Math.Ceiling(TotalReg / (double)CurrentUI.TotalPerPage);
                if (totalPages <= 1)
                    hasPages = false;
            }

            html.Append(TAB, 5);
            html.AppendLine("<th class=\"jjselect\">");
            html.Append(TAB, 6);
            html.Append("<input type=\"checkbox\" ");
            html.Append("id=\"jjchk_all\" ");
            html.Append("name=\"jjchk_all\" ");
            html.Append($"{BootstrapHelper.DataToggle}=\"tooltip\" ");
            html.AppendFormat("title=\"{0}\" ", Translate.Key("Mark|Unmark all from page"));
            html.AppendLine("onclick =\"$('td.jjselect input').not(':disabled').prop('checked',$('#jjchk_all').is(':checked')).change();\">");

            if (hasPages)
            {
                html.Append(TAB, 6);
                html.AppendLine("<span class=\"dropdown\">");
                html.Append(TAB, 7);
                html.AppendLine($"<a href=\"#\" {BootstrapHelper.DataToggle}=\"dropdown\" class=\"dropdown-toggle\">{(BootstrapHelper.Version == 3 ? "<span class=\"fa fa-caret-down fa-fw fa-lg\"></span>" : string.Empty)}</a>");
                html.Append(TAB, 7);
                html.AppendLine("<ul class=\"dropdown-menu\">");
                html.Append(TAB, 8);
                html.AppendLine("<li class=\"dropdown-item\">");
                html.Append(TAB, 9);
                html.Append("<a href=\"javascript: void(0);\" title=\"\" onclick=\"jjview.doUnSelectAll('");
                html.Append(Name);
                html.Append("');\">");
                html.Append(Translate.Key("Unmark all selected records"));
                html.AppendLine("</a>");
                html.Append(TAB, 8);
                html.AppendLine("</li>");

                if (TotalReg <= 50000)
                {
                    html.Append(TAB, 8);
                    html.AppendLine("<li class=\"dropdown-item\">");
                    html.Append(TAB, 9);
                    html.Append("<a href=\"javascript: void(0);\" title=\"\" onclick=\"jjview.doSelectAll('");
                    html.Append(Name);
                    html.Append("');\">");
                    html.Append(Translate.Key("Mark all {0} records", TotalReg));
                    html.AppendLine("</a>");
                    html.Append(TAB, 8);
                    html.AppendLine("</li>");
                }

                html.Append(TAB, 7);
                html.AppendLine("</ul>");
                html.Append(TAB, 6);
                html.AppendLine("</span>");
            }
            html.Append(TAB, 5);
            html.AppendLine("</th>");
        }

        foreach (var field in VisibleFields)
        {
            string thStyle = string.Empty;
            if (field.Component == FormComponent.ComboBox)
            {
                if (field.DataItem != null &&
                    field.DataItem.ShowImageLegend && !field.DataItem.ReplaceTextOnGrid)
                {
                    thStyle = " style=\"text-align:center;\" ";
                }
            }
            else if (field.Component == FormComponent.CheckBox)
            {
                thStyle = " style=\"text-align:center;width:60px;\" ";
            }
            else if (field.Component == FormComponent.Cnpj)
            {
                thStyle = " style=\"min-width:130px;\" ";
            }
            else if (field.DataType == FieldType.Float || field.DataType == FieldType.Int)
            {
                if (!field.IsPk)
                    thStyle = " style=\"text-align:right;\" ";
            }
            html.Append(TAB, 5);
            html.Append("<th" + thStyle + ">");

            html.Append("<span ");
            if (EnableSorting && field.DataBehavior != FieldBehavior.Virtual)
            {
                html.Append("class=\"jjenable-sorting\" ");
                html.Append("onclick =\"jjview.doSorting('");
                html.Append(Name);
                html.Append("',");
                html.Append(EnableAjax ? "true" : "false");
                html.Append(",'");
                html.Append(field.Name);
                html.Append("');\"");
            }
            html.Append(">");

            //Título
            html.Append("<span");
            if (!string.IsNullOrEmpty(field.HelpDescription))
            {
                html.AppendFormat($" {BootstrapHelper.DataToggle}=\"tooltip\" title=\"{0}\"", Translate.Key(field.HelpDescription));
            }
            html.Append(">");
            html.Append(field.GetTranslatedLabel());
            html.Append("</span>");

            //Ico Ordem
            if (!string.IsNullOrEmpty(CurrentOrder))
            {
                foreach (string orderField in CurrentOrder.Split(','))
                {
                    string order = orderField.Trim();
                    if (string.IsNullOrWhiteSpace(order))
                        break;

                    if (order.StartsWith("["))
                    {
                        order = order.Replace("[", "");
                        order = order.Replace("]", "");
                    }

                    if (order.Equals(field.Name + " DESC"))
                        html.Append(GetDescendingIcon());
                    else if (order.Equals(field.Name + " ASC") || order.Equals(field.Name))
                        html.Append(GetAscendingIcon());
                }
            }
            else
            {
                if (field.Name.EndsWith("::DESC"))
                    html.Append(GetDescendingIcon());
                else if (field.Name.EndsWith("::ASC"))
                    html.Append(GetAscendingIcon());
            }

            //Ico Filter
            if (CurrentFilter != null &&
                field.Filter.Type != FilterMode.None &&
                !RelationValues.ContainsKey(field.Name) &&
                (CurrentFilter.ContainsKey(field.Name) || CurrentFilter.ContainsKey(field.Name + "_from"))
                )
            {
                html.Append("&nbsp;");
                html.Append("<span class=\"fa fa-filter\" ");
                html.Append($"{BootstrapHelper.DataToggle}=\"tooltip\" ");
                html.AppendFormat("title=\"{0}\">", Translate.Key("Applied filter"));
                html.Append("</span>");
            }

            html.Append("</span>");
            html.Append(TAB, 5);
            html.AppendLine("</th>");

        }

        foreach (var action in listAction)
        {
            html.Append(TAB, 5);
            html.Append("<th>");
            if (action.ShowTitle)
                html.Append(action.Text);
            html.AppendLine("\t</th>");
        }

        if (listActionGroup.Count > 0)
        {
            html.Append(TAB, 5);
            html.Append("<th>");
            html.AppendLine("\t</th>");
        }

        html.Append(TAB, 4);
        html.AppendLine("</tr>");

        return html.ToString();
    }

    private string GetDescendingIcon()
    {
        var html = new StringBuilder();
        html.Append("<span class=\"fa fa-sort-amount-desc\" ");
        html.Append($"{BootstrapHelper.DataToggle}=\"tooltip\" ");
        html.AppendFormat("title=\"{0}\">", Translate.Key("Descending order"));
        html.Append("</span>");

        return html.ToString();
    }

    private string GetAscendingIcon()
    {
        var html = new StringBuilder();
        html.Append("<span class=\"fa fa-sort-amount-asc\" ");
        html.Append($"{BootstrapHelper.DataToggle}=\"tooltip\" ");
        html.AppendFormat("title=\"{0}\">", Translate.Key("Ascending order"));
        html.Append("</span>");

        return html.ToString();
    }

    /// <summary>
    /// Renderiza a linha da grid
    /// </summary>
    /// <returns>
    /// Retorna Html renderizado
    /// </returns>
    private string GetHtmlRow(DataRow row, int nRow, bool isAjax, List<Hashtable> listSelectedValues)
    {
        Hashtable values = new Hashtable();
        for (int i = 0; i < row.Table.Columns.Count; i++)
        {
            values.Add(row.Table.Columns[i].ColumnName, row[i]);
        }

        if (EnableEditMode)
        {
            string prefixname = GetFieldName("", values);
            values = FieldManager.GetFormValues(prefixname, FormElement, PageState.List, values, AutoReloadFormFields);
        }

        //Actions
        var basicActions = GridActions.OrderBy(x => x.Order).ToList();
        var defaulAction = basicActions.Find(x => x.IsVisible && x.IsDefaultOption);

        StringBuilder html = new StringBuilder();
        if (!isAjax)
        {
            html.AppendFormat("\t\t\t\t<tr id=\"row{0}\"", nRow);
            if (!EnableEditMode && (defaulAction != null || EnableMultSelect))
            {
                html.Append(" class=\"jjgrid-action\"");
            }
            html.AppendLine(">");
        }

        string scriptOnClick = "";
        if (!EnableEditMode && defaulAction != null)
        {
            var linkDefaultAction = ActionManager.GetLinkGrid(defaulAction, values);
            var onRender = OnRenderAction;
            if (onRender != null)
            {
                var args = new ActionEventArgs(defaulAction, linkDefaultAction, values);
                onRender.Invoke(this, args);

                if (args.ResultHtml != null)
                {
                    linkDefaultAction = null;
                }
            }

            if (linkDefaultAction != null)
            {
                if (linkDefaultAction.Visible)
                {
                    if (!string.IsNullOrEmpty(linkDefaultAction.OnClientClick))
                        scriptOnClick = $" onclick =\"{linkDefaultAction.OnClientClick}\"";
                    else if (!string.IsNullOrEmpty(linkDefaultAction.UrlAction))
                        scriptOnClick = $" onclick =\"window.location.href = '{linkDefaultAction.UrlAction}'\"";
                }
            }
        }

        if (EnableMultSelect)
        {
            string value = GetPkValues(values);
            var chkBase = new JJCheckBox();
            chkBase.Name = "jjchk_" + nRow;
            chkBase.Value = Cript.Cript64(value);
            chkBase.IsChecked = listSelectedValues.Any(x => x.ContainsValue(value));

            html.Append("\t\t\t\t\t<td class=\"jjselect\">");
            var renderSelectedCell = OnRenderSelectedCell;
            if (renderSelectedCell != null)
            {
                var args = new GridSelectedCellEventArgs();
                args.DataRow = row;
                args.CheckBox = chkBase;
                OnRenderSelectedCell?.Invoke(this, args);
                if (args.CheckBox != null)
                    html.Append(chkBase.GetHtml());
            }
            else
            {
                html.Append(chkBase.GetHtml());
            }

            html.AppendLine("</td>");
            if (string.IsNullOrEmpty(scriptOnClick))
            {
                scriptOnClick = $" onclick=\"$('#{chkBase.Name}').not(':disabled').prop('checked',!$('#{chkBase.Name}').is(':checked')).change();\"";
            }
        }

        foreach (var f in VisibleFields)
        {
            string value = string.Empty;
            if (values.Contains(f.Name))
            {
                value = FieldManager.ParseVal(values, f);
            }

            string tdStyle = "";
            switch (f.Component)
            {
                case FormComponent.ComboBox:
                    {
                        if (f.DataItem != null &&
                            f.DataItem.ShowImageLegend && !f.DataItem.ReplaceTextOnGrid)
                        {
                            tdStyle = " style=\"text-align:center;\" ";
                        }

                        break;
                    }
                case FormComponent.CheckBox:
                    tdStyle = " style=\"text-align:center;\" ";
                    break;
                case FormComponent.File:
                    scriptOnClick = "";
                    break;
                default:
                    {
                        if (f.DataType == FieldType.Float || f.DataType == FieldType.Int)
                        {
                            if (!f.IsPk)
                                tdStyle = " style=\"text-align:right;\" ";
                        }

                        break;
                    }
            }

            html.Append("\t\t\t\t\t<td");
            html.Append(tdStyle);
            html.Append(scriptOnClick);
            html.Append(">");

            if (EnableEditMode && f.DataBehavior != FieldBehavior.ViewOnly)
            {
                string name = GetFieldName(f.Name, values);
                bool hasError = Erros?.ContainsKey(name) ?? false;
                if (hasError)
                    html.Append($"<div class=\"{BootstrapHelper.HasError}\">");

                if ((f.Component == FormComponent.ComboBox
                   | f.Component == FormComponent.CheckBox
                   | f.Component == FormComponent.Search)
                   & values.Contains(f.Name))
                {
                    value = values[f.Name].ToString();
                }
                var baseField = FieldManager.GetField(f, PageState.List, value, values, name);
                baseField.Attributes.Add("nRowId", nRow.ToString());
                baseField.CssClass = f.Name;

                var renderCell = OnRenderCell;
                if (renderCell != null)
                {
                    var args = new GridCellEventArgs();
                    args.Field = f;
                    args.DataRow = row;
                    args.Sender = baseField;
                    //args.ResultHtml = baseField.GetHtml(); medrei
                    OnRenderCell.Invoke(this, args);
                    html.Append(args.ResultHtml);
                }
                else
                {
                    html.AppendLine(baseField.GetHtml());
                }

                if (hasError)
                    html.Append("</div>");
            }
            else
            {
                var renderCell = OnRenderCell;
                if (renderCell != null)
                {
                    var args = new GridCellEventArgs
                    {
                        Field = f,
                        DataRow = row,
                        Sender = new JJText(value)
                    };
                    OnRenderCell.Invoke(this, args);
                    html.Append(args.ResultHtml);
                }
                else
                {
                    if (f.Component == FormComponent.File)
                    {
                        var upload = (JJTextFile)FieldManager.GetField(f, PageState.List, value, values, null);
                        html.Append(upload.GetHtmlForGrid());
                    }
                    else
                    {
                        html.Append(value.Trim());
                    }
                }
            }

            html.AppendLine("\t</td>");
        }

        html.AppendLine(GetHtmlAction(values));

        if (!isAjax)
        {
            html.AppendLine("\t\t\t\t</tr>");
        }
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
                if (args.ResultHtml != null)
                {
                    html.AppendLine(args.ResultHtml);
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
            html.AppendLine($"\t\t\t\t\t\t\t<{(BootstrapHelper.Version == 3 ? "button" : "a")} type=\"button\" class=\"btn-link dropdown-toggle\" {BootstrapHelper.DataToggle}=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">");
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

    /// <summary>
    /// Renderiza a paginação da grid
    /// </summary>
    /// <returns></returns>
    private string GetHtmlPaging(List<Hashtable> listSelectedValues)
    {
        if (!IsPaggingEnable())
            return "";

        int totalPages = (int)Math.Ceiling(TotalReg / (double)CurrentUI.TotalPerPage);
        int qtdButtons = CurrentUI.TotalPaggingButton;
        int startButton = (((int)Math.Floor((CurrentPage - 1) / (double)qtdButtons)) * qtdButtons) + 1;
        int endButton = startButton + qtdButtons;

        StringBuilder html = new();
        if (BootstrapHelper.Version != 3)
        {
            html.AppendLine("<div class=\"container-fluid p-0\">");
        }

        html.AppendLine("\t<!-- Start Pagging -->");
        html.AppendLine("\t\t<div class=\"row justify-content-between\">");
        html.AppendLine("\t\t\t<div class=\"col-sm-9\">");
        html.AppendLine("\t\t\t\t<ul class=\"pagination\">");

        if (startButton > qtdButtons)
        {

            html.Append("\t\t\t\t\t<li class=\"page-item\">");
            html.Append("<a class=\"page-link\" style=\"cursor:pointer; cursor:hand;\" ");
            html.Append($"{BootstrapHelper.DataToggle}=\"tooltip\" title=\"");
            html.Append(Translate.Key("First record"));
            html.Append("\" ");
            html.Append("onclick =\"javascript:jjview.doPaging('");
            html.Append(Name);
            html.Append("',");
            html.Append(EnableAjax ? "true" : "false");
            html.Append(",'");
            html.Append(1);
            html.Append("');\">");
            html.Append(new JJIcon(IconType.AngleDoubleLeft).GetHtml());
            html.Append("</a>");
            html.AppendLine("</li>");

            html.Append("\t\t\t\t\t<li class=\"page-item\">");
            html.Append("<a class=\"page-link\" style=\"cursor:pointer; cursor:hand;\" ");
            html.Append("onclick =\"javascript:jjview.doPaging('");
            html.Append(Name);
            html.Append("',");
            html.Append(EnableAjax ? "true" : "false");
            html.Append(",'");
            html.Append(startButton - 1);
            html.Append("');\">");
            html.Append(new JJIcon(IconType.AngleLeft).GetHtml());
            html.Append("</a>");
            html.AppendLine("</li>");

        }


        for (int i = startButton; i < endButton; i++)
        {
            if (i > totalPages || totalPages <= 1)
                break;

            html.Append("\t\t\t\t\t<li  class=\"page-item");
            if (i == CurrentPage)
                html.Append(" active");
            html.Append("\">");

            html.Append("<a href=\"#\"  class=\"page-link\" style=\"cursor:pointer; cursor:hand;\" ");
            html.Append("onclick =\"javascript:jjview.doPaging('");
            html.Append(Name);
            html.Append("',");
            html.Append(EnableAjax ? "true" : "false");
            html.Append(",'");
            html.Append(i);
            html.Append("');\">");
            html.Append(i);
            html.Append("</a>");
            html.AppendLine("</li>");
        }

        if (endButton <= totalPages)
        {
            html.Append("\t\t\t\t\t<li class=\"page-item\">");
            html.Append("<a class=\"page-link\" href=\"#\" style=\"cursor:pointer; cursor:hand;\" ");
            html.Append($"{BootstrapHelper.DataToggle}=\"tooltip\" title=\"");
            html.Append(totalPages);
            html.Append(" ");
            html.Append(Translate.Key("pages"));
            html.Append("\" ");
            html.Append("onclick =\"javascript:jjview.doPaging('");
            html.Append(Name);
            html.Append("',");
            html.Append(EnableAjax ? "true" : "false");
            html.Append(",'");
            html.Append(endButton);
            html.Append("');\">");
            html.Append(new JJIcon(IconType.AngleRight).GetHtml());
            html.Append("</a>");
            html.AppendLine("</li>");

            html.Append("\t\t\t\t\t<li class=\"page-item\">");
            html.Append("<a class=\"page-link\" href=\"#\" style=\"cursor:pointer; cursor:hand;\" ");
            html.Append($"{BootstrapHelper.DataToggle}=\"tooltip\" title=\"");
            html.Append(Translate.Key("Last record"));
            html.Append("\" ");
            html.Append("onclick =\"javascript:jjview.doPaging('");
            html.Append(Name);
            html.Append("',");
            html.Append(EnableAjax ? "true" : "false");
            html.Append(",'");
            html.Append(totalPages);
            html.Append("');\">");
            html.Append(new JJIcon(IconType.AngleDoubleRight).GetHtml());
            html.Append("</a>");
            html.AppendLine("</li>");
        }

        html.AppendLine("\t\t\t\t</ul>");
        html.AppendLine("\t\t\t</div>");
        html.AppendLine($"\t\t\t<div class=\"col-sm-3 {BootstrapHelper.TextRight}\">");
        html.AppendFormat("\t\t\t\t<label id=\"infotext_{0}\" class=\"small\">", Name);
        html.Append(Translate.Key("Showing"));
        html.Append(" ");
        if (totalPages <= 1)
        {
            html.Append("<span id=\"");
            html.Append(Name);
            html.Append("_totrows\">");
            html.Append(TotalReg.ToString("N0"));
            html.Append("</span> ");
            html.Append(Translate.Key("record(s)"));
        }
        else
        {
            html.Append((CurrentUI.TotalPerPage * CurrentPage) - CurrentUI.TotalPerPage + 1);
            html.Append("-");
            if ((CurrentUI.TotalPerPage * CurrentPage) > TotalReg)
                html.Append(TotalReg);
            else
                html.Append(CurrentUI.TotalPerPage * CurrentPage);
            html.Append(" ");
            html.Append(Translate.Key("From"));
            html.Append(" <span id=\"");
            html.Append(Name);
            html.Append("_totrows\">");
            html.Append(TotalReg.ToString("N0"));
            html.Append("</span>");
        }
        html.AppendLine("</label>");

        if (EnableMultSelect)
        {
            string textInfo;
            if (listSelectedValues == null || listSelectedValues.Count == 0)
            {
                textInfo = Translate.Key("No record selected");
            }
            else if (listSelectedValues.Count == 1)
            {
                textInfo = Translate.Key("A selected record");
            }
            else
            {
                textInfo = Translate.Key("{0} selected records", listSelectedValues.Count);
            }
            html.AppendLine("\t\t\t\t<br>");
            html.Append("\t\t\t\t<span id=\"selectedtext_");
            html.Append(Name);
            html.Append("\" noSelStr=\"");
            html.Append(Translate.Key("No record selected"));
            html.Append("\" oneSelStr=\"");
            html.Append(Translate.Key("A selected record"));
            html.Append("\" paramSelStr=\"");
            html.Append(Translate.Key("{0} selected records"));
            html.Append("\">");
            html.Append(textInfo);
            html.AppendLine("</span>");
        }

        html.AppendLine("\t\t\t</div>");
        html.AppendLine("\t</div>");
        html.AppendLine("\t<!-- End Pagging -->");
        if (BootstrapHelper.Version != 3)
        {
            html.AppendLine("</div>");
        }

        return html.ToString();
    }

    /// <summary>
    /// Renderiza o formulário modal de configuração de pagina
    /// </summary>
    private string GetHtmlSetup()
    {
        var action = ConfigAction;
        bool isVisible = ActionManager.Expression.GetBoolValue(action.VisibleExpression, action.Name, PageState.List, RelationValues);
        if (!isVisible)
            return string.Empty;

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

        modal.HtmlContent = CurrentUI.GetHtmlFormSetup(IsPaggingEnable());

        return modal.GetHtml();
    }

    /// <summary>
    /// Rendeiza o formulário modal de exportação
    /// </summary>
    private string GetHtmlExport()
    {
        var action = ExportAction;
        bool isVisible = ActionManager.Expression.GetBoolValue(action.VisibleExpression, action.Name, PageState.List, RelationValues);
        if (!isVisible)
            return string.Empty;

        var modal = new JJModalDialog
        {
            Name = "export_modal_" + Name,
            Title = "Export"
        };

        return modal.GetHtml();
    }

    /// <summary>
    /// Renderiza o formulário modal de legenda
    /// </summary>
    private string GetHtmlLegend()
    {
        var action = LegendAction;
        bool isVisible = ActionManager.Expression.GetBoolValue(action.VisibleExpression, action.Name, PageState.List, RelationValues);
        if (!isVisible)
            return string.Empty;

        var legend = new JJLegendView(FormElement, DataAccess);
        legend.ShowAsModal = true;
        legend.Name = "iconlegend_modal_" + Name;
        return legend.GetHtml();
    }

    /// <summary>
    /// Constroi o nome único de um campo para grid
    /// </summary>
    private string GetFieldName(string fieldName, IDictionary row)
    {
        string name = "";
        foreach (var fpk in PrimaryKeyFields)
        {
            if (name.Length > 0)
                name += "_";

            name += row[fpk.Name].ToString()
                .Replace(" ", "_")
                .Replace("'", "")
                .Replace("\"", "");
        }
        name += fieldName;

        return name;
    }

    /// <summary>
    /// Constroi o nome único de um campo para grid
    /// </summary>
    private string GetPkValues(IDictionary row, char separator = ';')
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

    /// <summary>
    /// Constroi o nome único de um campo para grid
    /// </summary>
    private string GetPKValues(DataRow row, char separator = ';')
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

    /// <summary>
    /// Recupera a chave do registro ao selecionar uma ação
    /// </summary>
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

    /// <summary>
    /// Exporta o arquivo com base nas parametrizações do usuário
    /// </summary>
    private void DoExport()
    {
        var exp = DataExp;
        string exptype = CurrentContext.Request.QueryString("exptype");
        if (exptype.Equals("showoptions"))
        {
            CurrentContext.Response.SendResponse(exp.GetHtml());
        }
        else if (exptype.Equals("export"))
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
                    var err = new JJValidationSummary(ExceptionManager.GetMessage(ex));
                    err.MessageTitle = "Error";

                    CurrentContext.Response.SendResponse(err.GetHtml());
                    return;
                }
            }

            CurrentContext.Response.SendResponse(exp.GetHtmlWaitProcess());
        }
        else if (exptype.Equals("checkProcess"))
        {
            var dto = exp.GetCurrentProcess();
            string json = JsonConvert.SerializeObject(dto);
            CurrentContext.Response.SendResponse(json, "text/json");
        }
        else if (exptype.Equals("stopProcess"))
        {
            exp.AbortProcess();
            CurrentContext.Response.SendResponse("{}", "text/json");
        }
    }




    /// <summary>
    /// Executa uma ação SQL
    /// </summary>
    private string DoSqlCommand(ActionMap map, SqlCommandAction cmdAction)
    {
        try
        {
            var listSql = new ArrayList();
            if (map.ContextAction == ActionOrigin.Toolbar && EnableMultSelect && cmdAction.ApplyOnSelected)
            {
                var selectedRows = GetSelectedGridValues();
                if (selectedRows.Count == 0)
                {
                    string msg = Translate.Key("No lines selected.");
                    return new JJMessageBox(msg, MessageIcon.Warning, true).GetHtml();
                }

                foreach (var row in selectedRows)
                {
                    string sql = ActionManager.Expression.ParseExpression(cmdAction.CommandSQL, PageState.List, false, row);
                    listSql.Add(sql);
                }

                DataAccess.SetCommand(listSql);
                ClearSelectedGridValues();
            }
            else
            {
                Hashtable formValues;
                if (map.PKFieldValues != null ||
                    map.PKFieldValues.Count > 0)
                {
                    formValues = DataDictionaryManager.GetHashtable(map.PKFieldValues).Result;
                }
                else
                {
                    var formManager = new FormManager(FormElement, UserValues, DataAccess);
                    formValues = formManager.GetDefaultValues(null, PageState.List);
                }

                string sql = ActionManager.Expression.ParseExpression(cmdAction.CommandSQL, PageState.List, false, formValues);
                listSql.Add(sql);
                DataAccess.SetCommand(listSql);
            }

            _dataSource = null;
        }
        catch (Exception ex)
        {
            string msg = ExceptionManager.GetMessage(ex);
            return new JJMessageBox(msg, MessageIcon.Error, true).GetHtml();
        }

        return null;
    }

    private string DoPythonScriptAction(ActionMap map, PythonScriptAction action)
    {

        var scriptManager = FormEventEngineFactory.GetEngine<IPythonEngine>();

        try
        {
            if (map.ContextAction == ActionOrigin.Toolbar && EnableMultSelect && action.ApplyOnSelected)
            {
                var selectedRows = GetSelectedGridValues();
                if (selectedRows.Count == 0)
                {
                    string msg = Translate.Key("No lines selected.");
                    return new JJMessageBox(msg, MessageIcon.Warning, true).GetHtml();
                }

                foreach (var row in selectedRows)
                    scriptManager.Execute(ActionManager.Expression.ParseExpression(action.PythonScript, PageState.List, false, row));


                ClearSelectedGridValues();
            }
            else
            {
                Hashtable formValues;
                if (map.PKFieldValues != null ||
                    map.PKFieldValues.Count > 0)
                {
                    formValues = DataDictionaryManager.GetHashtable(map.PKFieldValues).Result;
                }
                else
                {
                    var formManager = new FormManager(FormElement, UserValues, DataAccess);
                    formValues = formManager.GetDefaultValues(null, PageState.List);
                }
                scriptManager.Execute(ActionManager.Expression.ParseExpression(action.PythonScript, PageState.List, false, formValues));
            }

            _dataSource = null;
        }
        catch (Exception ex)
        {
            string msg = ExceptionManager.GetMessage(ex);
            return new JJMessageBox(msg, MessageIcon.Error, true).GetHtml();
        }

        return null;
    }

    /// <summary>
    /// Recupera os registros do banco de dados com base nos filtros atuais.  
    /// </summary>
    /// <returns>
    /// Retorna um DataTable com os registros localizados. 
    /// Se nenhum registro for localizado retorna nulo.
    /// O componente utiliza seguinte regra para recuperar os dados da grid:
    /// <para/>1) Utiliza a propriedade DataSource;
    /// <para/>2) Se a propriedade DataSource for nula, tenta executar a ação OnDataLoad;
    /// <para/>3) Se a ação OnDataLoad não for implementada, tenta recuperar 
    /// utilizando a proc informada no FormElement;
    /// </returns>
    public DataTable GetDataTable()
    {
        int tot = 0;
        if (_dataSource == null || IsUserSetDataSource)
        {
            _dataSource = GetDataTable(CurrentFilter, CurrentOrder, CurrentUI.TotalPerPage, CurrentPage, ref tot);
            TotalReg = tot;

            //Se estiver paginando e não retornar registros volta para pagina inicial
            if (CurrentPage > 1 && _dataSource.Rows.Count == 0)
            {
                CurrentPage = 1;
                _dataSource = GetDataTable(CurrentFilter, CurrentOrder, CurrentUI.TotalPerPage, CurrentPage, ref tot);
                TotalReg = tot;
            }
        }

        return DataSource;
    }

    /// <summary>
    /// Recupera os registros do banco de dados com base no filtro.  
    /// </summary>
    /// <param name="filters">Lista com os filtros a serem utilizados. [key(campo do BD), valor(valor armazenado no BD)]</param>
    /// <param name="orderby">Order do registro apenas um campo seguido por espaço ASC ou DESC</param>
    /// <param name="regporpag">Quantidade de registros a ser retornado por página</param>
    /// <param name="currentPage">Pagina atual</param>
    /// <param name="tot">Se o valor for igual a zero retorna como referência a quantidade de registros com base no filtro.</param>
    /// <returns>
    /// Retorna um DataTable com os registros localizados. 
    /// Se nenhum registro for localizado retorna nulo.
    /// O componente utiliza seguinte regra para recuperar os dados da grid:
    /// <para/>1) Utiliza a propriedade DataSource;
    /// <para/>2) Se a propriedade DataSource for nula, tenta executar a ação OnDataLoad;
    /// <para/>3) Se a ação OnDataLoad não for implementada, tenta recuperar 
    /// utilizando a proc informada no FormElement;
    /// </returns>
    private DataTable GetDataTable(Hashtable filters, string orderby, int regporpag, int currentPage, ref int tot)
    {
        DataTable dt;
        if (IsUserSetDataSource)
        {
            var tempdt = DataSource;
            if (tempdt != null)
                tot = tempdt.Rows.Count;

            var dv = new DataView(tempdt);
            dv.Sort = orderby;

            dt = dv.ToTable();
            dv.Dispose();
        }
        else if (OnDataLoad != null)
        {
            var args = new GridDataLoadEventArgs
            {
                Filters = filters,
                OrderBy = orderby,
                RegporPag = regporpag,
                CurrentPage = currentPage,
                Tot = tot
            };
            OnDataLoad.Invoke(this, args);
            tot = args.Tot;
            dt = args.DataSource;
        }
        else
        {
            var result = DataDictionaryManager.GetDataTable(filters, orderby, regporpag, currentPage);
            dt = result.Result;
            tot = result.Total;
        }

        return dt;
    }

    /// <summary>
    /// Recupera os valores dos campos alterados na Grid
    /// </summary>
    /// <param name="regPerPage">Registros por página</param>
    /// <param name="currentPage">Página atual</param>
    /// <returns>
    /// Retorna uma lista com um hash contendo nome do campo e valor
    /// </returns>
    /// <remarks>
    /// Utilizado com a propriedade EnableEditMode
    /// </remarks>
    public List<Hashtable> GetGridValues(int regPerPage, int currentPage)
    {
        int tot = 1;
        DataTable dt = GetDataTable(CurrentFilter, CurrentOrder, regPerPage, currentPage, ref tot);

        return GetGridValues(dt);
    }

    /// <summary>
    /// Recupera os valores dos campos alterados na Grid
    /// </summary>
    /// <returns>
    /// Retorna uma lista com um hash contendo nome do campo e valor
    /// </returns>
    /// <remarks>
    /// Utilizado com a propriedade EnableEditMode
    /// </remarks>
    public List<Hashtable> GetGridValues()
    {
        DataTable dt = GetDataTable();
        return GetGridValues(dt);
    }

    /// <summary>
    /// Recupera os valores dos campos alterados na Grid
    /// </summary>
    /// <returns>
    /// Retorna uma lista com um hash contendo nome do campo e valor
    /// </returns>
    /// <remarks>
    /// Utilizado com a propriedade EnableEditMode
    /// </remarks>
    private List<Hashtable> GetGridValues(DataTable dt)
    {
        if (dt == null)
            return null;

        var listValues = new List<Hashtable>();
        foreach (DataRow row in dt.Rows)
        {
            var values = new Hashtable();
            for (int i = 0; i < row.Table.Columns.Count; i++)
            {
                values.Add(row.Table.Columns[i].ColumnName, row[i]);
            }

            string prefixname = GetFieldName("", values);
            var newvalues = FieldManager.GetFormValues(prefixname, FormElement, PageState.List, values, AutoReloadFormFields);
            listValues.Add(newvalues);
        }

        return listValues;
    }

    /// <summary>
    /// Recupera os ids das linhas selecionadas na Grid
    /// </summary>
    /// <returns>
    /// Retorna uma lista com um hash contendo nome do campo e valor
    /// </returns>
    /// <remarks>
    /// Utilizado com a propriedade EnableMultSelect
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

    /// <summary>
    /// Desmarca todas as linhas selecionadas na Grid
    /// </summary>
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

            string values = GetPKValues(row);
            sIds.Append(Cript.Cript64(values));

        }

        return sIds.ToString();
    }

    /// <summary>
    /// Valida os campos habilitados para edição e 
    /// retorna uma lista com erros encontrados
    /// </summary>
    /// <param name="values">Dados do Formulário</param>
    /// <returns>
    /// Chave = Nome do Campo
    /// Valor = Mensagem de erro
    /// </returns>
    public Hashtable ValidateGridFields(List<Hashtable> values)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        var errors = new Hashtable();
        int line = 0;
        foreach (Hashtable row in values)
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

    /// <summary>
    /// Verifica se a paginação esta habilitada
    /// </summary>
    private bool IsPaggingEnable()
    {
        return !(!ShowPagging || CurrentPage == 0 || CurrentUI.TotalPerPage == 0 || TotalReg == 0);
    }

    /// <summary>
    /// Adicionar botão customizado na toolbar
    /// </summary>
    public void AddToolBarAction(SqlCommandAction action)
    {
        ValidateAction(action);
        ToolBarActions.Add(action);
    }

    /// <summary>
    /// Adicionar botão customizado na toolbar
    /// </summary>
    public void AddToolBarAction(UrlRedirectAction action)
    {
        ValidateAction(action);
        ToolBarActions.Add(action);
    }

    /// <summary>
    /// Adicionar botão customizado na toolbar
    /// </summary>
    public void AddToolBarAction(InternalAction action)
    {
        ValidateAction(action);
        ToolBarActions.Add(action);
    }

    /// <summary>
    /// Adicionar botão customizado na toolbar
    /// </summary>
    public void AddToolBarAction(ScriptAction action)
    {
        ValidateAction(action);
        ToolBarActions.Add(action);
    }

    /// <summary>
    /// Remover botão customizado na grid
    /// </summary>
    ///<remarks>
    ///Somente ações dos tipos ScriptAction, UrlRedirectAction ou InternalAction 
    ///podem ser removidas
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

    /// <summary>
    /// Adicionar botão customizado na grid
    /// </summary>
    public void AddGridAction(SqlCommandAction action)
    {
        ValidateAction(action);
        GridActions.Add(action);
    }

    /// <summary>
    /// Adicionar botão customizado na grid
    /// </summary>
    public void AddGridAction(UrlRedirectAction action)
    {
        ValidateAction(action);
        GridActions.Add(action);
    }

    /// <summary>
    /// Adicionar botão customizado na gtrid
    /// </summary>
    public void AddGridAction(InternalAction action)
    {
        ValidateAction(action);
        GridActions.Add(action);
    }

    /// <summary>
    /// Adicionar botão customizado na gtrid
    /// </summary>
    public void AddGridAction(ScriptAction action)
    {
        ValidateAction(action);
        GridActions.Add(action);
    }

    /// <summary>
    /// Remover botão customizado na grid
    /// </summary>
    ///<remarks>
    ///Somente ações dos tipos ScriptAction, UrlRedirectAction ou InternalAction 
    ///podem ser removidas
    ///</remarks>
    public void RemoveGridAction(string actionName)
    {
        if (string.IsNullOrEmpty(actionName))
            throw new ArgumentNullException(nameof(actionName));

        var action = GridActions.Find(x => x.Name.Equals(actionName));
        if (action == null)
            throw new ArgumentException(Translate.Key("Action {0} not found", actionName));

        if (action is ScriptAction ||
            action is UrlRedirectAction ||
            action is InternalAction)
        {
            GridActions.Remove(action);
        }
        else
        {
            throw new ArgumentException(Translate.Key("This action can not be removed"));
        }
    }

    /// <summary>
    /// Recupera uma ação da ToolBar pelo nome
    /// </summary>
    /// <param name="actionName">Nome cadastrado na ação</param>
    public BasicAction GetToolBarAction(string actionName)
    {
        return ToolBarActions.Find(x => x.Name.Equals(actionName));
    }

    /// <summary>
    /// Recupera uma ação da Grid pelo nome
    /// </summary>
    /// <param name="actionName">>Nome cadastrado na ação</param>
    public BasicAction GetGridAction(string actionName)
    {
        return GridActions.Find(x => x.Name.Equals(actionName));
    }

    /// <summary>
    /// Adiciona ou altera um valor no CurrentFilter.<br></br>
    /// Se existir altera, caso contrário inclui.
    /// </summary>
    /// <param name="field">Nome do Campo</param>
    /// <param name="value">Valor do Campo</param>
    public void SetCurrentFilter(string field, object value)
    {
        if (CurrentFilter.ContainsKey(field))
            CurrentFilter[field] = value;
        else
            CurrentFilter.Add(field, value);
    }

    /// <summary>
    /// Verifica se ação é valida, se não for retorna uma exception
    /// </summary>
    private void ValidateAction(BasicAction action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        if (string.IsNullOrEmpty(action.Name))
            throw new ArgumentException(Translate.Key("Property name action is not valid"));
    }

    /// <summary>
    /// Atribui as configurações do usuário cadastrado no dicionário de dados
    /// </summary>
    /// <param name="o">
    /// Configurações do usuário
    /// </param>
    public void SetGridOptions(UIGrid o)
    {
        if (o == null)
            throw new ArgumentNullException(nameof(o), "Grid Options");

        EnableAjax = true;
        EnableSorting = o.EnableSorting;
        EnableMultSelect = o.EnableMultSelect;
        MaintainValuesOnLoad = o.MaintainValuesOnLoad;
        ShowPagging = o.ShowPagging;
        ShowToolbar = o.ShowToolBar;

        if (!GridUI.HasFormValues(CurrentContext) | !ShowToolbar | !ConfigAction.IsVisible)
        {
            GridUI ui = null;
            if (MaintainValuesOnLoad && FormElement != null)
                ui = JJSession.GetSessionValue<GridUI>(string.Format("jjcurrentui_{0}", FormElement.Name));


            if (ui == null)
            {
                ui = CurrentUI;
                ui.ShowRowHover = o.ShowRowHover;
                ui.ShowRowStriped = o.ShowRowStriped;
                ui.ShowBorder = o.ShowBorder;
                ui.TotalPerPage = o.TotalPerPage;
                ui.TotalPaggingButton = o.TotalPaggingButton;
                ui.HeaderFixed = o.HeaderFixed;
            }



            CurrentUI = ui;
        }

        ShowHeaderWhenEmpty = o.ShowHeaderWhenEmpty;
        EmptyDataText = o.EmptyDataText;
    }

    internal BasicAction GetCurrentAction(ActionMap actionMap)
    {
        if (actionMap == null)
            return null;

        return actionMap.ContextAction switch
        {
            ActionOrigin.Form => null,//TODO: formAction
            ActionOrigin.Grid => GridActions.Find(x => x.Name.Equals(actionMap.ActionName)),
            ActionOrigin.Toolbar => ToolBarActions.Find(x => x.Name.Equals(actionMap.ActionName)),
            ActionOrigin.Field => FormElement.Fields[actionMap.FieldName].Actions.Get(actionMap.ActionName),
            _ => null,
        };
    }

    /// <summary>
    /// Retorna verdadeiro se o post é uma chamada para exportação
    /// </summary>
    public bool IsExportPost()
    {
        var actionMap = CurrentActionMap;
        var action = GetCurrentAction(actionMap);
        return action is ExportAction;
    }
}
