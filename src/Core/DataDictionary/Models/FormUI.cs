﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace JJMasterData.Core.DataDictionary.Models;


public class FormUI
{
        
    /// <summary>
    /// Number of columns at the form to be rendered
    /// Default value: 1
    /// </summary>
    /// <remarks>
    /// Only multiple of twelve are supported
    /// </remarks>
    [JsonPropertyName("formCols")]
    [Display(Name = "Form Columns")]
    public int FormCols { get; set; } = 1;
    
    
    [JsonPropertyName("isVerticalLayout")]
    [Display(Name = "Use Vertical Layout")]
    public bool IsVerticalLayout { get; set; } = true;

    /// <summary>
    /// Quando o painel estiver no modo de visualização
    /// remover as bordas dos campos exibindo como texto.
    /// </summary>
    /// <remarks>
    /// Valor padrão falso
    /// </remarks>
    [JsonPropertyName("showViewModeAsStatic")]
    [Display(Name = "Show View Mode as Static")]
    public bool ShowViewModeAsStatic { get; set; }

    /// <summary>
    /// Comportamento da Tecla enter no formulário 
    /// Default = DISABLED
    /// </summary>
    [JsonPropertyName("enterKey")]
    [Display(Name = "Enter Key Behavior")]
    public FormEnterKey EnterKey { get; set; } = FormEnterKey.Disabled;

    public FormUI DeepCopy()
    {
        return (FormUI)MemberwiseClone();
    }
}