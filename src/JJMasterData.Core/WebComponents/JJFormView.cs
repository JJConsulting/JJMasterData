using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Html;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using JJMasterData.Commons.DI;
using JJMasterData.Core.DI;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Represents a CRUD.
/// </summary>
/// <example>
/// [!code-cshtml[Example](../../../example/JJMasterData.WebExample/Pages/Components/JJFormViewExample.cshtml)]
/// The GetHtml method will return something like this:
/// <img src="../media/JJFormViewExample.png"/>
/// </example>
public class JJFormView : JJGridView
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
    private ActionMap _currentActionMap;
    private JJFormLog _logHistory;
    private FormService _service;
    

    internal JJFormLog FormLog =>
        _logHistory ??= new JJFormLog(FormElement, EntityRepository);
        

    /// <summary>
    /// Url a ser direcionada após os eventos de Update/Delete/Save
    /// </summary>
    internal string UrlRedirect { get; set; }

    /// <summary>
    /// Configurações de importação
    /// </summary>
    public new JJDataImp DataImp 
    {
        get
        {
            var dataimp = base.DataImp;
            dataimp.OnAfterDelete = OnAfterDelete;
            dataimp.OnAfterInsert = OnAfterDelete;
            dataimp.OnAfterUpdate = OnAfterDelete;

            return dataimp;
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
                    Name = "jjpainel_" + FormElement.Name.ToLower(),
                    EntityRepository = EntityRepository,
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


    private FormService Service 
    { 
        get
        {
            if (_service == null)
            { 
                var dataContext = new DataContext(DataContextSource.Form, UserId);
                _service = new FormService(FormManager, dataContext)
                {
                    EnableErrorLink = true,
                    EnableHistoryLog = LogAction.IsVisible
                };

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
        Name = "jjview" + formElement.Name.ToLower();
    }
    
    #endregion

    internal override HtmlBuilder RenderHtml()
    {
        string requestType = CurrentContext.Request.QueryString("t");
        string objName = CurrentContext.Request.QueryString("objname");
        var dataPanel = DataPanel;
        
        if (JJLookup.IsLookupRoute(this))
            return new HtmlBuilder(dataPanel.GetHtml());
        
        if (JJTextFile.IsFormUploadRoute(this))
            return new HtmlBuilder(dataPanel.GetHtml());
        
        if (JJDownloadFile.IsDownloadRoute(this))
            return JJDownloadFile.ResponseRoute(this);

        if ("jjsearchbox".Equals(requestType))
        {
            string pnlname = CurrentContext.Request.QueryString("pnlname");
            if (dataPanel.Name.Equals(pnlname))
            {
                dataPanel.GetHtml();
            }
            else if (Name.Equals(pnlname))
            {
                Filter.GetFilterHtmlBuilder();
            }
            else
            {
                return null;
            }
        }
        else if ("reloadpainel".Equals(requestType))
        {
            //TODO: eliminar metodo GetSelectedRowId
            Hashtable filter = GetSelectedRowId();
            Hashtable values = null;
            if (filter is { Count: > 0 })
                values = EntityRepository.GetFields(FormElement, filter);

            string htmlPanel = GetHtmlDataPainel(values, null, PageState, true).ToString();
            CurrentContext.Response.SendResponse(htmlPanel);
            return null;
        }
        else if ("jjupload".Equals(requestType) || "ajaxdataimp".Equals(requestType))
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
        var currentAction = GetCurrentAction(actionMap);

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
                    CurrentContext.Response.Redirect(UrlRedirect);
                    return null;
                }

                pageState = PageState.List;
                return GetHtmlGrid();
            }

            pageState = PageState.Update;
            return GetHtmlDataPainel(values, errors, pageState, true);
        }

        if ("CANCEL".Equals(formAction))
        {
            ClearTempFiles();
            CurrentContext.Response.Redirect(CurrentContext.Request.AbsoluteUri);
            return null;
        }
        if ("REFRESH".Equals(formAction))
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
                values = EntityRepository.GetFields(FormElement, acMap.PKFieldValues);
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
        
        string formAction = "";

        if (CurrentContext.Request["current_painelaction_" + Name] != null)
            formAction = CurrentContext.Request["current_painelaction_" + Name];

        if (formAction.Equals("OK"))
        {
            var values = GetFormValues();
            var erros = InsertFormValues(values);

            if (erros.Count == 0)
            {
                if (!string.IsNullOrEmpty(UrlRedirect))
                {
                    CurrentContext.Response.Redirect(UrlRedirect);
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
 
                pageState = PageState.List;
                return GetHtmlGrid();

            }
  
            pageState = PageState.Insert;
            return GetHtmlDataPainel(values, erros, pageState, true);

        }

        if (formAction.Equals("CANCEL"))
        {
            pageState = PageState.List;
            ClearTempFiles();
            return GetHtmlGrid();
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
            return GetHtmlDataPainel(GetFormValues(), null, pageState, true);
        }

        pageState = PageState.Insert;
        
        if (string.IsNullOrEmpty(action.ElementNameToSelect))
            return GetHtmlDataPainel(RelationValues, null, PageState.Insert, false);
        return GetHtmlElementList(action);
    }

    private HtmlBuilder GetHtmlElementList(InsertAction action)
    {
        var sHtml = new HtmlBuilder(HtmlTag.Div);
        sHtml.AppendHiddenInput($"current_painelaction_{Name}", "ELEMENTLIST");
        sHtml.AppendHiddenInput($"current_selaction_{Name}", "");

        var dicParser = JJServiceCore.DataDictionaryRepository.GetMetadata(action.ElementNameToSelect);
        var formsel = new JJFormView(dicParser.GetFormElement())
        {
            EntityRepository = EntityRepository,
            UserValues = UserValues,
            Name = action.ElementNameToSelect
        };
        formsel.SetOptions(dicParser.UIOptions);

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
        formsel.AddToolBarAction(goBackAction);

        var selAction = new ScriptAction
        {
            Name = "_jjselaction",
            Icon = IconType.CaretRight,
            ToolTip = "Select",
            IsDefaultOption = true
        };
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
        var dicRepository = JJServiceCore.DataDictionaryRepository;
        var dictionary = dicRepository.GetMetadata(InsertAction.ElementNameToSelect);
        var element = dictionary.Table;
        var selValues = EntityRepository.GetFields(element, map.PKFieldValues);
        var values = FormManager.MergeWithExpressionValues(selValues, PageState.Insert, true);
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

        pageState = PageState.View;
        var filter = acMap.PKFieldValues;
        var values = EntityRepository.GetFields(FormElement, filter);
        return GetHtmlDataPainel(values, null, PageState.View, false);
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
            CurrentContext.Response.Redirect(UrlRedirect);
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
                if (errors is { Count: > 0 })
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
            var html = FormLog.GetDetailLog(actionMap.PKFieldValues);
            html.AppendElement(GetFormLogBottombar(actionMap.PKFieldValues));
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
        painel.Errors = erros;
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

            var collapse = new JJCollapsePanel
            {
                Name = "collapse_" + Name,
                Title = FormElement.Title,
                ExpandedByDefault = true,
                HtmlBuilderContent = sPainel
            };
            html.AppendElement(collapse);
        }

        var dicRepository = JJServiceCore.DataDictionaryRepository;
        foreach (var relation in relations)
        {
            var dic = dicRepository.GetMetadata(relation.ChildElement);
            var childElement = dic.GetFormElement();

            var filter = new Hashtable();
            foreach (var col in relation.Columns.Where(col => values.ContainsKey(col.PkColumn)))
            {
                filter.Add(col.FkColumn, values[col.PkColumn]);
            }

            if (relation.ViewType == RelationType.View)
            {
                var childvalues = EntityRepository.GetFields(childElement, filter);
                var chieldView = new JJDataPanel(childElement)
                {
                    EntityRepository = EntityRepository,
                    PageState = PageState.View,
                    UserValues = UserValues,
                    Values = childvalues,
                    RenderPanelGroup = true
                };

                if (dic.UIOptions != null)
                {
                    chieldView.UISettings = dic.UIOptions.Form;
                }

                html.AppendElement(chieldView);
            }
            else if (relation.ViewType == RelationType.List)
            {
                var childGrid = new JJFormView(childElement)
                {
                    EntityRepository = EntityRepository,
                    UserValues = UserValues,
                    FilterAction =
                    {
                        ShowAsCollapse = false
                    },
                    Name = "jjgridview_" + childElement.Name
                };
                childGrid.Filter.ApplyCurrentFilter(filter);

                if (dic.UIOptions != null)
                {
                    childGrid.SetOptions(dic.UIOptions);
                }

                childGrid.ShowTitle = false;

                var collapse = new JJCollapsePanel
                {
                    Name = "collapse_" + childGrid.Name,
                    Title = childElement.Title,
                    HtmlContent = childGrid.GetHtml()
                };

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

        var toolbar = new JJToolbar
        {
            CssClass = "pb-3 mt-3"
        };
        toolbar.ListElement.Add(btnBack.GetHtmlBuilder());
        toolbar.ListElement.Add(btnHideLog.GetHtmlBuilder());
        return toolbar;
    }

    private JJToolbar GetFormBottombar(PageState pageState, Hashtable values)
    {
        var toolbar = new JJToolbar
        {
            CssClass = "pb-3 mt-3"
        };
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

        if (sender is not JJGridView grid) return;
        
        var map = new ActionMap(ActionOrigin.Grid, grid.FormElement, e.FieldValues, e.Action.Name);
        string criptId = map.GetCriptJson();
        e.LinkButton.OnClientClick = $"jjview.doSelElementInsert('{Name}','{criptId}');";
    }

    /// <summary>
    /// Insert the records in the database.
    /// </summary>
    /// <returns>The list of errors.</returns>
    public Hashtable InsertFormValues(Hashtable values, bool validateFields = true)
    {
        var result = Service.Insert(values);
        UrlRedirect = result.UrlRedirect;
        return result.Errors;
    }

    /// <summary>
    /// Update the records in the database.
    /// </summary>
    /// <returns>The list of errors.</returns>
    public Hashtable UpdateFormValues(Hashtable values)
    {
        var result = Service.Update(values);
        UrlRedirect = result.UrlRedirect;
        return result.Errors;
    }
    
    public Hashtable DeleteFormValues(Hashtable filter)
    {
        var values = Service.FormManager.MergeWithExpressionValues(filter, PageState.Delete, true);
        var result = Service.Delete(values);
        UrlRedirect = result.UrlRedirect;
        return result.Errors;
    }
    
    public Hashtable GetFormValues()
    {
        var painel = DataPanel;
        var values = painel.GetFormValues();

        if (RelationValues == null) 
            return values;

        DataHelper.CopyIntoHash(ref values, RelationValues, true);

        return values;
    }
    
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
    
    public void SetOptions(UIOptions options)
    {
        FormFactory.SetFormOptions(this, options);
    }

    private JJLinkButton GetButtonOk()
    {
        var btn = new JJLinkButton
        {
            Text = "Save",
            IconClass = IconType.Check.GetCssClass(),
            OnClientClick = $"return jjview.doPainelAction('{Name}','OK');"
        };
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
        var btn = new JJLinkButton
        {
            Type = LinkButtonType.Button,
            CssClass = $"{BootstrapHelper.DefaultButton} btn-small",
            OnClientClick = $"jjview.doPainelAction('{Name}','CANCEL');",
            IconClass = IconType.Times.GetCssClass(),
            Text = "Cancel"
        };
        return btn;
    }

    private JJLinkButton GetButtonBack()
    {
        var btn = GetButtonCancel();
        btn.IconClass = IconType.ArrowLeft.GetCssClass();
        btn.Text = "Back";
        return btn;
    }

    private JJLinkButton GetButtonHideLog(Hashtable values)
    {
        string scriptAction = ActionManager.GetFormActionScript(ViewAction, values, ActionOrigin.Grid);
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

    private JJLinkButton GetButtonViewLog(Hashtable values)
    {
        string scriptAction = ActionManager.GetFormActionScript(LogAction, values, ActionOrigin.Toolbar);
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

}