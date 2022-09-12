using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JJMasterData.Commons.Dao;

namespace JJMasterData.Core.DataDictionary;

/// <summary>
/// Configurações de objetos do tipo lista
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
[Serializable]
[DataContract]
public class FormElementDataItem
{
    private DataAccessCommand _Command;
    private List<DataItemValue> _Itens;

    /// <summary>
    /// Tipo da origem dos dados
    /// </summary>
    [DataMember(Name = "dataItemType")]
    public DataItemType DataItemType { get; set; }

    /// <summary>
    /// Commando para recuperar o resultado no banco
    /// 1) Coluna Código;
    /// 2) Coluna Descrição
    /// </summary>
    [DataMember(Name = "command")]
    public DataAccessCommand Command
    {
        get
        {
            if (_Command == null)
                _Command = new DataAccessCommand();

            return _Command;
        }
        set
        {
            _Command = value;
        }
    }

    /// <summary>
    /// Lista de itens da combo [Chave, Valor]
    /// </summary>
    [DataMember(Name = "itens")]
    public List<DataItemValue> Itens
    {
        get
        {
            if (_Itens == null)
            {
                _Itens = new List<DataItemValue>();
            }
                    
            return _Itens;
        }
        set
        {
            _Itens = value;
        }
    }

    /// <summary>
    /// Mapeamento do dicionário
    /// </summary>
    [DataMember(Name = "elementMap")]
    public DataElementMap ElementMap { get; set; }

    /// <summary>
    /// Exibir texto (Todos) como primeira opção (Default = NONE)
    /// </summary>
    [DataMember(Name = "firstoption")]
    public FirstOptionMode FirstOption { get; set; }

    /// <summary>
    /// Substituir o valor do campo com a descrição da combo ao exibir a grid (Default = true)
    /// </summary>
    /// <remarks>
    /// Se utilizar filtro de campos na query ({CAMPO}), o texto pode ser exibido incorretamente na grid.
    /// O sistema faz cache dos dados da combo do primeiro registro exibido na lista.<br></br>
    /// Nesse caso por questão de desempenho recomendamos desabilitar essa opção e 
    /// criar um novo campo do tipo VIEWONLY para tratar o resultado na procedure
    /// </remarks>
    [DataMember(Name = "replacetextongrid")]
    public bool ReplaceTextOnGrid { get; set; }

    /// <summary>
    /// Permite incluir imagens na lista como legendas
    /// </summary>
    /// <remarks>
    /// Exibir como Legenda inclui imagem nos componentes combobox e na grid.<para></para>
    /// Para exibir apenas a imagem de status na grid certifique-se a 
    /// propriedade ReplaceTextOnGrid esta configurada para falso.
    /// </remarks>
    [DataMember(Name = "showimagelegend")]
    public bool ShowImageLegend { get; set; }

    public FormElementDataItem()
    {
        FirstOption = FirstOptionMode.None;
        ReplaceTextOnGrid = true;
        ElementMap = new DataElementMap();
    }


}