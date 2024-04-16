namespace JJMasterData.Core.UI.Components;

public enum CheckBoxSwitchSize
{
    Default,
    Small,
    Medium,
    Large,
    ExtraLarge
}

public static class CheckboxSwitchSizeExtensions
{
    public static string GetCssClass(this CheckBoxSwitchSize size)
    {
        return size switch
        {
            CheckBoxSwitchSize.Default => "",
            CheckBoxSwitchSize.Small => "form-switch-sm",
            CheckBoxSwitchSize.Medium => "form-switch-md",
            CheckBoxSwitchSize.Large => "form-switch-lg",
            CheckBoxSwitchSize.ExtraLarge => "form-switch-xl",
            _ => ""
        };
    }
}