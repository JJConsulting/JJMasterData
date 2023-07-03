using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataDictionary.Actions.FormToolbar;
using JJMasterData.Core.DataDictionary.Actions.GridTable;
using JJMasterData.Core.DataDictionary.Actions.GridToolbar;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DI;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.DI;
using JJMasterData.Core.DataManager.Services.Abstractions;

namespace JJMasterData.Core.Web.Components;

/// <summary>
/// Represents a CRUD.
/// </summary>
/// <example>
/// [!code-cshtml[Example](../../../example/JJMasterData.WebExample/Pages/Components/JJFormViewExample.cshtml)]
/// The GetHtml method will return something like this:
/// <img src="../media/JJFormViewExample.png"/>
/// </example>
public class JJFormView : JJBaseView
{
    #region "Events"

    public event EventHandler<FormBeforeActionEventArgs> OnBeforeInsert;
    public event EventHandler<FormBeforeActionEventArgs> OnBeforeUpdate;
    public event EventHandler<FormBeforeActionEventArgs> OnBeforeDelete;
    public event EventHandler<FormAfterActionEventArgs> OnAfterInsert;
    public event EventHandler<FormAfterActionEventArgs> OnAfterUpdate;
    public event EventHandler<FormAfterActionEventArgs> OnAfterDelete;

    #endregion

    #region "Properties"

    private JJDataPanel _dataPanel;
    private JJGridView _gridView;
    private ActionMap _currentActionMap;
    private JJFormLog _logHistory;
    private IFormService _service;


    internal JJFormLog FormLog =>
        _logHistory ??= new JJFormLog(FormElement, EntityRepository);


    /// <summary>
    /// Url a ser direcionada após os eventos de Update/Delete/Save
    /// </summary>
    internal string UrlRedirect { get; set; }

    /// <summary>
    /// Configurações de importação
    /// </summary>
    public JJDataImp DataImp
    {
        get
        {
            var dataImp = _gridView.DataImp;
            dataImp.OnAfterDelete += OnAfterDelete;
            dataImp.OnAfterInsert += OnAfterDelete;
            dataImp.OnAfterUpdate += OnAfterDelete;

            return dataImp;
        }
    }

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
                    Name = "jjpanel_" + FormElement.Name.ToLower(),
                    EntityRepository = EntityRepository,
                    UserValues = UserValues,
                    RenderPanelGroup = true,
                    IsExternalRoute = IsExternalRoute
                };
            }

            _dataPanel.PageState = PageState;

            return _dataPanel;
        }
    }

    /// <summary>
    /// Values to be replaced by relationship.
    /// If the field name exists in the relationship, the value will be replaced
    /// </summary>
    /// <remarks>
    /// Key = Field name, Value=Field value
    /// </remarks>
    public IDictionary<string, dynamic> RelationValues { get; set; }

    public FormElement FormElement { get; set; }

    public IEntityRepository EntityRepository { get; } = JJService.EntityRepository;

    public JJGridView GridView =>
        _gridView ??= new JJGridView
        {
            Name = "jjgridview_" + Name.ToLower(),
            UserValues = UserValues,
            IsExternalRoute = IsExternalRoute
        };

    /// <summary>
    /// Estado atual da pagina
    /// </summary>
    private PageState _pageState;

    public PageState PageState
    {
        get
        {
            if (CurrentContext.Request["current_pagestate_" + Name] != null)
                _pageState = (PageState)int.Parse(CurrentContext.Request["current_pagestate_" + Name]);

            return _pageState;
        }
        internal init => _pageState = value;
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


    private IFormService Service
    {
        get
        {
            if (_service == null)
            {
                _service = JJService.Provider.GetScopedDependentService<IFormService>();
                _service.EnableErrorLink = true;
                _service.EnableHistoryLog = LogAction.IsVisible;

                _service.OnBeforeInsert += OnBeforeInsert;
                _service.OnBeforeUpdate += OnBeforeUpdate;
                _service.OnBeforeDelete += OnBeforeDelete;

                _service.OnAfterDelete += OnAfterDelete;
                _service.OnAfterUpdate += OnAfterUpdate;
                _service.OnAfterInsert += OnAfterInsert;
            }

            return _service;
        }
    }

    public DeleteSelectedRowsAction DeleteSelectedRowsAction
        => (DeleteSelectedRowsAction)GridView.ToolBarActions.Find(x => x is DeleteSelectedRowsAction);

    public InsertAction InsertAction => (InsertAction)GridView.ToolBarActions.Find(x => x is InsertAction);

    public EditAction EditAction => (EditAction)GridView.GridActions.Find(x => x is EditAction);

    public DeleteAction DeleteAction => (DeleteAction)GridView.GridActions.Find(x => x is DeleteAction);

    public ViewAction ViewAction => (ViewAction)GridView.GridActions.Find(x => x is ViewAction);

    public LogAction LogAction => (LogAction)GridView.ToolBarActions.Find(x => x is LogAction);
    public IDataDictionaryRepository DataDictionaryRepository { get; }

    internal IExpressionsService ExpressionsService { get; } =
        JJService.Provider.GetScopedDependentService<IExpressionsService>();

    internal IFormFieldsService FormFieldsService { get; } =
        JJService.Provider.GetScopedDependentService<IFormFieldsService>();
    
    internal JJMasterDataEncryptionService EncryptionService { get; } =
        JJService.Provider.GetScopedDependentService<JJMasterDataEncryptionService>();

    public bool ShowTitle { get; set; }

    internal bool IsModal { get; init; }

    #endregion

    #region "Constructors"

    internal JJFormView()
    {
        Name = "jjview";
        DataDictionaryRepository = JJServiceCore.DataDictionaryRepository;
        
        GridView.ShowTitle = true;
        GridView.ToolBarActions.Add(new InsertAction());
        GridView.ToolBarActions.Add(new DeleteSelectedRowsAction());
        GridView.ToolBarActions.Add(new LogAction());
        GridView.GridActions.Add(new ViewAction());
        GridView.GridActions.Add(new EditAction());
        GridView.GridActions.Add(new DeleteAction());
    }

    public JJFormView(string elementName) : this()
    {
        FormFactory.SetFormViewParams(this, elementName);
        IsExternalRoute = true;
    }

    public JJFormView(FormElement formElement) : this()
    {
        FormElement = formElement ?? throw new ArgumentNullException(nameof(formElement));
        GridViewFactory.SetGridViewParams(GridView,  FormElement);
        Name = "jjview" + formElement.Name.ToLower();
    }

    #endregion

    internal override HtmlBuilder RenderHtml()
    {
        var requestType = CurrentContext.Request.QueryString("t");
        var objName = CurrentContext.Request.QueryString("objname");
        var dataPanel = DataPanel;

        if (JJLookup.IsLookupRoute(this))
            return dataPanel.RenderHtml();

        if (JJTextFile.IsFormUploadRoute(this))
            return dataPanel.RenderHtml();

        if (JJDownloadFile.IsDownloadRoute(this))
            return JJDownloadFile.ResponseRoute(this);

        if (JJSearchBox.IsSearchBoxRoute(this))
            return JJSearchBox.ResponseJson(DataPanel);
        
        if ("reloadpainel".Equals(requestType))
        {
            //TODO: eliminar metodo GetSelectedRowId
            var filter = GridView.GetSelectedRowId();
            IDictionary<string, dynamic> values = null;
            if (filter is { Count: > 0 })
                values = EntityRepository.GetDictionaryAsync(FormElement, filter).GetAwaiter().GetResult();

            string htmlPanel = GetDataPanelHtml(new(values, null, PageState), true).ToString();
            CurrentContext.Response.SendResponse(htmlPanel);
            return null;
        }

        if ("jjupload".Equals(requestType) || "ajaxdataimp".Equals(requestType))
        {
            if (!DataImp.Upload.Name.Equals(objName))
                return null;

            //Ajax upload de arquivo
            var pageState = PageState;
            GetHtmlDataImp(ref pageState);
        }
        else if ("geturlaction".Equals(requestType))
        {
            dataPanel.ResponseUrlAction();
            return null;
        }

        var htmlForm = GetHtmlForm();

        if ("ajax".Equals(requestType) && Name.Equals(objName))
        {
            CurrentContext.Response.SendResponse(htmlForm.ToString());
            return null;
        }

        return htmlForm;
    }

    private HtmlBuilder GetHtmlForm()
    {
        HtmlBuilder html;
        var pageState = PageState;

        var actionMap = CurrentActionMap;
        var currentAction = GridView.GetCurrentAction(actionMap);

        if (currentAction is EditAction || pageState == PageState.Update)
        {
            html = GetHtmlUpdate(ref pageState);
        }
        else if (currentAction is InsertAction || pageState == PageState.Insert)
        {
            html = GetHtmlInsert(ref pageState);
        }
        else if (currentAction is ImportAction || pageState == PageState.Import)
        {
            html = GetHtmlDataImp(ref pageState);
        }
        else if (currentAction is LogAction || pageState == PageState.Log)
        {
            html = GetHtmlLog(ref pageState);
        }
        else if (currentAction is DeleteAction)
        {
            html = GetHtmlDelete(ref pageState);
        }
        else if (currentAction is DeleteSelectedRowsAction)
        {
            html = GetHtmlDeleteSelectedRows(ref pageState);
        }
        else if (currentAction is ViewAction || pageState == PageState.View)
        {
            html = GetHtmlView(ref pageState);
        }
        else
        {
            html = GetGridHtml();
        }

        if (html != null)
        {
            html.AppendHiddenInput($"current_pagestate_{Name}", ((int)pageState).ToString());
            html.AppendHiddenInput($"current_formaction_{Name}", "");
        }

        return html;
    }

    private HtmlBuilder GetGridHtml()
    {
        return GridView.RenderHtml();
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
                    CurrentContext.Response.Redirect(UrlRedirect);
                    return null;
                }

                pageState = PageState.List;
                return GetGridHtml();
            }

            pageState = PageState.Update;
            return GetDataPanelHtml(new(values, errors, pageState), true);
        }

        if ("CANCEL".Equals(formAction))
        {
            pageState = PageState.List;
            return GetGridHtml();
        }

        if ("REFRESH".Equals(formAction))
        {
            var values = GetFormValues();
            return GetDataPanelHtml(new(values, null, pageState), true);
        }
        else
        {
            bool autoReloadFields;
            IDictionary<string, dynamic> values;
            if (pageState == PageState.Update)
            {
                autoReloadFields = true;
                values = GetFormValues();
            }
            else
            {
                autoReloadFields = false;
                var acMap = CurrentActionMap;
                values = EntityRepository.GetDictionaryAsync(FormElement, acMap.PkFieldValues).GetAwaiter().GetResult();
            }

            pageState = PageState.Update;
            return GetDataPanelHtml(new(values, null, pageState), autoReloadFields);
        }
    }

    private HtmlBuilder GetHtmlInsert(ref PageState pageState)
    {
        var action = InsertAction;
        bool isVisible = ExpressionsService.GetBoolValue(action.VisibleExpression, action.Name, PageState.List,
            RelationValues);
        if (!isVisible)
            throw new UnauthorizedAccessException(Translate.Key("Insert action not enabled"));

        string formAction = "";

        if (CurrentContext.Request["current_painelaction_" + Name] != null)
            formAction = CurrentContext.Request["current_painelaction_" + Name];

        if (formAction.Equals("OK"))
        {
            var values = GetFormValues();
            var errors = InsertFormValues(values);

            if (errors.Count == 0)
            {
                if (!string.IsNullOrEmpty(UrlRedirect))
                {
                    CurrentContext.Response.Redirect(UrlRedirect);
                    return null;
                }

                if (action.ReopenForm)
                {
                    pageState = PageState.Insert;

                    var alert = new JJAlert
                    {
                        Name = $"pnl_insertmsg_{Name}",
                        Color = PanelColor.Success,
                        ShowIcon = true,
                        Icon = IconType.CheckCircleO
                    };
                    alert.Messages.Add(Translate.Key("Record added successfully"));
                    var alertHtml = alert.GetHtmlBuilder();
                    alertHtml.AppendElement(HtmlTag.Div, div =>
                    {
                        div.WithAttribute("id", $"pnl_insert_{Name}")
                            .WithAttribute("style", "display:none")
                            .AppendElement(GetDataPanelHtml(new(RelationValues, null, PageState.Insert), false));
                    });
                    alertHtml.AppendScript($"jjview.showInsertSucess('{Name}');");
                    return alertHtml;
                }

                pageState = PageState.List;
                return GetGridHtml();
            }

            pageState = PageState.Insert;
            return GetDataPanelHtml(new(values, errors, pageState), true);
        }

        if (formAction.Equals("CANCEL"))
        {
            pageState = PageState.List;
            ClearTempFiles();
            return GetGridHtml();
        }

        if (formAction.Equals("ELEMENTSEL"))
        {
            return GetHtmlElementInsert(ref pageState);
        }

        if (formAction.Equals("ELEMENTLIST"))
        {
            pageState = PageState.Insert;
            return GetHtmlElementList(action);
        }

        if (pageState == PageState.Insert)
        {
            return GetDataPanelHtml(new(GetFormValues(), null, pageState), true);
        }

        pageState = PageState.Insert;

        if (string.IsNullOrEmpty(action.ElementNameToSelect))
            return GetDataPanelHtml(new(RelationValues, null, PageState.Insert), false);
        return GetHtmlElementList(action);
    }

    private HtmlBuilder GetHtmlElementList(InsertAction action)
    {
        var sHtml = new HtmlBuilder(HtmlTag.Div);
        sHtml.AppendHiddenInput($"current_painelaction_{Name}", "ELEMENTLIST");
        sHtml.AppendHiddenInput($"current_selaction_{Name}", "");

        var formElement = DataDictionaryRepository.GetMetadata(action.ElementNameToSelect);
        var selectedForm = new JJFormView(formElement)
        {
            UserValues = UserValues,
            Name = action.ElementNameToSelect
        };
        selectedForm.SetOptions(formElement.Options);

        var goBackScript = new StringBuilder();
        goBackScript.Append($"$('#current_pagestate_{Name}').val('{((int)PageState.List).ToString()}'); ");
        goBackScript.AppendLine("$('form:first').submit(); ");

        var goBackAction = new ScriptAction
        {
            Name = "_jjgobacktion",
            Icon = IconType.ArrowLeft,
            Text = "Back",
            ShowAsButton = true,
            OnClientClick = goBackScript.ToString(),
            IsDefaultOption = true
        };
        selectedForm.AddToolBarAction(goBackAction);

        var selAction = new ScriptAction
        {
            Name = "_jjselaction",
            Icon = IconType.CaretRight,
            ToolTip = "Select",
            IsDefaultOption = true
        };
        selectedForm.AddGridAction(selAction);

        sHtml.AppendElement(selectedForm);

        return sHtml;
    }

    private HtmlBuilder GetHtmlElementInsert(ref PageState pageState)
    {
        string criptMap = CurrentContext.Request.Form("current_selaction_" + Name);
        string jsonMap = Cript.Descript64(criptMap);
        var map = JsonConvert.DeserializeObject<ActionMap>(jsonMap);
        var html = new HtmlBuilder(HtmlTag.Div);
        var dicRepository = JJServiceCore.DataDictionaryRepository;
        var formElement = dicRepository.GetMetadata(InsertAction.ElementNameToSelect);
        var selValues = EntityRepository.GetDictionaryAsync(formElement, map.PkFieldValues).GetAwaiter().GetResult();
        var values = FormFieldsService.MergeWithExpressionValues(formElement, selValues, PageState.Insert, true);
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
            html.AppendElement(GetDataPanelHtml(new(values, null, pageState), false));
        }

        return html;
    }

    private HtmlBuilder GetHtmlView(ref PageState pageState)
    {
        var acMap = CurrentActionMap;
        if (acMap == null)
        {
            pageState = PageState.List;
            return GetGridHtml();
        }

        pageState = PageState.View;
        var filter = acMap.PkFieldValues;
        var values = EntityRepository.GetDictionaryAsync(FormElement, filter).GetAwaiter().GetResult();
        return GetDataPanelHtml(new(values, null, pageState), false);
    }

    private HtmlBuilder GetHtmlDelete(ref PageState pageState)
    {
        var html = new HtmlBuilder(HtmlTag.Div);
        try
        {
            var acMap = CurrentActionMap;
            var filter = acMap.PkFieldValues;

            var errors = DeleteFormValues(filter);

            if (errors != null && errors.Count > 0)
            {
                var errorMessage = new StringBuilder();
                foreach (var err in errors)
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
            CurrentContext.Response.Redirect(UrlRedirect);
            return null;
        }

        html.AppendElement(GetGridHtml());
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
            foreach (var errors in rows.Select(DeleteFormValues))
            {
                if (errors is { Count: > 0 })
                {
                    foreach (var err in errors)
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
            html.AppendElement(GetGridHtml());
            pageState = PageState.List;
        }

        return html;
    }


    private HtmlBuilder GetHtmlLog(ref PageState pageState)
    {
        var actionMap = _currentActionMap;
        var script = new StringBuilder();
        script.Append($"$('#current_pagestate_{Name}').val('{(int)PageState.List}'); ");
        script.AppendLine("$('form:first').submit(); ");

        var goBackAction = new ScriptAction
        {
            Name = "goBackAction",
            Icon = IconType.Backward,
            ShowAsButton = true,
            Text = "Back",
            OnClientClick = script.ToString()
        };

        if (pageState == PageState.View)
        {
            var html = FormLog.GetDetailLog(actionMap.PkFieldValues);
            html.AppendElement(GetFormLogBottomBar(actionMap.PkFieldValues));
            pageState = PageState.Log;
            return html;
        }

        FormLog.GridView.AddToolBarAction(goBackAction);
        FormLog.DataPainel = DataPanel;
        pageState = PageState.Log;
        return FormLog.GetHtmlBuilder();
    }

    private HtmlBuilder GetHtmlDataImp(ref PageState pageState)
    {
        var action = GridView.ImportAction;
        bool isVisible = ExpressionsService.GetBoolValue(action.VisibleExpression, action.Name, PageState.List,
            RelationValues);
        if (!isVisible)
            throw new UnauthorizedAccessException(Translate.Key("Import action not enabled"));

        var html = new HtmlBuilder(HtmlTag.Div);

        if (ShowTitle)
            html.AppendElement(GridView.GetTitle(UserValues));

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

    private HtmlBuilder GetDataPanelHtml(FormContext formContext, bool autoReloadFormFields)
    {
        var html = new HtmlBuilder(HtmlTag.Div);
        var relationships = FormElement.Relationships.Where(r => r.ViewType != RelationshipViewType.None || r.IsParent)
            .ToList();

        var (values, errors, pageState) = formContext;

        var parentPanel = DataPanel;
        parentPanel.PageState = pageState;
        parentPanel.Errors = errors;
        parentPanel.Values = values;
        parentPanel.IsExternalRoute = IsExternalRoute;
        parentPanel.AutoReloadFormFields = autoReloadFormFields;

        if (ShowTitle)
            html.AppendElement(GridView.GetTitle(values));

        if (relationships.Count == 0)
        {
            return GetParentPanelHtml(parentPanel);
        }

        var layout = new FormViewRelationshipLayout(this);

        var topActions = FormElement.Options.FormToolbarActions
            .GetAllSorted()
            .Where(a => a.FormToolbarActionLocation is FormToolbarActionLocation.Top).ToList();

        html.AppendElement(GetFormToolbar(topActions, parentPanel.PageState, parentPanel.Values));

        var layoutHtml = layout.GetRelationshipsHtml(parentPanel, relationships);

        html.AppendRange(layoutHtml);

        var bottomActions = FormElement.Options.FormToolbarActions
            .GetAllSorted()
            .Where(a => a.FormToolbarActionLocation is FormToolbarActionLocation.Bottom).ToList();

        html.AppendElement(GetFormToolbar(bottomActions, parentPanel.PageState, parentPanel.Values));

        return html;
    }

    internal HtmlBuilder GetParentPanelHtml(JJDataPanel parentPanel)
    {
        var parentPanelHtml = parentPanel.GetHtmlBuilder();

        if (parentPanel.Errors != null)
            parentPanelHtml.AppendElement(new JJValidationSummary(parentPanel.Errors));

        var panelActions = parentPanel.FormElement.Options.FormToolbarActions
            .Where(a => a.FormToolbarActionLocation == FormToolbarActionLocation.Panel).ToList();

        parentPanelHtml.AppendElement(GetFormToolbar(panelActions, parentPanel.PageState, parentPanel.Values));
        parentPanelHtml.AppendHiddenInput($"current_painelaction_{Name}");

        return parentPanelHtml;
    }

    private JJToolbar GetFormLogBottomBar(IDictionary<string, dynamic> values)
    {
        var backScript = new StringBuilder();
        backScript.Append($"$('#current_pagestate_{Name}').val('{(int)PageState.List}'); ");
        backScript.AppendLine("$('form:first').submit(); ");

        var btnBack = GetBackButton();
        btnBack.OnClientClick = backScript.ToString();

        var btnHideLog = GetButtonHideLog(values);

        var toolbar = new JJToolbar
        {
            CssClass = "pb-3 mt-3"
        };
        toolbar.Items.Add(btnBack.GetHtmlBuilder());
        toolbar.Items.Add(btnHideLog.GetHtmlBuilder());
        return toolbar;
    }

    private JJToolbar GetFormToolbar(IList<BasicAction> actions, PageState pageState,
        IDictionary<string, dynamic> values)
    {
        var toolbar = new JJToolbar
        {
            CssClass = "pb-3 mt-3"
        };

        var context = new ActionContext(values, pageState, ActionSource.FormToolbar, null);

        foreach (var action in actions.Where(a => !a.IsGroup))
        {
            if (action is SaveAction saveAction)
            {
                saveAction.EnterKeyBehavior = DataPanel.FormUI.EnterKey;
            }

            toolbar.Items.Add(GridView.ActionManager.GetLinkFormToolbar(action, values, pageState).GetHtmlBuilder());
        }

        if (actions.Any(a => a.IsGroup))
        {
            toolbar.Items.Add(
                GridView.ActionManager.GetGroupedActionsHtml(actions.Where(a => a.IsGroup).ToList(), context));
        }


        if (pageState == PageState.View)
        {
            if (LogAction.IsVisible)
                toolbar.Items.Add(GetButtonViewLog(values).GetHtmlBuilder());
        }

        return toolbar;
    }

    private void FormSelectedOnRenderAction(object sender, ActionEventArgs e)
    {
        if (!e.Action.Name.Equals("_jjselaction")) return;

        if (sender is not JJGridView grid) return;

        var map = new ActionMap(ActionSource.GridTable, grid.FormElement, e.FieldValues, e.Action.Name);
        string criptId = map.GetCriptJson();
        e.LinkButton.OnClientClick = $"jjview.doSelElementInsert('{Name}','{criptId}');";
    }

    /// <summary>
    /// Insert the records in the database.
    /// </summary>
    /// <returns>The list of errors.</returns>
    public IDictionary<string, dynamic> InsertFormValues(IDictionary<string, dynamic> values,
        bool validateFields = true)
    {
        var result = Service.Insert(FormElement, values, new DataContext(DataContextSource.Form, UserId),
            validateFields);
        UrlRedirect = result.UrlRedirect;
        return result.Errors;
    }

    /// <summary>
    /// Update the records in the database.
    /// </summary>
    /// <returns>The list of errors.</returns>
    public IDictionary<string, dynamic> UpdateFormValues(IDictionary<string, dynamic> values)
    {
        var result = Service.Update(FormElement, values, new DataContext(DataContextSource.Form, UserId));
        UrlRedirect = result.UrlRedirect;
        return result.Errors;
    }

    public IDictionary<string, dynamic> DeleteFormValues(IDictionary<string, dynamic> filter)
    {
        var values = FormFieldsService.MergeWithExpressionValues(FormElement, filter, PageState.Delete, true);
        var result = Service.Delete(FormElement, values, new DataContext(DataContextSource.Form, UserId));
        UrlRedirect = result.UrlRedirect;
        return result.Errors;
    }


    [Obsolete("Must be async")]
    public IDictionary<string, dynamic> GetFormValues()
    {
        var painel = DataPanel;
        var values = painel.GetFormValues().GetAwaiter().GetResult();

        if (RelationValues == null)
            return values;

        DataHelper.CopyIntoDictionary(ref values, RelationValues, true);

        return values;
    }

    public IDictionary<string, dynamic> ValidateFields(IDictionary<string, dynamic> values, PageState pageState)
    {
        var painel = DataPanel;
        painel.Values = values;

        var errors = painel.ValidateFields(values, pageState);
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

    public void SetOptions(FormElementOptions options)
    {
        FormFactory.SetFormOptions(this, options);
    }

    private JJLinkButton GetBackButton()
    {
        var btn = new JJLinkButton
        {
            Type = LinkButtonType.Button,
            CssClass = $"{BootstrapHelper.DefaultButton} btn-small",
            OnClientClick = $"jjview.doPainelAction('{Name}','CANCEL');",
            IconClass = IconType.Times.GetCssClass(),
            Text = "Cancel"
        };
        btn.IconClass = IconType.ArrowLeft.GetCssClass();
        btn.Text = "Back";
        return btn;
    }

    private JJLinkButton GetButtonHideLog(IDictionary<string, dynamic> values)
    {
        string scriptAction = GridView.ActionManager.GetFormActionScript(ViewAction, values, ActionSource.GridTable);
        var btn = new JJLinkButton
        {
            Type = LinkButtonType.Button,
            Text = "Hide Log",
            IconClass = IconType.Film.GetCssClass(),
            CssClass = "btn btn-primary btn-small",
            OnClientClick = $"$('#current_pagestate_{Name}').val('{(int)PageState.List}');{scriptAction}"
        };
        return btn;
    }

    private JJLinkButton GetButtonViewLog(IDictionary<string, dynamic> values)
    {
        string scriptAction = GridView.ActionManager.GetFormActionScript(LogAction, values, ActionSource.GridToolbar);
        var btn = new JJLinkButton
        {
            Type = LinkButtonType.Button,
            Text = "View Log",
            IconClass = IconType.Film.GetCssClass(),
            CssClass = BootstrapHelper.DefaultButton + " btn-small",
            OnClientClick = scriptAction
        };
        return btn;
    }

    #region "Legacy GridView inherited compatibility"

    [Obsolete("Please use GridView.GridActions")]
    public List<BasicAction> GridActions
    {
        get => GridView.GridActions;
        internal set => GridView.GridActions = value;
    }

    [Obsolete("Please use GridView.ToolBarActions")]
    public List<BasicAction> ToolBarActions
    {
        get => GridView.ToolBarActions;
        internal set => GridView.ToolBarActions = value;
    }

    [Obsolete("Please use GridView.SetCurrentFilter")]
    public void SetCurrentFilter(string userid, string userId)
    {
        GridView.SetCurrentFilter(UserId, UserId);
    }

    [Obsolete("Please use GridView.ImportAction")]
    public ImportAction ImportAction => GridView.ImportAction;


    [Obsolete("Please use GridView.FilterAction")]
    public FilterAction FilterAction { get; set; }

    [Obsolete("Please use GridView.GetSelectedGridValues")]
    private List<IDictionary<string, dynamic>> GetSelectedGridValues() => GridView.GetSelectedGridValues();

    [Obsolete("Please use GridView.AddToolBarAction")]
    private void AddToolBarAction(UserCreatedAction userCreatedAction)
    {
        switch (userCreatedAction)
        {
            case UrlRedirectAction urlRedirectAction:
                GridView.AddToolBarAction(urlRedirectAction);
                break;
            case SqlCommandAction sqlCommandAction:
                GridView.AddToolBarAction(sqlCommandAction);
                break;
            case ScriptAction scriptAction:
                GridView.AddToolBarAction(scriptAction);
                break;
            case InternalAction internalAction:
                GridView.AddToolBarAction(internalAction);
                break;
        }
    }

    [Obsolete("Please use GridView.AddGridAction")]
    private void AddGridAction(UserCreatedAction userCreatedAction)
    {
        switch (userCreatedAction)
        {
            case UrlRedirectAction urlRedirectAction:
                GridView.AddGridAction(urlRedirectAction);
                break;
            case SqlCommandAction sqlCommandAction:
                GridView.AddGridAction(sqlCommandAction);
                break;
            case ScriptAction scriptAction:
                GridView.AddGridAction(scriptAction);
                break;
            case InternalAction internalAction:
                GridView.AddGridAction(internalAction);
                break;
        }
    }

    private void ClearSelectedGridValues()
    {
        GridView.ClearSelectedGridValues();
    }

    public bool EnableMultSelect
    {
        get => GridView.EnableMultSelect;
        set => GridView.EnableMultSelect = value;
    }

    #endregion
}