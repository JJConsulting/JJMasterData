using System;
using System.Collections;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataDictionary.DictionaryDAL;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.FormEvents.Handlers;
using Newtonsoft.Json;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Representa um GRUD.
/// Onde é possível: Listar, Visualizar, Incluir, Alterar e Excluir registros no banco de dados. 
/// </summary>
/// <example>
/// Exemplo da pagina
/// [!code-cshtml[Example](../../../doc/JJMasterData.Examples/Views/Example/JJFormView.cshtml)]
/// Exemplo objeto
/// [!code-cs[Example](../../../doc/JJMasterData.Sample/JJFormViewExample.aspx.cs)]
/// O Resultado html ficará parecido com esse:
/// <img src="../media/JJFormViewExample.png"/>
/// </example>
public class JJFormView : JJGridView
{
    #region "Events"

    /// <summary>
    /// Evento disparado antes de inserir o registro no banco de dados.
    /// </summary>
    public event EventHandler<FormBeforeActionEventArgs> OnBeforeInsert;

    /// <summary>
    /// Evento disparado antes de atualizar o registro no banco de dados.
    /// </summary>
    public event EventHandler<FormBeforeActionEventArgs> OnBeforeUpdate;

    /// <summary>
    /// Evento disparado antes de excluir o registro no banco de dados.
    /// </summary>
    public event EventHandler<FormBeforeActionEventArgs> OnBeforeDelete;

    /// <summary>
    /// Evento disparado após incluir o registro no banco de dados.
    /// </summary>
    public event EventHandler<FormAfterActionEventArgs> OnAfterInsert;

    /// <summary>
    /// Evento disparado após alterar o registro no banco de dados.
    /// </summary>
    public event EventHandler<FormAfterActionEventArgs> OnAfterUpdate;

    /// <summary>
    /// Evento disparado após excluir o registro no banco de dados.
    /// </summary>
    public event EventHandler<FormAfterActionEventArgs> OnAfterDelete;

    /// <summary>
    /// Evento disparado após instanciar o elemento do dicionário de dados.
    /// </summary>
    public event FormViewHandler OnInstanceCreated;

    #endregion

    #region "Properties"

    private JJDataPanel _dataPanel;
    private ActionMap _currentActionMap;
    private JJFormLog _logHistory;
    private DataDictionaryManager _dataDictionaryManager;
    private JJFormLog LogHistory =>
        _logHistory ??= new JJFormLog(FormElement)
        {
            DataAccess = DataAccess,
            Factory = Factory
        };

    /// <summary>
    /// Url a ser direcionada após os eventos de Update/Delete/Save
    /// </summary>
    internal string UrlRedirect { get; set; }

    /// <summary>
    /// Configurações de importação
    /// </summary>
    public new JJDataImp DataImp => base.DataImp;

    /// <summary>
    /// Configuração do painel com os campos do formulário
    /// </summary>
    public JJDataPanel DataPanel =>
        _dataPanel ??= new JJDataPanel(FormElement)
        {
            Name = "jjpainel_" + Name,
            DataAccess = DataAccess,
            UserValues = UserValues,
            RenderPanelGroup = true,
            PageState = PageState
        };


    /// <summary>
    /// Estado atual da pagina
    /// </summary>
    public PageState PageState
    {
        get
        {
            PageState pageState = PageState.List;
            if (CurrentContext.Request["current_pagestate_" + Name] != null)
                pageState = (PageState)int.Parse(CurrentContext.Request["current_pagestate_" + Name]);

            return pageState;
        }
    }

    private ActionMap CurrentActionMap
    {
        get
        {
            if (_currentActionMap != null) return _currentActionMap;

            string criptMap = CurrentContext.Request["current_formaction_" + Name];
            if (string.IsNullOrEmpty(criptMap))
                return null;

            string jsonMap = Cript.Descript64(criptMap);
            _currentActionMap = JsonConvert.DeserializeObject<ActionMap>(jsonMap);
            return _currentActionMap;
        }
    }

    private DataDictionaryManager DataDictionaryManager =>
        _dataDictionaryManager ??= new DataDictionaryManager(FormElement,
            LogAction.IsVisible ? LogHistory.Service : null);

    public InsertAction InsertAction => (InsertAction)ToolBarActions.Find(x => x is InsertAction);

    public EditAction EditAction => (EditAction)GridActions.Find(x => x is EditAction);

    public DeleteAction DeleteAction => (DeleteAction)GridActions.Find(x => x is DeleteAction);

    public DeleteSelectedRowsAction DeleteSelectedRowsAction
        => (DeleteSelectedRowsAction)ToolBarActions.Find(x => x is DeleteSelectedRowsAction);

    public ViewAction ViewAction => (ViewAction)GridActions.Find(x => x is ViewAction);

    public LogAction LogAction => (LogAction)ToolBarActions.Find(x => x is LogAction);

    #endregion

    #region "Constructors"

    private JJFormView()
    {
        ShowTitle = true;
        ToolBarActions.Add(new InsertAction());
        ToolBarActions.Add(new DeleteSelectedRowsAction());
        ToolBarActions.Add(new LogAction());
        GridActions.Add(new ViewAction());
        GridActions.Add(new EditAction());
        GridActions.Add(new DeleteAction());
    }

    public JJFormView(string elementName) : this()
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName), Translate.Key("Dictionary name cannot be empty"));

        Name = "jjview" + elementName.ToLower();
        var dicParser = GetDictionary(elementName);

        FormElement = dicParser.GetFormElement();

        SetOptions(dicParser.UIOptions);

        AddFormEvent(dicParser.Table.Name);

        OnInstanceCreated?.Invoke(this);
    }

    public JJFormView(FormElement formElement) : this()
    {
        FormElement = formElement ?? throw new ArgumentNullException(nameof(formElement));
    }

    public JJFormView(FormElement formElement, IDataAccess dataAccess) : this(formElement)
    {
        DataAccess = dataAccess;
    }

    #endregion

    protected override string RenderHtml()
    {
        string t = CurrentContext.Request.QueryString("t");
        string objname = CurrentContext.Request.QueryString("objname");
        var dataPainel = DataPanel;
        var sHtml = new StringBuilder();

        //Lookup Route
        if (JJLookup.IsLookupRoute(this))
            return dataPainel.GetHtml();

        //FormUpload Route
        if (JJTextFile.IsFormUploadRoute(this))
            return dataPainel.GetHtml();

        //DownloadFile Route
        if (JJDownloadFile.IsDownloadRoute(this))
            return JJDownloadFile.ResponseRoute(this);

        if ("jjsearchbox".Equals(t))
        {
            string pnlname = CurrentContext.Request.QueryString("pnlname");
            if (dataPainel.Name.Equals(pnlname))
            {
                dataPainel.GetHtml();
            }
            else if (Name.Equals(pnlname))
            {
                Filter.GetHtmlFilter();
            }
            else
            {
                return null;
            }
        }
        else if ("reloadpainel".Equals(t))
        {
            //TODO: eliminar metodo GetSelectedRowId
            Hashtable filter = GetSelectedRowId();
            Hashtable values = null;
            if (filter != null && filter.Count > 0)
                values = Factory.GetFields(FormElement, filter);

            sHtml.AppendLine(GetHtmlDataPainel(values, null, PageState, true));

            CurrentContext.Response.SendResponse(sHtml.ToString());
            return null;
        }
        else if ("jjupload".Equals(t) || "ajaxdataimp".Equals(t))
        {
            if (!DataImp.Upload.Name.Equals(objname))
                return null;

            //Ajax upload de arquivo
            var pageState = PageState;
            GetHtmlDataImp(ref pageState);
        }
        else if ("geturlaction".Equals(t))
        {
            dataPainel.ResponseUrlAction();
            return null;
        }

        sHtml.Append(GetHtmlForm());

        if ("ajax".Equals(t) && Name.Equals(objname))
        {
            CurrentContext.Response.SendResponse(sHtml.ToString());
            return null;
        }

        return sHtml.ToString();
    }

    private string GetHtmlForm()
    {
        var html = new StringBuilder();
        PageState pageState = PageState;

        var acMap = CurrentActionMap;
        var ac = GetCurrentAction(acMap);

        if (ac is EditAction || pageState == PageState.Update)
        {
            html.AppendLine(GetHtmlUpdate(ref pageState));
        }
        else if (ac is InsertAction || pageState == PageState.Insert)
        {
            html.AppendLine(GetHtmlInsert(ref pageState));
        }
        else if (ac is ImportAction || pageState == PageState.Import)
        {
            html.AppendLine(GetHtmlDataImp(ref pageState));
        }
        else if (ac is LogAction || pageState == PageState.Log)
        {
            html.AppendLine(GetHtmlLog(ref pageState));
        }
        else if (ac is DeleteAction)
        {
            html.AppendLine(GetHtmlDelete(ref pageState));
        }
        else if (ac is DeleteSelectedRowsAction)
        {
            html.AppendLine(GetHtmlDeleteSelectedRows(ref pageState));
        }
        else if (ac is ViewAction || pageState == PageState.View)
        {
            html.AppendLine(GetHtmlView(ref pageState));
        }
        else
        {
            html.AppendLine(GetHtmlGrid());
        }

        //Render PageState
        html.AppendLine(
            $"<input type=\"hidden\" id=\"current_pagestate_{Name}\" name=\"current_pagestate_{Name}\" value=\"{(int)pageState}\" /> ");
        html.AppendLine(
            $"<input type=\"hidden\" id=\"current_formaction_{Name}\" name=\"current_formaction_{Name}\" value=\"\" /> ");

        return html.ToString();
    }

    private string GetHtmlGrid()
    {
        return base.RenderHtml();
    }

    private string GetHtmlUpdate(ref PageState pageState)
    {
        var html = new StringBuilder();
        string formAction = "";
        
        if (CurrentContext.Request["current_painelaction_" + Name] != null)
            formAction = CurrentContext.Request["current_painelaction_" + Name];


        switch (formAction)
        {
            case "OK":
            {
                var values = GetFormValues();
                var errors = UpdateFormValues(values);

                if (errors.Count == 0)
                {
                    if (!string.IsNullOrEmpty(UrlRedirect))
                    {
                        CurrentContext.Response.ResponseRedirect(UrlRedirect);
                        return null;
                    }

                    html.AppendLine(GetHtmlGrid());
                    pageState = PageState.List;
                }
                else
                {
                    pageState = PageState.Update;
                    html.AppendLine(GetHtmlDataPainel(values, errors, pageState, true));
                }

                break;
            }
            case "CANCEL":
                ClearTempFiles();
                CurrentContext.Response.ResponseRedirect(CurrentContext.Request.AbsoluteUri);
                break;
            case "REFRESH":
            {
                var values = GetFormValues();
                html.AppendLine(GetHtmlDataPainel(values, null, pageState, true));
                break;
            }
            default:
            {
                bool autoReloadFields;
                Hashtable values;
                if (pageState == PageState.Update)
                {
                    autoReloadFields = true;
                    values = GetFormValues();
                }
                else
                {
                    autoReloadFields = false;
                    var acMap = CurrentActionMap;
                    values = Factory.GetFields(FormElement, acMap.PKFieldValues);
                }

                pageState = PageState.Update;
                html.AppendLine(GetHtmlDataPainel(values, null, pageState, autoReloadFields));
                break;
            }
        }

        return html.ToString();
    }

    private string GetHtmlInsert(ref PageState pageState)
    {
        var action = InsertAction;
        bool isVisible =
            ActionManager.Expression.GetBoolValue(action.VisibleExpression, action.Name, PageState.List,
                RelationValues);
        if (!isVisible)
            throw new UnauthorizedAccessException(Translate.Key("Insert action not enabled"));

        var html = new StringBuilder();
        string formAction = "";

        if (CurrentContext.Request["current_painelaction_" + Name] != null)
            formAction = CurrentContext.Request["current_painelaction_" + Name];

        if (formAction.Equals("OK"))
        {
            Hashtable values = GetFormValues();
            Hashtable erros = InsertFormValues(values);

            if (erros.Count == 0)
            {
                if (!string.IsNullOrEmpty(UrlRedirect))
                {
                    CurrentContext.Response.ResponseRedirect(UrlRedirect);
                    return null;
                }

                if (action.ReopenForm)
                {
                    var alert = new JJAlert();
                    alert.Id = $"pnl_insertmsg_{Name}";
                    alert.Message = "Record added successfully";
                    alert.Type = PanelColor.Success;
                    alert.Icon = IconType.CheckCircleO;

                    html.Append(alert.GetHtml());

                    html.AppendLine($"<div id=\"pnl_insert_{Name}\" style=\"display:none\">");
                    html.AppendLine(GetHtmlDataPainel(RelationValues, null, PageState.Insert, false));
                    html.AppendLine("</div>");

                    html.AppendLine("<script>");
                    html.AppendLine($"jjview.showInsertSucess('{Name}');");
                    html.AppendLine("</script>");

                    pageState = PageState.Insert;
                }
                else
                {
                    html.AppendLine(GetHtmlGrid());
                    pageState = PageState.List;
                }
            }
            else
            {
                pageState = PageState.Insert;
                html.AppendLine(GetHtmlDataPainel(values, erros, pageState, true));
            }
        }
        else if (formAction.Equals("CANCEL"))
        {
            ClearTempFiles();
            html.AppendLine(GetHtmlGrid());
            pageState = PageState.List;
        }
        else if (formAction.Equals("ELEMENTSEL"))
        {
            html.Append(GetHtmlElementInsert(ref pageState));
        }
        else if (formAction.Equals("ELEMENTLIST"))
        {
            html.AppendLine(GetHtmlElementList(action));
            pageState = PageState.Insert;
        }
        else
        {
            if (pageState == PageState.Insert)
            {
                html.AppendLine(GetHtmlDataPainel(GetFormValues(), null, pageState, true));
            }
            else
            {
                if (string.IsNullOrEmpty(action.ElementNameToSelect))
                    html.AppendLine(GetHtmlDataPainel(RelationValues, null, PageState.Insert, false));
                else
                    html.AppendLine(GetHtmlElementList(action));
            }

            pageState = PageState.Insert;
        }

        return html.ToString();
    }

    private string GetHtmlElementList(InsertAction action)
    {
        var sHtml = new StringBuilder();
        sHtml.AppendLine(
            $"<input type=\"hidden\" id=\"current_painelaction_{Name}\" name=\"current_painelaction_{Name}\" value=\"ELEMENTLIST\" /> ");
        sHtml.AppendLine(
            $"<input type=\"hidden\" id=\"current_selaction_{Name}\" name=\"current_selaction_{Name}\" value=\"\" /> ");

        var dicParser = GetDictionary(action.ElementNameToSelect);
        var formsel = new JJFormView(dicParser.GetFormElement(), DataAccess);
        formsel.Name = action.ElementNameToSelect;
        formsel.SetOptions(dicParser.UIOptions);

        var sGoBackScript = new StringBuilder();
        sGoBackScript.Append($"$('#current_pagestate_{Name}').val('{((int)PageState.List).ToString()}'); ");
        sGoBackScript.AppendLine("$('form:first').submit(); ");
        var goBackAction = new ScriptAction();
        goBackAction.Name = "_jjgobacktion";
        goBackAction.Icon = IconType.ArrowLeft;
        goBackAction.Text = "Back";
        goBackAction.ShowAsButton = true;
        goBackAction.OnClientClick = sGoBackScript.ToString();
        goBackAction.IsDefaultOption = true;
        formsel.AddToolBarAction(goBackAction);

        var selAction = new ScriptAction();
        selAction.Name = "_jjselaction";
        selAction.Icon = IconType.CaretRight;
        selAction.ToolTip = "Select";
        selAction.IsDefaultOption = true;
        formsel.AddGridAction(selAction);

        formsel.OnRenderAction += FormSelectedOnRenderAction;

        sHtml.AppendLine(formsel.GetHtml());

        return sHtml.ToString();
    }

    private string GetHtmlElementInsert(ref PageState pageState)
    {
        string criptMap = CurrentContext.Request.Form("current_selaction_" + Name);
        string jsonMap = Cript.Descript64(criptMap);
        var map = JsonConvert.DeserializeObject<ActionMap>(jsonMap);

        var sHtml = new StringBuilder();
        var selValues = Factory.GetFields(InsertAction.ElementNameToSelect, map.PKFieldValues);
        var formManager = new FormManager(FormElement, UserValues, DataAccess);
        var values = formManager.GetTriggerValues(selValues, PageState.Insert, true);
        var erros = InsertFormValues(values, false);

        if (erros.Count > 0)
        {
            var sMsg = new StringBuilder();
            foreach (string err in erros.Values)
            {
                sMsg.Append(" - ");
                sMsg.Append(err);
                sMsg.Append("<br>");
            }

            sHtml.Append(new JJMessageBox(sMsg.ToString(), MessageIcon.Warning, true).GetHtml());
            sHtml.AppendLine(GetHtmlElementList(InsertAction));
            pageState = PageState.Insert;
        }
        else
        {
            pageState = PageState.Update;
            sHtml.AppendLine(GetHtmlDataPainel(values, null, pageState, false));
        }

        return sHtml.ToString();
    }

    private string GetHtmlView(ref PageState pageState)
    {
        var html = new StringBuilder();
        var acMap = CurrentActionMap;
        if (acMap == null)
        {
            html.AppendLine(GetHtmlGrid());
            pageState = PageState.List;
        }
        else
        {
            var filter = acMap.PKFieldValues;
            var values = Factory.GetFields(FormElement, filter);
            html.AppendLine(GetHtmlDataPainel(values, null, PageState.View, false));
            pageState = PageState.View;
        }

        return html.ToString();
    }

    private string GetHtmlDelete(ref PageState pageState)
    {
        var html = new StringBuilder();
        try
        {
            var acMap = CurrentActionMap;
            var filter = acMap.PKFieldValues;

            var errors = DeleteFormValues(filter);

            if (errors != null && errors.Count > 0)
            {
                var errorMessage = new StringBuilder();
                foreach (DictionaryEntry err in errors)
                {
                    errorMessage.Append("- ");
                    errorMessage.Append(err.Value);
                    errorMessage.AppendLine("<br>");
                }

                html.AppendLine(new JJMessageBox(errorMessage.ToString(), MessageIcon.Warning, true).GetHtml());
            }
            else
            {
                if (EnableMultSelect)
                    ClearSelectedGridValues();
            }
        }
        catch (Exception ex)
        {
            html.AppendLine(new JJMessageBox(ex.Message, MessageIcon.Error, true).GetHtml());
        }

        if (!string.IsNullOrEmpty(UrlRedirect))
        {
            CurrentContext.Response.ResponseRedirect(UrlRedirect);
            return null;
        }

        html.AppendLine(GetHtmlGrid());
        pageState = PageState.List;

        return html.ToString();
    }

    private string GetHtmlDeleteSelectedRows(ref PageState pageState)
    {
        var html = new StringBuilder();
        var errorMessage = new StringBuilder();
        int errorCount = 0;
        int successCount = 0;

        try
        {
            var rows = GetSelectedGridValues();
            foreach (Hashtable row in rows)
            {
                Hashtable errors = DeleteFormValues(row);
                if (errors != null && errors.Count > 0)
                {
                    foreach (DictionaryEntry err in errors)
                    {
                        errorMessage.Append(" - ");
                        errorMessage.Append(err.Value);
                        errorMessage.Append("<br>");
                    }

                    errorCount++;
                }
                else
                {
                    successCount++;
                }
            }

            if (rows.Count > 0)
            {
                var message = new StringBuilder();
                MessageIcon icon = MessageIcon.Info;
                if (successCount > 0)
                {
                    message.Append("<p class=\"text-success\">");
                    message.Append(Translate.Key("{0} Record(s) deleted successfully", successCount));
                    message.Append("</p><br>");
                }

                if (errorCount > 0)
                {
                    message.Append("<p class=\"text-danger\">");
                    message.Append(Translate.Key("{0} Record(s) with error", successCount));
                    message.Append(Translate.Key("Details:"));
                    message.Append("<br>");
                    message.Append(errorMessage);
                    icon = MessageIcon.Warning;
                }

                html.AppendLine(new JJMessageBox(message.ToString(), icon, true).GetHtml());

                ClearSelectedGridValues();
            }
        }
        catch (Exception ex)
        {
            html.AppendLine(new JJMessageBox(ex.Message, MessageIcon.Error, true).GetHtml());
        }
        finally
        {
            html.AppendLine(GetHtmlGrid());
            pageState = PageState.List;
        }

        return html.ToString();
    }

    private string GetHtmlLog(ref PageState pageState)
    {
        var acMap = _currentActionMap;

        var sScript = new StringBuilder();
        sScript.Append($"$('#current_pagestate_{Name}').val('{(int)PageState.List}'); ");
        sScript.AppendLine("$('form:first').submit(); ");

        var goBackAction = new ScriptAction();
        goBackAction.Name = "goBackAction";
        goBackAction.Icon = IconType.Backward;
        goBackAction.ShowAsButton = true;
        goBackAction.Text = "Back";
        goBackAction.OnClientClick = sScript.ToString();

        if (pageState == PageState.View)
        {
            var sHtml = new StringBuilder();
            sHtml.AppendLine(LogHistory.GetDetailLog(acMap.PKFieldValues));
            sHtml.AppendLine(GetHtmlFormToolbar(acMap.PKFieldValues));
            pageState = PageState.Log;
            return sHtml.ToString();
        }

        LogHistory.GridView.AddToolBarAction(goBackAction);
        LogHistory.DataPainel = DataPanel;
        pageState = PageState.Log;
        return LogHistory.GetHtml();
    }

    private string GetHtmlDataImp(ref PageState pageState)
    {
        var action = ImportAction;
        bool isVisible =
            ActionManager.Expression.GetBoolValue(action.VisibleExpression, action.Name, PageState.List,
                RelationValues);
        if (!isVisible)
            throw new UnauthorizedAccessException(Translate.Key("Import action not enabled"));

        var sHtml = new StringBuilder();

        if (ShowTitle)
            sHtml.AppendLine(GetHtmlTitle());

        pageState = PageState.Import;
        var sScriptImport = new StringBuilder();
        sScriptImport.Append($"$('#current_pagestate_{Name}').val('{(int)PageState.List}'); ");
        sScriptImport.AppendLine("$('form:first').submit(); ");

        var dataImpView = DataImp;
        dataImpView.UserValues = UserValues;
        dataImpView.BackButton.OnClientClick = sScriptImport.ToString();
        dataImpView.ProcessOptions = action.ProcessOptions;
        dataImpView.EnableHistoryLog = LogAction.IsVisible;
        sHtml.AppendLine(dataImpView.GetHtml());

        return sHtml.ToString();
    }

    private string GetHtmlDataPainel(Hashtable values, Hashtable erros, PageState pageState, bool autoReloadFormFields)
    {
        var painel = DataPanel;
        painel.PageState = pageState;
        painel.Erros = erros;
        painel.Values = values;
        painel.AutoReloadFormFields = autoReloadFormFields;


        var relations = FormElement.Relations.FindAll(x => x.ViewType != RelationType.None);

        var sHtml = new StringBuilder();

        if (ShowTitle)
            sHtml.AppendLine(GetHtmlTitle());

        if (relations.Count == 0)
        {
            sHtml.AppendLine(painel.GetHtml());

            if (erros != null)
                sHtml.AppendLine(new JJValidationSummary(erros).GetHtml());

            sHtml.AppendLine(GetHtmlFormToolbarDefault(pageState, values));
        }
        else
        {
            var sPainel = new StringBuilder();
            sPainel.AppendLine(painel.GetHtml());


            if (erros != null)
                sPainel.AppendLine(new JJValidationSummary(erros).GetHtml());

            sPainel.AppendLine(GetHtmlFormToolbarDefault(pageState, values));

            var collapse = new JJCollapsePanel();
            collapse.Name = "collapse_" + Name;
            collapse.Title = FormElement.Title;
            collapse.ExpandedByDefault = true;
            collapse.HtmlContent = sPainel.ToString();

            sHtml.AppendLine(collapse.GetHtml());
        }


        var dicDao = new DictionaryDao(DataAccess);
        foreach (var relation in relations)
        {
            var dic = dicDao.GetDictionary(relation.ChildElement);
            var childElement = dic.GetFormElement();

            var filter = new Hashtable();
            foreach (var col in relation.Columns)
            {
                if (values.ContainsKey(col.PkColumn))
                {
                    filter.Add(col.FkColumn, values[col.PkColumn]);
                }
            }

            if (relation.ViewType == RelationType.View)
            {
                var childvalues = Factory.GetFields(childElement, filter);
                var chieldView = new JJDataPanel(childElement);
                chieldView.DataAccess = DataAccess;
                chieldView.PageState = PageState.View;
                chieldView.UserValues = UserValues;
                chieldView.Values = childvalues;
                chieldView.RenderPanelGroup = true;

                if (dic.UIOptions != null)
                {
                    chieldView.FormCols = dic.UIOptions.Form.FormCols;
                    chieldView.Layout = dic.UIOptions.Form.IsVerticalLayout
                        ? DataPanelLayout.Vertical
                        : DataPanelLayout.Horizontal;
                    chieldView.ShowViewModeAsStatic = dic.UIOptions.Form.ShowViewModeAsStatic;
                }

                sHtml.AppendLine(chieldView.GetHtml());
            }
            else if (relation.ViewType == RelationType.List)
            {
                var chieldGrid = new JJFormView(childElement);
                chieldGrid.DataAccess = DataAccess;
                chieldGrid.UserValues = UserValues;
                chieldGrid.FilterAction.ShowAsCollapse = false;
                chieldGrid.Name = "jjgridview_" + childElement.Name;
                chieldGrid.Filter.ApplyCurrentFilter(filter);

                if (dic.UIOptions != null)
                {
                    chieldGrid.SetOptions(dic.UIOptions);
                }

                chieldGrid.ShowTitle = false;

                var collapse = new JJCollapsePanel();
                collapse.Name = "collapse_" + chieldGrid.Name;
                collapse.Title = childElement.Title;
                collapse.HtmlContent = chieldGrid.GetHtml();

                sHtml.AppendLine(collapse.GetHtml());
            }
        }


        return sHtml.ToString();
    }

    private string GetHtmlFormToolbar(Hashtable values)
    {
        var backScript = new StringBuilder();
        backScript.Append($"$('#current_pagestate_{Name}').val('{(int)PageState.List}'); ");
        backScript.AppendLine("$('form:first').submit(); ");

        var html = new StringBuilder();
        html.AppendLine("");
        html.AppendLine("<!-- Start Toolbar -->");
        if (BootstrapHelper.Version > 3)
            html.AppendLine("<div class=\"container-fluid\"> ");
        html.AppendLine($"\t<div class=\"{BootstrapHelper.FormGroup}\"> ");
        html.AppendLine("\t\t<div class=\"row\"> ");
        html.AppendLine("\t\t\t<div class=\"col-sm-12\"> ");


        html.Append($"\t\t\t\t<button type=\"button\" class=\"{BootstrapHelper.DefaultButton} btn-small\" onclick=\"");
        html.AppendLine($"{backScript}\"> ");
        html.AppendLine("\t\t\t\t\t<span class=\"fa fa-arrow-left\"></span> ");
        html.Append("\t\t\t\t\t<span>&nbsp;");
        html.Append(Translate.Key("Back"));
        html.AppendLine("</span>");
        html.AppendLine("\t\t\t\t</button> ");

        string scriptAction = ActionManager.GetFormActionScript(ViewAction, values, ActionOrigin.Grid);

        html.Append("\t\t\t<button type=\"button\" class=\"btn btn-primary btn-small\" onclick=\"");
        html.AppendLine($"$('#current_pagestate_{Name}').val('{(int)PageState.List}');{scriptAction}\"> ");
        html.AppendLine("\t\t\t\t<span class=\"fa fa-film\"></span> ");
        html.Append("\t\t\t\t<span>&nbsp;");
        html.Append(Translate.Key("Hide Log"));
        html.AppendLine("</span>");
        html.AppendLine("\t\t\t</button> ");


        html.AppendLine("\t\t</div> ");
        html.AppendLine("\t</div> ");
        html.AppendLine("</div> ");
        if (BootstrapHelper.Version > 3)
            html.AppendLine("</div> ");
        html.AppendLine("");
        html.AppendLine("<!-- End Toolbar -->");

        return html.ToString();
    }

    private string GetHtmlFormToolbarDefault(PageState pageState, Hashtable values)
    {
        var html = new StringBuilder();
        html.AppendLine("");
        html.AppendLine("<!-- Start Toolbar -->");
        if (BootstrapHelper.Version > 3)
            html.AppendLine("<div class=\"container-fluid\"> ");
        html.AppendLine($"<div class=\"{BootstrapHelper.FormGroup}\"> ");
        html.AppendLine("\t<div class=\"row\"> ");
        html.AppendLine("\t\t<div class=\"col-sm-12\"> ");

        if (pageState == PageState.View)
        {
            html.Append(
                $"\t\t\t<button type=\"button\" class=\"{BootstrapHelper.DefaultButton} btn-small\" onclick=\"");
            html.AppendLine($"jjview.doPainelAction('{Name}','CANCEL');\"> ");
            html.AppendLine("\t\t\t\t<span class=\"fa fa-arrow-left\"></span> ");
            html.Append("\t\t\t\t<span>&nbsp;");
            html.Append(Translate.Key("Back"));
            html.AppendLine("</span>");
            html.AppendLine("\t\t\t</button> ");

            if (LogAction.IsVisible)
            {
                string scriptAction = ActionManager.GetFormActionScript(LogAction, values, ActionOrigin.Toolbar);

                html.Append(
                    $"\t\t\t<button type=\"button\" class=\"{BootstrapHelper.DefaultButton} btn-small\" onclick=\"");
                html.AppendLine($"{scriptAction}\"> ");
                html.AppendLine("\t\t\t\t<span class=\"fa fa-film\"></span> ");
                html.Append("\t\t\t\t<span>&nbsp;");
                html.Append(Translate.Key("View Log"));
                html.AppendLine("</span>");
                html.AppendLine("\t\t\t</button> ");
            }
        }
        else
        {
            string typeButton = "button";
            string classButton = BootstrapHelper.DefaultButton;
            if (DataPanel.EnterKey == FormEnterKey.Submit)
            {
                typeButton = "submit";
                classButton = "btn-primary";
            }

            html.AppendFormat("\t\t\t<button type=\"{0}\" class=\"btn {1} btn-small\" onclick=\"", typeButton,
                classButton);
            html.AppendLine($"return jjview.doPainelAction('{Name}','OK');\"> ");
            html.AppendLine("\t\t\t\t<span class=\"fa fa-check\"></span> ");
            html.Append("\t\t\t\t<span>&nbsp;");
            html.Append(Translate.Key("Save"));
            html.AppendLine("</span> ");
            html.AppendLine("\t\t\t</button> ");
            html.Append(
                $"\t\t\t<button type=\"button\" class=\"{BootstrapHelper.DefaultButton} btn-small\" onclick=\"");
            html.AppendLine($"jjview.doPainelAction('{Name}','CANCEL');\"> ");
            html.AppendLine("\t\t\t\t<span class=\"fa fa-times\"></span> ");
            html.Append("\t\t\t\t<span>&nbsp;");
            html.Append(Translate.Key("Cancel"));
            html.Append("</span> ");
            html.AppendLine("\t\t\t</button> ");
        }

        html.AppendLine("\t\t</div> ");
        html.AppendLine("\t</div> ");
        html.AppendLine("</div> ");
        if (BootstrapHelper.Version > 3)
            html.AppendLine("</div> ");
        html.AppendLine("");
        html.AppendLine(
            $"<input type=\"hidden\" id=\"current_painelaction_{Name}\" name=\"current_painelaction_{Name}\" value=\"\" /> ");
        html.AppendLine("<!-- End Toolbar -->");

        return html.ToString();
    }

    private void FormSelectedOnRenderAction(object sender, ActionEventArgs e)
    {
        if (!e.Action.Name.Equals("_jjselaction")) return;
        
        if (sender is JJGridView grid)
        {
            var map = new ActionMap(ActionOrigin.Grid, grid.FormElement, e.FieldValues, e.Action.Name);
            string criptId = map.GetCriptJson();
            e.LinkButton.OnClientClick = $"jjview.doSelElementInsert('{Name}','{criptId}');";
        }
    }


    /// <summary>
    /// Insert the records in the database.
    /// </summary>
    /// <returns>The list of errors.</returns>
    public Hashtable InsertFormValues(Hashtable values, bool validateFields = true)
    { 
        var result = DataDictionaryManager.Insert(this, values,
            () => validateFields ? ValidateFields(values, PageState.Insert) : null);
        
        UrlRedirect = result.UrlRedirect;
        
        return result.Errors;
    }

    /// <summary>
    /// Update the records in the database.
    /// </summary>
    /// <returns>The list of errors.</returns>
    public Hashtable UpdateFormValues(Hashtable values)
    {
        var result = DataDictionaryManager.Update(this, values,
            () => ValidateFields(values, PageState.Insert));
        
        UrlRedirect = result.UrlRedirect;
        
        return result.Errors;
    }

    /// <summary>
    /// Exclui o registro no banco de dados. 
    /// Retorna lista de erros
    /// </summary>
    /// <param name="filter">Valores chaves para o filtro</param>
    /// <returns>Retorna lista de erros</returns>
    public Hashtable DeleteFormValues(Hashtable filter)
    {
        var formManager = new FormManager(FormElement, UserValues, DataAccess);

        var values = formManager.ApplyDefaultValues(filter, PageState.Delete);

        var result = DataDictionaryManager.Delete(this, values);

        UrlRedirect = result.UrlRedirect;

        return result.Errors;
    }

    /// <summary>
    /// Recupera os dados do Form
    /// </summary>
    public Hashtable GetFormValues()
    {
        var painel = DataPanel;
        var values = painel.GetFormValues();

        if (RelationValues == null) return values;
        
        foreach (DictionaryEntry val in RelationValues)
        {
            if (values.ContainsKey(val.Key))
                values[val.Key] = val.Value;
            else
                values.Add(val.Key, val.Value);
        }

        return values;
    }

    /// <summary>
    /// Valida os campos do formulário e 
    /// retorna uma lista com erros encontrados
    /// </summary>
    /// <param name="values">Dados do Formulário</param>
    /// <param name="pageState">Estado atual da pagina</param>
    /// <returns>
    /// Chave = Nome do Campo
    /// Valor = Mensagem de erro
    /// </returns>
    public Hashtable ValidateFields(Hashtable values, PageState pageState)
    {
        var painel = DataPanel;
        painel.Values = values;

        Hashtable errors = painel.ValidateFields(values, pageState);
        return errors;
    }

    private void ClearTempFiles()
    {
        var uploadFields = FormElement.Fields.ToList().FindAll(x => x.Component == FormComponent.File);
        foreach (var field in uploadFields)
        {
            string sessionName = $"{field.Name}_formupload_jjfiles";
            if (CurrentContext?.Session[sessionName] != null)
                CurrentContext.Session[sessionName] = null;
        }
    }

    /// <summary>
    /// Atribui as configurações do usuário cadastrado no dicionário de dados
    /// </summary>
    /// <param name="options">
    /// Configurações do usuário
    /// </param>
    public void SetOptions(UIOptions options)
    {
        if (options == null) return;
        
        ToolBarActions = options.ToolBarActions.GetAll();
        GridActions = options.GridActions.GetAll();
        ShowTitle = options.Grid.ShowTitle;
        DataPanel.SetOptions(options.Form);
        SetGridOptions(options.Grid);
    }
    
    internal void InvokeOnBeforeUpdate(object sender, FormBeforeActionEventArgs eventArgs) =>
        OnBeforeUpdate?.Invoke(sender, eventArgs);
    internal void InvokeOnAfterUpdate(object sender, FormAfterActionEventArgs eventArgs) =>
        OnAfterUpdate?.Invoke(sender, eventArgs);
    internal void InvokeOnBeforeInsert(object sender, FormBeforeActionEventArgs eventArgs) =>
        OnBeforeInsert?.Invoke(sender, eventArgs);
    internal void InvokeOnAfterInsert(object sender, FormAfterActionEventArgs eventArgs) =>
        OnAfterInsert?.Invoke(sender, eventArgs);
    internal void InvokeOnBeforeDelete(object sender, FormBeforeActionEventArgs eventArgs) =>
        OnBeforeDelete?.Invoke(sender, eventArgs);
    internal void InvokeOnAfterDelete(object sender, FormAfterActionEventArgs eventArgs) =>
        OnAfterDelete?.Invoke(sender, eventArgs);
    
    private void AddFormEvent(string name)
    {
        var assemblyFormEvent = FormEventManager.GetFormEvent(name);
        if (assemblyFormEvent != null)
        {
            AddFormEvent(assemblyFormEvent);
        }
    }
    
    private void AddFormEvent(IFormEvent assemblyFormEvent)
    {
        foreach (var method in FormEventManager.GetFormEventMethods(assemblyFormEvent))
        {
            switch (method)
            {
                case "OnBeforeImport":
                    DataImp.OnBeforeImport += assemblyFormEvent.OnBeforeImport;
                    break;
                case "OnAfterImport":
                    DataImp.OnAfterImport += assemblyFormEvent.OnAfterImport;
                    break;
                case "OnInstanceCreated":
                    OnInstanceCreated += assemblyFormEvent.OnInstanceCreated;
                    break;
            }
        }
    }
}