using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class ActionButtonFactory(
    IComponentFactory<JJLinkButton> linkButtonFactory,
    ActionScripts actionScripts,
    ExpressionsService expressionsService,
    IEncryptionService encryptionService,
    IStringLocalizer<MasterDataResources> stringLocalizer)
{
    public JJLinkButton Create() => linkButtonFactory.Create();

    public JJLinkButton Create(BasicAction action, bool visible, bool enabled)
    {
        var button = linkButtonFactory.Create();
        button.Tooltip = action.Tooltip;
        button.Text = action.Text;
        button.Color = action.Color;
        button.IsGroup = action.IsGroup;
        button.IsDefaultOption = action.IsDefaultOption;
        button.DividerLine = action.DividerLine;
        button.ShowAsButton = !action.IsGroup && action.ShowAsButton;
        button.Type = action is SubmitAction ? LinkButtonType.Submit : default;
        button.UrlAction = action is SubmitAction submitAction ? submitAction.FormAction : null;
        button.CssClass = action.CssClass;
        button.IconClass = $"{action.Icon.GetCssClass()} fa-fw";
        button.Enabled = enabled;
        button.Visible = visible;
        
        return button;
    }

    private JJLinkButton Create(BasicAction action, FormStateData formStateData)
    {
        var isVisible = expressionsService.GetBoolValue(action.VisibleExpression, formStateData);
        var isEnabled = expressionsService.GetBoolValue(action.EnableExpression, formStateData);
        return Create(action, isVisible, isEnabled);
    }

    public JJLinkButton CreateGridTableButton(BasicAction action, JJGridView gridView, FormStateData formStateData)
    {
        var actionContext = gridView.GetActionContext(action, formStateData);
        var button = Create(action, actionContext.FormStateData);

        if (action.IsCustomAction)
            button.OnClientClick = actionScripts.GetUserActionScript(actionContext, ActionSource.GridTable);
        else if (action is GridTableAction)
            button.OnClientClick = actionScripts.GetFormActionScript(actionContext, ActionSource.GridTable);
        else
            throw new JJMasterDataException("Action is not user created or a GridTableAction.");

        return button;
    }

    public JJLinkButton CreateGridToolbarButton(BasicAction action, JJGridView gridView, FormStateData formStateData)
    {
        var actionContext = gridView.GetActionContext(action, formStateData);
        var button = Create(action, formStateData);

        if (action.IsCustomAction)
        {
            button.OnClientClick =  actionScripts.GetUserActionScript(actionContext, ActionSource.GridToolbar);
            return button;
        }

        switch (action)
        {
            case ConfigAction:
                button.OnClientClick = BootstrapHelper.GetModalScript($"config-modal-{actionContext.ParentComponentName}");
                break;
            case AuditLogGridToolbarAction:
                button.OnClientClick =
                    actionScripts.GetFormActionScript(actionContext, ActionSource.GridToolbar);
                break;
            
            case ImportAction:
                var importationScripts = new DataImportationScripts($"{actionContext.ParentComponentName}-importation", actionContext.FormElement,stringLocalizer, encryptionService);
                button.OnClientClick =
                    importationScripts.GetShowScript();
                break;
                
            case ExportAction:
                var exportationScripts = new DataExportationScripts(actionContext.ParentComponentName, actionContext.FormElement, encryptionService);
                button.OnClientClick =
                    exportationScripts.GetExportModalScript();
                break;
            case FilterAction filterAction:
                if (filterAction.ShowAsCollapse)
                    button.Visible = false;

                button.OnClientClick =
                    BootstrapHelper.GetModalScript($"{actionContext.ParentComponentName}-filter-modal");
                break;
            case InsertAction:
                button.OnClientClick = actionScripts.GetFormActionScript(actionContext,
                    ActionSource.GridToolbar);
                break;
            case LegendAction:
                button.OnClientClick =
                    BootstrapHelper.GetModalScript($"{actionContext.ParentComponentName}-caption-modal");
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

    public JJLinkButton CreateFormToolbarButton(
        BasicAction action, 
        FormStateData formStateData,
        JJFormView formView)
    {
        var actionContext = formView.GetActionContext(action,formStateData);
        var button = Create(action, actionContext.FormStateData);
    
        if (action.IsCustomAction)
        {
            button.OnClientClick = actionScripts.GetUserActionScript(actionContext, ActionSource.FormToolbar);
        }
        else if (action is FormToolbarAction)
        {
            var gridTableActions = actionContext.FormElement.Options.GridTableActions;
            switch (action)
            {
                case CancelAction:
                    var isAtRelationshipLayout = formView.ContainsRelationshipLayout(formStateData) || formView.RelationshipType is RelationshipType.OneToOne;

                    if (isAtRelationshipLayout)
                    {
                        button.OnClientClick = formView.Scripts.GetSetPanelStateScript(PageState.View);
                    }
                    else
                    {
                        var isCancelModal = gridTableActions.EditAction.ShowAsModal;
                        if (isCancelModal)
                        {
                            button.OnClientClick = ActionScripts.GetHideModalScript(actionContext.ParentComponentName);
                        }
                        else
                        {
                            button.OnClientClick = actionScripts.GetFormActionScript(actionContext, ActionSource.FormToolbar);
                        }        
                    }
                    break;
                case BackAction:

                    var isBackModal = gridTableActions.ViewAction.ShowAsModal && formStateData.PageState is PageState.View;
                    if (isBackModal)
                    {
                        button.OnClientClick = ActionScripts.GetHideModalScript(actionContext.ParentComponentName);
                    }
                    else
                    {
                         button.OnClientClick = actionScripts.GetFormActionScript(actionContext, ActionSource.FormToolbar);
                    }
                    break;

                case FormEditAction:
                    button.OnClientClick = formView.Scripts.GetSetPanelStateScript(PageState.Update);
                    break;
                case AuditLogFormToolbarAction:
                    button.OnClientClick = actionScripts.GetFormActionScript(actionContext, ActionSource.FormToolbar);
                    break;
                case SaveAction saveAction:
                    var isAtModal = formView.DataPanel.IsAtModal;
                    if (saveAction.EnterKeyBehavior == FormEnterKey.Submit)
                        button.Type = LinkButtonType.Submit;
                    else
                        button.Type = saveAction.IsGroup ? LinkButtonType.Link : LinkButtonType.Button;
                    
                    button.OnClientClick = actionScripts.GetFormActionScript(actionContext, ActionSource.FormToolbar,true,isAtModal);
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

        if (action.IsCustomAction)
        {
            button.OnClientClick = actionScripts.GetUserActionScript(actionContext, ActionSource.Field);
        }

        button.Enabled = button.Enabled && actionContext.FormStateData.PageState is not PageState.View;

        return button;
    }
}