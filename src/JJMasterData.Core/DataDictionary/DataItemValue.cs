using System;
using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary;

/// <summary>
/// Objeto responsável por armazenar os itens de uma lista
/// </summary>
[Serializable]
[DataContract]
public class DataItemValue
{
        
    /// <summary>
    /// Código único do item na lista
    /// </summary>
    [DataMember(Name = "id")]
    public string Id { get; set; }

    /// <summary>
    /// Descrição a ser exibida
    /// </summary>
    [DataMember(Name = "description")]
    public string Description { get; set; }

    /// <summary>
    /// Icone
    /// </summary>
    [DataMember(Name = "icon")]
    public IconType Icon { get; set; }

    /// <summary>
    /// Cor da imagem em hexadecimal. (exmplo #ffffff)
    /// </summary>
    [DataMember(Name = "imagecolor")]
    public string ImageColor { get; set; }

    /// <summary>
    /// Obtem uma nova instancia do objeto
    /// </summary>
    public DataItemValue() { }


    /// <summary>
    /// Obtem uma nova instancia do objeto
    /// </summary>
    /// <param name="id">Código único do item na lista</param>
    /// <param name="description">Descrição a ser exibida</param>
    public DataItemValue(string id, string description)
    {
        Id = id;
        Description = description;
    }

    /// <summary>
    /// Obtem uma nova instancia do objeto
    /// </summary>
    /// <param name="id">Código único do item na lista</param>
    /// <param name="description">Descrição a ser exibida</param>
    /// <param name="icon">Tipo da Imagem</param>
    /// <param name="imageColor">Cor da imagem em hexadecimal. (exmplo #ffffff)</param>
    public DataItemValue(string id, string description, IconType icon, string imageColor)
    {
        Id = id;
        Description = description;
        Icon = icon;
        ImageColor = imageColor;
    }

}