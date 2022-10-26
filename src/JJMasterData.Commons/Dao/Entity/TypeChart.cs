using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JJMasterData.Commons.Dao.Entity
{
    public enum TypeChart
    {
        [Display(Name = "Bar")] bar,
        [Display(Name = "Pie")] pie,
        [Display(Name = "Line")] line,
        [Display(Name = "Polar Area")] polarArea,
        [Display(Name = "Doughnut")] doughnut

    }
}
