#nullable enable

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary;

public class FormElementDataFile
{
    /// <summary>
    /// Physical path where the file will be saved
    /// </summary>
    [Required]
    public string FolderPath { get; set; } = null!;

    /// <summary>
    /// Maximum file size in bytes
    /// </summary>
    [JsonProperty("maxFileSize")]
    public int MaxFileSize { get; set; }
    
    
    [JsonProperty("dragDrop")]
    public bool DragDrop { get; set; }

    /// <summary>
    /// Allowed extension types, separated by a comma.
    /// Default: *
    /// </summary>
    /// <remarks>
    /// Example: txt,csv,log
    /// </remarks>
    [Required]
    public string AllowedTypes { get; set; } = "*";

    /// <summary>
    /// Permite upload simultaneo de arquivos.
    /// Default: True
    /// </summary>
    [JsonProperty("multipleFile")]
    public bool MultipleFile { get; set; }

    /// <summary>
    /// Export file name with download link
    /// </summary>
    [JsonProperty("exportAsLink")]
    public bool ExportAsLink { get; set; }


    /// <summary>
    /// Preview images in gallery format
    /// </summary>
    [JsonProperty("viewGallery")]
    public bool ViewGallery { get; set; }

}