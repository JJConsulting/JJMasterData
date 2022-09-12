using System;
using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary;

/// <summary>
/// Opções configuradas no dicionário de dados
/// </summary>
[Serializable]
[DataContract]
public class UIGrid
{
    /// <summary>
    /// Total de Registros por página 
    /// (Default = 5)
    /// </summary>
    /// <remarks>
    /// Se o TotalPerPage for zero a paginação não será exibida
    /// </remarks>
    [DataMember(Name = "totalPerPage")]
    public int TotalPerPage { get; set; }

    /// <summary>
    /// Total de botões na paginação 
    /// (Default = 5)
    /// </summary>
    [DataMember(Name = "totalPaggingButton")]
    public int TotalPaggingButton { get; set; }

    /// <summary>
    /// Exibi borda na grid 
    /// (Default = false)
    /// </summary>
    [DataMember(Name = "showBorder")]
    public bool ShowBorder { get; set; }

    /// <summary>
    /// Exibir colunas zebradas 
    /// (Default = true)
    /// </summary>*
    [DataMember(Name = "showRowStriped")]
    public bool ShowRowStriped { get; set; }

    /// <summary>
    /// Alterar a cor da linha ao passar o mouse 
    /// (Default = true)
    /// </summary>
    [DataMember(Name = "showRowHover")]
    public bool ShowRowHover { get; set; }

    /// <summary>
    /// Quantidade total de registros existentes no banco
    /// </summary>
    [DataMember(Name = "totalReg")]
    public int TotalReg { get; set; }

    /// <summary>
    /// Exibir título no cabeçalho da página
    /// </summary>
    [DataMember(Name = "showTitle")]
    public bool ShowTitle { get; set; }

    /// <summary>
    /// Exibir toolbar (Default = true) 
    /// </summary>
    [DataMember(Name = "showToolbar")]
    public bool ShowToolBar { get; set; }

    /// <summary>
    /// Habilita Ordenação das colunas (Default = true)
    /// </summary>
    /// <remarks>
    /// Habilita ou não o link nos titulos permitindo a ordenação.
    /// Mesmo quando configurado como falso, a grid respeita a propriedade CurrentOrder
    /// </remarks>
    [DataMember(Name = "enableSorting")]
    public bool EnableSorting { get; set; }

    /// <summary>
    /// Permite selecionar multiplas linhas na Grid 
    /// habilitando um checkbox na primeira coluna. (Defaut = false)
    /// </summary>
    [DataMember(Name = "enableMultSelect")]
    public bool EnableMultSelect { get; set; }

    /// <summary>
    /// Mantem os filtros, ordem e paginação da grid na sessão, 
    /// e recupera na primeira carga da pagina. (Default = false)
    /// </summary>
    /// <remarks>
    /// Ao utilizar esta propriedade, recomendamos alterar o parametro [Name] do objeto.
    /// A propriedade [Name] é utilizada para compor o nome da variável de sessão.
    /// </remarks>
    [DataMember(Name = "maintainValuesOnLoad")]
    public bool MaintainValuesOnLoad { get; set; }

    /// <summary>
    /// Obtém ou define um valor que indica se o cabeçalho da gridview ficará visível quando não existir dados.
    /// </summary>
    /// <remarks>
    /// Valor padrão = (Verdadeiro).
    /// <para/>
    /// Para alterar o texto da mensagem veja o método EmptyDataText
    /// </remarks>
    [DataMember(Name = "showHeaderWhenEmpty")]
    public bool ShowHeaderWhenEmpty { get; set; }

    /// <summary>
    /// Obtém ou define o texto a ser exibido na linha de dados vazia quando um controle JJGridView não contém registros.
    /// </summary>
    /// <remarks>
    /// Valor padrão = (Não existe registro para ser exibido).
    /// <para/>
    /// </remarks>
    [DataMember(Name = "emptyDataText")]
    public string EmptyDataText { get; set; }

    /// <summary>
    /// Exibe os controles de paginação (Default = true) 
    /// </summary>
    /// <remarks>
    /// Oculta todos os botões da paginação 
    /// porem mantem os controles de paginação pré-definidos.
    /// <para/>
    /// A Paginãção será exibida se o numero de registros da grid ultrapassar a quantidade minima de registros em uma pagina.
    /// <para/>
    /// Se a propriedade CurrentPage for igual zero  a paginação não será exibida.
    /// <para/>
    /// Se a propriedade CurrentUI.TotalPerPage for igual zero a paginação não será exibida.
    /// <para/>
    /// Se a propriedade TotalReg for igual zero a paginação não será exibida.
    /// </remarks>
    [DataMember(Name = "showPagging")]
    public bool ShowPagging { get; set; }

    /// <summary>
    /// Fixar o cabeçalho da grid ao realizar Scroll (Default = false)
    /// </summary>
    [DataMember(Name = "headerFixed")]
    public bool HeaderFixed { get; set; }


    public UIGrid()
    {
        TotalPerPage = 5;
        TotalPaggingButton = 5;
        ShowBorder = false;
        ShowRowStriped = true;
        ShowRowHover = true;
        ShowTitle = true;
        ShowToolBar = true;
        EnableSorting = true;
        ShowHeaderWhenEmpty = true;
        ShowPagging = true;
        EmptyDataText = "There is no record to display";

    }
}