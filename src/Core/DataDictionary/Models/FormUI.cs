using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models;


public class FormUI
{
        
    /// <summary>
    /// Número de colunas onde os campos serão renderizados
    /// Default 1
    /// </summary>
    /// <remarks>
    /// Somente multiplos de 12
    /// </remarks>
    [JsonProperty("formCols")]
    public int FormCols { get; set; } = 1;

    /// <summary>
    /// Tipo de layout para renderizar os campos
    /// </summary>
    [JsonProperty("isVerticalLayout")]
    public bool IsVerticalLayout { get; set; } = true;

    /// <summary>
    /// Quando o painel estiver no modo de visualização
    /// remover as bordas dos campos exibindo como texto.
    /// </summary>
    /// <remarks>
    /// Valor padrão falso
    /// </remarks>
    [JsonProperty("showViewModeAsStatic")]
    [Display(Name = "Show View Mode as Static")]
    public bool ShowViewModeAsStatic { get; set; }

    /// <summary>
    /// Comportamento da Tecla enter no formulário 
    /// Default = DISABLED
    /// </summary>
    [JsonProperty("enterKey")]
    public FormEnterKey EnterKey { get; set; } = FormEnterKey.Disabled;
}