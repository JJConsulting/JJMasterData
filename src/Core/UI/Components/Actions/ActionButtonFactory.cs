using System.Threading.Tasks;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.UI.Components.FormView;
using JJMasterData.Core.UI.Components.Importation;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Components.Scripts;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components.Actions;

public class ActionButtonFactory
{
    private IComponentFactory<JJLinkButton> LinkButtonFactory { get; }

    private ActionsScripts _actionsScripts;
    private ExpressionsService ExpressionsService { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
  
    private JJMasterDataUrlHelper UrlHelper { get; }
    private IEncryptionService EncryptionService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private ActionsScripts ActionsScripts => _actionsScripts ??= new ActionsScripts(ExpressionsService,DataDictionaryRepository, UrlHelper, EncryptionService, StringLocalizer);
    
    public ActionButtonFactory(IComponentFactory<JJLinkButton> linkButtonFactory,
        ExpressionsService expressionsService,
        IDataDictionaryRepository dataDictionaryRepository, 
        JJMasterDataUrlHelper urlHelper, 
        IEncryptionService encryptionService, IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        LinkButtonFactory = linkButtonFactory;
        ExpressionsService = expressionsService;
        DataDictionaryRepository = dataDictionaryRepository;
        UrlHelper = urlHelper;
        EncryptionService = encryptionService;
        StringLocalizer = stringLocalizer;
    }

    public JJLinkButton Create()
    {
        return LinkButtonFactory.Create();
    }
    
    private JJLinkButton Create(BasicAction action, bool enabled, bool visible)
    {
        var button = LinkButtonFactory.Create();
        button.Tooltip = action.Tooltip;
        button.Text = action.Text;
        button.IsGroup = action.IsGroup;
        button.IsDefaultOption = action.IsDefaultOption;
        button.DividerLine = action.DividerLine;
        button.ShowAsButton = !action.IsGroup && action.ShowAsButton;
        button.Type = action is SubmitAction ? LinkButtonType.Submit : default;
        button.CssClass = action.CssClass;
        button.UrlAction = action is SubmitAction submitAction ? submitAction.FormAction : null;
        button.IconClass = $"{action.Icon.GetCssClass()} fa-fw";
        button.Enabled = enabled;
        button.Visible = visible;
        return button;
    }

    private async Task<JJLinkButton> CreateAsync(BasicAction action, FormStateData formStateData)
    {
        var isVisible = await ExpressionsService.GetBoolValueAsync(action.VisibleExpression, formStateData);
        var isEnabled = await ExpressionsService.GetBoolValueAsync(action.EnableExpression, formStateData);
        return Create(action, isEnabled, isVisible);
    }

    public async Task<JJLinkButton> CreateGridTableButtonAsync(BasicAction action, JJGridView gridView, FormStateData formStateData)
    {
        var actionContext = ActionContext.FromGridView(gridView, formStateData);
        var button = await CreateAsync(action, actionContext.FormStateData);

        switch (action)
        {
            case UserCreatedAction userCreatedAction:
                button.OnClientClick = await ActionsScripts.GetUserActionScriptAsync(userCreatedAction, actionContext, ActionSource.GridTable);
                break;
            case GridTableAction gridTableAction:

                if (gridTableAction is EditAction editAction)
                {
                    actionContext.IsModal = editAction.ShowAsModal;
                }
                
                button.OnClientClick = ActionsScripts.GetFormActionScript(action, actionContext, ActionSource.GridTable);
                break;
            default:
                throw new JJMasterDataException("Action is not user created or a GridTableAction.");
        }


        return button;
    }

    public async Task<JJLinkButton> CreateGridToolbarButtonAsync(BasicAction action, JJGridView gridView, FormStateData formStateData)
    {
        var actionContext = ActionContext.FromGridView(gridView, formStateData);
        var button = await CreateAsync(action, formStateData);

        if (action is UserCreatedAction userCreatedAction)
        {
            button.OnClientClick = await ActionsScripts.GetUserActionScriptAsync(userCreatedAction, actionContext, ActionSource.GridToolbar);
            return button;
        }

        if (action is not GridToolbarAction toolbarAction)
            throw new JJMasterDataException("Invalid GridToolBarAction.");

        switch (toolbarAction)
        {
            case ConfigAction:
                button.OnClientClick = BootstrapHelper.GetModalScript($"config-modal-{actionContext.ParentComponentName}");
                break;
            case DeleteSelectedRowsAction or AuditLogGridToolbarAction:
                button.OnClientClick =
                    ActionsScripts.GetFormActionScript(toolbarAction, actionContext, ActionSource.GridToolbar);
                break;
            
            case ImportAction:
                var importationScripts = new DataImportationScripts(actionContext.ParentComponentName, actionContext.FormElement, EncryptionService);
                button.OnClientClick =
                    importationScripts.GetShowScript();
                break;
                
            case ExportAction:
                var exportationScripts = new DataExportationScripts(actionContext.ParentComponentName, actionContext.FormElement, EncryptionService);
                button.OnClientClick =
                    exportationScripts.GetExportPopupScript();
                break;
            case FilterAction filterAction:
                if (filterAction.ShowAsCollapse)
                    button.Visible = false;

                button.OnClientClick =
                    BootstrapHelper.GetModalScript($"{actionContext.ParentComponentName}-filter-modal");
                break;
            case InsertAction insertAction:
                if (insertAction.ShowAsModal)
                {
                    actionContext.IsModal = true;
                }
                button.OnClientClick = ActionsScripts.GetFormActionScript(insertAction, actionContext,
                    ActionSource.GridToolbar);
                break;
            case LegendAction:
                button.OnClientClick =
                    BootstrapHelper.GetModalScript($"{actionContext.ParentComponentName}-legend-modal");
                break;
            case RefreshAction:
                button.OnClientClick = gridView.Scripts.GetRefreshScript();
                break;
            case SortAction:
                button.OnClientClick =
                    BootstrapHelper.GetModalScript($"{actionContext.ParentComponentName}-sort-modal");
                break;
        }

        return button;
    }

    public async Task<JJLinkButton> CreateFormToolbarButtonAsync(BasicAction action, JJFormView formView)
    {
        var actionContext = await ActionContext.FromFormViewAsync(formView);
        var button = await CreateAsync(action, actionContext.FormStateData);

        if (action is UserCreatedAction userCreatedAction)
        {
            button.OnClientClick = await ActionsScripts.GetUserActionScriptAsync(userCreatedAction, actionContext, ActionSource.FormToolbar);
        }
        else if (action is FormToolbarAction)
        {
            switch (action)
            {
                case CancelAction when !formView.ContainsRelationships():
                case BackAction:
                    if (actionContext.IsModal)
                    {
                        button.OnClientClick = ActionsScripts.GetHideModalScript(actionContext.ParentComponentName);
                    }
                    else
                    {
                         button.OnClientClick = ActionsScripts.GetFormActionScript(action,actionContext, ActionSource.FormToolbar);
                    }
                    break;

                case FormEditAction:
                    button.OnClientClick = formView.Scripts.GetSetPanelStateScript(PageState.Update);
                    break;
                case CancelAction:
                    button.OnClientClick = formView.Scripts.GetSetPanelStateScript(PageState.View);
                    break;
                case AuditLogFormToolbarAction:
                    button.OnClientClick = ActionsScripts.GetFormActionScript(action, actionContext, ActionSource.FormToolbar);
                    break;
                case SaveAction saveAction:
                    if (saveAction.EnterKeyBehavior == FormEnterKey.Submit)
                        button.Type = LinkButtonType.Submit;
                    else
                        button.Type = saveAction.IsGroup ? LinkButtonType.Link : LinkButtonType.Button;

                    actionContext.IsSubmit = saveAction.SubmitOnSave;
                    button.OnClientClick = ActionsScripts.GetFormActionScript(action,actionContext, ActionSource.FormToolbar);
                    break;
            }
        }
        else
        {
            throw new JJMasterDataException("Invalid FormToolBarAction.");
        }


        return button;
    }

    public async Task<JJLinkButton> CreateFieldLinkAsync(BasicAction action, ActionContext actionContext)
    {
        var button = await CreateAsync(action, actionContext.FormStateData);

        if (action is UserCreatedAction userCreatedAction)
        {
            button.OnClientClick = await ActionsScripts.GetUserActionScriptAsync(userCreatedAction, actionContext, ActionSource.Field);
        }

        return button;
    }
}