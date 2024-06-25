using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

public class JJTextGroup(IComponentFactory<JJLinkButtonGroup> linkButtonGroupFactory, IFormValues formValues) 
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

    protected override Task<ComponentResult> BuildResultAsync()
    {
        var inputGroup = GetHtmlBuilder();

        return Task.FromResult<ComponentResult>(new RenderedComponentResult(inputGroup));
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
                input.WithSingleAttribute("placeholder");
                return new Div().WithCssClass("form-floating")
                    .Append(input)
                    .AppendLabel(label =>
                    {
                        label.AppendText(FloatingLabel);
                        label.WithAttribute("for", Name);
                    });
            }
            
            return input;
        }

        if (defaultAction is { Enabled: true })
        {
            input.WithCssClass("default-option");
            input.WithOnChange( defaultAction.OnClientClick);
        }

        var inputGroup = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("input-group jjform-action ")
            .WithCssClass(GroupCssClass);
        
        if (hasAddons)
            inputGroup.Append(GetHtmlAddons());
        
        if (UseFloatingLabel)
        {
            input.WithSingleAttribute("placeholder");
            inputGroup.AppendDiv(div =>
            {
                div.WithCssClass("form-floating");
                div.Append(input);
                div.AppendLabel(label =>
                {
                    label.AppendText(FloatingLabel);
                    label.WithAttribute("for", Name);
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
            Actions.ForEach(a=>a.CssClass += "btn-floating-action");
        
        btnGroup.ShowAsButton = true;

        //Add builder Actions
        btnGroup.AddActionsAt(builderGroup);
    }


    private HtmlBuilder GetHtmlAddons()
    {
        var html = new HtmlBuilder(HtmlTag.Span)
             .WithCssClass(BootstrapHelper.InputGroupAddon)
             .WithToolTip(Addons.Tooltip)
             .AppendIf(Addons.Icon != null,()=> Addons.Icon.BuildHtml())
             .AppendTextIf(!string.IsNullOrEmpty(Addons.Text), Addons.Text);

        return html;
    }
}
