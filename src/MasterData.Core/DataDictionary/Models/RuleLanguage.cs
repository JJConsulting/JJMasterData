using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataDictionary.Models;

public enum RuleLanguage
{
    [Display(Name = "SQL")]
    Sql = 0,
    [Display(Name = "JavaScript")]
    JavaScript = 1
}
