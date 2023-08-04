﻿using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Components;
using System.Threading.Tasks;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Actions.FormToolbar;
using JJMasterData.Core.DataDictionary.Actions.GridTable;
using JJMasterData.Core.DataDictionary.Actions.GridToolbar;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.UI.Components.FormView;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components.Scripts;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components.Widgets;

public class LinkButtonFactory : IComponentFactory<JJLinkButton>
{
    private IExpressionsService ExpressionsService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }
    private JJMasterDataEncryptionService EncryptionService { get; }

    private ActionsScripts ActionsScripts { get; }

    public LinkButtonFactory(IExpressionsService expressionsService,
        JJMasterDataUrlHelper urlHelper,
        JJMasterDataEncryptionService encryptionService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        ActionsScripts = new ActionsScripts(expressionsService, urlHelper, encryptionService, stringLocalizer);
        ExpressionsService = expressionsService;
        StringLocalizer = stringLocalizer;
        UrlHelper = urlHelper;
        EncryptionService = encryptionService;
    }

    public JJLinkButton Create()
    {
        return new JJLinkButton();
    }

    public JJLinkButton Create(BasicAction action, bool enabled, bool visible)
    {
        return new JJLinkButton
        {
            ToolTip = action.ToolTip,
            Text = action.Text,
            IsGroup = action.IsGroup,
            IsDefaultOption = action.IsDefaultOption,
            DividerLine = action.DividerLine,
            ShowAsButton = !action.IsGroup && action.ShowAsButton,
            Type = action is SubmitAction ? LinkButtonType.Submit : default,
            CssClass = action.CssClass,
            IconClass = action.Icon.GetCssClass() + " fa-fw",
            Enabled = enabled,
            Visible = visible
        };
    }

    private async Task<JJLinkButton> CreateAsync(BasicAction action, FormStateData formStateData)
    {
        var isVisible = await ExpressionsService.GetBoolValueAsync(action.VisibleExpression, formStateData);
        var isEnabled = await ExpressionsService.GetBoolValueAsync(action.EnableExpression, formStateData);
        return Create(action, isEnabled, isVisible);
    }

    public async Task<JJLinkButton> CreateGridTableButtonAsync(BasicAction action, JJGridView gridView, FormStateData formStateData)
    {
        var actionContext = ActionContext.FromGridView(gridView,formStateData);
        var button = await CreateAsync(action, actionContext.FormStateData);

        switch (action)
        {
            case UserCreatedAction userCreatedAction:
                button.OnClientClick = ActionsScripts.GetUserActionScript(userCreatedAction, actionContext,ActionSource.GridTable);
                break;
            case GridTableAction:
                button.OnClientClick =
                    ActionsScripts.GetFormActionScript(action, actionContext, ActionSource.GridTable);
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
            button.OnClientClick = ActionsScripts.GetUserActionScript(userCreatedAction, actionContext, ActionSource.GridToolbar);
        }
        else if (action is GridToolbarAction gridToolbarAction)
        {
            switch (gridToolbarAction)
            {
                case ConfigAction:
                    button.OnClientClick =
                        BootstrapHelper.GetModalScript($"config_modal_{actionContext.ParentComponentName}");
                    break;
                case DeleteSelectedRowsAction or ImportAction or LogAction:
                    button.OnClientClick =
                        ActionsScripts.GetFormActionScript(action, actionContext, ActionSource.GridToolbar);
                    break;
                case ExportAction exportAction:
                    var exportationScripts = new DataExportationScripts(UrlHelper, EncryptionService);
                    button.OnClientClick =
                        exportationScripts.GetExportPopupScript(actionContext.FormElement.Name,
                            actionContext.ParentComponentName, actionContext.IsExternalRoute);
                    break;
                case FilterAction filterAction:
                    if (filterAction.ShowAsCollapse)
                        button.Visible = false;

                    button.OnClientClick =
                        BootstrapHelper.GetModalScript($"filter_modal_{actionContext.ParentComponentName}");
                    break;
                case InsertAction insertAction:
                    button.OnClientClick = ActionsScripts.GetFormActionScript(action, actionContext,
                        ActionSource.GridToolbar, insertAction.ShowAsPopup);
                    break;
                case LegendAction legendAction:
                    button.OnClientClick =
                        BootstrapHelper.GetModalScript($"iconlegend_modal_{actionContext.ParentComponentName}");
                    break;
                case RefreshAction refreshAction:
                    button.OnClientClick = $"JJView.refresh('{actionContext.ParentComponentName}');";
                    break;
                case SortAction sortAction:
                    button.OnClientClick =
                        BootstrapHelper.GetModalScript($"sort_modal_{actionContext.ParentComponentName}");
                    break;
            }
        }
        else
        {
            throw new JJMasterDataException("Invalid GridToolBarAction.");
        }


        return button;
    }

    public async Task<JJLinkButton> CreateFormToolbarButtonAsync(BasicAction action, JJFormView formView)
    {
        var actionContext = await ActionContext.FromFormViewAsync(formView);
        var button = await CreateAsync(action, actionContext.FormStateData);

        if (action is UserCreatedAction userCreatedAction)
        {
            button.OnClientClick = ActionsScripts.GetUserActionScript(userCreatedAction, actionContext,ActionSource.FormToolbar);
        }
        else if (action is FormToolbarAction)
        {
            switch (action)
            {
                case BackAction or CancelAction:
                    button.OnClientClick =
                        $"return ActionManager.executePanelAction('{actionContext.ParentComponentName}','CANCEL');";
                    break;
                case SaveAction saveAction:
                    if (saveAction.EnterKeyBehavior == FormEnterKey.Submit)
                        button.Type = LinkButtonType.Submit;
                    else
                        button.Type = saveAction.IsGroup ? LinkButtonType.Link : LinkButtonType.Button;

                    button.OnClientClick =
                        $"return ActionManager.executePanelAction('{actionContext.ParentComponentName}','OK');";
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
            button.OnClientClick = ActionsScripts.GetUserActionScript(userCreatedAction,actionContext, ActionSource.Field);
        }

        return button;
    }
}