using System;
using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary;

[Serializable]
[DataContract]
public class FormElementDataFile
{
    /// <summary>
    /// Physical path where the file will be saved
    /// </summary>
    [DataMember(Name = "folderPath")]
    public string FolderPath { get; set; }

    /// <summary>
    /// Maximum file size in bytes
    /// </summary>
    [DataMember(Name = "maxFileSize")]
    public int MaxFileSize { get; set; }
    
    
    [DataMember(Name = "dragDrop")]
    public bool DragDrop { get; set; }

    /// <summary>
    /// Allowed extension types, separated by a comma.
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
    /// Export file name with download link
    /// </summary>
    [DataMember(Name = "exportAsLink")]
    public bool ExportAsLink { get; set; }


    /// <summary>
    /// Preview images in gallery format
    /// </summary>
    [DataMember(Name = "viewGallery")]
    public bool ViewGallery { get; set; }

}