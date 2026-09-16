using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataManager.Exportation.Abstractions;

public abstract class ExportFormatOptions 
{
    [Display(Name="Include First Row as Header")]
    public bool IncludeFirstRowAsHeader { get; set; }
}