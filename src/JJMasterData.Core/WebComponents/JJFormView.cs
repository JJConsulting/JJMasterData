using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataDictionary.DictionaryDAL;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.FormEvents.Handlers;
using JJMasterData.Core.Html;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Linq;
using System.Text;

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
    private FormService _dataDictionaryManager;
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
    public JJDataPanel DataPanel
    {
        get
        {
            if (_dataPanel == null)
            {
                _dataPanel = new JJDataPanel(FormElement)
                {
                    Name = "jjpainel_" + Name,
                    DataAccess = DataAccess,
                    UserValues = UserValues,
                    RenderPanelGroup = true
                };
            }
            _dataPanel.PageState = PageState;

            return _dataPanel;
        }
    }


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

    private FormService DataDictionaryManager =>
        _dataDictionaryManager ??= new FormService(FormElement,
            LogAction.IsVisible ? LogHistory.Service : null);

    public DeleteSelectedRowsAction DeleteSelectedRowsAction
       => (DeleteSelectedRowsAction)ToolBarActions.Find(x => x is DeleteSelectedRowsAction);

    public InsertAction InsertAction => (InsertAction)ToolBarActions.Find(x => x is InsertAction);

    public EditAction EditAction => (EditAction)GridActions.Find(x => x is EditAction);

    public DeleteAction DeleteAction => (DeleteAction)GridActions.Find(x => x is DeleteAction);

    public ViewAction ViewAction => (ViewAction)GridActions.Find(x => x is ViewAction);

    public LogAction LogAction => (LogAction)ToolBarActions.Find(x => x is LogAction);

    #endregion

    #region "Constructors"

    internal JJFormView()
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
        FormFactory.SetFormViewParams(this, elementName);
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

    internal override HtmlBuilder RenderHtml()
    {
        string t = CurrentContext.Request.QueryString("t");
        string objname = CurrentContext.Request.QueryString("objname");
        var dataPanel = DataPanel;

        //Lookup Route
        if (JJLookup.IsLookupRoute(this))
            return new HtmlBuilder(dataPanel.GetHtml());

        //FormUpload Route
        if (JJTextFile.IsFormUploadRoute(this))
            return new HtmlBuilder(dataPanel.GetHtml());

        //DownloadFile Route
        if (JJDownloadFile.IsDownloadRoute(this))
            return JJDownloadFile.ResponseRoute(this);

        if ("jjsearchbox".Equals(t))
        {
            string pnlname = CurrentContext.Request.QueryString("pnlname");
            if (dataPanel.Name.Equals(pnlname))
            {
                dataPanel.GetHtml();
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

            string htmlPanel = GetHtmlDataPainel(values, null, PageState, true).ToString();
            CurrentContext.Response.SendResponse(htmlPanel);
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
            dataPanel.ResponseUrlAction();
            return null;
        }
        var htmlForm = GetHtmlForm();

        if ("ajax".Equals(t) && Name.Equals(objname))
        {
            CurrentContext.Response.SendResponse(htmlForm.ToString());
            return null;
        }

        return htmlForm;
    }

    private HtmlBuilder GetHtmlForm()
    {
        HtmlBuilder html;
        PageState pageState = PageState;

        var acMap = CurrentActionMap;
        var ac = GetCurrentAction(acMap);

        if (ac is EditAction || pageState == PageState.Update)
        {
            html = GetHtmlUpdate(ref pageState);
        }
        else if (ac is InsertAction || pageState == PageState.Insert)
        {
            html = GetHtmlInsert(ref pageState);
        }
        else if (ac is ImportAction || pageState == PageState.Import)
        {
            html = GetHtmlDataImp(ref pageState);
        }
        else if (ac is LogAction || pageState == PageState.Log)
        {
            html = GetHtmlLog(ref pageState);
        }
        else if (ac is DeleteAction)
        {
            html = GetHtmlDelete(ref pageState);
        }
        else if (ac is DeleteSelectedRowsAction)
        {
            html = GetHtmlDeleteSelectedRows(ref pageState);
        }
        else if (ac is ViewAction || pageState == PageState.View)
        {
            html = GetHtmlView(ref pageState);
        }
        else
        {
            html = GetHtmlGrid();
        }

        if (html != null)
        {
            html.AppendHiddenInput($"current_pagestate_{Name}", ((int)pageState).ToString());
            html.AppendHiddenInput($"current_formaction_{Name}", "");
        }

        return html;
    }

    private HtmlBuilder GetHtmlGrid()
    {
        //TODO: Use GetHtmlBuilder
        return base.RenderHtml();
    }

    private HtmlBuilder GetHtmlUpdate(ref PageState pageState)
    {
        string formAction = "";

        if (CurrentContext.Request["current_painelaction_" + Name] != null)
            formAction = CurrentContext.Request["current_painelaction_" + Name];

        if ("OK".Equals(formAction))
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

                pageState = PageState.List;
                return GetHtmlGrid();
            }
            else
            {
                pageState = PageState.Update;
                return GetHtmlDataPainel(values, errors, pageState, true);
            }
        }
        else if ("CANCEL".Equals(formAction))
        {
            ClearTempFiles();
            CurrentContext.Response.ResponseRedirect(CurrentContext.Request.AbsoluteUri);
            return null;
        }
        else if ("REFRESH".Equals(formAction))
        {
            var values = GetFormValues();
            return GetHtmlDataPainel(values, null, pageState, true);
        }
        else
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
            return GetHtmlDataPainel(values, null, pageState, autoReloadFields);
        }
    }

    private HtmlBuilder GetHtmlInsert(ref PageState pageState)
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
                    pageState = PageState.Insert;

                    var alert = new JJAlert();
                    alert.Name = $"pnl_insertmsg_{Name}";
                    alert.Messages.Add(Translate.Key("Record added successfully"));
                    alert.Color = PanelColor.Success;
                    alert.ShowIcon = true;
                    alert.Icon = IconType.CheckCircleO;

                    var alertHtml = alert.GetHtmlBuilder();
                    alertHtml.AppendElement(HtmlTag.Div, div =>
                    {
                        div.WithAttribute("id", $"pnl_insert_{Name}")
                           .WithAttribute("style", "display:none")
                           .AppendElement(GetHtmlDataPainel(RelationValues, null, PageState.Insert, false));
                    });
                    alertHtml.AppendScript($"jjview.showInsertSucess('{Name}');");
                    return alertHtml;
                }
                else
                {
                    pageState = PageState.List;
                    return GetHtmlGrid();
                }
            }
            else
            {
                pageState = PageState.Insert;
                return GetHtmlDataPainel(values, erros, pageState, true);
            }
        }
        else if (formAction.Equals("CANCEL"))
        {
            pageState = PageState.List;
            ClearTempFiles();
            return GetHtmlGrid();
        }
        else if (formAction.Equals("ELEMENTSEL"))
        {
            return GetHtmlElementInsert(ref pageState);
        }
        else if (formAction.Equals("ELEMENTLIST"))
        {
            pageState = PageState.Insert;
            return GetHtmlElementList(action);
        }
        else
        {
            if (pageState == PageState.Insert)
            {
                return GetHtmlDataPainel(GetFormValues(), null, pageState, true);
            }
            else
            {
                pageState = PageState.Insert;
                if (string.IsNullOrEmpty(action.ElementNameToSelect))
                    return GetHtmlDataPainel(RelationValues, null, PageState.Insert, false);
                else
                    return GetHtmlElementList(action);
            }
        }
    }

    private HtmlBuilder GetHtmlElementList(InsertAction action)
    {
        var sHtml = new HtmlBuilder(HtmlTag.Div);
        sHtml.AppendHiddenInput($"current_painelaction_{Name}", "ELEMENTLIST");
        sHtml.AppendHiddenInput($"current_selaction_{Name}", "");


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

        sHtml.AppendElement(formsel);

        return sHtml;
    }

    private HtmlBuilder GetHtmlElementInsert(ref PageState pageState)
    {
        string criptMap = CurrentContext.Request.Form("current_selaction_" + Name);
        string jsonMap = Cript.Descript64(criptMap);
        var map = JsonConvert.DeserializeObject<ActionMap>(jsonMap);

        var html = new HtmlBuilder(HtmlTag.Div);
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

            html.AppendElement(new JJMessageBox(sMsg.ToString(), MessageIcon.Warning));
            html.AppendElement(GetHtmlElementList(InsertAction));
            pageState = PageState.Insert;
        }
        else
        {
            pageState = PageState.Update;
            html.AppendElement(GetHtmlDataPainel(values, null, pageState, false));
        }

        return html;
    }

    private HtmlBuilder GetHtmlView(ref PageState pageState)
    {
        var acMap = CurrentActionMap;
        if (acMap == null)
        {
            pageState = PageState.List;
            return GetHtmlGrid();
        }
        else
        {
            pageState = PageState.View;
            var filter = acMap.PKFieldValues;
            var values = Factory.GetFields(FormElement, filter);
            return GetHtmlDataPainel(values, null, PageState.View, false);
        }
    }

    private HtmlBuilder GetHtmlDelete(ref PageState pageState)
    {
        var html = new HtmlBuilder(HtmlTag.Div);
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

                html.AppendElement(new JJMessageBox(errorMessage.ToString(), MessageIcon.Warning));
            }
            else
            {
                if (EnableMultSelect)
                    ClearSelectedGridValues();
            }
        }
        catch (Exception ex)
        {
            html.AppendElement(new JJMessageBox(ex.Message, MessageIcon.Error));
        }

        if (!string.IsNullOrEmpty(UrlRedirect))
        {
            CurrentContext.Response.ResponseRedirect(UrlRedirect);
            return null;
        }

        html.AppendElement(GetHtmlGrid());
        pageState = PageState.List;

        return html;
    }

    private HtmlBuilder GetHtmlDeleteSelectedRows(ref PageState pageState)
    {
        var html = new HtmlBuilder(HtmlTag.Div);
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

                html.AppendElement(new JJMessageBox(message.ToString(), icon));

                ClearSelectedGridValues();
            }
        }
        catch (Exception ex)
        {
            html.AppendElement(new JJMessageBox(ex.Message, MessageIcon.Error));
        }
        finally
        {
            html.AppendElement(GetHtmlGrid());
            pageState = PageState.List;
        }

        return html;
    }

    private HtmlBuilder GetHtmlLog(ref PageState pageState)
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
            var html = LogHistory.GetDetailLog(acMap.PKFieldValues);
            html.AppendElement(GetFormLogBottombar(acMap.PKFieldValues));
            pageState = PageState.Log;
            return html;
        }

        LogHistory.GridView.AddToolBarAction(goBackAction);
        LogHistory.DataPainel = DataPanel;
        pageState = PageState.Log;
        return LogHistory.GetHtmlBuilder();
    }

    private HtmlBuilder GetHtmlDataImp(ref PageState pageState)
    {
        var action = ImportAction;
        bool isVisible =
            ActionManager.Expression.GetBoolValue(action.VisibleExpression, action.Name, PageState.List,
                RelationValues);
        if (!isVisible)
            throw new UnauthorizedAccessException(Translate.Key("Import action not enabled"));

        var html = new HtmlBuilder(HtmlTag.Div);

        if (ShowTitle)
            html.AppendElement(GetTitle());

        pageState = PageState.Import;
        var sScriptImport = new StringBuilder();
        sScriptImport.Append($"$('#current_pagestate_{Name}').val('{(int)PageState.List}'); ");
        sScriptImport.AppendLine("$('form:first').submit(); ");

        var dataImpView = DataImp;
        dataImpView.UserValues = UserValues;
        dataImpView.BackButton.OnClientClick = sScriptImport.ToString();
        dataImpView.ProcessOptions = action.ProcessOptions;
        dataImpView.EnableHistoryLog = LogAction.IsVisible;
        html.AppendElement(dataImpView);

        return html;
    }

    private HtmlBuilder GetHtmlDataPainel(Hashtable values, Hashtable erros, PageState pageState, bool autoReloadFormFields)
    {
        var html = new HtmlBuilder(HtmlTag.Div);
        var relations = FormElement.Relations.FindAll(x => x.ViewType != RelationType.None);

        var painel = DataPanel;
        painel.PageState = pageState;
        painel.Erros = erros;
        painel.Values = values;
        painel.AutoReloadFormFields = autoReloadFormFields;

        if (ShowTitle)
            html.AppendElement(GetTitle());

        if (relations.Count == 0)
        {
            html.AppendElement(painel);

            if (erros != null)
                html.AppendElement(new JJValidationSummary(erros));

            html.AppendElement(GetFormBottombar(pageState, values));
            html.AppendHiddenInput($"current_painelaction_{Name}");
        }
        else
        {
            var sPainel = painel.GetHtmlBuilder();

            if (erros != null)
                sPainel.AppendElement(new JJValidationSummary(erros));

            sPainel.AppendElement(GetFormBottombar(pageState, values));
            sPainel.AppendHiddenInput($"current_painelaction_{Name}");

            var collapse = new JJCollapsePanel();
            collapse.Name = "collapse_" + Name;
            collapse.Title = FormElement.Title;
            collapse.ExpandedByDefault = true;
            collapse.HtmlBuilderContent = sPainel;
            html.AppendElement(collapse);
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
                    chieldView.UISettings = dic.UIOptions.Form;
                }

                html.AppendElement(chieldView);
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

                html.AppendElement(collapse);
            }
        }

        return html;
    }

    private JJToolbar GetFormLogBottombar(Hashtable values)
    {
        var backScript = new StringBuilder();
        backScript.Append($"$('#current_pagestate_{Name}').val('{(int)PageState.List}'); ");
        backScript.AppendLine("$('form:first').submit(); ");

        var btnBack = GetButtonBack();
        btnBack.OnClientClick = backScript.ToString();

        var btnHideLog = GetButtonHideLog(values);

        var toolbar = new JJToolbar();
        toolbar.CssClass = "pb-3 mt-3";
        toolbar.ListElement.Add(btnBack.GetHtmlBuilder());
        toolbar.ListElement.Add(btnHideLog.GetHtmlBuilder());
        return toolbar;
    }

    private JJToolbar GetFormBottombar(PageState pageState, Hashtable values)
    {
        var toolbar = new JJToolbar();
        toolbar.CssClass = "pb-3 mt-3";
        if (pageState == PageState.View)
        {
            toolbar.ListElement.Add(GetButtonBack().GetHtmlBuilder());

            if (LogAction.IsVisible)
                toolbar.ListElement.Add(GetButtonViewLog(values).GetHtmlBuilder());
        }
        else
        {
            toolbar.ListElement.Add(GetButtonOk().GetHtmlBuilder());
            toolbar.ListElement.Add(GetButtonCancel().GetHtmlBuilder());
        }
        return toolbar;
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
            () => ValidateFields(values, PageState.Update));

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
        FormFactory.SetFormptions(this, options);
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
    internal void InvokeOnInstanceCreated(JJFormView sender) =>
        OnInstanceCreated?.Invoke(sender);

    private JJLinkButton GetButtonOk()
    {
        var btn = new JJLinkButton();
        btn.Text = "Save";
        btn.IconClass = IconHelper.GetClassName(IconType.Check);
        btn.OnClientClick = $"return jjview.doPainelAction('{Name}','OK');";
        if (DataPanel.UISettings.EnterKey == FormEnterKey.Submit)
        {
            btn.Type = LinkButtonType.Submit;
            btn.CssClass = "btn btn-primary btn-small";
        }
        else
        {
            btn.Type = LinkButtonType.Button;
            btn.CssClass = BootstrapHelper.DefaultButton + " btn-small";
        }
        return btn;
    }

    private JJLinkButton GetButtonCancel()
    {
        var btn = new JJLinkButton();
        btn.Type = LinkButtonType.Button;
        btn.CssClass = $"{BootstrapHelper.DefaultButton} btn-small";
        btn.OnClientClick = $"jjview.doPainelAction('{Name}','CANCEL');";
        btn.IconClass = IconHelper.GetClassName(IconType.Times);
        btn.Text = "Cancel";
        return btn;
    }

    private JJLinkButton GetButtonBack()
    {
        var btn = GetButtonCancel();
        btn.IconClass = IconHelper.GetClassName(IconType.ArrowLeft);
        btn.Text = "Back";
        return btn;
    }

    private JJLinkButton GetButtonHideLog(Hashtable values)
    {
        string scriptAction = ActionManager.GetFormActionScript(ViewAction, values, ActionOrigin.Grid);
        var btn = new JJLinkButton();
        btn.Type = LinkButtonType.Button;
        btn.Text = "Hide Log";
        btn.IconClass = IconHelper.GetClassName(IconType.Film);
        btn.CssClass = "btn btn-primary btn-small";
        btn.OnClientClick = $"$('#current_pagestate_{Name}').val('{(int)PageState.List}');{scriptAction}";
        return btn;
    }

    private JJLinkButton GetButtonViewLog(Hashtable values)
    {
        string scriptAction = ActionManager.GetFormActionScript(LogAction, values, ActionOrigin.Toolbar);
        var btn = new JJLinkButton();
        btn.Type = LinkButtonType.Button;
        btn.Text = "View Log";
        btn.IconClass = IconHelper.GetClassName(IconType.Film);
        btn.CssClass = BootstrapHelper.DefaultButton + " btn-small";
        btn.OnClientClick = scriptAction;
        return btn;
    }

}