
namespace JJMasterData.Core.UI.Components;

public enum InputType
{
    Text = 0,
    Password = 1,
    Number = 2,
    Tel = 3,
    Currency = 4,
    Percentage = 5
}

public static class InputTypeExtensions
{
    public static string GetInputType(this InputType inputType)
    {
        return inputType switch
        {
            InputType.Text => "text",
            InputType.Password => "password",
            InputType.Number => "number",
            InputType.Tel => "tel",
            InputType.Currency => "number",
            InputType.Percentage => "number",
            _ => "text"
        };
    }
}