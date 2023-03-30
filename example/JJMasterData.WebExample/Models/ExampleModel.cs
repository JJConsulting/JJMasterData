using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace JJMasterData.WebExample.Models
{
    public class ExampleModel
    {
        [Display(Name = "My Name")]
        public string Name { get; set; }

        [DisplayName("My Count")]
        public int Count { get; set; }

        public ExampleModel(string name, int count)
        {
            Name = name;
            Count = count;
        }
    }
}
