using System;
using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary.Action;

/// <summary>
/// Action basic info
/// </summary>
[Serializable]
[DataContract]
public abstract class BasicAction : IAction
{
    /// <summary>
    /// Identifier of the component
    /// </summary>
    /// <remarks>
    /// <div class="IMPORTANT">
    ///<h5>IMPORTANT</h5>
    ///<p>Don't try to create a action with a repeated name. This can cause unforessen consequences.</p>
    ///</div>
    /// </remarks>
    [DataMember(Name = "name")]
    public string Name { get; set; }

    /// <summary>
    /// Action description
    /// </summary>
    [DataMember(Name = "text")]
    public string Text { get; set; }

    /// <summary>
    /// Control mouse tooltip
    /// </summary>
    [DataMember(Name = "tooltip")]
    public string ToolTip { get; set; }

    /// <summary>
    /// Execute this action as default
    /// This action will be triggered in any line location
    /// </summary>
    [DataMember(Name = "isDefaultOption")]
    public bool IsDefaultOption { get; set; }

    /// <summary>
    /// Display this action in a group menu
    /// Default = false
    /// </summary>
    [DataMember(Name = "isGroup")]
    public bool IsGroup { get; set; }

    /// <summary>
    /// Creates a line separator before this action
    /// Default = false
    /// </summary>
    /// <remarks>
    /// Only valid if IsGroup is true.
    /// </remarks>
    [DataMember(Name = "dividerLine")]
    public bool DividerLine { get; set; }

    /// <summary>
    /// Link/Button FontAwesome icon.
    /// </summary>
    [DataMember(Name = "icon")]
    public IconType Icon { get; set; }

    /// <summary>
    /// Display grid title
    /// </summary>
    [DataMember(Name = "showTitle")]
    public bool ShowTitle { get; set; }

    /// <summary>
    /// If completed when performing the action this message will be displayed with the option (Yes/No)
    /// No = Cancels the action
    /// Sim = Action will be executed
    /// </summary>
    [DataMember(Name = "confirmationMessage")]
    public string ConfirmationMessage { get; set; }

    /// <summary>
    /// Expression that returns a boolean. Operators such as equal symbol and in can be used.
    /// </summary>
    /// <example>
    /// <see cref="FormElementField.EnableExpression"/>
    /// </example>
    /// <remarks>
    /// [See expressions](../articles/expressions.md)
    /// </remarks>
    [DataMember(Name = "enableExpression")]
    public string EnableExpression { get; set; }

    /// <summary>
    /// Expression that returns a boolean. Operators such as equal symbol and in can be used.
    /// </summary>
    /// <example>
    /// <see cref="FormElementField.VisibleExpression"/>
    /// </example>
    /// <remarks>
    /// [See expressions](../articles/expressions.md)
    /// </remarks>
    [DataMember(Name = "visibleExpression")]
    public string VisibleExpression { get; set; }

    /// <summary>
    /// Order of the action (0,1,2...)
    /// </summary>
    [DataMember(Name = "order")]
    public int Order { get; set; }

    /// <summary>
    /// Show the action as a button
    /// </summary>
    [DataMember(Name = "showAsButton")]
    public bool ShowAsButton { get; set; }

    /// <summary>
    /// CSS3 Class
    /// </summary>
    [DataMember(Name = "cssClass")]
    public string CssClass { get; set; }

        
    /// <summary>
    /// Returns true if the action is user-created.
    /// </summary>
    [DataMember(Name = "isCustomAction")]
    public bool IsCustomAction { get; internal set; }

    public BasicAction()
    {
        SetVisible(true);
        SetEnable(true);
    }

    /// <summary>
    /// Set action visibility
    /// </summary>
    public void SetVisible(bool value)
    {
        if (value)
            VisibleExpression = "val:1";
        else
            VisibleExpression = "val:0";
    }

    /// <summary>
    /// Set if the action is enabled.
    /// </summary>
    /// <param name="value"></param>
    public void SetEnable(bool value)
    {
        if (value)
            EnableExpression = "val:1";
        else
            EnableExpression = "val:0";
    }

    /// <summary>
    /// Verifica se o controle é visivel, porém não aplica a expressao
    /// </summary>
    public bool IsVisible => !"val:0".Equals(VisibleExpression);

    /// <summary>
    /// Copy the actions of another action.
    /// </summary>
    public void SetOptions(BasicAction ac)
    {
        Text = ac.Text;
        ToolTip = ac.ToolTip;
        IsDefaultOption = ac.IsDefaultOption;
        IsGroup = ac.IsGroup;
        DividerLine = ac.DividerLine;
        Icon = ac.Icon;
        ShowTitle = ac.ShowTitle;
        ConfirmationMessage = ac.ConfirmationMessage;
        EnableExpression = ac.EnableExpression;
        VisibleExpression = ac.VisibleExpression;
        Order = ac.Order;
        ShowAsButton = ac.ShowAsButton;
        CssClass = ac.CssClass;
    }

}