using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataDictionary.Actions.FormToolbar;
using JJMasterData.Core.DataDictionary.Actions.GridTable;
using JJMasterData.Core.DataDictionary.Actions.GridToolbar;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.UI.Components.FormView;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Components.Scripts;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

namespace JJMasterData.Core.UI.Components.Widgets;

public class LinkButtonFactory : IComponentFactory<JJLinkButton>
{
    private ActionsScripts _actionsScripts;
    private IExpressionsService ExpressionsService { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }
    private JJMasterDataEncryptionService EncryptionService { get; }
    private ActionsScripts ActionsScripts => _actionsScripts ??= new ActionsScripts(ExpressionsService,DataDictionaryRepository, UrlHelper, EncryptionService, StringLocalizer);
    
    public LinkButtonFactory(
        IExpressionsService expressionsService,
        IDataDictionaryRepository dataDictionaryRepository,
        JJMasterDataUrlHelper urlHelper,
        JJMasterDataEncryptionService encryptionService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        ExpressionsService = expressionsService;
        DataDictionaryRepository = dataDictionaryRepository;
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
            UrlAction = action is SubmitAction submitAction ? submitAction.FormAction : null,
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
        var actionContext = ActionContext.FromGridView(gridView, formStateData);
        var button = await CreateAsync(action, actionContext.FormStateData);

        switch (action)
        {
            case UserCreatedAction userCreatedAction:
                button.OnClientClick = await ActionsScripts.GetUserActionScriptAsync(userCreatedAction, actionContext, ActionSource.GridTable);
                break;
            case GridTableAction:
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
            case DeleteSelectedRowsAction or ImportAction or AuditLogAction:
                button.OnClientClick =
                    ActionsScripts.GetFormActionScript(toolbarAction, actionContext, ActionSource.GridToolbar);
                break;
            case ExportAction:
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
                button.OnClientClick = ActionsScripts.GetFormActionScript(insertAction, actionContext,
                    ActionSource.GridToolbar, insertAction.ShowAsPopup);
                break;
            case LegendAction:
                button.OnClientClick =
                    BootstrapHelper.GetModalScript($"iconlegend_modal_{actionContext.ParentComponentName}");
                break;
            case RefreshAction:
                button.OnClientClick = ActionsScripts.GetRefreshScript(actionContext);
                break;
            case SortAction:
                button.OnClientClick =
                    BootstrapHelper.GetModalScript($"sort-modal-{actionContext.ParentComponentName}");
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
            button.OnClientClick = await ActionsScripts.GetUserActionScriptAsync(userCreatedAction, actionContext, ActionSource.Field);
        }

        return button;
    }
}