#nullable enable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JJConsulting.FontAwesome;
using JJConsulting.Html.Bootstrap.Models;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

/// <summary>
/// Represents a basic action that can be executed with customizable behavior.
/// </summary>
public abstract class BasicAction
{
    ///  <summary>
    ///  Identifier of the action.
    ///  </summary>
    ///  <exception cref="ArgumentException"></exception>
    ///  <remarks>
    ///  <div class="IMPORTANT">
    /// <h5>IMPORTANT</h5>
    /// <p>Don't try to create an action with a repeated name. This can cause unforeseen consequences.</p>
    /// </div>
    ///  </remarks>
    [JsonPropertyName("name")]
    public string Name
    {
        get;
        set => field = value ?? throw new ArgumentException("Action name cannot be null");
    } = null!;

    /// <summary>
    /// Action description
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Control mouse tooltip
    /// </summary>
    [JsonPropertyName("tooltip")]
    public string? Tooltip { get; set; }

    /// <summary>
    /// Execute this action as default
    /// This action will be triggered in any line location
    /// </summary>
    [JsonPropertyName("isDefaultOption")]
    [Display(Name = "Is Default")] 
    public bool IsDefaultOption { get; set; }

    /// <summary>
    /// Display this action in a group menu
    /// Default = false
    /// </summary>
    [JsonPropertyName("isGroup")]
    [Display(Name = "Is Group")] 
    public bool IsGroup { get; set; }

    /// <summary>
    /// Creates a line separator before this action
    /// Default = false
    /// </summary>
    /// <remarks>
    /// Only valid if IsGroup is true.
    /// </remarks>
    [JsonPropertyName("dividerLine")]
    [Display(Name = "Divider Line")] 
    public bool DividerLine { get; set; }

    /// <summary>
    /// Link/Button FontAwesome icon.
    /// </summary>
    [JsonPropertyName("icon")]
    public FontAwesomeIcon Icon { get; set; }

    /// <summary>
    /// Display grid title
    /// </summary>
    [JsonPropertyName("showTitle")]
    [Display(Name = "Show Title")]
    public bool ShowTitle { get; set; }

    /// <summary>
    /// If completed when performing the action this message will be displayed with the option (Yes/No)
    /// No = Cancels the action
    /// Sim = Action will be executed
    /// </summary>
    [JsonPropertyName("confirmationMessage")]
    [Display(Name = "Confirmation Message")]
    public string? ConfirmationMessage { get; set; }

    /// <summary>
    /// Expression that returns a boolean. Operators such as equal symbol and in can be used.
    /// </summary>
    /// <example>
    /// <see cref="FormElementField.EnableExpression"/>
    /// </example>
    /// <remarks>
    /// [See expressions](../articles/expressions.md)
    /// </remarks>
    [JsonPropertyName("enableExpression")]
    [Display(Name = "Enable Expression")]
    public string? EnableExpression { get; set; }

    /// <summary>
    /// Expression that returns a boolean. Operators such as equal symbol and in can be used.
    /// </summary>
    /// <example>
    /// <see cref="FormElementField.VisibleExpression"/>
    /// </example>
    /// <remarks>
    /// [See expressions](../articles/expressions.md)
    /// </remarks>
    [JsonPropertyName("visibleExpression")]
    [Display(Name = "Visible Expression")]
    public string? VisibleExpression { get; set; }

    /// <summary>
    /// Order of the action (0, 1, 2...)
    /// </summary>
    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("showAsButton")] 
    [Display(Name = "Show as Button")]
    public bool ShowAsButton { get; set; }

    [JsonPropertyName("color")]
    [Display(Name ="Color" )]
    public BootstrapColor Color { get; set; }
    
    /// <summary>
    /// CSS3 Stylesheet Class
    /// </summary>
    [JsonPropertyName("cssClass")]
    public string? CssClass { get; set; }
    
    [JsonIgnore]
    public abstract bool IsUserDefined { get; }

    [JsonIgnore]
    public virtual bool IsSystemDefined => false;

    [JsonPropertyName("formToolbarActionLocation")]
    public FormToolbarActionLocation? Location { get; set; }
    
    protected BasicAction()
    {
        SetVisible(true);
        SetEnabled(true);
    }

    /// <summary>
    /// Set action visibility
    /// </summary>
    public void SetVisible(bool value)
    {
        VisibleExpression = value ? "val:1" : "val:0";
    }

    /// <summary>
    /// Set if the action is enabled.
    /// </summary>
    /// <param name="value"></param>
    public void SetEnabled(bool value)
    {
        EnableExpression = value ? "val:1" : "val:0";
    }

    /// <summary>
    /// Verify if the action is static toggled to be hidden.
    /// </summary>
    [JsonIgnore]
    public bool IsVisible => !"val:0".Equals(VisibleExpression);

    /// <summary>
    /// Copy the actions of another action.
    /// </summary>
    public void SetOptions(BasicAction action)
    {
        Text = action.Text;
        Tooltip = action.Tooltip;
        IsDefaultOption = action.IsDefaultOption;
        IsGroup = action.IsGroup;
        DividerLine = action.DividerLine;
        Icon = action.Icon;
        ShowTitle = action.ShowTitle;
        ConfirmationMessage = action.ConfirmationMessage;
        EnableExpression = action.EnableExpression;
        VisibleExpression = action.VisibleExpression;
        Order = action.Order;
        ShowAsButton = action.ShowAsButton;
        CssClass = action.CssClass;
    }

    public abstract BasicAction DeepCopy();
}