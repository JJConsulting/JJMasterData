using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Commons.Data.Entity.Models;

public enum RelationshipType
{
    Parent,
    [Display(Name = "1x1")]
    OneToOne,
    [Display(Name = "1xn")]
    OneToMany
}