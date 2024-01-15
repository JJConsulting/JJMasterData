#nullable enable

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models;

public class FormElementDataFile
{
    /// <summary>
    /// Physical path where the file will be saved
    /// </summary>
    [Required]
    [Display(Name = "Folder Path")]
    public string FolderPath { get; set; } = null!;
    
    [JsonProperty("maxFileSize")]
    [Display(Name = "Max File Size (MB)")]
    public int MaxFileSize { get; set; }
    
    
    [JsonProperty("dragDrop")]
    [Display(Name = "Drag and Drop")]
    public bool DragDrop { get; set; }

    /// <summary>
    /// Allowed extension types, separated by a comma.
    /// Default: *
    /// </summary>
    /// <remarks>
    /// Example: txt,csv,log
    /// </remarks>
    [Required]
    [Display(Name = "Allowed Extensions")]
    public string AllowedTypes { get; set; } = "*";

    /// <summary>
    /// Permite upload simultaneo de arquivos.
    /// Default: True
    /// </summary>
    [JsonProperty("multipleFile")]
    [Display(Name = "Allow Multiple Files")]
    public bool MultipleFile { get; set; }

    /// <summary>
    /// Export file name with download link
    /// </summary>
    [JsonProperty("exportAsLink")]
    [Display(Name = "Export as Link")]
    public bool ExportAsLink { get; set; }


    /// <summary>
    /// Preview images in gallery format
    /// </summary>
    [JsonProperty("viewGallery")]
    [Display(Name = "Show Files in Gallery")]
    public bool ViewGallery { get; set; }

    [JsonProperty("allowPasting")]
    [Display(Name = "Allow pasting files")]
    public bool AllowPasting { get; set; } = true;
    
    [JsonProperty("showAsUploadView")]
    [Display(Name = "Show as Upload View")]
    public bool ShowAsUploadView { get; set; }
}