using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

public sealed class JJTextGroup(
    IComponentFactory<JJLinkButtonGroup> linkButtonGroupFactory,
    IFormValues formValues)
    : JJTextBox(formValues), IFloatingLabelControl
{
    public string FloatingLabel { get; set; }

    public bool UseFloatingLabel { get; set; }

    /// <summary>
    /// Actions of input
    /// </summary>
    public List<JJLinkButton> Actions { get; } = [];

    /// <summary>
    /// Text info on left of component
    /// </summary>
    public InputAddons Addons { get; set; }
    
    public string GroupCssClass { get; set; }

    protected override ValueTask<ComponentResult> BuildResultAsync()
    {
        var inputGroup = GetHtmlBuilder();

        return new ValueTask<ComponentResult>(new RenderedComponentResult(inputGroup));
    }

    public override HtmlBuilder GetHtmlBuilder()
    {
        var defaultAction = Actions.Find(x => x.IsDefaultOption && x.Visible);
        if (!Enabled)
        {
            if (defaultAction != null)
            {
                ReadOnly = true;
                Enabled = true;
            }
        }

        var input = base.GetHtmlBuilder();
        var hasAction = Actions.Exists(x => x.Visible);
        var hasAddons = Addons != null;

        if (!hasAction && !hasAddons)
        {
            if (UseFloatingLabel)
            {
                input.WithAttribute("placeholder");
                return new HtmlBuilder(HtmlTag.Div).WithCssClass("form-floating")
                    .Append(input)
                    .AppendLabel(this, static (state,label) =>
                    {
                        label.AppendText(state.FloatingLabel);
                        label.WithAttribute("for", state.Name);
                    });
            }

            return input;
        }

        if (defaultAction is { Enabled: true })
        {
            input.WithCssClass("default-option");
            input.WithOnChange(defaultAction.OnClientClick);
        }

        var inputGroup = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("input-group jjform-action ")
            .WithCssClass(GroupCssClass);
        
        if (hasAddons)
            inputGroup.Append(GetHtmlAddons());

        if (UseFloatingLabel)
        {
            input.WithAttribute("placeholder");
            inputGroup.AppendDiv((Input:input, TextGroup:this), static (state, div) =>
            {
                div.WithCssClass("form-floating");
                div.Append(state.Input);
                div.AppendLabel(state.TextGroup, static (state,label) =>
                {
                    label.AppendText(state.FloatingLabel);
                    label.WithAttribute("for", state.Name);
                });
            });
        }
        else
        {
            inputGroup.Append(input);
        }

        if (hasAction)
            AddActionsAt(inputGroup);

        return inputGroup;
    }

    private void AddActionsAt(HtmlBuilder inputGroup)
    {
        HtmlBuilder builderGroup;
        if (BootstrapHelper.Version >= 5)
        {
            builderGroup = inputGroup;
        }
        else
        {
            builderGroup = new HtmlBuilder(HtmlTag.Div)
                .WithCssClass(BootstrapHelper.InputGroupBtn);

            inputGroup.Append(builderGroup);
        }

        var btnGroup = linkButtonGroupFactory.Create();
        btnGroup.Actions = Actions;

        if (UseFloatingLabel)
            Actions.ForEach(a => a.CssClass += "btn-floating-action");

        btnGroup.ShowAsButton = true;

        //Add builder Actions
        btnGroup.AddActionsAt(builderGroup);
    }
    
    private HtmlBuilder GetHtmlAddons()
    {
        var html = new HtmlBuilder(HtmlTag.Span)
            .WithCssClass(BootstrapHelper.InputGroupAddon)
            .WithToolTip(Addons.Tooltip)
            .AppendIf(Addons.Icon != null, () => Addons.Icon.BuildHtml())
            .AppendTextIf(!string.IsNullOrEmpty(Addons.Text), Addons.Text);

        return html;
    }
}