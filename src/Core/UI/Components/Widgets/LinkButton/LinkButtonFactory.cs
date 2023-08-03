using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Components;
using System.Threading.Tasks;

namespace JJMasterData.Core.UI.Components.Widgets;

internal class LinkButtonFactory
{
    private readonly IExpressionsService _expressionsService;

    public LinkButtonFactory(IExpressionsService expressionsService)
    {
        _expressionsService = expressionsService;
    }

    public JJLinkButton Create(BasicAction action, bool enable, bool visible)
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
            Enabled = enable,
            Visible = visible
        };
    }

    public async Task<JJLinkButton> CreateAsync(BasicAction action, FormStateData formStateData)
    {
        bool isVisible = await _expressionsService.GetBoolValueAsync(action.VisibleExpression, formStateData);
        bool isEnabled = await _expressionsService.GetBoolValueAsync(action.EnableExpression, formStateData);
        return Create(action, isEnabled, isVisible);
    }

}