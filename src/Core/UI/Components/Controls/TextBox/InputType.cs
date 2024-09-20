
namespace JJMasterData.Core.UI.Components;

public enum InputType
{
    Text = 0,
    Password = 1,
    Number = 2,
    Tel = 3,
    Currency = 4
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
            InputType.Currency => "currency",
            _ => "text"
        };
    }
}