using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Actions.Abstractions;

/// <summary>
/// Action basic info
/// </summary>
public abstract class BasicAction
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
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Action description
    /// </summary>
    [JsonProperty("text")]
    public string Text { get; set; }

    /// <summary>
    /// Control mouse tooltip
    /// </summary>
    [JsonProperty("tooltip")]
    public string Tooltip { get; set; }

    /// <summary>
    /// Execute this action as default
    /// This action will be triggered in any line location
    /// </summary>
    [JsonProperty("isDefaultOption")]
    public bool IsDefaultOption { get; set; }

    /// <summary>
    /// Display this action in a group menu
    /// Default = false
    /// </summary>
    [JsonProperty("isGroup")]
    public bool IsGroup { get; set; }

    /// <summary>
    /// Creates a line separator before this action
    /// Default = false
    /// </summary>
    /// <remarks>
    /// Only valid if IsGroup is true.
    /// </remarks>
    [JsonProperty("dividerLine")]
    public bool DividerLine { get; set; }

    /// <summary>
    /// Link/Button FontAwesome icon.
    /// </summary>
    [JsonProperty("icon")]
    public IconType Icon { get; set; }

    /// <summary>
    /// Display grid title
    /// </summary>
    [JsonProperty("showTitle")]
    public bool ShowTitle { get; set; }

    /// <summary>
    /// If completed when performing the action this message will be displayed with the option (Yes/No)
    /// No = Cancels the action
    /// Sim = Action will be executed
    /// </summary>
    [JsonProperty("confirmationMessage")]
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
    [JsonProperty("enableExpression")]
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
    [JsonProperty("visibleExpression")]
    public string VisibleExpression { get; set; }

    /// <summary>
    /// Order of the action (0, 1, 2...)
    /// </summary>
    [JsonProperty("order")]
    public int Order { get; set; }

    [JsonProperty("showAsButton")] public bool ShowAsButton { get; set; }

    /// <summary>
    /// CSS3 Stylesheet Class
    /// </summary>
    [JsonProperty("cssClass")]
    public string CssClass { get; set; }


    [JsonProperty("isCustomAction")] 
    public abstract bool IsUserCreated { get; }

    [JsonProperty(PropertyName = "formToolbarActionLocation", NullValueHandling = NullValueHandling.Ignore)]
    public FormToolbarActionLocation? FormToolbarActionLocation { get; set; }
    
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
        VisibleExpression = value ? "val:1" : "val:0";
    }

    /// <summary>
    /// Set if the action is enabled.
    /// </summary>
    /// <param name="value"></param>
    public void SetEnable(bool value)
    {
        EnableExpression = value ? "val:1" : "val:0";
    }

    /// <summary>
    /// Verifica se o controle é visivel, porém não aplica a expressao
    /// </summary>
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

}