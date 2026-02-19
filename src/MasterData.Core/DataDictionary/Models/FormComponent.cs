using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataDictionary.Models;

/// <summary>
/// Tipo do componente no formulário
/// </summary>
public enum FormComponent
{
    [Display(Name = "TextBox", GroupName = "1. Text")]
    Text = 1,
    [Display(GroupName = "1. Text")]
    TextArea = 2,
    [Display(GroupName = "3. Calendar")]
    Hour = 3,
    [Display(GroupName = "3. Calendar")]
    Date = 4,
    [Display(GroupName = "3. Calendar")]
    DateTime = 5,
    [Display(GroupName = "6. Mask")]
    Password = 6,
    [Display(GroupName = "6. Mask")]
    Email = 7,
    [Display(GroupName = "2. Numeric")]
    Number = 8,
    [Display(GroupName = "5. Data Item")]
    ComboBox = 9,
    [Display(Name = "SearchBox", GroupName = "5. Data Item")]
    Search = 10,
    [Display(Name = "RadioButtonGroup",GroupName = "5. Data Item")]
    RadioButtonGroup = 11,
    [Display(GroupName = "4. Boolean")]
    CheckBox = 12,
    [Display(Name = "CNPJ", GroupName = "6. Mask")]
    Cnpj = 13,
    [Display(Name = "CPF", GroupName = "6. Mask")]
    Cpf = 14,
    [Display(Name = "CNPJ/CPF", GroupName = "6. Mask")]
    CnpjCpf = 15,
    [Display(Name = "Currency", GroupName = "2. Numeric")]
    Currency = 16,
    [Display(Name = "Phone", GroupName = "6. Mask")]
    Tel = 17,
    [Display(Name = "CEP", GroupName = "6. Mask")]
    Cep = 18,
    [Display(GroupName = "5. Data Item")]
    Lookup = 21,
    [Display(GroupName = "7. File")]
    File = 22,
    [Display(GroupName = "2. Numeric")]
    Slider = 23,
    [Display(Name = "Color",GroupName = "8. Especial")]
    Color = 24,
    [Display(Name = "Icon",GroupName = "8. Especial")]
    Icon = 25,
    [Display(Name = "Percentage", GroupName = "2. Numeric")]
    Percentage = 26,
    [Display(Name = "Code Editor",GroupName = "8. Especial")]
    CodeEditor = 27
}

public static class FormComponentExtensions
{
    extension(FormComponent formComponent)
    {
        public bool SupportActions =>
            formComponent is FormComponent.Text
                or FormComponent.Email
                or FormComponent.Number
                or FormComponent.Cep
                or FormComponent.Cnpj
                or FormComponent.Cpf
                or FormComponent.CnpjCpf;
    }
}