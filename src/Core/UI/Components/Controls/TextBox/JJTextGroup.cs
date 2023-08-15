﻿using System.Collections.Generic;
using System.Linq;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Components;

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

    public JJTextGroup(string name, InputAddons addons, string text, IHttpContext httpContext) : base(httpContext)
    {
        Name = name;
        Addons = addons;
        Text = text;
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
            inputGroup.Append(GetHtmlAddons());

        inputGroup.Append(input);

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
             .AppendIf(Addons.Icon != null,()=> Addons.Icon.RenderHtml())
             .AppendTextIf(!string.IsNullOrEmpty(Addons.Text), Addons.Text);

        return html;
    }



}