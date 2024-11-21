#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace JJMasterData.Core.DataDictionary.Models;

public class FormElementDataFile
{
    /// <summary>
    /// Physical path where the file will be saved
    /// </summary>
    [Required]
    [JsonPropertyName("folderPath")]
    [Display(Name = "Folder Path")]
    public string FolderPath { get; set; } = null!;
    
    [JsonPropertyName("maxFileSize")]
    [Display(Name = "Max File Size (MB)")]
    public int MaxFileSize { get; set; }
    
    
    [JsonPropertyName("dragDrop")]
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
    [JsonPropertyName("allowedTypes")]
    [Display(Name = "Allowed Extensions")]
    public string AllowedTypes { get; set; } = "*";

    /// <summary>
    /// Permite upload simultaneo de arquivos.
    /// Default: True
    /// </summary>
    [JsonPropertyName("multipleFile")]
    [Display(Name = "Allow Multiple Files")]
    public bool MultipleFile { get; set; }

    /// <summary>
    /// Export file name with download link
    /// </summary>
    [JsonPropertyName("exportAsLink")]
    [Display(Name = "Export as Link")]
    public bool ExportAsLink { get; set; }


    /// <summary>
    /// Preview images in gallery format
    /// </summary>
    [JsonPropertyName("viewGallery")]
    [Display(Name = "Show Files in Gallery")]
    public bool ViewGallery { get; set; }

    [JsonPropertyName("allowPasting")]
    [Display(Name = "Allow pasting files")]
    public bool AllowPasting { get; set; } = true;
    
    [JsonPropertyName("showAsUploadView")]
    [Display(Name = "Show Upload Outside Modal")]
    public bool ShowAsUploadView { get; set; }

    public FormElementDataFile DeepCopy()
    {
        return (FormElementDataFile)MemberwiseClone();
    }
}