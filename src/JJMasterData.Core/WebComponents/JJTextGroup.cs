using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.WebComponents.Factories;

namespace JJMasterData.Core.WebComponents;

public class JJTextGroup : JJTextBox
{
    private List<JJLinkButton> _actions;

    /// <summary>
    /// Actions of input
    /// </summary>
    public List<JJLinkButton> Actions
    {
        get => _actions ??= new List<JJLinkButton>();
        set => _actions = value;
    }

    /// <summary>
    /// Text info on left of component
    /// </summary>
    public InputAddons Addons { get; set; }


    public string GroupCssClass { get; set; }

    public JJTextGroup(IHttpContext httpContext) : base(httpContext)
    {
    }
    
    public static JJTextGroup GetInstance(FormElementField f,IHttpContext httpContext, string name = null)
    {
        return new TextGroupFactory(httpContext).CreateTextGroup(f, name);
    }

    internal override HtmlBuilder RenderHtml()
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

        var input = base.RenderHtml();
        bool hasAction = Actions.ToList().Exists(x => x.Visible);
        bool hasAddons = Addons != null;

        if (!hasAction && !hasAddons)
            return input;


        if (defaultAction is { Enabled: true })
        {
            input.WithCssClass("default-option");
            input.WithAttribute("onchange", defaultAction.OnClientClick);
        }

        var inputGroup = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("input-group jjform-action ")
            .WithCssClass(GroupCssClass);

        if (hasAddons)
            inputGroup.AppendElement(GetHtmlAddons());

        inputGroup.AppendElement(input);

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

            inputGroup.AppendElement(builderGroup);
        }

        var btnGroup = new JJLinkButtonGroup();
        btnGroup.Actions = Actions;
        btnGroup.ShowAsButton = true;

        //Add builder Actions
        btnGroup.AddActionsAt(builderGroup);
    }


    private HtmlBuilder GetHtmlAddons()
    {
        var html = new HtmlBuilder(HtmlTag.Span)
             .WithCssClass(BootstrapHelper.InputGroupAddon)
             .WithToolTip(Addons.ToolTip)
             .AppendElementIf(Addons.Icon != null,()=> Addons.Icon.RenderHtml())
             .AppendTextIf(!string.IsNullOrEmpty(Addons.Text), Addons.Text);

        return html;
    }



}
