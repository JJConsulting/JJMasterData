using System;
using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataDictionary.Models;

public sealed class ElementBean
{
    [Display(Name = "Schema", Prompt = "Schema")]
    public string Schema { get; set; } = "dbo";
    
    [Display(Name = "Table or View Name", Prompt = "Table or View Name")]
    public string Name { get; init; } = null!;
    
    [Display(Name = "Import Fields")]
    public bool ImportFields { get; init; }
    
    [Display(Name = "Connection String")]
    public Guid? ConnectionId { get; init; }
}