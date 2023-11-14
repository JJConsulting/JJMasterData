using System.Threading.Tasks;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Http;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class ActionButtonFactory(IComponentFactory<JJLinkButton> linkButtonFactory,
    ExpressionsService expressionsService,
    MasterDataUrlHelper urlHelper, 
    IEncryptionService encryptionService,
    IStringLocalizer<MasterDataResources> stringLocalizer)
{
    private IComponentFactory<JJLinkButton> LinkButtonFactory { get; } = linkButtonFactory;

    private ActionScripts _actionScripts;
    private ExpressionsService ExpressionsService { get; } = expressionsService;

    private MasterDataUrlHelper UrlHelper { get; } = urlHelper;
    private IEncryptionService EncryptionService { get; } = encryptionService;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
    private ActionScripts ActionScripts => _actionScripts ??= new ActionScripts(ExpressionsService, UrlHelper, EncryptionService, StringLocalizer);

    public JJLinkButton Create()
    {
        return LinkButtonFactory.Create();
    }
    
    public JJLinkButton Create(BasicAction action, bool visible, bool enabled)
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

    private JJLinkButton Create(BasicAction action, FormStateData formStateData)
    {
        var isVisible = ExpressionsService.GetBoolValue(action.VisibleExpression, formStateData);
        var isEnabled = ExpressionsService.GetBoolValue(action.EnableExpression, formStateData);
        return Create(action, isVisible, isEnabled);
    }

    public JJLinkButton CreateGridTableButton(BasicAction action, JJGridView gridView, FormStateData formStateData)
    {
        var actionContext = gridView.GetActionContext(action, formStateData);
        var button = Create(action, actionContext.FormStateData);

        switch (action)
        {
            case UserCreatedAction userCreatedAction:
                button.OnClientClick = ActionScripts.GetUserActionScript(actionContext, ActionSource.GridTable);
                break;
            case GridTableAction gridTableAction:

                if (gridTableAction is EditAction editAction)
                {
                    actionContext.IsModal = editAction.ShowAsModal;
                }
                
                if (gridTableAction is ViewAction viewAction)
                {
                    actionContext.IsModal = viewAction.ShowAsModal;
                }
                
                button.OnClientClick = ActionScripts.GetFormActionScript(actionContext, ActionSource.GridTable);
                break;
            default:
                throw new JJMasterDataException("Action is not user created or a GridTableAction.");
        }


        return button;
    }

    public JJLinkButton CreateGridToolbarButton(BasicAction action, JJGridView gridView, FormStateData formStateData)
    {
        var actionContext = gridView.GetActionContext(action, formStateData);
        var button = Create(action, formStateData);

        if (action is UserCreatedAction userCreatedAction)
        {
            button.OnClientClick =  ActionScripts.GetUserActionScript(actionContext, ActionSource.GridToolbar);
            return button;
        }

        switch (action)
        {
            case ConfigAction:
                button.OnClientClick = BootstrapHelper.GetModalScript($"config-modal-{actionContext.ParentComponentName}");
                break;
            case DeleteSelectedRowsAction or AuditLogGridToolbarAction:
                button.OnClientClick =
                    ActionScripts.GetFormActionScript(actionContext, ActionSource.GridToolbar);
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
                button.OnClientClick = ActionScripts.GetFormActionScript(actionContext,
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
            default:
                throw new JJMasterDataException("Invalid GridToolBarAction.");
        }

        return button;
    }

    public async Task<JJLinkButton> CreateFormToolbarButton(BasicAction action, JJFormView formView)
    {
        var actionContext = await formView.GetActionContextAsync(action);
        var button = Create(action, actionContext.FormStateData);

        if (action is UserCreatedAction)
        {
            button.OnClientClick =  ActionScripts.GetUserActionScript(actionContext, ActionSource.FormToolbar);
        }
        else if (action is FormToolbarAction)
        {
            switch (action)
            {
                case CancelAction when !formView.ContainsRelationships():
                case BackAction:
                    if (actionContext.IsModal)
                    {
                        button.OnClientClick = ActionScripts.GetHideModalScript(actionContext.ParentComponentName);
                    }
                    else
                    {
                         button.OnClientClick = ActionScripts.GetFormActionScript(actionContext, ActionSource.FormToolbar);
                    }
                    break;

                case FormEditAction:
                    button.OnClientClick = formView.Scripts.GetSetPanelStateScript(PageState.Update);
                    break;
                case CancelAction:
                    button.OnClientClick = formView.Scripts.GetSetPanelStateScript(PageState.View);
                    break;
                case AuditLogFormToolbarAction:
                    button.OnClientClick = ActionScripts.GetFormActionScript(actionContext, ActionSource.FormToolbar);
                    break;
                case SaveAction saveAction:
                    if (saveAction.EnterKeyBehavior == FormEnterKey.Submit)
                        button.Type = LinkButtonType.Submit;
                    else
                        button.Type = saveAction.IsGroup ? LinkButtonType.Link : LinkButtonType.Button;

                    actionContext.IsSubmit = saveAction.SubmitOnSave;
                    button.OnClientClick = ActionScripts.GetFormActionScript(actionContext, ActionSource.FormToolbar);
                    break;
            }
        }
        else
        {
            throw new JJMasterDataException("Invalid FormToolBarAction.");
        }


        return button;
    }

    public JJLinkButton CreateFieldButton(BasicAction action, ActionContext actionContext)
    {
        var button = Create(action, actionContext.FormStateData);

        if (action is UserCreatedAction userCreatedAction)
        {
            button.OnClientClick = ActionScripts.GetUserActionScript(actionContext, ActionSource.Field);
        }

        button.Enabled = button.Enabled && actionContext.FormStateData.PageState is not PageState.View;

        return button;
    }
    
}