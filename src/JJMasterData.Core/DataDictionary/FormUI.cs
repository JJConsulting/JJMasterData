using System;
using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary;

[Serializable]
[DataContract]
public class FormUI
{
        
    /// <summary>
    /// Número de colunas onde os campos serão renderizados
    /// Default 1
    /// </summary>
    /// <remarks>
    /// Somente multiplos de 12
    /// </remarks>
    [DataMember(Name = "formCols")]
    public int FormCols { get; set; }

    /// <summary>
    /// Tipo de layout para renderizar os campos
    /// </summary>
    [DataMember(Name = "isVerticalLayout")]
    public bool IsVerticalLayout { get; set; }

    /// <summary>
    /// Quando o painel estiver no modo de visualização
    /// remover as bordas dos campos exibindo como texto.
    /// </summary>
    /// <remarks>
    /// Valor padrão falso
    /// </remarks>
    [DataMember(Name = "showViewModeAsStatic")]
    public bool ShowViewModeAsStatic { get; set; }

    /// <summary>
    /// Comportamento da Tecla enter no formulário 
    /// Default = DISABLED
    /// </summary>
    [DataMember(Name = "enterKey")]
    public FormEnterKey EnterKey { get; set; }

    public FormUI()
    {
        IsVerticalLayout = true;
        FormCols = 1;
        EnterKey = FormEnterKey.Disabled;
    }

}