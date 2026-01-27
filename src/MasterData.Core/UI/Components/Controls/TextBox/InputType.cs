
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
    extension(InputType inputType)
    {
        public bool IsNumeric => inputType is InputType.Number or InputType.Currency;
        
        public string GetHtmlInputType()
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
}