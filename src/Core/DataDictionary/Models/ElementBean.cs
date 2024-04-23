using System;
using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataDictionary.Models;

public sealed class ElementBean
{
    [Display(Name = "Table Or View Name", Prompt = "Table Or View Name")]
    public string Name { get; init; } = null!;
    
    [Display(Name = "Import Fields")]
    public bool ImportFields { get; init; }
    
    [Display(Name = "Connection")]
    public Guid? ConnectionId { get; init; }
}