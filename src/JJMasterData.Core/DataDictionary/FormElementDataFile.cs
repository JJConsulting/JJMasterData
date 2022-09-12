using System;
using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary;

[Serializable]
[DataContract]
public class FormElementDataFile
{
    /// <summary>
    /// Caminnho físico onde será gravado o arquivo
    /// </summary>
    [DataMember(Name = "folderPath")]
    public string FolderPath { get; set; }

    /// <summary>
    /// Tamanho máximo do arquivo em bytes
    /// </summary>
    [DataMember(Name = "maxFileSize")]
    public int MaxFileSize { get; set; }

    /// <summary>
    /// Habilita Drag and Drop
    /// </summary>
    [DataMember(Name = "dragDrop")]
    public bool DragDrop { get; set; }

    /// <summary>
    /// Tipod de extensão permitida, separados por virgula.
    /// Default: *
    /// </summary>
    /// <remarks>
    /// Example: txt,csv,log
    /// </remarks>
    [DataMember(Name = "allowedTypes")]
    public string AllowedTypes { get; set; }

    /// <summary>
    /// Permite upload simultaneo de arquivos.
    /// Default: True
    /// </summary>
    [DataMember(Name = "multipleFile")]
    public bool MultipleFile { get; set; }

    /// <summary>
    /// Exportar o nome do arquivo com link para download
    /// </summary>
    [DataMember(Name = "exportAsLink")]
    public bool ExportAsLink { get; set; }


    /// <summary>
    /// Pré-Visualizar imagens no formato galeria
    /// </summary>
    [DataMember(Name = "viewGallery")]
    public bool ViewGallery { get; set; }

}